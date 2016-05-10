using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using OpenCvSharp;

namespace Overstat
{
  public class ScreenCaptureUtil
  {
    public static ScreenCaptureUtil Instance { get; } = new ScreenCaptureUtil();

    #region P/Invoke

    public delegate bool EnumWindowsDelegate(IntPtr hWnd, IntPtr lparam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumWindows(EnumWindowsDelegate lpEnumFunc, IntPtr lparam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest, IntPtr hdcSource,
      int xSrc, int ySrc, CopyPixelOperation rop);

    [DllImport("user32.dll")]
    private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowDC(IntPtr ptr);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
      public int left;
      public int top;
      public int right;
      public int bottom;
    }

    [DllImport("user32.dll")]
    private static extern int GetWindowRect(IntPtr hwnd, ref RECT lpRect);

    #endregion

    public ScreenCaptureUtil()
    {
      EnumWindows((hWnd, _) =>
      {
        var textLen = GetWindowTextLength(hWnd);
        if (0 < textLen)
        {
          var tsb = new StringBuilder(textLen + 1);
          GetWindowText(hWnd, tsb, tsb.Capacity);
          if (tsb.ToString() == "Overwatch")
          {
            hOverwatchWindow = hWnd;
            return false;
          }
        }

        return true;
      }, IntPtr.Zero);
    }

    RECT ScreenRect
    {
      get
      {
        RECT winRect = new RECT();
        GetWindowRect(hOverwatchWindow, ref winRect);
        return winRect;
      }
    }

    public IntPtr hOverwatchWindow;
    public static Bitmap CaptureWindow(int x = 0, int y = 0, int width = -1, int height = -1)
    {
      x = Math.Min(x, Instance.ScreenRect.right - Instance.ScreenRect.left);
      y = Math.Min(y, Instance.ScreenRect.bottom - Instance.ScreenRect.top);
      if (width == -1) width = Instance.ScreenRect.right - Instance.ScreenRect.left;
      if (height == -1) height = Instance.ScreenRect.bottom - Instance.ScreenRect.top;
      width = Math.Max(width, 1);
      height = Math.Max(height, 1);
      IntPtr hSrce = GetWindowDC(Instance.hOverwatchWindow);
      RECT winRect = new RECT();
      GetWindowRect(Instance.hOverwatchWindow, ref winRect);
      var bmp = new Bitmap(width, height);
      using (var g = Graphics.FromImage(bmp))
      {
        IntPtr hDC = g.GetHdc();

        BitBlt(hDC, 0, 0, width, height, hSrce, x, y, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
        g.ReleaseHdc(hDC);
      }
      ReleaseDC(Instance.hOverwatchWindow, hSrce);

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