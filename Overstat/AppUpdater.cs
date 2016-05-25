using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Squirrel;

namespace Overstat
{
  public static class AppUpdater
  {
    private static UpdateManager _updateManagerInstance;

    public static UpdateManager AppUpdateManager
    {
      get
      {
        if (_updateManagerInstance != null)
        {
          return _updateManagerInstance;
        }

        var updateManagerTask = UpdateManager.GitHubUpdateManager("https://github.com/aki017/Overstat-app");
        updateManagerTask.Wait(TimeSpan.FromMinutes(1));
        _updateManagerInstance = updateManagerTask.Result;
        return _updateManagerInstance;
      }
    }

    public static void Dispose()
    {
      _updateManagerInstance?.Dispose();
    }

    public static async Task<bool> CheckForUpdates()
    {
      try
      {
        var updateInfo = await AppUpdateManager.CheckForUpdate();
        if (updateInfo.ReleasesToApply.Count > 0)
        {
          return true;
        }
      }
      catch (Exception)
      {
        // Update failed. 
      }
      return false;
    }

    public static async Task<ReleaseEntry> ApplyUpdates()
    {
      var result = await AppUpdateManager.UpdateApp();
      return result;
    }

    public static void CreateShortcutForThisExe()
    {
      AppUpdateManager.CreateShortcutForThisExe();
    }

    public static void RemoveShortcutForThisExe()
    {
      AppUpdateManager.RemoveShortcutForThisExe();
    }
  }
}
