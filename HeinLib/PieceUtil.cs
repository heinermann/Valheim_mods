using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Heinermann
{
  public static class PieceUtil
  {
    public static void AddPiece(AssetBundle bundle, string name, string pieceTable, string category, string craftingStation, RequirementConfig[] requirements)
    {
      PieceConfig config = new PieceConfig()
      {
        Name = $"$heinermann_{name}_display_name",
        PieceTable = pieceTable,
        Category = category,
        CraftingStation = craftingStation,
        Description = $"$heinermann_{name}_display_description",
        Requirements = requirements
      };

      PieceManager.Instance.AddPiece(new CustomPiece(bundle, $"heinermann_{name}", false, config));
    }
  }
}
