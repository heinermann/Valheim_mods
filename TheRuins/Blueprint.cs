using Jotunn.Managers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

// Formats and source from https://github.com/sirskunkalot/PlanBuild/blob/ab37752ba519baf30b898fb056f07b777a902aa3/PlanBuild/Blueprints/Blueprint.cs
// and https://github.com/sirskunkalot/PlanBuild/blob/7eb59369b2300e4131db6f4b434c3d44926bd55d/PlanBuild/Blueprints/PieceEntry.cs
namespace Heinermann.TheRuins
{
  internal class PieceEntry
  {
    public string prefabName { get; set; }
    public Quaternion rotation { get; set; }
    public Vector3 position { get; set; }

    public PieceEntry(string name, Vector3 position) : this(name, position, Quaternion.identity)
    {}

    public PieceEntry(string name, Vector3 position, Quaternion rotation)
    {
      this.prefabName = name;
      this.rotation = rotation;
      this.position = position;
    }

    public static PieceEntry FromBlueprint(string line)
    {
      // backwards compatibility
      if (line.IndexOf(',') > -1)
      {
        line = line.Replace(',', '.');
      }

      var parts = line.Split(';');
      string name = parts[0];
      float posX = InvariantFloat(parts[2]);
      float posY = InvariantFloat(parts[3]);
      float posZ = InvariantFloat(parts[4]);
      float rotX = InvariantFloat(parts[5]);
      float rotY = InvariantFloat(parts[6]);
      float rotZ = InvariantFloat(parts[7]);
      float rotW = InvariantFloat(parts[8]);

      Vector3 pos = new Vector3(posX, posY, posZ);
      Quaternion rot = new Quaternion(rotX, rotY, rotZ, rotW).normalized;

      return new PieceEntry(name, pos, rot);
    }

    public static PieceEntry FromVBuild(string line)
    {
      // backwards compatibility
      if (line.IndexOf(',') > -1)
      {
        line = line.Replace(',', '.');
      }

      var parts = line.Split(' ');
      string name = parts[0];
      float rotX = InvariantFloat(parts[1]);
      float rotY = InvariantFloat(parts[2]);
      float rotZ = InvariantFloat(parts[3]);
      float rotW = InvariantFloat(parts[4]);
      float posX = InvariantFloat(parts[5]);
      float posY = InvariantFloat(parts[6]);
      float posZ = InvariantFloat(parts[7]);

      Vector3 pos = new Vector3(posX, posY, posZ);
      Quaternion rot = new Quaternion(rotX, rotY, rotZ, rotW).normalized;

      return new PieceEntry(name, pos, rot);
    }

    internal static float InvariantFloat(string s)
    {
      if (string.IsNullOrEmpty(s)) return 0f;
      return float.Parse(s, NumberStyles.Any, NumberFormatInfo.InvariantInfo);
    }

    public GameObject prefab()
    {
      return PrefabManager.Instance.GetPrefab(prefabName);
    }
  }

  internal class Blueprint
  {
    private const string HeaderSnapPoints = "#SnapPoints";
    private const string HeaderPieces = "#Pieces";

    private enum Format
    {
      VBuild,
      Blueprint
    }

    public List<PieceEntry> Pieces { get; } = new List<PieceEntry>();
    public string Name { get; set; }

    public static Blueprint FromFile(string fileLocation)
    {
      string ext = Path.GetExtension(fileLocation).ToLower();

      Format format = Format.Blueprint;
      if (ext == ".vbuild")
      {
        format = Format.VBuild;
      } 

      string[] lines = File.ReadAllLines(fileLocation);
      string filename = Path.GetFileNameWithoutExtension(fileLocation);
      return FromArray(filename, lines, format);
    }

    private static Blueprint FromArray(string id, string[] lines, Format format)
    {
      Blueprint result = new Blueprint();

      bool ignore = false;
      foreach (var line in lines)
      {
        if (line == HeaderSnapPoints)
        {
          ignore = true;
          continue;
        }
        if (line == HeaderPieces)
        {
          ignore = false;
          continue;
        }

        if (ignore || line.StartsWith("#") || string.IsNullOrWhiteSpace(line)) continue;

        PieceEntry piece = null;
        if (format == Format.VBuild) piece = PieceEntry.FromVBuild(line);
        else if (format == Format.Blueprint) piece = PieceEntry.FromBlueprint(line);

        result.Pieces.Add(piece);
      }
      result.Name = id;
      return result;
    }
  }
}
