using System;
using System.Text.Json;

namespace Alis.Reactive.Grammar
{
    /// <summary>
    /// Writes a plan as camelCase JSON. Each node's public properties are its JSON members, in
    /// declaration order, and a node's <c>Kind</c> is its discriminator; the contract generator reads
    /// the same properties, so the JSON and the TypeScript contract cannot disagree.
    /// </summary>
    internal static class GrammarSerializer
    {
        private static readonly JsonSerializerOptions Compact = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Default,
        };

        private static readonly JsonSerializerOptions Formatted = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };

        internal static string Serialize(GrammarPlan plan) => JsonSerializer.Serialize(plan, Compact);

        internal static string SerializeFormatted(GrammarPlan plan) => JsonSerializer.Serialize(plan, Formatted);
    }

    /// <summary>The TypeScript type of a node property when its CLR type does not say it exactly.</summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class ContractTypeAttribute : Attribute
    {
        internal ContractTypeAttribute(string typeScript)
        {
            TypeScript = typeScript;
        }

        internal string TypeScript { get; }
    }
}
