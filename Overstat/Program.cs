using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Squirrel;

namespace Overstat
{
  static class Program
  {
    public static OverlayScreen overlay;
    
    /// <summary>
    /// アプリケーションのメイン エントリ ポイントです。
    /// </summary>
    [STAThread]
    private static void Main()
    {
      Task.Run(async () =>
      {
        using (var mgr = UpdateManager.GitHubUpdateManager("https://github.com/aki017/Overstat-app"))
        {
          await mgr.Result.UpdateApp();
        }
      });

      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      var notify = new NotifyIcon();
      notify.Text = "DoubleClick to exit OverStat";
      notify.Visible = true;
      notify.Icon = new System.Drawing.Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("Overstat.icon.ico"));
      notify.Click += (_, __) => new Setting().Show();

      overlay = new OverlayScreen();
      var thread = new Thread(new MainLoop().Loop);
      thread.IsBackground = true;
      thread.Start();
      if (Properties.Settings.Default.AccessToken == "" || Properties.Settings.Default.AccessTokenSecret == "" || Properties.Settings.Default.SaveDir == "")
      {
        new Setting().Show();
      }

      notify.MouseDoubleClick += new MouseEventHandler((_, __) =>
      {
        thread.Abort();
        Application.Exit();
      });
      Application.Run(overlay);
    }

  }
}
