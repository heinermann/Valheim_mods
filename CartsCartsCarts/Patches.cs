using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace Heinermann.CartsCartsCarts
{
  static class Patches
  {
    static Vagon GetAttachedCart()
    {
      List<Vagon> instances = AccessTools.StaticFieldRefAccess<Vagon, List<Vagon>>("m_instances");
      return instances.Find(cart => cart.IsAttached(Player.m_localPlayer));
    }

    // Prefix that gives a new hover text only when pulling a cart
    [HarmonyPatch(typeof(Tameable), "GetHoverText"), HarmonyPrefix]
    static bool Tameable_GetHoverText_Prefix(ref string __result, Tameable __instance)
    {
      Character chara = __instance.GetComponent<Character>();

      bool IsPullingCart = GetAttachedCart() != null;
      if (chara.IsTamed() && IsPullingCart)
      {
        __result = Localization.instance.Localize(chara.m_name);
        __result += Localization.instance.Localize(" ( $hud_tame, " + __instance.GetStatusString() + " )");
        __result += Localization.instance.Localize("\n[<color=yellow><b>$KEY_Use</b></color>] $heinermann_cart_attach");
        return false;
      }
      return true;
    }

    // Interact hook to attach a cart
    [HarmonyPatch(typeof(Tameable), "Interact"), HarmonyPrefix]
    static bool Tameable_Interact_Prefix(ref bool __result, Tameable __instance, Humanoid user, bool hold, bool alt)
    {
      Character chara = __instance.GetComponent<Character>();
      Vagon cart = GetAttachedCart();

      if (chara.IsTamed() && cart != null)
      {
        var AttachToFn = AccessTools.Method("Vagon:AttachTo");
        AttachToFn.Invoke(cart, new object[]{ chara });
        
        __result = true;
        return false;
      }
      return true;
    }

    // Rewrite of AttachTo so as not to remove all attachments and keep the correct distance based on animal size.
    // Note, hooking CanAttach is probably not necessary.
    [HarmonyPatch(typeof(Vagon), "AttachTo"), HarmonyPrefix]
    static bool Vagon_AttachTo_Prefix(Vagon __instance, GameObject go)
    {
      var DetachFn = AccessTools.Method("Vagon:Detach");
      DetachFn.Invoke(__instance, new object[] { });

      // TODO rewrite AttachTo

      return false;
    }

    // TODO possibly mass tweaking
  }
}
