using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Overstat
{
  public partial class OverlayScreen : Form
  {
    public OverlayScreen()
    {
      InitializeComponent();
      InitPosition();
    }

    public void InitPosition()
    {
      var size = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size;
      this.Location = new System.Drawing.Point(size.Width - 300, size.Height - 50);
    }

    delegate void SetTextCallback(State text);
    public string t = "";
    private int i = 0;
    public void ChangeState(State state)
    {
      if (this.label1.InvokeRequired)
      {
        SetTextCallback d = new SetTextCallback(ChangeState);
        this.Invoke(d, new object[] { state });
      }
      else
      {
        if(this.label1.Text != t)
        {
          i++;
          FadeIn(i);
        }
        this.label1.Text = t;
      }
    }

    private async void FadeIn(int c, int interval = 33)
    {
      this.Opacity = 0.6;
      //Object is not fully invisible. Fade it in
      while (i == c && this.Opacity > 0.05)
      {
        await Task.Delay(interval);
        this.Opacity -= 0.01;
      }
      if (i==c) this.Opacity = 0;
    }
  }
}
