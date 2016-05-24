using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
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
  }
}
