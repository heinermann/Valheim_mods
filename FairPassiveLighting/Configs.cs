
using BepInEx.Configuration;

namespace Heinermann.FairPassiveLighting
{
  public static class Configs
  {
    const string Category = "Configuration";
    public static ConfigEntry<float> RangeMultiplier;

    public static void Init(ConfigFile config)
    {
      config.Bind(Category, "Light Range Multiplier", 2.5f, "The amount to multiply the light range of items put into passive lanterns.");
    }
  }
}
