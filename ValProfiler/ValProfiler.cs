using BepInEx;
using HarmonyLib;

namespace Heinermann.ValProfiler
{
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class ValProfiler : BaseUnityPlugin
  {
    public const string PluginGUID = "com.heinermann.valprofiler";
    public const string PluginName = "ValProfiler";
    public const string PluginVersion = "1.0.0";

    public static Harmony harmony = new Harmony(PluginGUID);

    static ProfilerBase profiler = new DetourProfiler();

    void Awake()
    {
      harmony.PatchAll();
      profiler.DoPrePatch(harmony);
    }
  }
}
