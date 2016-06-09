using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Overstat.Capture
{
  public static class OverwatchDetector
  {

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
      public int Left;
      public int Top;
      public int Right;
      public int Bottom;

      public int Width => Right - Left;
      public int Height => Bottom - Top;
    }

    public static RECT ScreenRect
    {
      get
      {
        RECT winRect = new RECT();
        GetWindowRect(OverwatchWindowHandle, ref winRect);
        return winRect;
      }
    }


    private static IntPtr hOverwatchWindow;

    public static IntPtr OverwatchWindowHandle
    {
      get
      {
        if (hOverwatchWindow != default(IntPtr))
        {
          return hOverwatchWindow;
        }

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

        return hOverwatchWindow;
      }
    }

    public delegate bool EnumWindowsDelegate(IntPtr hWnd, IntPtr lparam);

    [DllImport("user32.dll")]
    private static extern int GetWindowRect(IntPtr hwnd, ref RECT lpRect);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumWindows(EnumWindowsDelegate lpEnumFunc, IntPtr lparam);
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetWindowTextLength(IntPtr hWnd);
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
  }
}