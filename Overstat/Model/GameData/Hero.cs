namespace Overstat.Model.GameData
{
  enum Hero
  {
    Unknown,
    Genji,
    Mccree,
    Pharah,
    Reaper,
    Soldier76,
    Tracer,
    Bastion,
    Hanzo,
    Junkrat,
    Mei,
    Torbjorn,
    Widowmaker,
    Dva,
    Reinhardt,
    Roadhog,
    Winston,
    Zarya,
    Lucio,
    Mercy,
    Symmetra,
    Zenyatta
  }

  static class HeroEx
  {
    static string ImageName(this Hero h)
    {
      if (h == Hero.Soldier76)
      {
        return "soldier-76";
      }
      return h.ToString().ToLower();
    }
  }
}
