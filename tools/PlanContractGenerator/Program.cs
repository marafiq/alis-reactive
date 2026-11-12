using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Alis.Reactive.Declarations;

namespace Alis.Reactive.PlanContractGenerator;

internal static class Program
{
    private const string Schema = "Alis.Reactive.Schema.";

    private static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Usage: PlanContractGenerator <the folder that holds plan.ts and plan.schema.json>");
            return 2;
        }

        var core = typeof(TypedDslExemptionAttribute).Assembly;
        var options = (JsonSerializerOptions)core.GetType(Schema + "PlanJson", throwOnError: true)!
            .GetField("Options", BindingFlags.Static | BindingFlags.NonPublic)!
            .GetValue(null)!;
        var contract = PlanContract.Read(options, core.GetType(Schema + "PlanDocument", throwOnError: true)!, PathSteps.Rows(core, options));

        var folder = Path.GetFullPath(args[0]);
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "plan.ts"), contract.TypeScript());
        File.WriteAllText(Path.Combine(folder, "plan.schema.json"), contract.Schema());
        Console.WriteLine("[contract] wrote plan.ts and plan.schema.json to " + folder);
        return 0;
    }
}
