using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Grammar
{
    internal static class GrammarSerializer
    {
        private static readonly JsonSerializerOptions Compact = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Default,
        };

        internal static string Serialize(PlanDocument plan) => JsonSerializer.Serialize(plan, Compact);
    }

    internal sealed class CamelCaseEnum<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
    {
        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
            => writer.WriteStringValue(JsonNamingPolicy.CamelCase.ConvertName(value.ToString()));

        public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotSupportedException("Plan types are write-only.");
    }
}
