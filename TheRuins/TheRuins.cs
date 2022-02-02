using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;

namespace Heinermann.TheRuins
{
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  [BepInDependency(Jotunn.Main.ModGuid)]
  [NetworkCompatibility(CompatibilityLevel.NotEnforced, VersionStrictness.None)]
  internal class TheRuins : BaseUnityPlugin
  {
    public const string PluginGUID = "com.heinermann.theruins";
    public const string PluginName = "TheRuins";
    public const string PluginVersion = "1.0.0";

    // Use this class to add your own localization to the game
    // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
    public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

    private void Awake()
    {
      Ruins.LoadAll();
      
      On.ZNetScene.Awake += ZNetSceneAwake;
      // TODO:
      // - Hook Object.Instantiate (from ZoneSystem.SpawnLocation).
      // - Compare with this mod's created location prefabs
      // - Call WearNTear.UpdateSupport a few times for all pieces (ignore if piece doesn't degrade, or has already reached max degredation, until no more pieces decay)
      // - If !WearNTear.HaveSupport() then use ZNetScene.instance.Destroy
    }

    private void ZNetSceneAwake(On.ZNetScene.orig_Awake orig, ZNetScene self)
    {
      orig(self);
      Ruins.RegisterRuins();
    }
  }
}