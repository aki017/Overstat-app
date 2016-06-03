using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using OpenCvSharp;
using Overstat.Model;
using Overstat.Model.GameData;
using Overstat.View;
using Squirrel;
using Window = System.Windows.Window;

namespace Overstat
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();

      var c = new Capture.Capture();
      c.Start();

      var notificationWindow = new NotifyWindow(c);
      var desktop = System.Windows.Forms.Screen.AllScreens.First().Bounds;
      notificationWindow.Top = desktop.Height - (notificationWindow.Height + 10);
      notificationWindow.Left = desktop.Left + desktop.Width - (notificationWindow.Width + 10);
      notificationWindow.Show();
    }
    
    ObservableCollection<MatchResult> results = new ObservableCollection<MatchResult>();
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      int itemcount = 100;
      
      Random random = new Random();
      for (int j = 0; j < itemcount; j++)
      {
        results.Add(new MatchResult
        {
          Map = randomEnum<Map>(random),
          Hero = randomEnum<Hero>(random),
          Kills = random.Next(30),
          ObjectiveKills = random.Next(10),
          ObjectiveTime = random.Next(60),
          Damage = random.Next(3000),
          Heal = random.Next(10000),
          Deaths = random.Next(15)
        });
      }
      (FindResource("myView") as CollectionViewSource).Source  = results;
    }
    T randomEnum<T>(Random r)
    {
      return (T)Enum.GetValues(typeof(Map)).GetValue(r.Next(Enum.GetValues(typeof(Map)).Length));
    }

    private GridViewColumnHeader _lastHeaderClicked;
    private ListSortDirection _lastDirection = ListSortDirection.Ascending;

    private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
    {
      GridViewColumnHeader clickedHeader = e.OriginalSource as GridViewColumnHeader;
      if (clickedHeader == null)
        return;
      if (clickedHeader.Role != GridViewColumnHeaderRole.Padding)
      {
        ListSortDirection direction = ListSortDirection.Ascending;
        if (clickedHeader == _lastHeaderClicked && _lastDirection == ListSortDirection.Ascending)
        {
          direction = ListSortDirection.Descending;
        }
        string header = (clickedHeader.Column.DisplayMemberBinding as Binding).Path.Path;
        Sort(header, direction);
        _lastHeaderClicked = clickedHeader;
        _lastDirection = direction;
      }
    }
    private void Sort(string sortBy, ListSortDirection direction)
    {
      SortDescription sd = new SortDescription(sortBy, direction);
      (FindResource("myView") as CollectionViewSource).SortDescriptions.Clear();
      (FindResource("myView") as CollectionViewSource).SortDescriptions.Add(sd);
    }

    private void OpenSetting(object sender, RoutedEventArgs e)
    {
      new SettingWindow().Show();
    }
    private void ExitApplication(object sender, RoutedEventArgs e)
    {
      Application.Current.Shutdown();
    }
  }
}
