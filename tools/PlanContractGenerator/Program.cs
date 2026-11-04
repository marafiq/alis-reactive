using System;
using System.IO;

namespace Alis.Reactive.ContractGeneration
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var typeScriptPath = Path.GetFullPath(args.Length > 0
                ? args[0]
                : Path.Combine("Alis.Reactive.Assets", "runtime", "types", "plan.ts"));
            var directory = Path.GetDirectoryName(typeScriptPath)
                ?? throw new InvalidOperationException("Output path must include a directory.");
            var schemaPath = Path.Combine(directory, "plan.schema.json");

            var contract = GrammarContract.Read(typeof(Alis.Reactive.ReactivePlan<>).Assembly);
            Directory.CreateDirectory(directory);
            File.WriteAllText(typeScriptPath, contract.TypeScript());
            File.WriteAllText(schemaPath, contract.Schema());
            Console.WriteLine("Generated " + typeScriptPath);
            Console.WriteLine("Generated " + schemaPath);
            return 0;
        }
    }
}
