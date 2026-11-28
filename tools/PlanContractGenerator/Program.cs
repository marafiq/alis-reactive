using System;
using System.IO;

namespace Alis.Reactive.PlanContractGenerator;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Usage: PlanContractGenerator <the folder that holds plan.ts and plan.schema.json>");
            return 2;
        }

        var contract = PlanContract.ReadCore();

        var folder = Path.GetFullPath(args[0]);
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "plan.ts"), contract.TypeScript());
        File.WriteAllText(Path.Combine(folder, "plan.schema.json"), contract.Schema());
        Console.WriteLine("[contract] wrote plan.ts and plan.schema.json to " + folder);
        return 0;
    }
}
