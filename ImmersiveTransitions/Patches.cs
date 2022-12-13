using HarmonyLib;
using UnityEngine;

namespace Heinermann.ImmersiveTransitions
{
  [HarmonyPatch]
  static class Patches
  {
    [HarmonyPatch(typeof(Location), "Awake"), HarmonyPostfix]
    static void LocationAwakePostfix(Location __instance)
    {
      __instance.gameObject.AddComponent<PortalLocationPrep>();
    }

    [HarmonyPatch(typeof(Player), "SetLocalPlayer"), HarmonyPostfix]
    static void SetLocalPlayerPostfix(Player __instance)
    {
      //__instance.gameObject.tag = "Player";
      var traveller = __instance.gameObject.AddComponent<PortalTraveller>();
      traveller.graphicsObject = __instance.transform.Find("Visual/body").gameObject;

      Camera.main.gameObject.AddComponent<MainCamera>();
    }

    [HarmonyPatch(typeof(Hud), "UpdateBlackScreen"), HarmonyPrefix]
    static bool UpdateBlackScreenPrefix(Player player, float dt)
    {
      if (player == null) return true;
      return !player.IsTeleporting();
    }

    [HarmonyPatch(typeof(Player), "UpdateTeleport"), HarmonyPrefix]
    static bool UpdateTeleportPrefix(Player __instance, float dt, ref bool ___m_teleporting, ref float ___m_teleportTimer, Vector3 ___m_teleportTargetPos, Quaternion ___m_teleportTargetRot, bool ___m_distantTeleport)
    {
      if (!___m_teleporting || ___m_distantTeleport) return true;

      __instance.transform.position = ___m_teleportTargetPos;
      __instance.transform.rotation = ___m_teleportTargetRot;

      // TODO call/rewrite SetLookDir for camera (based on difference in rotation for teleport)
      //__instance.SetLookDir(___m_teleportTargetRot * Vector3.forward);

      ___m_teleporting = false;
      ___m_teleportTimer = 0f;
      __instance.ResetCloth();
      return false;
    }
  }
}
