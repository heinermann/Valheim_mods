using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace ValProfiler
{
  using ProfileDict = Dictionary<string, ulong>;

  static class ProfilerProcess
  {
    static readonly string dumpDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/dumps";
    static readonly string dumpTimePrefix = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");

    public static string getDumpPath(int threadId)
    {
      string threadName = $"thread{threadId}";
      return $"{dumpDirectory}/{dumpTimePrefix}_{threadName}_profile.folded";
    }

    static void Main(string[] args)
    {
      if (args == null)
      {
        Console.WriteLine("Args are NULL");
        return;
      }
      Console.WriteLine("ARGS: " + string.Join(", ", args));

      if (args.Length < 3)
      {
        Console.WriteLine("Invalid args");
        return;
      }

      int pid = int.Parse(args[0]);
      int msPerTrace = int.Parse(args[1]);
      int msPerDump = int.Parse(args[2]) * 1000;
      
      Directory.CreateDirectory(dumpDirectory);

      using (var dataTarget = DataTarget.AttachToProcess(pid, false))
      {
        ClrRuntime runtime = dataTarget.ClrVersions[0].CreateRuntime();
        ProfileRuntime(runtime, msPerTrace, msPerDump);
      }
    }

    static void ProfileRuntime(ClrRuntime runtime, int msPerTrace, int msPerDump)
    {
      var profileData = new Dictionary<int, ProfileDict>();
      Stopwatch dumpTimer = Stopwatch.StartNew();
      Stopwatch elapsed = Stopwatch.StartNew();

      while (runtime.Threads.Length > 0)
      {
        Thread.Sleep((int)Math.Max(0, msPerTrace - elapsed.ElapsedMilliseconds));
        elapsed.Restart();

        foreach (var thread in runtime.Threads)
        {
          ProfileThread(thread, profileData);
        }

        if (dumpTimer.ElapsedMilliseconds > msPerDump)
        {
          dumpTimer.Restart();
          WriteDump(profileData);
        }
      }
    }

    static void ProfileThread(ClrThread thread, Dictionary<int, ProfileDict> profileData)
    {
      ProfileDict flatTraces;
      if (!profileData.TryGetValue(thread.ManagedThreadId, out flatTraces))
      {
        flatTraces = new ProfileDict();
        profileData.Add(thread.ManagedThreadId, flatTraces);
      }

      string trace = GetFlatStackTrace(thread.EnumerateStackTrace());

      flatTraces.TryGetValue(trace, out ulong count);
      flatTraces[trace] = count + 1;
    }

    static string CreateFlatFile(ProfileDict profile)
    {
      StringBuilder result = new StringBuilder();
      foreach(var pair in profile)
      {
        result.Append(pair.Key);
        result.Append(' ');
        result.Append(pair.Value);
        result.Append('\n');
      }
      return result.ToString();
    }

    static void WriteDump(Dictionary<int, ProfileDict> profileData)
    {
      foreach(var profilePair in profileData)
      {
        int threadId = profilePair.Key;
        var traces = profilePair.Value;

        string dumpFilePath = getDumpPath(threadId);
        try
        {
          File.WriteAllText(dumpFilePath, CreateFlatFile(traces));
        } catch {}
      }
    }

    static string GetFlatStackTrace(IEnumerable<ClrStackFrame> stack)
    {
      ArrayList result = new ArrayList();
      foreach (var frame in stack)
      {
        result.Add(frame.Method?.Signature ?? $"{frame.InstructionPointer}");
      }
      return string.Join(";", result.Cast<string>().Reverse());
    }
  }
}
