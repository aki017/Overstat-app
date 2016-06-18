using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Overstat.View
{
  public class NotifyBinding : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    public string Notify
    {
      get
      {
        var a = BackLog.Skip(BackLogIndex).Take(30 - BackLogIndex);
        var b = BackLog.Take(BackLogIndex);
        return string.Join("\n", a) + "\n" + string.Join("\n", b);
      }
      set
      {
        if (value == null || value == BackLog[BackLogIndex]) return;
        BackLog[(30 + BackLogIndex - 1) % 30] = value;
        BackLogIndex--;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Notify"));
      }
    }

    public string[] BackLog = new string[30];
    private int _BackLogIndex;

    private int BackLogIndex
    {
      get
      {
        return _BackLogIndex;
      }
      set { _BackLogIndex = (30 + value) % 30; }
    }
  }
}
