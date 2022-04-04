using BepInEx;
using BepInEx.Configuration;
using System.Diagnostics;
using System.Reflection;

namespace Heinermann.ValProfiler
{
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class ValProfiler : BaseUnityPlugin
  {
    public const string PluginGUID = "com.heinermann.valprofiler";
    public const string PluginName = "ValProfiler";
    public const string PluginVersion = "2.0.0";

    const string Options = "Options";
    public static ConfigEntry<int> MillisecondsPerTrace;
    public static ConfigEntry<int> SecondsPerDump;

    void Awake()
    {
      MillisecondsPerTrace = Config.Bind(Options, "MillisecondsPerTrace", 20);
      SecondsPerDump = Config.Bind(Options, "SecondsPerDump", 30);

      Process proc = new Process();
      proc.StartInfo.FileName = Assembly.GetExecutingAssembly().Location;
      proc.StartInfo.Arguments = $"{Process.GetCurrentProcess().Id} {MillisecondsPerTrace.Value} {SecondsPerDump.Value}";
      proc.StartInfo.UseShellExecute = false;
      proc.Start();
    }
  }
}
