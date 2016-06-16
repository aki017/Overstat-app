using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace Overstat
{
  public class ScreenCaptureUtil
  {
    public static double DetectImage(Mat src, Mat template, Mat mask = null)
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