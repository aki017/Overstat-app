using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Overstat
{
  public static class ScreenUtil
  {
    // P/Invoke declarations
    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int
    wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, System.Drawing.CopyPixelOperation rop);
    [DllImport("user32.dll")]
    private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);
    [DllImport("gdi32.dll")]
    private static extern IntPtr DeleteDC(IntPtr hDc);
    [DllImport("gdi32.dll")]
    private static extern IntPtr DeleteObject(IntPtr hDc);
    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);
    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();
    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowDC(IntPtr ptr);

    public static System.Drawing.Bitmap CaptureImage(System.Windows.Forms.Screen screen, int x = 0, int y = 0, int width = -1, int height = -1)
    {
      if (width == -1) width = screen.Bounds.Width;
      if (height == -1) height = screen.Bounds.Height;

      IntPtr hDesk = GetDesktopWindow();
      IntPtr hSrce = GetWindowDC(hDesk);
      IntPtr hDest = CreateCompatibleDC(hSrce);
      IntPtr hBmp = CreateCompatibleBitmap(hSrce, width, height);
      IntPtr hOldBmp = SelectObject(hDest, hBmp);
      bool b = BitBlt(hDest, 0, 0, width, height, hSrce, x, y, System.Drawing.CopyPixelOperation.SourceCopy | System.Drawing.CopyPixelOperation.CaptureBlt);
      System.Drawing.Bitmap bmp = System.Drawing.Bitmap.FromHbitmap(hBmp);
      SelectObject(hDest, hOldBmp);
      DeleteObject(hBmp);
      DeleteDC(hDest);
      ReleaseDC(hDesk, hSrce);
      return bmp;
    }

    public static double DetectImage(Mat src, Mat template, Mat mask)
    {
      using (var result = new Mat())
      {
        Cv2.MatchTemplate(src, template, result, TemplateMatchModes.CCorrNormed, mask);
        double _ = 0;
        double v = 0;
        result.MinMaxLoc(out _, out v);
        return v;
      }
    }
  }
}
