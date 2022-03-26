using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heinermann.TheRuins.Components
{
  public class StructuralPiece : MonoBehaviour
  {
    private struct MaterialProperties
    {
      public float maxSupport;
      public float minSupport;
      public float horizontalLoss;
      public float verticalLoss;
    }

    private struct Bounds
    {
      public Vector3 m_pos;
      public Quaternion m_rot;
      public Vector3 m_size;
    }

    private static Dictionary<WearNTear.MaterialType, MaterialProperties> materialLookup = new Dictionary<WearNTear.MaterialType, MaterialProperties>()
    {
      { WearNTear.MaterialType.Wood, new MaterialProperties{maxSupport = 100f, minSupport = 10f, verticalLoss = 0.125f, horizontalLoss = 0.2f} },
      { WearNTear.MaterialType.HardWood, new MaterialProperties{maxSupport = 140f, minSupport = 10f, verticalLoss = 0.1f, horizontalLoss = 1f / 6} },
      { WearNTear.MaterialType.Stone, new MaterialProperties{maxSupport = 1000f, minSupport = 010f, verticalLoss = 0.125f, horizontalLoss = 1f} },
      { WearNTear.MaterialType.Iron, new MaterialProperties{maxSupport = 1500f, minSupport = 20f, verticalLoss = 1f / 13, horizontalLoss = 1f / 13} },
    };

    private WearNTear m_wear;
    private Collider[] m_colliders;
    private List<Bounds> m_bounds = new List<Bounds>();
    private float m_support = 1f;
    private MaterialProperties m_matProps = default(MaterialProperties);

    private static Collider[] tempColliders = new Collider[128];
    private static int m_rayMask = 0;

    private void Awake()
    {
      m_wear = GetComponent<WearNTear>();
      if (m_rayMask == 0) m_rayMask = LayerMask.GetMask("piece", "Default", "static_solid", "Default_small", "terrain");

      materialLookup.TryGetValue(m_wear.m_materialType, out m_matProps);
      //Jotunn.Logger.LogInfo($"Structural.Awake - {name} in {transform.parent?.name}");
    }

    void SetupColliders()
    {
      m_colliders = GetComponentsInChildren<Collider>(includeInactive: true);
      foreach(Collider collider in m_colliders)
      {
        Bounds bounds = default(Bounds);

        if (collider is BoxCollider boxCollider)
        {
          bounds.m_rot = boxCollider.transform.rotation;
          bounds.m_pos = boxCollider.transform.position + boxCollider.transform.TransformVector(boxCollider.center);
          bounds.m_size = Vector3.Scale(boxCollider.size, boxCollider.transform.lossyScale);
        }
        else
        {
          bounds.m_rot = Quaternion.identity;
          bounds.m_pos = collider.bounds.center;
          bounds.m_size = collider.bounds.size;
        }
        bounds.m_size.x += 0.3f;
        bounds.m_size.y += 0.3f;
        bounds.m_size.z += 0.3f;
        bounds.m_size *= 0.5f;
        m_bounds.Add(bounds);
      }
    }

    Vector3 GetRotationPosition()
    {
      return transform.position + transform.rotation * m_wear.m_comOffset;
    }

    private static Vector3 FindSupportPoint(Vector3 rotPos, StructuralPiece otherPiece, Collider otherCollider)
    {
      MeshCollider meshCollider = otherCollider as MeshCollider;
      if (meshCollider != null && !meshCollider.convex)
      {
        if (meshCollider.Raycast(new Ray(rotPos, Vector3.down), out var hitInfo, 10f))
        {
          return hitInfo.point;
        }
        return (rotPos + otherPiece.GetRotationPosition()) * 0.5f;
      }
      return otherCollider.ClosestPoint(rotPos);
    }

    // This is a modified partial copy of the original support algorithm.
    void UpdateSupport()
    {
      if (m_colliders == null) SetupColliders();

      Vector3 rotPos = GetRotationPosition();

      float maxSupport = 0;

      foreach (Bounds bound in m_bounds)
      {
        int numColliders = Physics.OverlapBoxNonAlloc(bound.m_pos, bound.m_size, tempColliders, bound.m_rot, m_rayMask);
        for (int i = 0; i < numColliders; i++)
        {
          Collider collider = tempColliders[i];
          if (m_colliders.Contains(collider) || collider.attachedRigidbody != null || collider.isTrigger)
            continue;

          StructuralPiece touchingWear = collider.GetComponentInParent<StructuralPiece>();
          if (touchingWear == null)
          {
            m_support = m_matProps.maxSupport;
            return;
          }

          if (!touchingWear.m_wear.m_supports)
            continue;

          float distanceToTouching = Vector3.Distance(rotPos, touchingWear.transform.position) + 0.1f;
          float support = touchingWear.m_support;

          maxSupport = Mathf.Max(maxSupport, support - m_matProps.horizontalLoss * distanceToTouching * support);
          Vector3 vector = FindSupportPoint(rotPos, touchingWear, collider);
          if (vector.y < rotPos.y + 0.05f)
          {
            Vector3 normalized = (vector - rotPos).normalized;
            if (normalized.y < 0f)
            {
              float angle = Mathf.Acos(1f - Mathf.Abs(normalized.y)) / ((float)Math.PI / 2f);
              float loss = Mathf.Lerp(m_matProps.horizontalLoss, m_matProps.verticalLoss, angle);
              float angledSupport = support - loss * distanceToTouching * support;
              maxSupport = Mathf.Max(maxSupport, angledSupport);
            }
          }
        }
      }
      m_support = Mathf.Min(maxSupport, m_matProps.maxSupport);
    }

    private static List<WearNTear.MaterialType> m_materialIterationOrder = GetMaterialIterationOrder();
    private static List<WearNTear.MaterialType> GetMaterialIterationOrder()
    {
      // We need to iterate materials twice for edge cases such as placing stone on top of core wood or iron/stone on each other.
      List<Tuple<WearNTear.MaterialType, float>> materialsToIterate = new List<Tuple<WearNTear.MaterialType, float>>();
      foreach (var material in materialLookup)
      {
        materialsToIterate.Add(Tuple.Create(material.Key, material.Value.minSupport));
        materialsToIterate.Add(Tuple.Create(material.Key, material.Value.maxSupport));
      }

      // Sort materials in descending order according to their support values
      materialsToIterate.Sort((mat1, mat2) => mat2.Item2.CompareTo(mat1.Item2));
      return materialsToIterate.Select((mat) => mat.Item1).ToList();
    }

    // ALSO NOT WORKING
    private static RaycastHit[] raycastResult = new RaycastHit[1];
    private static void ApplyGravity(GameObject location)
    {
      foreach (PickableItem pickable in location.GetComponentsInChildren<PickableItem>())
      {
        Physics.RaycastNonAlloc(pickable.transform.position, Vector3.down, raycastResult);
        if (raycastResult.Length > 0)
        {
          Jotunn.Logger.LogWarning($"Dropped {pickable.name}");
          pickable.transform.position = raycastResult[0].point;
        }
      }
    }

    private static Dictionary<StructuralPiece, float> InitSettlingObjects(GameObject location, WearNTear.MaterialType material)
    {
      var result = new Dictionary<StructuralPiece, float>();
      var integrityObjects = location.GetComponentsInChildren<StructuralPiece>(includeInactive: true);

      foreach (StructuralPiece piece in integrityObjects)
      {
        if (piece.m_wear == null)
        {
          //Jotunn.Logger.LogError("m_wear is null");
          piece.m_wear = piece.GetComponent<WearNTear>();
        }
        if (piece.m_wear.m_materialType != material) continue;

        piece.UpdateSupport();

        float support = piece.m_support;
        if (support < piece.m_matProps.maxSupport)
        {
          result.Add(piece, support);
        }
      }
      return result;
    }

    private static void SettleIntegrityForMaterial(GameObject location, WearNTear.MaterialType material)
    {
      Dictionary<StructuralPiece, float> remainingObjects = InitSettlingObjects(location, material);

      int numTotalObjectsDeleted = 0;
      var objectsToRemoveInIteration = new HashSet<StructuralPiece>();
      var objectsToDelete = new HashSet<StructuralPiece>();
      while (remainingObjects.Count > 0)
      {
        objectsToRemoveInIteration.Clear();
        foreach (var wearAndOldSupport in remainingObjects)
        {
          wearAndOldSupport.Key.UpdateSupport();
        }
        foreach (var wearAndOldSupport in remainingObjects)
        {
          StructuralPiece piece = wearAndOldSupport.Key;
          float oldSupport = wearAndOldSupport.Value;

          piece.UpdateSupport();

          float newSupport = piece.m_support;
          if (newSupport == oldSupport)
          {
            objectsToRemoveInIteration.Add(piece);
          }
          else
          {
            remainingObjects[piece] = newSupport;
          }

          if (piece.m_support < piece.m_matProps.minSupport)
          {
            objectsToDelete.Add(piece);
          }
        }

        foreach (var removeMe in objectsToRemoveInIteration)
        {
          remainingObjects.Remove(removeMe);
        }

        foreach (var deleteMe in objectsToDelete)
        {
          remainingObjects.Remove(deleteMe);
          ZNetScene.instance.Destroy(deleteMe.gameObject);
        }
        numTotalObjectsDeleted += objectsToDelete.Count;
      }

      if (numTotalObjectsDeleted > 0)
        Jotunn.Logger.LogInfo($"Settling {location.name} - Deleted {numTotalObjectsDeleted} objects in {material} material pass");
    }

    public static void SettleIntegrity(GameObject location)
    {
      // Settle the location by tier of structural integrity
      foreach (var material in m_materialIterationOrder)
      {
        SettleIntegrityForMaterial(location, material);
        ApplyGravity(location);
      }
    }
  }
}
