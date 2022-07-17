using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heinermann
{
  public class SimplePiece
  {
    public string Name { get; set; }
    public string PieceTable { get; set; }
    public string Category { get; set; }
    public string CraftingStation { get; set; }
    public Dictionary<string, int> Requirements { get; set; }

    public PieceConfig PieceConfig()
    {
      return new PieceConfig()
      {
        Name = $"$heinermann_{Name}_display_name",
        PieceTable = PieceTable,
        Category = Category,
        CraftingStation = CraftingStation,
        Description = $"$heinermann_{Name}_display_description",
        Requirements = Requirements.Select(item => new RequirementConfig(item.Key, item.Value, recover: true)).ToArray()
      };
    }
  }

  public static class PieceUtil
  {
    public static void AddPieces(AssetBundle bundle, IEnumerable<SimplePiece> pieces)
    {
      foreach (SimplePiece piece in pieces)
      {
        PieceManager.Instance.AddPiece(new CustomPiece(bundle, $"heinermann_{piece.Name}", false, piece.PieceConfig()));
      }
    }
  }
}
