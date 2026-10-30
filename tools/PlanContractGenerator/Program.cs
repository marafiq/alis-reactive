using System;
using System.IO;

namespace Alis.Reactive.ContractGeneration
{
    /// <summary>Writes runtime/types/plan.ts from the C# grammar (<see cref="GrammarContract"/>).</summary>
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var outputPath = args.Length > 0
                ? args[0]
                : Path.Combine("Alis.Reactive.Assets", "runtime", "types", "plan.ts");
            var fullPath = Path.GetFullPath(outputPath);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)
                ?? throw new InvalidOperationException("Output path must include a directory."));
            File.WriteAllText(fullPath, GrammarContract.Render());
            Console.WriteLine("Generated " + fullPath);
            return 0;
        }
    }
}
