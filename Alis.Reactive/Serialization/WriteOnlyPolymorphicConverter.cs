using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Serialization
{
    /// <summary>Serializes polymorphic types by writing the concrete type properties. Read is not supported.</summary>
    internal class WriteOnlyPolymorphicConverter<T> : JsonConverter<T>
    {
        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            => JsonSerializer.Serialize(writer, value, value!.GetType(), options);

        /// <inheritdoc/>
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotSupportedException("Plan types are write-only.");
    }
}
