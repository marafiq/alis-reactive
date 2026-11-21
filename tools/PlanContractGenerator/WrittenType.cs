using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Alis.Reactive.PlanContractGenerator;

public sealed class WrittenType
{
    public WrittenType(Type type, string typeScript, JsonNode schema, IReadOnlyList<string> kinds, IReadOnlyList<Type> uses)
    {
        Type = type;
        TypeScript = typeScript;
        Schema = schema;
        Kinds = kinds;
        Uses = uses;
    }

    public Type Type { get; }

    public string TypeScript { get; }

    public JsonNode Schema { get; }

    public IReadOnlyList<string> Kinds { get; }

    public IReadOnlyList<Type> Uses { get; }
}
