using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace Overstat.Capture
{
  public class BitBltCapture : ICapture
  {
    public string ApiName()
    {
      return "BitBlt";
    }

    public Mat GetCapture(int x = 0, int y = 0, int width = 1920, int height = 1080)
    {

      x = Math.Min(x, OverwatchDetector.ScreenRect.Width);
      y = Math.Min(y, OverwatchDetector.ScreenRect.Height);
      if (width == -1) width = OverwatchDetector.ScreenRect.Width;
      if (height == -1) height = OverwatchDetector.ScreenRect.Height;
      width = Math.Max(width, 1);
      height = Math.Max(height, 1);
      IntPtr hSrce = GetWindowDC(OverwatchDetector.OverwatchWindowHandle);
      using (var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
      {
        using (var g = Graphics.FromImage(bmp))
        {
          IntPtr hDC = g.GetHdc();

          BitBlt(hDC, 0, 0, width, height, hSrce, x, y, CopyPixelOperation.SourceCopy);
          g.ReleaseHdc(hDC);
        }
        ReleaseDC(OverwatchDetector.OverwatchWindowHandle, hSrce);
        return bmp.ToMat();
      }
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowDC(IntPtr ptr);
    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);

    [DllImport("user32.dll")]
    private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);
  }
}