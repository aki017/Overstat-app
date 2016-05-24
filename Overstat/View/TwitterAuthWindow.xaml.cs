using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CoreTweet;

namespace Overstat.View
{
  /// <summary>
  /// TwitterAuthWindow.xaml の相互作用ロジック
  /// </summary>
  public partial class TwitterAuthWindow : Window
  {
    private OAuth.OAuthSession session;

    public TwitterAuthWindow()
    {
      InitializeComponent();

      session = OAuth.Authorize(Properties.Settings.Default.ConsumerKey, Properties.Settings.Default.ConsumerSecret);
      System.Diagnostics.Process.Start(session.AuthorizeUri.ToString());
    }

    private void Button_OnClick(object sender, RoutedEventArgs e)
    {
      var tokens = session.GetTokens(textBox.Text);

      Properties.Settings.Default.AccessToken = tokens.AccessToken;
      Properties.Settings.Default.AccessTokenSecret = tokens.AccessTokenSecret;
      Properties.Settings.Default.Save();
      Close();
    }
  }
}
