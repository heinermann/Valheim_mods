using CommandLine;
using System;

namespace TheRuinsFeatureGen
{
  /**
   * Feature ideas
   * - Nearest vertical beam bottom (dx, dy, dz, abs distance)
   * - Nearest horizontal beam bottom (dx, dy, dz, abs distance)
   * - Nearest floor bottom (dx, dy, dz, abs distance)
   * - Nearest roof bottom (dx, dy, dz, abs distance, rotation)
   * - Nearest wall bottom (dx, dy, dz, abs distance, rotation)
   * - Nearest angled beam bottom (dx, dy, dz, abs distance, rotation)
   * - Nearest campfire bottom (dx, dy, dz, abs distance)
   * - Nearest other piece (dx, dy, dz, abs distance)
   * - Lowest cell piece y
   * - Lowest/median/avg neighbour cell piece y
   * - Diff lowest cell and lowest neighbour
   * - Matrix of nearest + lowest and each neighbour pieces (16 x 8?)
   */
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
      // TODO
      return 0;
    }
  }
}
