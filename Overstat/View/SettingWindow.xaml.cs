using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using Overstat.Capture;
using Overstat.Properties;

namespace Overstat.View
{
  /// <summary>
  /// Interaction logic for SettingWindow.xaml
  /// </summary>
  public partial class SettingWindow : Window
  {
    public SettingWindow()
    {
      InitializeComponent();

      TwitterSetting.DataContext = new TweetUtil();
      VersionLabel.Content = Assembly.GetExecutingAssembly().GetName().Version.ToString();
      CaptureSelect.ItemsSource = CaptureWorker.Implements;
      CaptureSelect.SelectedValue = CaptureWorker.CaptureWorkerType;
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
      var dlg = new CommonOpenFileDialog();
      dlg.Title = "Select SaveDir";
      dlg.IsFolderPicker = true;
      dlg.InitialDirectory = ImageSaveFolderBox.Text;
      dlg.AddToMostRecentlyUsedList = false;
      dlg.AllowNonFileSystemItems = false;
      dlg.DefaultDirectory = ImageSaveFolderBox.Text;
      dlg.EnsureFileExists = true;
      dlg.EnsurePathExists = true;
      dlg.EnsureReadOnly = false;
      dlg.EnsureValidNames = true;
      dlg.Multiselect = false;
      dlg.ShowPlacesList = true;

      if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
      {
        Settings.Default.SaveFolder = dlg.FileName;
        Settings.Default.Save();
      }
    }

    private void AuthTwitter_Click(object sender, RoutedEventArgs e)
    {
      TweetUtil.Auth();
    }
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
      Settings.Default.Save();

      base.OnClosing(e);
    }

    private void CaptureSelect_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      var index = ((ComboBox)sender).SelectedValue;
      if (index == null) return;
      CaptureWorker.CaptureWorkerType = (Type)index;
    }

    private void CaptureSettingOpen_Click(object sender, RoutedEventArgs e)
    {
      if (CaptureWorker.CaptureWorkerType == typeof(DXGICapture))
          new DXGICaptureSettingWindow().Show();
      if (CaptureWorker.CaptureWorkerType == typeof(BitBltCapture))
          new BitBltCaptureSettingWindow().Show();
    }

    private void NotificationWindowOpen_OnClickOpen_Click(object sender, RoutedEventArgs e)
    {
      new NotifyWindow().Show();
    }
  }
}
