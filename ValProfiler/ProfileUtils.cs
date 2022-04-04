using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using UnityEngine;

namespace Heinermann.ValProfiler
{
  public class ProfileUtils
  {
    public static Assembly GetAssemblyByName(string name)
    {
      return AppDomain.CurrentDomain.GetAssemblies().
             SingleOrDefault(assembly => assembly.GetName().Name == name);
    }

    static readonly string[] BANNED_TYPES = {
      "System.Enum",
      "System.Number",
      "System.Environment",
      "Mono.",
      "System.Reflection.",
      "System.IO.IsolatedStorage.",
      "System.Threading.",
      "System.Security.",
      "System.Runtime.",
      "System.Diagnostics.",
      "System.Configuration.",
      "System.IO.Ports.",
      "System.IO.Compression.",
      "System.Net.",
      "System.CodeDom.",
      "Microsoft."
    };

    static readonly HashSet<string> BANNED_ASSEMBLIES = new HashSet<string>
    {
      "ValProfiler",
      "HarmonyDTFAssembly1",
      "0Harmony",
      "HarmonyXInterop",
      "BepInEx",
      "BepInEx.Preloader",
      "BepInEx.MonoMod.HookGenPatcher",
      "MMHOOK_assembly_valheim",
      "MMHOOK_assembly_utils",
      "System",
      "MonoMod",
      "UnityEngine",
      "mscorlib",
      "Assembly-CSharp",
      "assembly_steamworks",
      "assembly_googleanalytics"
    };

    public static IEnumerable<MethodBase> GetTargetMethodsForAssembly(Assembly assembly)
    {
      string assemblyName = assembly.GetName().Name;
      if (BANNED_ASSEMBLIES.Contains(assemblyName) ||
        assemblyName.StartsWith("System.") ||
        assemblyName.StartsWith("MonoMod.") ||
        assemblyName.StartsWith("Mono.") ||
        assemblyName.StartsWith("UnityEngine.")) {
        Debug.Log($"Skipping over assembly from ignore list: {assembly.FullName}");
        return Enumerable.Empty<MethodBase>();
      }

      Debug.Log($"Retrieving info for assembly: {assembly.FullName}");

      if (assembly == null)
      {
        Debug.LogError("Failed to find assembly");
        return Enumerable.Empty<MethodBase>();
      }

      try
      {
        var assemblyTypes = assembly.GetTypes()
          .Where(type =>
            type.IsClass &&
            !type.Attributes.HasFlag(TypeAttributes.HasSecurity) &&
            !type.Attributes.HasFlag(TypeAttributes.Import) &&
            !type.Attributes.HasFlag(TypeAttributes.Interface) &&
            !type.IsImport &&
            !type.IsInterface &&
            !type.IsSecurityCritical &&
            !BANNED_TYPES.Any(type.FullName.StartsWith)
          );

        var assemblyMethods = assemblyTypes
          .SelectMany(type => AccessTools.GetDeclaredMethods(type))
          .Where(method => {
            foreach (object attr in method.GetCustomAttributes(false))
            {
              if ((attr is DllImportAttribute) ||
                (attr is MethodImplAttribute) ||
                (attr is CLSCompliantAttribute) ||
                (attr is SecurityCriticalAttribute) ||
                (attr is ObsoleteAttribute)
              )
              {
                return false;
              }
            }

            if (method.GetMethodBody() == null || method.Name.Equals("ReadUInt64")) return false;

            return !method.ContainsGenericParameters &&
            !method.IsAbstract &&
            !method.IsVirtual &&
            !method.GetMethodImplementationFlags().HasFlag(MethodImplAttributes.Native) &&
            !method.GetMethodImplementationFlags().HasFlag(MethodImplAttributes.Unmanaged) &&
            !method.GetMethodImplementationFlags().HasFlag(MethodImplAttributes.InternalCall) &&
            !method.Attributes.HasFlag(MethodAttributes.PinvokeImpl) &&
            !method.Attributes.HasFlag(MethodAttributes.Abstract) &&
            !method.Attributes.HasFlag(MethodAttributes.UnmanagedExport);
          });
        
        return assemblyMethods.Cast<MethodBase>();
      } catch (Exception e)
      {
        Debug.LogError(e);
        return Enumerable.Empty<MethodBase>();
      }
    }

    public static IEnumerable<MethodBase> GetTargetMethods()
    {
      return AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetTargetMethodsForAssembly);
    }
    public static long ticksToNanoTime(long ticks)
    {
      return 10000L * ticks / TimeSpan.TicksPerMillisecond * 100L;
    }

  }
}
