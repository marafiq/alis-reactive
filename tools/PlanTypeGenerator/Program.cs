using System;
using System.IO;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.PlanTypeGenerator
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var check = Array.IndexOf(args, "--check") >= 0;

            var outputPath = ResolveOutputPath(args);
            var fullPath = System.IO.Path.GetFullPath(outputPath);

            return check ? CheckDrift(fullPath) : WriteContract(fullPath);
        }

        private static string ResolveOutputPath(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg != "--check")
                    return arg;
            }

            return System.IO.Path.Combine("Alis.Reactive.Assets", "runtime", "types", "plan.ts");
        }

        private static int WriteContract(string fullPath)
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fullPath)
                ?? throw new InvalidOperationException("Output path must include a directory."));

            File.WriteAllText(fullPath, PlanContractGenerator.Render());
            Console.WriteLine("Generated " + fullPath);
            return 0;
        }

        private static int CheckDrift(string fullPath)
        {
            if (!File.Exists(fullPath))
            {
                Console.Error.WriteLine("plan.ts not found at " + fullPath + ". Run the generator first.");
                return 1;
            }

            var result = ContractDriftGate.Check(fullPath);
            if (!result.HasDrift)
            {
                Console.WriteLine("plan.ts is in sync with PlanContractGenerator.");
                return 0;
            }

            Console.Error.WriteLine("plan.ts drift detected. Rerun the generator to regenerate it.");
            Console.Error.WriteLine(result.Diff);
            return 1;
        }
    }
}
