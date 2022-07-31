using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace Heinermann.FairPassiveLighting
{
  // TODO assets
  // TODO additional logic to make things brighter
  public static class Pieces
  {
    private static readonly SimplePiece[] NewPieces =
    {
      new SimplePiece()
      {
        Name = "passive_stone_lantern",
        PieceTable = "Hammer",
        Category = "Furniture",
        CraftingStation = "piece_stonecutter",
        Requirements =
        {
          {"Stone", 8}
        }
      }
    };

    public static void Init()
    {
      AssetBundle pieceBundle = AssetUtils.LoadAssetBundleFromResources("heinermann_passive_lighting");
      PieceUtil.AddPieces(pieceBundle, NewPieces);
    }
  }
}
