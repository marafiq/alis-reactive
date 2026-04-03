using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Serialization
{
    internal sealed class WriteOnlyPolymorphicConverter<T> : JsonConverter<T>
    {
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            => JsonSerializer.Serialize(writer, value, value!.GetType(), options);

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotSupportedException("Plan contracts are write-only.");
    }
}
