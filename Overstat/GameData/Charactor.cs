using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Overstat.GameData
{
  public enum Charactor
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

  public static class CharactorEx
  {
    public static string ImageName(this Charactor c)
    {
      if (c == Charactor.Soldier76)
      {
        return "soldier-76";
      }
      return c.ToString().ToLower();
    }
  }
}
