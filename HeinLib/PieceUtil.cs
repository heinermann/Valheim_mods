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
    public Dictionary<string, int> Requirements { get; set; } = new Dictionary<string, int>();

    public PieceConfig PieceConfig()
    {
      return new PieceConfig()
      {
        Name = $"$heinermann_{Name}",
        PieceTable = PieceTable,
        Category = Category,
        CraftingStation = CraftingStation,
        Description = $"$heinermann_{Name}_desc",
        Requirements = Requirements.Select(item => new RequirementConfig(item.Key, item.Value, recover: true)).ToArray()
      };
    }
  }

  public static class PieceUtil
  {
    private static Sprite CreateIcon(GameObject prefab)
    {
      return RenderManager.Instance.Render(prefab, RenderManager.IsometricRotation);
    }

    public static void AddPieces(AssetBundle bundle, IEnumerable<SimplePiece> pieces)
    {
      foreach (SimplePiece piece in pieces)
      {
        GameObject prefab = bundle.LoadAsset<GameObject>($"heinermann_{piece.Name}");
        PieceConfig config = piece.PieceConfig();

        Piece pieceComponent = prefab.GetComponent<Piece>();
        if (pieceComponent != null)
        {
          if (pieceComponent.m_icon == null)
          {
            pieceComponent.m_icon = CreateIcon(prefab);
          }
          config.Icon = pieceComponent.m_icon;
        }
        else
        {
          config.Icon = CreateIcon(prefab);
        }

        PieceManager.Instance.AddPiece(new CustomPiece(prefab, fixReference: true, piece.PieceConfig()));
      }
    }
  }
}
