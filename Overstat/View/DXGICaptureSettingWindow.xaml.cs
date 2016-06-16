using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Overstat.Capture;
using Window = System.Windows.Window;

namespace Overstat.View
{
  /// <summary>
  /// DXGICaptureSettingWindow.xaml の相互作用ロジック
  /// </summary>
  public partial class DXGICaptureSettingWindow : Window
  {
    private BackgroundWorker worker;
    private WriteableBitmap _previewImage;
    private DXGICapture CaptureInstance = new DXGICapture();

    public DXGICaptureSettingWindow()
    {
      InitializeComponent();

      worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true};

      // ProgressChangedイベントを発生させるようにする

      worker.DoWork += (sender, e) =>
      {
        while (true)
        {
          if (((BackgroundWorker) sender).CancellationPending) break;
          worker.ReportProgress(0, CaptureInstance.GetCapture());
          Thread.Sleep(1000/60);
        }
      };

      worker.ProgressChanged += worker_ProgressChanged;

    }

    private void RefreshTarget()
    {
      AdaptorSelecter.ItemsSource = CaptureInstance.Adapters.Select(a => a.Description1.Description.TrimEnd('\0'));
      AdaptorSelecter.SelectedIndex = (int)CaptureInstance.AdapterID;
      OutputSelecter.ItemsSource = CaptureInstance.Outputs.Select(o => o.Description.DeviceName.TrimEnd('\0'));
      OutputSelecter.SelectedIndex = (int)CaptureInstance.OutputID;
    }

    private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      using (var im = (Mat)e.UserState)
      {
        if (_previewImage == null)
        {
          _previewImage = im?.ToWriteableBitmap();
        }
        else
        {
          WriteableBitmapConverter.ToWriteableBitmap(im, _previewImage);
        }
        image.Source = _previewImage;
      }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      // DoWorkイベントハンドラの実行を開始
      worker.RunWorkerAsync();
      RefreshTarget();
    }

    private void RefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
      RefreshTarget();
    }

    private void AdaptorSelecter_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      var index = ((ComboBox) sender).SelectedIndex;
      if (index == -1) return;
      if (CaptureInstance.AdapterID == (uint) index) return;
      CaptureInstance.AdapterID = (uint)index;
      CaptureInstance.OutputID = 0;
      RefreshTarget();
    }

    private void OutputSelecter_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      var index = ((ComboBox)sender).SelectedIndex;
      if (index == -1) return;
      if (CaptureInstance.OutputID == (uint)index) return;
      if (((ComboBox)sender).SelectedIndex == -1) return;
      CaptureInstance.OutputID = (uint)index;
      RefreshTarget();
    }

    private void DXGICaptureSettingWindow_OnClosed(object sender, EventArgs e)
    {
      worker.CancelAsync();
      worker.Dispose();
      worker = null;
    }
  }
}
