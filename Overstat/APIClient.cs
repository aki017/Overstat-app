using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Overstat.Model;
using Overstat.Properties;

namespace Overstat
{
  static class APIClient
  {
    internal static string Token
    {
      get
      {
        if (!string.IsNullOrEmpty(Settings.Default.Token)) return Settings.Default.Token;
        Settings.Default.Token = System.Guid.NewGuid().ToString();
        Settings.Default.Save();
        return Settings.Default.Token;
      }
    }


    internal static async void Submit(MatchResult m, string filePath1, string filePath2)
    {
      try
      {
        var url = "http://overstat-1306.appspot.com/api/v1/results";
        var httpClient = new HttpClient();
        var form = new MultipartFormDataContent();

        form.Add(new StringContent(Token), "token");


        var bytes1 = File.ReadAllBytes(filePath1);
        form.Add(new ByteArrayContent(bytes1, 0, bytes1.Count()), "screenshot1", "screenshot.png");


        var bytes2 = File.ReadAllBytes(filePath2);
        form.Add(new ByteArrayContent(bytes2, 0, bytes2.Count()), "screenshot2", "screenshot.png");

        form.Add(new StringContent(m.MapName), "map_name");
        form.Add(new StringContent(m.HeroName), "hero_name");
        form.Add(new StringContent(m.Kills.ToString()), "kills");
        form.Add(new StringContent(m.ObjectiveKills.ToString()), "objective_kills");
        form.Add(new StringContent(m.ObjectiveTime.ToString()), "objective_time");
        form.Add(new StringContent(m.Damage.ToString()), "damage");
        form.Add(new StringContent(m.Heal.ToString()), "heal");
        form.Add(new StringContent(m.Deaths.ToString()), "deaths");
        var response = await httpClient.PostAsync(url, form);

        response.EnsureSuccessStatusCode();
        httpClient.Dispose();
        Console.WriteLine(response.Content.ReadAsStringAsync().Result);
      }
      catch
      {
        //dismiss
      }
    }
  }
}
