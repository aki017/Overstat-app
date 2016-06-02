using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Overstat.Model.GameData;
using Overstat.Model;
using Overstat.View;
using Squirrel;

namespace Overstat
{
  namespace Capture
  {
    public class Capture
    {
      private Mat Playing1Template;
      private Mat Playing1TemplateMask;
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
          Notify.Notify = playingStates.GroupBy(s => s).OrderBy(g => g.Count()).First().Key.ToString();
          return playingStates.GroupBy(s => s).OrderBy(g => g.Count()).First().Key;
        }
        set { playingStates[playingStatePointer++ % 5] = value; }
      }

      public NotifyBinding Notify = new NotifyBinding();

      private List<MatchResult> MatchResults = new List<MatchResult>();
      private int PostCount = 0;

      public Capture()
      {
        Notify.Notify = "Test";
        Playing1Template = new Mat(GetTemplateImage("Playing1.png"));
        Playing1TemplateMask = new Mat(GetTemplateImage("Playing1Mask.png"));
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
         var dir = Path.Combine(Environment.CurrentDirectory, "Model", "GameData", "Template", "Hero", dirname);
         return new DirectoryInfo(dir).GetFiles().Select(f => new Mat(f.FullName, ImreadModes.GrayScale)).ToArray();
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
        thread = new Thread(Loop);
        thread.IsBackground = true;
        thread.Start();
      }

      public void Loop()
      {
        while (true)
        {
          if (!PlayingOverwatch())
          {
            Thread.Sleep(10000);
            continue;
          }


          playingState = DetectState();
          var targetPath = "";
          switch (playingState)
          {
            case PlayingState.Playing:
              if (MatchResults.Count <= PostCount)
              {
                MatchResults.Add(new MatchResult());
              }
              DetectHero().ToString();
              MatchResults.Last().Hero = MatchResults.Last().Hero == Hero.Unknown ? DetectHero() : MatchResults.Last().Hero;
              break;
            case PlayingState.Result1:
              targetPath = GetOutputPath(1);
              if (!File.Exists(targetPath))
              {
                using (var im = ScreenCaptureUtil.CaptureWindow())
                {
                  im.Save(targetPath);
                }
              }
              break;
            case PlayingState.Result2:
              targetPath = GetOutputPath(2);
              if (!File.Exists(targetPath))
              {
                using (var im = ScreenCaptureUtil.CaptureWindow())
                {
                  im.Save(targetPath);
                }
              }

              if (PostCount < MatchResults.Count)
              {
                DetectScore();
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
          Thread.Sleep(1000);
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
        return Path.Combine($"{Properties.Settings.Default.SaveFolder}\\{PrefixTime}{MatchResults.Count}_{num}.png").ToString();
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
        using (var bmp = ScreenCaptureUtil.CaptureWindow(860, 800, 200, 230))
        using (var src = bmp.ToMat())
        {
          if (ScreenCaptureUtil.DetectImage(src, Playing1Template, Playing1TemplateMask) > 0.9)
          {
            return true;
          }
          return false;
        }
      }

      public bool CheckResult1()
      {
        using (var bmp = ScreenCaptureUtil.CaptureWindow(1470, 45, 228, 58))
        using (var src = bmp.ToMat())
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
        using (var bmp = ScreenCaptureUtil.CaptureWindow(1470, 45, 180, 58))
        using (var src = bmp.ToMat())
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
        using (var bmp = ScreenCaptureUtil.CaptureWindow(1490, 900, 200, 100))
        using (var src = bmp.ToMat())
        using (var gray = src.CvtColor(ColorConversionCodes.RGB2GRAY))
        using (var binary = gray.Threshold(200, 255, ThresholdTypes.Binary))
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
              var v = ScreenCaptureUtil.DetectImage(binary, t, binary);
              if ( v > 0.9)
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

      public void DetectScore()
      {
        using (var bmp = ScreenCaptureUtil.CaptureWindow())
        using (var src = bmp.ToMat())
        {
          var result = MatchResults.Last();
          var target = src.ExtractChannel(1).RowRange(420, 520);

          var pos = 0;

          using (var tmp = new Mat())
          {
            for (var ix = 10; ix < bmp.Width / 10 - 10; ix++)
            {
              var t = target.ColRange(ix * 10, Math.Min(ix * 10 + 60, bmp.Width - 60));

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