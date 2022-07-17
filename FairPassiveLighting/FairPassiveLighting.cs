// FairPassiveLighting
// a Valheim mod skeleton using Jötunn
// 
// File:    FairPassiveLighting.cs
// Project: FairPassiveLighting

using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heinermann.FairPassiveLighting
{
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  [BepInDependency(Jotunn.Main.ModGuid)]
  [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
  internal class FairPassiveLighting : BaseUnityPlugin
  {
    public const string PluginGUID = "com.heinermann.fairpassivelighting";
    public const string PluginName = "FairPassiveLighting";
    public const string PluginVersion = "0.0.1";

    // Use this class to add your own localization to the game
    // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
    public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

    private void Awake()
    {
      Configs.Init(Config);
      //Pieces.Init();

      PrefabManager.OnVanillaPrefabsAvailable += PrefabsAvailable;
    }

    private void PrefabsAvailable()
    {
      // TODO: Get our new piece prefab and modify the "ItemStand" with all the valid supported items

      //ItemStand itemStand;

      // inclusions
      //itemStand.m_supportedItems.Add(item);
      List<ItemDrop> supportedItems = new List<ItemDrop>();
      List<string> supportedDebug = new List<string>();
      foreach (ItemDrop item in Resources.FindObjectsOfTypeAll<ItemDrop>())
      {
        Light[] lights = item.GetComponentsInChildren<Light>();
        if (lights.Length > 0 && !item.m_itemData.IsWeapon())
        {
          Light light = lights[0];
          supportedDebug.Add($"  - name: {item.name}, range: {light.range}, intensity: {light.intensity}, colour: {light.color}");
          
          supportedItems.Add(item);
        }
      }

      Jotunn.Logger.LogInfo("## Supported items:\n" + String.Join("\n", supportedDebug));
    }
  }
}