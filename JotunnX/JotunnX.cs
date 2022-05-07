using BepInEx;
using Jotunn.Utils;

namespace Jotunn
{
  [BepInPlugin(ModGuid, ModName, Version)]
  [NetworkCompatibility(CompatibilityLevel.VersionCheckOnly, VersionStrictness.Minor)]
  public class JotunnX : BaseUnityPlugin
  {
    public const string Version = "2.6.4";
    public const string ModName = "JotunnX";
    public const string ModGuid = "com.heinermann.jotunnx";
  }
}
