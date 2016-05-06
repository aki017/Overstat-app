using CoreTweet;
using OpenCvSharp;
using OpenCvSharp.Extensions;
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

    private int Counter = 0;

    public MainLoop()
    {
      Playing1Template = new Mat(GetImage("Playing1.png"));
      Playing1TemplateMask = new Mat(GetImage("Playing1Mask.png"));
      Result1Templates = new[] { new Mat(GetImage("Result1.png")) };
      Result1TemplateMasks = new[] { new Mat(GetImage("Result1Mask.png")) };
      Result2Templates = new[] { new Mat(GetImage("Result2.png")) };
      Result2TemplateMasks = new[] { new Mat(GetImage("Result2Mask.png")) };
    }
    public void Loop()
    {
      while (true)
      {
        var s = CheckState();
        if (this.State != s)
        {
          this.State = s;
        }

        Program.overlay.ChangeState(this.State);
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
      {
        using (var src = BitmapConverter.ToMat(bmp))
        {
          using (var result = new Mat())
          {
            Cv2.MatchTemplate(src, Playing1Template, result, TemplateMatchModes.CCorrNormed, Playing1TemplateMask);
            double _ = 0;
            double v = 0;
            result.MinMaxLoc(out _, out v);
            if (v > 0.9)
            {
              return true;
            }
          }
        }
        return false;
      }
    }

    public bool CheckResult1()
    {
      var screen = System.Windows.Forms.Screen.PrimaryScreen;

      using (var bmp = ScreenUtil.CaptureImage(screen, 1470, 45, 228, 58))
      {
        using (var src = BitmapConverter.ToMat(bmp))
        {
          for (var i = 0; i < Result1Templates.Length; i++)
          {
            using (var result = new Mat())
            {
              Cv2.MatchTemplate(src, Result1Templates[i], result, TemplateMatchModes.CCorrNormed, Result1TemplateMasks[i]);
              double _ = 0;
              double v = 0;
              result.MinMaxLoc(out _, out v);
              if (v > 0.99)
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
        }
      }
      return false;
    }

    public bool CheckResult2()
    {
      var screen = System.Windows.Forms.Screen.PrimaryScreen;

      using (var bmp = ScreenUtil.CaptureImage(screen, 1470, 45, 180, 58))
      {
        using (var src = BitmapConverter.ToMat(bmp))
        {
          for (var i = 0; i < Result2Templates.Length; i++)
          {
            //1470x45
            using (var result = new Mat())
            {
              Cv2.MatchTemplate(src, Result2Templates[i], result, TemplateMatchModes.CCorrNormed, Result2TemplateMasks[i]);
              double _ = 0;
              double v = 0;
              result.MinMaxLoc(out _, out v);
              if (v > 0.99)
              {
                using (var im = ScreenUtil.CaptureImage(screen))
                {
                  im.Save(Path.Combine(Properties.Settings.Default.SaveDir + "\\" + Counter + "_2.png"));
                }
                SubmitResult(Counter);
                Counter++;
                System.Threading.Thread.Sleep(1000);
                return true;
              }
            }
          }
        }
      }
      return false;
    }
    public async void SubmitResult(int i)
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
  }
}
