using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Overstat.Capture;
using Window = System.Windows.Window;

namespace Overstat.View
{
  /// <summary>
  /// BitBltCaptureSettingWindow.xaml の相互作用ロジック
  /// </summary>
  public partial class BitBltCaptureSettingWindow : Window
  {
    private BackgroundWorker worker;
    private WriteableBitmap _previewImage;
    private BitBltCapture CaptureInstance => BitBltCapture.Instance;
    public BitBltCaptureSettingWindow()
    {
      InitializeComponent();
      worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };

      // ProgressChangedイベントを発生させるようにする

      worker.DoWork += (sender, e) =>
      {
        while (true)
        {
          if (((BackgroundWorker)sender).CancellationPending) break;
          worker.ReportProgress(0, CaptureInstance.GetCapture());
          Thread.Sleep(1000 / 60);
        }
      };

      worker.ProgressChanged += worker_ProgressChanged;
    }

    private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      using (var im = (Mat)e.UserState)
      {
        if (_previewImage == null)
        {
          _previewImage = im.ToWriteableBitmap();
        }
        else
        {
          WriteableBitmapConverter.ToWriteableBitmap(im, _previewImage);
        }
        image.Source = _previewImage;
      }
    }

    private void BitBltCaptureSettingWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
      // DoWorkイベントハンドラの実行を開始
      worker.RunWorkerAsync();
    }

    private void BitBltCaptureSettingWindow_OnClosed(object sender, EventArgs e)
    {
      worker.ProgressChanged -= worker_ProgressChanged;
      worker.CancelAsync();
      worker.Dispose();
      worker = null;
    }
  }
}
