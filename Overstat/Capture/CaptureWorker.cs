using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.XFeatures2D;
using Overstat.Model.GameData;
using Overstat.Model;
using Overstat.Properties;
using Overstat.View;

namespace Overstat
{
  namespace Capture
  {
    public class CaptureWorker
    {
      /// <summary>
      /// Normal
      /// </summary>
      private Mat Playing1Template;
      private Mat Playing1TemplateMask;
      /// <summary>
      /// Ultimate Ready
      /// </summary>
      private Mat Playing2Template;
      private Mat Playing2TemplateMask;

      private Mat PlayingGageTemplate;
      private Mat PlayingGageTemplateMask;
      private Mat[] Result1Templates;
      private Mat[] Result1TemplateMasks;
      private Mat[] Result2Templates;
      private Mat[] Result2TemplateMasks;
      private Dictionary<Hero, Mat[]> HeroTemplates;
      private Mat CharactorMask;
      private Mat[] NumberTemplates;
      private Mat[] NumberTemplateMasks;

      private PlayingState[] playingStates = new PlayingState[5];
      private int playingStatePointer;
      private PlayingState playingState
      {
        get
        {
          return playingStates.GroupBy(s => s).OrderBy(g => g.Count()).First().Key;
        }
        set { playingStates[playingStatePointer++ % 5] = value; }
      }

      public static Type[] Implements => Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ICapture))).ToArray();

      public NotifyBinding Notify = new NotifyBinding();

      private List<MatchResult> MatchResults = new List<MatchResult>();
      private int PostCount = 0;
      private Dictionary<Hero, int> HeroDetectLog = new Dictionary<Hero, int>();

      public ICapture CaptureInstance
      {
        get
        {
          if (CaptureWorkerType == typeof(BitBltCapture)) return BitBltCapture.Instance;
          if (CaptureWorkerType == typeof(DXGICapture)) return DXGICapture.Instance;
          return DXGICapture.Instance;
        }
      }

      public static Type CaptureWorkerType
      {
        get
        {
          try
          {
            return Type.GetType(Settings.Default.CaptureType);
          }
          catch
          {
            return typeof(DXGICapture);
          }
        }
        set
        {
          Settings.Default.CaptureType = value?.FullName;
          Settings.Default.Save();
        }
      }

      public static CaptureWorker Instance;

      public CaptureWorker()
      {
        Instance = this;
        Playing1Template = new Mat(GetTemplateImage("Playing1.png"));
        Playing1TemplateMask = new Mat(GetTemplateImage("Playing1Mask.png"));
        Playing2Template = new Mat(GetTemplateImage("Playing2.png"));
        Playing2TemplateMask = new Mat(GetTemplateImage("Playing2Mask.png"));
        PlayingGageTemplate = new Mat(GetTemplateImage("PlayingGage.png"));
        PlayingGageTemplateMask = new Mat(GetTemplateImage("PlayingGageMask.png"));
        Result1Templates = new[] { new Mat(GetTemplateImage("Result1.png")) };
        Result1TemplateMasks = new[] { new Mat(GetTemplateImage("Result1Mask.png")) };
        Result2Templates = new[] { new Mat(GetTemplateImage("Result2.png")) };
        Result2TemplateMasks = new[] { new Mat(GetTemplateImage("Result2Mask.png")) };
        HeroTemplates = Enum.GetValues(typeof(Hero)).Cast<Hero>().ToDictionary(hero => hero, hero =>
          {
            if (hero == Hero.Unknown)
            {
              return new Mat[0];
            }
            var dirname = hero.ImageName();
            var dir = Path.Combine(Environment.CurrentDirectory, "Model", "GameData", "Template", "Hero", "Icon", dirname);
            return new DirectoryInfo(dir).GetFiles().Select(f => new Mat(f.FullName).Resize(new Size(192, 192))).ToArray();
          });
        CharactorMask = new Mat(GetTemplateImage("Hero\\CharaMask.png"));
        NumberTemplates = new[]
        {
          new Mat(GetTemplateImage("NumFont\\0.png")).ExtractChannel(1),
          new Mat(GetTemplateImage("NumFont\\1.png")).ExtractChannel(1),
          new Mat(GetTemplateImage("NumFont\\2.png")).ExtractChannel(1),
          new Mat(GetTemplateImage("NumFont\\3.png")).ExtractChannel(1),
          new Mat(GetTemplateImage("NumFont\\4.png")).ExtractChannel(1),
          new Mat(GetTemplateImage("NumFont\\5.png")).ExtractChannel(1),
          new Mat(GetTemplateImage("NumFont\\6.png")).ExtractChannel(1),
          new Mat(GetTemplateImage("NumFont\\7.png")).ExtractChannel(1),
          new Mat(GetTemplateImage("NumFont\\8.png")).ExtractChannel(1),
          new Mat(GetTemplateImage("NumFont\\9.png")).ExtractChannel(1)
        };
        NumberTemplateMasks = new[]
        {
          new Mat(GetTemplateImage("NumFont\\0Mask.png")).ExtractChannel(1),
          new Mat(GetTemplateImage("NumFont\\1Mask.png")).ExtractChannel(1),
          new Mat(GetTemplateImage("NumFont\\2Mask.png")).ExtractChannel(1),
          new Mat(GetTemplateImage("NumFont\\3Mask.png")).ExtractChannel(1),
          new Mat(GetTemplateImage("NumFont\\4Mask.png")).ExtractChannel(1),
          new Mat(GetTemplateImage("NumFont\\5Mask.png")).ExtractChannel(1),
          new Mat(GetTemplateImage("NumFont\\6Mask.png")).ExtractChannel(1),
          new Mat(GetTemplateImage("NumFont\\7Mask.png")).ExtractChannel(1),
          new Mat(GetTemplateImage("NumFont\\8Mask.png")).ExtractChannel(1),
          new Mat(GetTemplateImage("NumFont\\9Mask.png")).ExtractChannel(1)
        };
      }


      public Thread thread;

      public void Start()
      {
        Task.Run(Loop);
      }

      private async Task WaitOverwatchRunning()
      {
        while (!PlayingOverwatch())
        {
          Notify.Notify = "Not executing overwatch";
          await Task.Delay(1000);
        }
      }

      private async Task WaitForCaptureInstance()
      {
        while (CaptureInstance == null)
        {
          Notify.Notify = "Capture error happen";
          await Task.Delay(1000);
        }
      }

      public async Task Loop()
      {
        while (true)
        {
          await WaitOverwatchRunning();
          await WaitForCaptureInstance();

          playingState = DetectState();
          var targetPath = "";
          Notify.Notify = playingState.ToString();
          switch (playingState)
          {
            case PlayingState.Playing:
              if (MatchResults.Count <= PostCount)
              {
                MatchResults.Add(new MatchResult());
              }
              var hero = DetectHero();
              if (!HeroDetectLog.ContainsKey(hero))
              {
                HeroDetectLog[hero] = 0;
              }
              HeroDetectLog[hero]++;
              //Notify.Notify = $"Play detected as {hero}";
              break;
            case PlayingState.Result1:
              targetPath = GetOutputPath(1);

              if (!File.Exists(targetPath))
              {
                using (var im = CaptureInstance.GetCapture())
                {
                  im.SaveImage(targetPath);
                }
              }
              break;
            case PlayingState.Result2:
              targetPath = GetOutputPath(2);
              if (!File.Exists(targetPath))
              {
                using (var im = CaptureInstance.GetCapture())
                {
                  im.SaveImage(targetPath);
                }
              }

              if (PostCount < MatchResults.Count)
              {
                MatchResults.Last().Hero = HeroDetectLog.OrderBy(kvp => kvp.Value).Last().Key;
                HeroDetectLog.Clear();
                DetectScore();
                Notify.Notify = MatchResults.Last().ToString();
                if (File.Exists(targetPath) &&
                    File.Exists(GetOutputPath(2)))
                {
                  TweetUtil.Tweet(MatchResults.Last(),
                    GetOutputPath(1),
                    GetOutputPath(2));
                  APIClient.Submit(MatchResults.Last(),
                    GetOutputPath(1),
                    GetOutputPath(2));
                }
                PostCount++;
              }
              break;
          }
          Thread.Sleep(100);
        }
      }

      private PlayingState DetectState()
      {
        if (CheckPlaying())
        {
          return PlayingState.Playing;
        }
        if (CheckResult1())
        {
          return PlayingState.Result1;
        }
        if (CheckResult2())
        {
          return PlayingState.Result2;
        }
        return PlayingState.Unknown;
      }

      private string PrefixTime = DateTime.Now.ToString("HHmmss");
      public string GetOutputPath(int num)
      {
        return Path.Combine($"{Settings.Default.SaveFolder}\\{PrefixTime}{MatchResults.Count}_{num}.png").ToString();
      }

      public string GetTemplateImage(string path)
      {
        return Path.Combine(Environment.CurrentDirectory, "Model", "GameData", "Template", path);
      }

      public bool PlayingOverwatch()
      {
        var process = Process.GetProcesses().FirstOrDefault(p => p.ProcessName == "Overwatch");

        return process != default(Process);
      }

      public bool CheckPlaying()
      {
        using (var src = CaptureInstance.GetCapture(890, 840, 140, 140))
        {
          var v1 = ScreenCaptureUtil.DetectImage(src, Playing1Template, Playing1TemplateMask);
          var v2 = ScreenCaptureUtil.DetectImage(src, Playing2Template, Playing2TemplateMask);
          //var gage = ScreenCaptureUtil.DetectImage(src, PlayingGageTemplate, PlayingGageTemplateMask);
          //Notify.Notify = $"{v1}\n{v2}";
          if (v1 > 0.98 || v2 > 0.98) return true;
          return false;
        }
      }

      public bool CheckResult1()
      {
        using (var src = CaptureInstance.GetCapture(1470, 45, 228, 58))
        {
          if (Result1Templates.Where((t, i) => ScreenCaptureUtil.DetectImage(src, t, Result1TemplateMasks[i]) > 0.99).Any())
          {
            return true;
          }
        }
        return false;
      }

      public bool CheckResult2()
      {
        using (var src = CaptureInstance.GetCapture(1470, 45, 180, 58))
        {
          for (var i = 0; i < Result2Templates.Length; i++)
          {
            if (ScreenCaptureUtil.DetectImage(src, Result2Templates[i], Result2TemplateMasks[i]) > 0.99)
            {
              return true;
            }
          }
        }
        return false;
      }

      public Hero DetectHero()
      {
        DetectHp();

        using (var src = CaptureInstance.GetCapture(84, 836, 192, 192))
        using (var gray = src.CvtColor(ColorConversionCodes.RGB2GRAY))
        {

          var max = 0d;
          var target = Hero.Unknown;
          foreach (var hero in Enum.GetValues(typeof(Hero)).Cast<Hero>())
          {
            if (hero == Hero.Unknown)
            {
              continue;
            }

            foreach (var t in HeroTemplates[hero])
            {
              var v = ScreenCaptureUtil.DetectImage(src, t, t.Threshold(30, 255, ThresholdTypes.Binary));
              if (v > 0.99)
              {
                return hero;
              }
              if (v > max)
              {
                max = v;
                target = hero;
              }
            }
          }

          return target;
        }
      }

      private void DetectHp()
      {
        using (var im = CaptureInstance.GetCapture(260, 910, 100, 40))
        {
          NumberDetector.Detect(im);
        }
      }

      private int DetectNum(Mat src)
      {
        var result = 0;
        var pos = 0;

        using (var tmp = new Mat())
        {
          // 10pxずつずらしながら認識
          for (var ix = 0; ix < src.Width / 10 - 10; ix++)
          {
            var t = src.ColRange(ix * 10, Math.Min(ix * 10 + 60, src.Width - 60));

            var r = Enumerable.Range(0, 10).Select((i) =>
            {
              Cv2.MatchTemplate(t, NumberTemplates[i], tmp, TemplateMatchModes.CCorrNormed, NumberTemplateMasks[i]);
              double _ = 0;
              double v1 = 0;
              Point __ = new Point();
              Point v2 = new Point();
              tmp.MinMaxLoc(out _, out v1, out __, out v2);
              return new { n = i, v = v1, x = ix * 10 + v2.X };
            }).OrderBy(s => s.v).Last();

            if (r.v > 0.91 && r.x - pos > 10)
            {
              result = result * 10 + r.n;
            }
          }
        }
        return result;
      }

      public void DetectScore()
      {
        using (var src = CaptureInstance.GetCapture())
        {
          var result = MatchResults.Last();
          var target = src.ExtractChannel(1).RowRange(420, 520);

          var pos = 0;

          using (var tmp = new Mat())
          {
            for (var ix = 10; ix < src.Width / 10 - 10; ix++)
            {
              var t = target.ColRange(ix * 10, Math.Min(ix * 10 + 60, src.Width - 60));

              var r = Enumerable.Range(0, 10).Select((i) =>
              {
                Cv2.MatchTemplate(t, NumberTemplates[i], tmp, TemplateMatchModes.CCorrNormed, NumberTemplateMasks[i]);
                double _ = 0;
                double v1 = 0;
                Point __ = new Point();
                Point v2 = new Point();
                tmp.MinMaxLoc(out _, out v1, out __, out v2);
                return new { n = i, v = v1, x = ix * 10 + v2.X };
              }).OrderBy(s => s.v).Last();

              if (r.v > 0.91 && r.x - pos > 10)
              {
                pos = r.x;
                // result screen 
                //     160 410  430 680  700 950  970 1220 1240 1490 1510 1760
                // -160-|250|-20-|250|-20-|250|-20-|250|--20-|250|--20-|250|--160-
                if (160 < pos && pos < 410) { result.Kills = result.Kills * 10 + r.n; };
                if (430 < pos && pos < 680) { result.ObjectiveKills = result.ObjectiveKills * 10 + r.n; };
                if (700 < pos && pos < 950) { result.ObjectiveTime = result.ObjectiveTime * 10 + r.n; };
                if (970 < pos && pos < 1220) { result.Damage = result.Damage * 10 + r.n; };
                if (1240 < pos && pos < 1490) { result.Heal = result.Heal * 10 + r.n; };
                if (1510 < pos && pos < 1760) { result.Deaths = result.Deaths * 10 + r.n; };
              }
            }
          }
        }
      }
    }
  }

}