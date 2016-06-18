using System.Linq;
using System.Windows;
using Overstat.Capture;

namespace Overstat.View
{
  /// <summary>
  /// Interaction logic for NotifyWindow.xaml
  /// </summary>
  public partial class NotifyWindow : Window
  {
    public NotifyWindow()
    {
      InitializeComponent();
      
      label.DataContext = CaptureWorker.Instance.Notify;
    }
  }
}
