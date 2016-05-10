using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {

      var dlg = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog();
      dlg.Title = "作業フォルダの選択";
      dlg.IsFolderPicker = true;
      dlg.InitialDirectory = this.ImageSaveFolderBox.Text;
      dlg.AddToMostRecentlyUsedList = false;
      dlg.AllowNonFileSystemItems = false;
      dlg.DefaultDirectory = this.ImageSaveFolderBox.Text;
      dlg.EnsureFileExists = true;
      dlg.EnsurePathExists = true;
      dlg.EnsureReadOnly = false;
      dlg.EnsureValidNames = true;
      dlg.Multiselect = false;
      dlg.ShowPlacesList = true;

      if (dlg.ShowDialog() == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
      {
        this.ImageSaveFolderBox.Text = dlg.FileName;
      }
    }
  }
}
