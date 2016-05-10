using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Overstat.Model
{
  class MatchResult
  {
    public GameData.Map Map;
    public string MapName => Map.ToString();
    public GameData.Hero Hero;
    public string HeroName => Hero.ToString();

    public int Kills { get; set; }
    public int ObjectiveKills { get; set; }
    public int ObjectiveTime { get; set; }
    public int Damage { get; set; }
    public int Heal { get; set; }
    public int Deaths { get; set; }
  }
}
