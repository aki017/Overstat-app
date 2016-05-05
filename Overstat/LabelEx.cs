using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Overstat
{
  public class LabelEx : System.Windows.Forms.Label {
    protected override void OnPaint(PaintEventArgs e)
    {
      e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

      base.OnPaint(e);
    }
  }
}
