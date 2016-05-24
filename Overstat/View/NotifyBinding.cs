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
      get { return a; }
      set
      {
        if (value != null && a == value) return;
        a = value;
        if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Notify"));
      }
    }

    public string a = "";
  }
}
