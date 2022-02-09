using System.Linq;
using UnityEngine;

namespace Heinermann.UnityExtensions
{
  public static class Extensions
  {
    public static T GetOrAddComponent<T>(this GameObject go) where T : UnityEngine.Component
    {
      T component = go.GetComponent<T>();
      if (component == null)
        component = go.AddComponent<T>();
      return component;
    }

    public static bool HasAnyComponent(this GameObject go, params string[] componentNames)
    {
      return componentNames.Any(component => go.GetComponent(component) != null);
    }
  }
}
