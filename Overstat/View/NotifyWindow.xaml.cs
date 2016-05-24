using System.Windows;

namespace Overstat.View
{
  /// <summary>
  /// Interaction logic for NotifyWindow.xaml
  /// </summary>
  public partial class NotifyWindow : Window
  {
    public NotifyWindow(Capture.Capture c)
    {
      InitializeComponent();
      
      label.DataContext = c.Notify;
    }
  }
}
