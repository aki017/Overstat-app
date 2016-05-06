using CoreTweet;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Overstat.GameData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Overstat
{
  class MainLoop
  {
    private State _state;
    private State State
    {
      get
      {
        return _state;
      }
      set
      {
        _state = value;
        Program.overlay.ChangeState(value);
      }
    }

    private OpenCvSharp.Mat Playing1Template;
    private OpenCvSharp.Mat Playing1TemplateMask;
    private OpenCvSharp.Mat[] Result1Templates;
    private OpenCvSharp.Mat[] Result1TemplateMasks;
    private OpenCvSharp.Mat[] Result2Templates;
    private OpenCvSharp.Mat[] Result2TemplateMasks;
    private Dictionary<Charactor, Mat> CharactorTemplates;
    private Mat CharactorMask;
    private Mat[] NumberTemplates;
    private Mat[] NumberTemplateMasks;

    private int Counter = 0;

    public MainLoop()
    {
      Playing1Template = new Mat(GetImage("Playing1.png"));
      Playing1TemplateMask = new Mat(GetImage("Playing1Mask.png"));
      Result1Templates = new[] { new Mat(GetImage("Result1.png")) };
      Result1TemplateMasks = new[] { new Mat(GetImage("Result1Mask.png")) };
      Result2Templates = new[] { new Mat(GetImage("Result2.png")) };
      Result2TemplateMasks = new[] { new Mat(GetImage("Result2Mask.png")) };
      CharactorTemplates = new Dictionary<Charactor, Mat>();
      CharactorMask = new Mat(GetImage("Chara\\CharaMask.png"));
      NumberTemplates = new[] {
        new Mat(GetImage("NumFont\\0.png")).ExtractChannel(1),
        new Mat(GetImage("NumFont\\1.png")).ExtractChannel(1),
        new Mat(GetImage("NumFont\\2.png")).ExtractChannel(1),
        new Mat(GetImage("NumFont\\3.png")).ExtractChannel(1),
        new Mat(GetImage("NumFont\\4.png")).ExtractChannel(1),
        new Mat(GetImage("NumFont\\5.png")).ExtractChannel(1),
        new Mat(GetImage("NumFont\\6.png")).ExtractChannel(1),
        new Mat(GetImage("NumFont\\7.png")).ExtractChannel(1),
        new Mat(GetImage("NumFont\\8.png")).ExtractChannel(1),
        new Mat(GetImage("NumFont\\9.png")).ExtractChannel(1)
      };
      NumberTemplateMasks = new[] {
        new Mat(GetImage("NumFont\\0Mask.png")).ExtractChannel(1),
        new Mat(GetImage("NumFont\\1Mask.png")).ExtractChannel(1),
        new Mat(GetImage("NumFont\\2Mask.png")).ExtractChannel(1),
        new Mat(GetImage("NumFont\\3Mask.png")).ExtractChannel(1),
        new Mat(GetImage("NumFont\\4Mask.png")).ExtractChannel(1),
        new Mat(GetImage("NumFont\\5Mask.png")).ExtractChannel(1),
        new Mat(GetImage("NumFont\\6Mask.png")).ExtractChannel(1),
        new Mat(GetImage("NumFont\\7Mask.png")).ExtractChannel(1),
        new Mat(GetImage("NumFont\\8Mask.png")).ExtractChannel(1),
        new Mat(GetImage("NumFont\\9Mask.png")).ExtractChannel(1)
      };
    }

    public void Loop()
    {
      while (true)
      {
        this.State = CheckState();

        if (this.State == State.Playing)
        {
          DetectCharactor();
        }

        System.Threading.Thread.Sleep(300);
      }
    }

    public string GetImage(string path)
    {
      return Path.Combine(System.Environment.CurrentDirectory, "Model", path);
    }

    public State CheckState()
    {
      var screen = System.Windows.Forms.Screen.PrimaryScreen;

      if (CheckPlaying())
      {
        return State.Playing;
      }
      if (CheckResult1())
      {
        return State.Result1;
      }
      if (CheckResult2())
      {
        return State.Result2;
      }
      return State.Unknown;
    }

    public bool CheckPlaying()
    {
      var screen = System.Windows.Forms.Screen.PrimaryScreen;
      using (var bmp = ScreenUtil.CaptureImage(screen, 860, 800, 200, 230))
      using (var src = BitmapConverter.ToMat(bmp))
      {
        if (ScreenUtil.DetectImage(src, Playing1Template, Playing1TemplateMask) > 0.9)
        {
          return true;
        }
        return false;
      }
    }

    public bool CheckResult1()
    {
      var screen = System.Windows.Forms.Screen.PrimaryScreen;

      using (var bmp = ScreenUtil.CaptureImage(screen, 1470, 45, 228, 58))
      using (var src = BitmapConverter.ToMat(bmp))
      {
        for (var i = 0; i < Result1Templates.Length; i++)
        {

          if (ScreenUtil.DetectImage(src, Result1Templates[i], Result1TemplateMasks[i]) > 0.99)
          {
            using (var im = ScreenUtil.CaptureImage(screen))
            {
              im.Save(Path.Combine(Properties.Settings.Default.SaveDir + "\\" + Counter + "_1.png"));
            }
            System.Threading.Thread.Sleep(1000);
            return true;
          }
        }
      }
      return false;
    }

    public bool CheckResult2()
    {
      var screen = System.Windows.Forms.Screen.PrimaryScreen;

      using (var bmp = ScreenUtil.CaptureImage(screen, 1470, 45, 180, 58))
      using (var src = BitmapConverter.ToMat(bmp))
      {
        for (var i = 0; i < Result2Templates.Length; i++)
        {
          if (ScreenUtil.DetectImage(src, Result2Templates[i], Result2TemplateMasks[i]) > 0.99)
          {
            using (var im = ScreenUtil.CaptureImage(screen))
            {
              im.Save(Path.Combine(Properties.Settings.Default.SaveDir + "\\" + Counter + "_2.png"));
            }
            var result = DetectScore();
            Program.overlay.t = string.Format("{0}/{1}/{2}/{3}/{4}/{5}", result.kills, result.objectiveKills, result.objectiveTime, result.damage, result.heal, result.deaths);
            //SubmitResult(Counter);
            Counter++;
            System.Threading.Thread.Sleep(1000);
            return true;
          }
        }
      }
      return false;
    }

    public async void SubmitResult(int i)
    {
      try
      {
        var tokens = Tokens.Create(Properties.Settings.Default.ConsumerKey, Properties.Settings.Default.ConsumerSecret,
         Properties.Settings.Default.AccessToken, Properties.Settings.Default.AccessTokenSecret);

        Program.overlay.t = "アップロード中";
        MediaUploadResult[] result = await Task.WhenAll(
          tokens.Media.UploadAsync(media: new FileInfo(Path.Combine(Properties.Settings.Default.SaveDir + "\\" + i + "_1.png"))),
          tokens.Media.UploadAsync(media: new FileInfo(Path.Combine(Properties.Settings.Default.SaveDir + "\\" + i + "_2.png")))
        );

        Program.overlay.t = "投稿中";
        Status s = await tokens.Statuses.UpdateAsync(
            status: "OverWatchをプレイ中！ #overwatch #overstat @OverStatApp",
            media_ids: result.Select(x => x.MediaId)
        );
        Program.overlay.t = "投稿しました";
        await Task.Delay(5000);
        Program.overlay.t = "";
      }
      catch (Exception ex)
      {
        Program.overlay.t = ex.ToString();
        await Task.Delay(5000);
        Program.overlay.t = "";
      }
    }

    private Charactor chara;
    public void DetectCharactor()
    {
      if (chara != Charactor.Unknown)
      {
        if (DetectCharactor(chara))
        {
          return;
        }
      }
      foreach (var i in Enum.GetValues(typeof(Charactor)))
      {
        var c = (Charactor)i;
        if (DetectCharactor(c))
        {
          chara = c;
          Program.overlay.t = c.ToString();
          return;
        }
      }
    }

    public bool DetectCharactor(Charactor c)
    {
      if (c == Charactor.Unknown) return false;
      var screen = System.Windows.Forms.Screen.PrimaryScreen;

      using (var bmp = ScreenUtil.CaptureImage(screen, 135, 880, 100, 120))
      using (var src = BitmapConverter.ToMat(bmp))
      {
        if (!CharactorTemplates.ContainsKey(c))
        {
          CharactorTemplates[c] = new Mat(GetImage("Chara\\" + c.ImageName() + ".png"));
        }

        if (ScreenUtil.DetectImage(src, CharactorTemplates[c], CharactorMask) > 0.97)
        {
          return true;
        }
        return false;
      }
    }

    public GameResultScore DetectScore()
    {
      var screen = System.Windows.Forms.Screen.PrimaryScreen;

      using (var bmp = ScreenUtil.CaptureImage(screen))
      using (var src = BitmapConverter.ToMat(bmp))
      {
        var result = new GameResultScore();
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
              return new { n = i, v = v1, x=ix*10+v2.X };
            }).OrderBy(s => s.v).Last();
            
            if (r.v > 0.91 && r.x - pos > 10) {
              pos = r.x;
              // result screen 
              //     160 410  430 680  700 950  970 1220 1240 1490 1510 1760
              // -160-|250|-20-|250|-20-|250|-20-|250|--20-|250|--20-|250|--160-
              if (160 < pos && pos < 410) { result.kills = result.kills * 10 + r.n; };
              if (430 < pos && pos < 680) { result.objectiveKills = result.objectiveKills * 10 + r.n; };
              if (700 < pos && pos < 950) { result.objectiveTime = result.objectiveTime * 10 + r.n; };
              if (970 < pos && pos <1220) { result.damage = result.damage * 10 + r.n; };
              if (1240< pos && pos <1490) { result.heal = result.heal * 10 + r.n; };
              if (1510< pos && pos <1760) { result.deaths = result.deaths * 10 + r.n; };
            }
          }
        }

        return result;
      }
    }
  }
}
