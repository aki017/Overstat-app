using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreTweet;
using Overstat.Model;
using Overstat.View;

namespace Overstat
{
  class TweetUtil
  {
    bool IsTweetable => Properties.Settings.Default.AccessToken != null && Properties.Settings.Default.AccessTokenSecret != null;

    public static void Auth()
    {
      new TwitterAuthWindow().Show();
    }
    async void Tweet(MatchResult result, string image1, string image2)
    {
      if (!IsTweetable) return;
      var tokens = Tokens.Create(Properties.Settings.Default.ConsumerKey, Properties.Settings.Default.ConsumerSecret,
         Properties.Settings.Default.AccessToken, Properties.Settings.Default.AccessTokenSecret);
      var results = await Task.WhenAll(
        tokens.Media.UploadAsync(media: new FileInfo(image1)),
        tokens.Media.UploadAsync(media: new FileInfo(image2))
      );
      
      Status s = await tokens.Statuses.UpdateAsync(
          status: string.Format("OverWatchを{0}でプレイ中！ {1}Kill/{2}Death #overstat", result.HeroName, result.Kills, result.Deaths),
          media_ids: results.Select(x => x.MediaId)
      );
    }
  }
}
