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
using UnityEngine;

namespace Heinermann.FairPassiveLighting
{
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  [BepInDependency(Jotunn.Main.ModGuid)]
  [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
  internal class FairPassiveLighting : BaseUnityPlugin
  {
    public const string PluginGUID = "com.heinermann.passivelanterns";
    public const string PluginName = "PassiveLanterns";
    public const string PluginVersion = "1.0.0";

    // Use this class to add your own localization to the game
    // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
    public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

    private void Awake()
    {
      Configs.Init(Config);

      PrefabManager.OnVanillaPrefabsAvailable += PrefabsAvailable;
    }

    private List<ItemDrop> GetSupportedItems()
    {
      List<ItemDrop> supportedItems = new List<ItemDrop>();
      List<string> supportedDebug = new List<string>();
      foreach (ItemDrop item in Resources.FindObjectsOfTypeAll<ItemDrop>())
      {
        Light[] lights = item.GetComponentsInChildren<Light>();
        if (lights.Length > 0 && !item.m_itemData.IsWeapon())
        {
          Light light = lights[0];

          if (item.transform.Find("attach") != null)
          {
            supportedDebug.Add($"  - name: {item.name}, range: {light.range}, intensity: {light.intensity}, colour: {light.color}");
          }

          supportedItems.Add(item);
        }
      }

      Jotunn.Logger.LogInfo("## Supported vanilla items:\n" + String.Join("\n", supportedDebug));
      return supportedItems;
    }

    private void PrefabsAvailable()
    {
      Pieces.Init();

      // Init stone lantern data
      CustomPiece stoneLantern = PieceManager.Instance.GetPiece("heinermann_passive_stone_lantern");

      ItemStand itemStand = stoneLantern.PiecePrefab.GetComponent<ItemStand>();
      itemStand.m_supportedItems = GetSupportedItems();

      stoneLantern.PiecePrefab.AddComponent<LightTracker>();
    }
  }
}