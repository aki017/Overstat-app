using System.Threading.Tasks;
using System.Windows;
using Squirrel;

namespace Overstat
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      Task.Run(async () =>
      {
        using (var mgr = await UpdateManager.GitHubUpdateManager("https://github.com/aki017/Overstat-app"))
        {
          await mgr.UpdateApp();
        }
      });
    }
  }
}
