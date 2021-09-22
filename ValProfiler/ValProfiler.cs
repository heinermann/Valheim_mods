using BepInEx;
using HarmonyLib;

namespace Heinermann.ValProfiler
{
  [BepInPlugin("heinermann.valprofiler", "Valheim Profiler", "0.1")]
  [BepInProcess("valheim.exe")]
  public class ValProfiler : BaseUnityPlugin
  {
    public static Harmony harmony = new Harmony("mod.heinermann.valprofiler");

    static ProfilerBase profiler = new DetourProfiler();

    void Awake()
    {
      harmony.PatchAll();
      profiler.DoPrePatch(harmony);
    }
  }
}
