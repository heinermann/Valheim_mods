using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Heinermann.ValProfiler
{
  using Debug = UnityEngine.Debug;
  using ProfileDict = Dictionary<string, ulong>;

  public class DetourProfiler : ProfilerBase
  {
    static ThreadLocal<ProfileDict> profileData = new ThreadLocal<ProfileDict>(() => new ProfileDict());
    static ThreadLocal<Stopwatch> traceTimer = new ThreadLocal<Stopwatch>(() => Stopwatch.StartNew());
    static ThreadLocal<Stopwatch> dumpTimer = new ThreadLocal<Stopwatch>(() => Stopwatch.StartNew());

    static readonly long TICKS_PER_MS = Stopwatch.Frequency / 1000;
    static readonly long TICKS_PER_TRACE = 20 /* ms */ * TICKS_PER_MS;
    static readonly long MILLISECONDS_PER_DUMP = 20000;


    public override void DoPrePatch(Harmony harmony)
    {
      Stopwatch timer = Stopwatch.StartNew();
      traceTimer.Value.Reset();

      HarmonyMethod postfix = new HarmonyMethod(typeof(DetourProfiler), nameof(DetourPostfix));

      List<MethodBase> targetMethods = ProfileUtils.GetTargetMethods().ToList();

      foreach (MethodBase method in targetMethods)
      {
        try
        {
          harmony.Patch(method, null, postfix);
        }
        catch (Exception e)
        {
          Debug.LogError(e.ToString());
        }
      }

      Debug.Log($"[ValProfiler] PrePatch took {timer.ElapsedMilliseconds}ms. Patched {targetMethods.Count} methods.");
      traceTimer.Value.Start();
    }

    static void DetourPostfix()
    {
      if (traceTimer.Value.ElapsedTicks > TICKS_PER_TRACE)
      {
        traceTimer.Value.Restart();

        string traceStr = GetFlatStackTrace();
        
        profileData.Value.TryGetValue(traceStr, out ulong count);
        profileData.Value[traceStr] = count + 1;

        if (dumpTimer.Value.ElapsedMilliseconds > MILLISECONDS_PER_DUMP)
        {
          dumpTimer.Value.Restart();
          DumpData();
        }
      }
    }

    static string GetFlatStackTrace()
    {
      ArrayList result = new ArrayList();
      for (int frameNo = 2; ; frameNo++)
      {
        StackFrame frame = new StackFrame(frameNo, false);
        MethodBase method = frame.GetMethod();

        if (method == null) break;

        string methodDescription = method.FullDescription();
        result.Add(methodDescription);
      }
      
      return string.Join(";", result.Cast<string>().Reverse());
    }

    static string CreateFlatFile()
    {
      StringBuilder result = new StringBuilder();
      foreach (var pair in profileData.Value)
      {
        result.Append(pair.Key);
        result.Append(' ');
        result.Append(pair.Value);
        result.Append('\n');
      }
      return result.ToString();
    }

    static void DumpData()
    {
      Stopwatch timer = Stopwatch.StartNew();
      
      string dumpPath = getDumpPath();
      Debug.Log($"[ValProfiler] Dumping data to {dumpPath}");

      try   // This can fail with a Sharing violation if another app opens it
      {
        File.WriteAllText(dumpPath, CreateFlatFile());
      } catch
      {
        Debug.LogError($"[ValProfiler] Failed to write dump: {dumpPath}");
      }

      Debug.Log($"[ValProfiler] Dump took {timer.ElapsedMilliseconds}ms");
    }
  }
}
