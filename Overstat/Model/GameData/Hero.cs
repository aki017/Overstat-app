namespace Overstat.Model.GameData
{
  public enum Hero
  {
    Unknown,
    Ana,
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
    public static string ImageName(this Hero h)
    {
      return h.ToString().ToLower();
    }
  }
}
