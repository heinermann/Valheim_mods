using CommandLine;
using Heinermann.TheRuins;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using System.Collections.Generic;
using System.IO;

namespace TheRuinsFeatureGen
{
  class Program
  {
    // See https://github.com/commandlineparser/commandline for CommandLine lib
    [Verb("generate", HelpText = "Generates a heightmap CSV of the appropriate size for each build and the feature vector inputs.")]
    public class GenOptions
    {
      [Option(Required = true, HelpText = "Directory to find vbuild/blueprint files.")]
      public string Directory { get; set; }
    }

    static void Main(string[] args)
    {
      Parser.Default.ParseArguments<GenOptions>(args)
        .MapResult(
          (GenOptions opts) => RunGenerate(opts),
          errs => 1
        );
    }
    
    static int RunGenerate(GenOptions opts)
    {
      List<Blueprint> blueprints = LoadBlueprints(opts.Directory);
      FeatureGenerator.GenerateFeaturesForAll(blueprints);
      return 0;
    }

    static List<Blueprint> LoadBlueprints(string directory)
    {
      var matcher = new Matcher();
      matcher.AddInclude($"**/*.blueprint");
      matcher.AddInclude($"**/*.vbuild");

      var files = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(directory)));

      var result = new List<Blueprint>();
      foreach (var file in files.Files)
      {
        string blueprintPath = Path.Combine(directory, file.Path);
        Blueprint blueprint = Blueprint.FromFile(blueprintPath);
        result.Add(blueprint);
      }
      return result;
    }
  }
}
