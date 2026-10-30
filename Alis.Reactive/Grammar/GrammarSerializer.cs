using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Grammar
{
    /// <summary>
    /// Writes a plan as camelCase JSON. Each node's public properties are its JSON members, in
    /// declaration order, and a node's <c>Kind</c> is its discriminator; the contract generator reads
    /// the same properties, so the JSON and the TypeScript contract cannot disagree.
    /// </summary>
    internal static class GrammarSerializer
    {
        // An enum is its member's name, or the text [JsonStringEnumMemberName] gives it, wherever a value the
        // plan carries holds one: alone, in a record or in a list (grammar.md 1.4).
        private static readonly JsonSerializerOptions Compact = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Default,
            Converters = { new JsonStringEnumConverter() },
        };

        internal static string Serialize(GrammarPlan plan) => JsonSerializer.Serialize(plan, Compact);

        /// <summary>A scalar as the plan writes it, as text: an enum by its member's name, a date in ISO 8601.</summary>
        internal static string TextOf(object value, Type type)
        {
            var json = JsonSerializer.SerializeToElement(value, type, Compact);
            return json.ValueKind == JsonValueKind.String ? json.GetString()! : json.GetRawText();
        }
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
