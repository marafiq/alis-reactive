using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Nodes;

namespace Alis.Reactive.PlanContractGenerator;

internal static class ContractUses
{
    internal static IReadOnlyDictionary<Type, IReadOnlyList<Type>> Edges(Assembly core)
        => new Dictionary<Type, IReadOnlyList<Type>>
        {
            [core.GetType("Alis.Reactive.Schema.RequestStatement", throwOnError: true)!] =
            [
                core.GetType("Alis.Reactive.RequestsAndResponses.HttpSend", throwOnError: true)!,
                core.GetType("Alis.Reactive.RequestsAndResponses.HttpResponse", throwOnError: true)!,
            ],
            [core.GetType("Alis.Reactive.Schema.FormPluginData", throwOnError: true)!] =
            [
                core.GetType("Alis.Reactive.Validation.FormCheck", throwOnError: true)!,
                core.GetType("Alis.Reactive.Validation.FormPlace", throwOnError: true)!,
            ],
        };

    internal static IReadOnlyList<WrittenType> Leaves(Assembly core)

        => [
            new WrittenType(core.GetType("Alis.Reactive.ValuesAndSources.EvaluatedValue", throwOnError: true)!,
                "export type EvaluatedValue = unknown;", new JsonObject(), kinds: [], uses: []),
            new WrittenType(core.GetType("Alis.Reactive.RequestsAndResponses.BrowserHeaders", throwOnError: true)!,
                "export type BrowserHeaders = Headers;", new JsonObject(), kinds: [], uses: []),
        ];
}
