using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Grammar
{
    /// <summary>
    /// Writes a plan as camelCase JSON. Each node's public properties are its JSON members, in
    /// declaration order, and a node's <c>Kind</c> is its discriminator; the contract generator reads
    /// the same properties, so the JSON, the TypeScript contract and the schema cannot disagree.
    /// </summary>
    internal static class GrammarSerializer
    {
        private static readonly JsonSerializerOptions Compact = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Default,
        };

        internal static string Serialize(PlanDocument plan) => JsonSerializer.Serialize(plan, Compact);
    }

    /// <summary>Writes an enum member by its camelCase name, the name the plan and the contract carry: <c>ToNumber</c> is <c>toNumber</c>.</summary>
    /// <typeparam name="TEnum">The enum: a closed set of names.</typeparam>
    internal sealed class CamelCaseEnum<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
    {
        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options) =>
            writer.WriteStringValue(JsonNamingPolicy.CamelCase.ConvertName(value.ToString()));

        /// <inheritdoc/>
        public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotSupportedException("Plan types are write-only.");
    }
}
