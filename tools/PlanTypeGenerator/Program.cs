using System;
using System.IO;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.PlanTypeGenerator
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var outputPath = args.Length > 0
                ? args[0]
                : System.IO.Path.Combine(
                    "Alis.Reactive.Assets",
                    "runtime",
                    "types",
                    "plan.ts");

            var fullPath = System.IO.Path.GetFullPath(outputPath);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fullPath)
                ?? throw new InvalidOperationException("Output path must include a directory."));

            File.WriteAllText(fullPath, PlanTypeScriptContract.Render());
            Console.WriteLine("Generated " + fullPath);
            return 0;
        }
    }
}
