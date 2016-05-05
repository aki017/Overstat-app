using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace Overstat
{
  static class Program
  {

    // P/Invoke declarations
    [DllImport("gdi32.dll")]
    static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int
    wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, System.Drawing.CopyPixelOperation rop);
    [DllImport("user32.dll")]
    static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);
    [DllImport("gdi32.dll")]
    static extern IntPtr DeleteDC(IntPtr hDc);
    [DllImport("gdi32.dll")]
    static extern IntPtr DeleteObject(IntPtr hDc);
    [DllImport("gdi32.dll")]
    static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
    [DllImport("gdi32.dll")]
    static extern IntPtr CreateCompatibleDC(IntPtr hdc);
    [DllImport("gdi32.dll")]
    static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);
    [DllImport("user32.dll")]
    public static extern IntPtr GetDesktopWindow();
    [DllImport("user32.dll")]
    public static extern IntPtr GetWindowDC(IntPtr ptr);

    /// <summary>
    /// アプリケーションのメイン エントリ ポイントです。
    /// </summary>
    [STAThread]
    private static void Main()
    {
      var notify = new NotifyIcon();
      notify.Text = "Exit OverStat";
      notify.Visible = true;
      notify.Icon =
        new System.Drawing.Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("Overstat.icon.ico"));
      var thread = new Thread(Loop);
      thread.IsBackground = true;
      thread.Start();

      notify.MouseDoubleClick += new MouseEventHandler((_, __) => {
        thread.Abort();
        Application.Exit();
      });
      Application.Run();
    }


    static void Loop()
    {
      Mat template = BitmapConverter.ToMat(new System.Drawing.Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("Overstat.template.jpg")));
      while (true)
      {
        using (var bmp = CaptureImage())
        {

          using (var src = BitmapConverter.ToMat(bmp))
          using (var result = new Mat())
          {
            Cv2.MatchTemplate(src, template, result, TemplateMatchModes.CCoeffNormed);

            double minVal, maxVal;
            Point minLoc, maxLoc;
            Cv2.MinMaxLoc(result, out minVal, out maxVal, out minLoc, out maxLoc);
            using (var view = src.Clone())
            {
              view.Rectangle(maxLoc, new Point(maxLoc.X + template.Width, maxLoc.Y + template.Height), Scalar.Red);
              
              if (maxVal > 0.7)
              {
                bmp.Save(Environment.CurrentDirectory+"\\Test\\"+DateTime.Now.ToString("HHmmss")+".png");
                Thread.Sleep(5000);
              }
            }
          }
        }
        Thread.Sleep(300);
      }
    }

    static System.Drawing.Bitmap CaptureImage()
    {
     System.Drawing.Size sz = Screen.PrimaryScreen.Bounds.Size;
      IntPtr hDesk = GetDesktopWindow();
      IntPtr hSrce = GetWindowDC(hDesk);
      IntPtr hDest = CreateCompatibleDC(hSrce);
      IntPtr hBmp = CreateCompatibleBitmap(hSrce, sz.Width, sz.Height);
      IntPtr hOldBmp = SelectObject(hDest, hBmp);
      bool b = BitBlt(hDest, 0, 0, sz.Width, sz.Height, hSrce, 0, 0,
        System.Drawing.CopyPixelOperation.SourceCopy | System.Drawing.CopyPixelOperation.CaptureBlt);
      System.Drawing.Bitmap bmp = System.Drawing.Bitmap.FromHbitmap(hBmp);
      SelectObject(hDest, hOldBmp);
      DeleteObject(hBmp);
      DeleteDC(hDest);
      ReleaseDC(hDesk, hSrce);
      return bmp;
    }
  }
}
