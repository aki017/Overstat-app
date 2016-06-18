using Overstat.Model.GameData;

namespace Overstat.Model
{
  class MatchResult
  {
    public Map Map;
    public string MapName => Map.ToString();
    public Hero Hero;
    public string HeroName => Hero.ToString();

    public int Kills { get; set; }
    public int ObjectiveKills { get; set; }
    public int ObjectiveTime { get; set; }
    public int Damage { get; set; }
    public int Heal { get; set; }
    public int Deaths { get; set; }

    public override string ToString()
    {
      return $"{Hero}/{Kills}/{ObjectiveKills}/{ObjectiveTime}/{Damage}/{Heal}/{Deaths}";
    }
  }
}
