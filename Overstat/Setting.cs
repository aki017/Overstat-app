using CoreTweet;
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
  public partial class Setting : Form
  {
    private OAuth.OAuthSession session;
    public Setting()
    {
      InitializeComponent();
      Init();
    }

    private void Init()
    {
      startAuthButton.Enabled = Properties.Settings.Default.AccessToken == "" || Properties.Settings.Default.AccessTokenSecret == "";
      pinButton.Enabled = Properties.Settings.Default.AccessToken == "" || Properties.Settings.Default.AccessTokenSecret == "";
      ScreenshotFolderSelectBox.Text = Properties.Settings.Default.SaveDir;
    }

    private void startAuthButton_Click(object sender, EventArgs e)
    {
      session = OAuth.Authorize(Properties.Settings.Default.ConsumerKey, Properties.Settings.Default.ConsumerSecret);
      
      System.Diagnostics.Process.Start(session.AuthorizeUri.ToString());
    }

    private void pinButton_Click(object sender, EventArgs e)
    {
      if (session == null) return;
      var tokens = session.GetTokens(this.pinInputBox.Text);

      Properties.Settings.Default.AccessToken = tokens.AccessToken;
      Properties.Settings.Default.AccessTokenSecret = tokens.AccessTokenSecret;
      Properties.Settings.Default.Save();
      Init();
    }

    private void folderSelectButton_Click(object sender, EventArgs e)
    {
      this.ScreenshotFolderSelectDialog.ShowDialog();
      this.ScreenshotFolderSelectBox.Text = ScreenshotFolderSelectDialog.SelectedPath;
      Properties.Settings.Default.SaveDir = ScreenshotFolderSelectDialog.SelectedPath;
      Properties.Settings.Default.Save();
      Init();
    }
  }
}
