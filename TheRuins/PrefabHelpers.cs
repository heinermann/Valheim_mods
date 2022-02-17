using UnityEngine;

namespace Heinermann.TheRuins
{
  internal static class PrefabHelpers
  {
    public static float GetPieceRadius(GameObject prefab)
    {
      var collider = prefab.GetComponentInChildren<Collider>();
      if (collider)
      {
        return Mathf.Max(collider.bounds.size.x, collider.bounds.size.z, 1f);
      }
      return 1f;
    }

    public static float GetPieceHeight(GameObject prefab)
    {
      var collider = prefab.GetComponentInChildren<Collider>();
      if (collider)
      {
        return collider.bounds.size.y;
      }
      return 0.25f;
    }
  }
}
