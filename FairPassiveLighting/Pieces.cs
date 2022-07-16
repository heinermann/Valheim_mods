using Jotunn.Configs;
using Jotunn.Utils;
using UnityEngine;

namespace Heinermann.FairPassiveLighting
{
  // TODO assets
  // TODO additional logic to make things brighter
  public static class Pieces
  {
    public static void Init()
    {
      AssetBundle pieceBundle = AssetUtils.LoadAssetBundleFromResources("pieces");

      PieceUtil.AddPiece(pieceBundle,
                         "passive_stone_lantern",
                         pieceTable: "Hammer",
                         category: "Furniture",
                         craftingStation: "piece_stonecutter",
                         requirements: new[] { new RequirementConfig("Stone", 8, recover: true) });
    }
  }
}
