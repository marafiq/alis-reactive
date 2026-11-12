using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Json.Schema;

namespace Alis.Reactive.ModuleTests;

internal static class PlanSchema
{
    private static readonly JsonSchema Schema = JsonSchema.FromText(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "plan.schema.json")));

    internal static IReadOnlyList<string> ProblemsOf(string plan)
    {
        using var document = JsonDocument.Parse(plan);
        return WhatFails(Schema.Evaluate(document.RootElement, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical })).ToList();
    }

    private static IEnumerable<string> WhatFails(EvaluationResults evaluated)
    {
        if (evaluated.IsValid) return [];
        var here = evaluated.Errors == null ? [] : evaluated.Errors.Keys.Select(keyword => PlaceOf(evaluated.InstanceLocation.ToString()) + ": " + keyword);
        var below = evaluated.Details == null ? [] : evaluated.Details.SelectMany(WhatFails);
        return here.Concat(below);
    }

    private static string PlaceOf(string location) => location.Length == 0 ? "the plan" : "the plan" + location;
}
