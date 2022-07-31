// PassiveLanterns
// a Valheim mod skeleton using Jötunn
// 
// File:    PassiveLanterns.cs
// Project: PassiveLanterns

using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Heinermann.PassiveLanterns
{
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  [BepInDependency(Jotunn.Main.ModGuid)]
  [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
  internal class PassiveLanterns : BaseUnityPlugin
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

    private static List<ItemDrop> supportedItems;

    private void FinalizeLantern(string pieceName)
    {
      CustomPiece piece = PieceManager.Instance.GetPiece(pieceName);

      ItemStand itemStand = piece.PiecePrefab.GetComponent<ItemStand>();
      itemStand.m_supportedItems = supportedItems;

      piece.PiecePrefab.AddComponent<LightTracker>();
    }

    private void PrefabsAvailable()
    {
      Pieces.Init();

      supportedItems = GetSupportedItems();

      FinalizeLantern("heinermann_passive_stone_lantern");
      FinalizeLantern("heinermann_passive_hanging_brazier");
      FinalizeLantern("heinermann_passive_standing_brazier");

      PrefabManager.OnVanillaPrefabsAvailable -= PrefabsAvailable;
    }
  }
}