using System.Collections.Generic;
using UnityEngine;

namespace Heinermann.ImmersiveTransitions
{
  public class PortalLocationPrep : MonoBehaviour
  {
    private static Mesh PlaneMesh = null;

    private class Tweak
    {
      public Vector3 Position { get; set; } = Vector3.zero;
      public Vector3 Rotation { get; set; } = Vector3.zero;
      public Vector3 MeshPosition { get; set; } = Vector3.zero;
      public Vector3 MeshRotation { get; set; } = Vector3.zero;

    }

    private static readonly Dictionary<string, Tweak> Tweaks = new Dictionary<string, Tweak>()
    {
      { "Crypt3(Clone)_ExteriorGateway", new Tweak(){ Position = new Vector3(0, 1f, 0)/*, Rotation = new Vector3(0, 180, 0)*/ } },
      { "Crypt3(Clone)_Gateway", new Tweak(){ Rotation = new Vector3(-180, 0, 180) } },
      { "TrollCave02(Clone)_ExteriorGateway", new Tweak(){
        Position = new Vector3(0, 2f, 0),
        Rotation = new Vector3(0, 180, 0),
        MeshPosition = new Vector3(0, 0, -1)
      } },
      { "TrollCave02(Clone)_Gateway", new Tweak(){ Position = new Vector3(0, 0, -0.5f) } },
      { "MountainCave02(Clone)_ExteriorGateway", new Tweak(){ Position = new Vector3(0, 0.5f, 0), Rotation = new Vector3(0, 180, 0) } },
      //{ "MountainCave02(Clone)_Gateway", new Tweak(){ MeshRotation = new Vector3(0, 180, 0) } },
    };

    private static void InitPlaneMesh()
    {
      if (PlaneMesh != null) return;
      GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
      PlaneMesh = Instantiate(obj.GetComponent<MeshFilter>().mesh);
      Destroy(obj);
    }

    void Awake()
    {
      InitPlaneMesh();

      Teleport[] teleports = gameObject.GetComponentsInChildren<Teleport>();
      if (teleports.Length != 2)
      {
        if (teleports.Length > 0) Jotunn.Logger.LogWarning($"Invalid number of teleports in location {gameObject.name}: {teleports.Length}");
        return;
      }

      GameObject portalA = teleports[0].gameObject;
      GameObject portalB = teleports[1].gameObject;

      Destroy(teleports[0]);
      Destroy(teleports[1]);

      var meshA = portalA.GetComponentInChildren<MeshRenderer>();
      if (meshA == null)
      {
        Jotunn.Logger.LogWarning($"MeshA is null");

        // TODO create a MeshRenderer that fits the entrance (i.e. Crypt4)
      }
      meshA.sharedMaterial.shader = ImmersiveTransitions.PortalShader;
      if (meshA.TryGetComponent(out BoxCollider boxColliderA))
      {
        Destroy(boxColliderA);
      }

      var meshB = portalB.GetComponentInChildren<MeshRenderer>();
      if (meshB == null)
      {
        Jotunn.Logger.LogWarning($"MeshB is null");
        // TODO create a MeshRenderer that fits the entrance (i.e. Crypt4)
      }
      meshB.sharedMaterial.shader = ImmersiveTransitions.PortalShader;
      if (meshB.TryGetComponent(out BoxCollider boxColliderB))
      {
        Destroy(boxColliderB);
      }

      // Re-scale stuff to match, otherwise we get weird effects
      //var meshScale = Vector3.Min(meshA.transform.localScale, meshB.transform.localScale);
      //meshA.transform.localScale = meshScale;
      //meshB.transform.localScale = meshScale;

      var portalScale = Vector3.Min(portalA.transform.localScale, portalB.transform.localScale);
      portalA.transform.localScale = portalScale;
      portalB.transform.localScale = portalScale;

      // Set up Cameras
      var cameraAObj = new GameObject("PortalCameraA");
      var cameraBObj = new GameObject("PortalCameraB");

      cameraAObj.transform.parent = portalA.transform;
      cameraBObj.transform.parent = portalB.transform;

      var cameraA = cameraAObj.AddComponent<Camera>();
      var cameraB = cameraBObj.AddComponent<Camera>();

      cameraA.depth = cameraB.depth = -2;
      cameraA.renderingPath = cameraB.renderingPath = RenderingPath.Forward;

      // Create Portal script
      var portalAComponent = portalA.AddComponent<Portal>();
      var portalBComponent = portalB.AddComponent<Portal>();

      portalAComponent.recursionLimit = portalBComponent.recursionLimit = 2;
      portalAComponent.linkedPortal = portalBComponent;
      portalBComponent.linkedPortal = portalAComponent;

      portalAComponent.screen = meshA;
      portalBComponent.screen = meshB;

      // Apply tweaks
      string portalNameA = $"{gameObject.name}_{portalA.name}";
      string portalNameB = $"{gameObject.name}_{portalB.name}";

      Jotunn.Logger.LogWarning(portalNameA);
      Jotunn.Logger.LogWarning(portalNameB);
      if (Tweaks.TryGetValue(portalNameA, out var tweakA))
      {
        portalA.transform.position += tweakA.Position;
        portalA.transform.Rotate(tweakA.Rotation.x, tweakA.Rotation.y, tweakA.Rotation.z);
        meshA.transform.position += tweakA.MeshPosition;
        meshA.transform.Rotate(tweakA.MeshRotation.x, tweakA.MeshRotation.y, tweakA.MeshRotation.z);
      }

      if (Tweaks.TryGetValue(portalNameB, out var tweakB))
      {
        portalB.transform.position += tweakB.Position;
        portalB.transform.Rotate(tweakB.Rotation.x, tweakB.Rotation.y, tweakB.Rotation.z);
        meshB.transform.position += tweakB.MeshPosition;
        meshB.transform.Rotate(tweakB.MeshRotation.x, tweakB.MeshRotation.y, tweakB.MeshRotation.z);
      }

      var colliderA = portalA.GetComponent<BoxCollider>();
      var colliderB = portalB.GetComponent<BoxCollider>();

      colliderA.center = Vector3.zero;
      colliderB.center = Vector3.zero;
      colliderA.size = new Vector3(meshA.transform.localScale.x, meshA.transform.localScale.y, 1.55f);
      colliderB.size = new Vector3(meshB.transform.localScale.x, meshB.transform.localScale.y, 1.55f);
      
      meshA.transform.localScale = new Vector3(meshA.transform.localScale.x, meshA.transform.localScale.y, 0.02f);
      meshB.transform.localScale = new Vector3(meshB.transform.localScale.x, meshB.transform.localScale.y, 0.02f);
    }
  }
}
