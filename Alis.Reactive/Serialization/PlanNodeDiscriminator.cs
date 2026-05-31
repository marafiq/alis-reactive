using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Serialization
{
    /// <summary>
    /// Serializes a polymorphic plan node by writing its concrete runtime type's properties, so the
    /// node's own <c>Kind</c> property becomes the JSON discriminator. The single discriminator
    /// mechanism for every plan node family; reading is unsupported because plans are write-only.
    /// </summary>
    /// <typeparam name="T">The abstract plan-node base (for example <c>ReactionGraph</c>).</typeparam>
    public sealed class PlanNodeDiscriminator<T> : JsonConverter<T>
    {
        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            => JsonSerializer.Serialize(writer, value, value!.GetType(), options);

        /// <inheritdoc/>
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotSupportedException("Plan types are write-only.");
    }
}
