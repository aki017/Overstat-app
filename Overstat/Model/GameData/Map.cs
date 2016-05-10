using System;

namespace Overstat.Model.GameData
{
  enum Map
  {
    Hanamura,
    TemplOfAnubis,
    VolskayaIndustries,

    Dorado,
    Route66,
    WatchpointGibraltar,

    Hollywood,
    KingsRow,
    Numbani,

    Ilios,
    LijiangTower,
    Nepal
  }

  static class MapEx
  {
    static MapType GetMapType(this Map map)
    {
      switch (map)
      {
        case Map.Hanamura:
        case Map.TemplOfAnubis:
        case Map.VolskayaIndustries:
          return MapType.Assault;


        case Map.Dorado:
        case Map.Route66:
        case Map.WatchpointGibraltar:
          return MapType.Escort;

        case Map.Hollywood:
        case Map.KingsRow:
        case Map.Numbani:
          return MapType.Hybrid;

        case Map.Ilios:
        case Map.LijiangTower:
        case Map.Nepal:
          return MapType.Control;
      }
      throw new NotSupportedException();
    }
  }
}
