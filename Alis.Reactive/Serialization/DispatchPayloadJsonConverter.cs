using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Commands;

namespace Alis.Reactive.Serialization
{
    /// <summary>
    /// Writes <see cref="DispatchPayload"/> as an object whose property values are individual
    /// command-value descriptors.
    /// </summary>
    public sealed class DispatchPayloadJsonConverter : JsonConverter<DispatchPayload>
    {
        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, DispatchPayload value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var field in value.Fields)
            {
                writer.WritePropertyName(field.Key);
                JsonSerializer.Serialize(writer, field.Value, options);
            }

            writer.WriteEndObject();
        }

        /// <inheritdoc />
        public override DispatchPayload Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotSupportedException("Plan descriptors are write-only.");
    }
}
