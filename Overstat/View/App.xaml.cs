using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Overstat.Capture;
using SharpDX.DXGI;
using Squirrel;
using Window = OpenCvSharp.Window;

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
      AppDomain.CurrentDomain.UnhandledException += (sender, args) => ErrorHandle(args.ExceptionObject as Exception);
      DispatcherUnhandledException += (sender, args) =>
      {
        args.Handled = true;
        ErrorHandle(args.Exception);
      };
      Dispatcher.UnhandledException += (sender, args) => ErrorHandle(args.Exception);

    }

    public void ErrorHandle(Exception e)
    {
      MessageBox.Show(e.ToString(), "エラー");
      Shutdown();
    }
  }
}
