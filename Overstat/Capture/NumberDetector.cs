using System;
using System.Diagnostics;
using OpenCvSharp;
using System.IO;
using System.Linq;
using System.Threading;
using OpenCvSharp.Extensions;
using OpenCvSharp.ML;
using Overstat.Capture;
using Overstat.View;

namespace Overstat.Capture
{
  public class NumberDetector
  {
    //45x52 
    //30x34
    private static Mat[] NumberTemplates = new Mat[10];
    private static Mat[] NumberTemplateMasks = new Mat[10];

    private static Window w;

    private static SVM _svm;
    private static SVM svm
    {
      get
      {
        if (_svm != null) return _svm;
        _svm = SVM.Create();

        _svm.Type = SVM.Types.CSvc;
        _svm.KernelType = SVM.KernelTypes.Rbf;
        _svm.TermCriteria = TermCriteria.Both(1000, 0.000001);
        _svm.Degree = 100.0;
        _svm.Gamma = 100.0;
        _svm.Coef0 = 1.0;
        _svm.C = 1.0;
        _svm.Nu = 0.5;
        _svm.P = 0.1;
        var files = new DirectoryInfo("Sample").GetDirectories().SelectMany(d => d.GetFiles()).Where(f => f.Extension == ".png");

        var mats = files.Select(i => new Mat(i.FullName)).ToArray();
        var points = mats.Select(m => m.Data).ToArray();
        var res = files.Select(f => int.Parse(f.Directory.Name)).ToArray();
        var dataMat = new Mat(points.Length, 32 * 40, MatType.CV_32FC1, points);
        var resMat = new Mat(points.Length, 1, MatType.CV_32SC1, res);
        _svm.Train(dataMat, SampleTypes.RowSample, resMat);
        return _svm;
      }
    }

    // static Random rand = new Random();
    public static int Detect(Mat src, int FontWidth = 25)
    {
      return 0;
      using (var arr = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3)))
      using (var gray = src.CvtColor(ColorConversionCodes.BGR2GRAY))
      using (var open = gray.MorphologyEx(MorphTypes.Open, arr, null, 1))
      using (var bin = open.Threshold(220, 255, ThresholdTypes.Binary))
      {
        /*
          var i = rand.Next();
          bin.ColRange(0, 0 + 32).SaveImage("Sample/"+DateTime.Now.ToString("1_HHmmss")+"_"+ i + ".png");
          bin.ColRange(22, 22 + 32).SaveImage("Sample/" + DateTime.Now.ToString("2_HHmmss") + "_"+i + ".png");
          bin.ColRange(44, 44 + 32).SaveImage("Sample/" + DateTime.Now.ToString("3_HHmmss") + "_" + i + ".png");
          */
        var sampleMat1 = new Mat(1, 32 * 40, MatType.CV_32FC1, bin.ColRange(0, 0 + 32).Resize(new Size(32, 40)).Data);
        var sampleMat2 = new Mat(1, 32 * 40, MatType.CV_32FC1, bin.ColRange(22, 22 + 32).Resize(new Size(32, 40)).Data);
        var sampleMat3 = new Mat(1, 32 * 40, MatType.CV_32FC1, bin.ColRange(44, 44 + 32).Resize(new Size(32, 40)).Data);
        var test = svm.Predict(sampleMat1)*100 + svm.Predict(sampleMat2) * 10 + svm.Predict(sampleMat3) * 1;
        CaptureWorker.Instance.Notify.Notify = test.ToString();
        /*
        Cv2.ImShow("th1", bin.ColRange(0, 0 + 32).Resize(new Size(64, 80)));
        Cv2.ImShow("th2", bin.ColRange(22, 22 + 32).Resize(new Size(64, 80)));
        Cv2.ImShow("th3", bin.ColRange(44, 44 + 32).Resize(new Size(64, 80)));
        */
        Cv2.WaitKey(1);
        //Thread.Sleep(999);
      }
      return 0;
    }

    private static Mat MinOfChannels(Mat src)
    {
      using (var c0 = src.ExtractChannel(0))
      using (var c1 = src.ExtractChannel(1))
      using (var c2 = src.ExtractChannel(2))
      {
        return c0 & c1 & c2;
      }
    }
    private static Mat MaxOfChannels(Mat src)
    {
      using (var c0 = src.ExtractChannel(0))
      using (var c1 = src.ExtractChannel(1))
      using (var c2 = src.ExtractChannel(2))
      {
        return (c0 + c1 + c2) / 3;
      }
    }


    private static Mat GetTemplate(int i)
    {
      return NumberTemplates[i] ?? (NumberTemplates[i] = LoadTemplate(i));
    }

    private static Mat GetTemplateMask(int i)
    {
      return NumberTemplateMasks[i] ?? (NumberTemplateMasks[i] = LoadTemplateMask(i));
    }

    private static Mat LoadTemplate(int i)
    {
      using (var mat = new Mat(Path.Combine(Environment.CurrentDirectory, "Model", "GameData", "Template", "NumFont", $"{i}.png")))
      {
        var c1 = mat.ExtractChannel(0);
        return c1;
      }
    }
    private static Mat LoadTemplateMask(int i)
    {
      using (var mat = new Mat(Path.Combine(Environment.CurrentDirectory, "Model", "GameData", "Template", "NumFont", $"{i}Mask.png")))
      {
        var c1 = mat.ExtractChannel(0);
        return c1;
      }
    }
    /*
    private static Mat LoadTemplate(int i)
    {
      using (var mat = new Mat(Path.Combine(Environment.CurrentDirectory, "Model", "GameData", "Template", "NumFont", $"{i}.png")))
      using (var c1 = mat.ExtractChannel(0))
      using (var c2 = c1.Resize(new Size(15, 30)))
      using (var arr = Cv2.GetStructuringElement(MorphShapes.Cross, new Size(3, 3)))
      {
        var bordered = c2.MorphologyEx(MorphTypes.Gradient, arr);
        return bordered;
      }
    }
    private static Mat LoadTemplateMask(int i)
    {
      using (var mat = LoadTemplate(i))
      using (var arr = Cv2.GetStructuringElement(MorphShapes.Cross, new Size(1, 1)))
      {
        var bordered = mat.Dilate(arr);
        return bordered;
      }
    }
    */
  }
}