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
        float x = collider.bounds.extents.x;
        float z = collider.bounds.extents.z;
        return Mathf.Max(1f, Mathf.Sqrt(x * x + z * z));
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
