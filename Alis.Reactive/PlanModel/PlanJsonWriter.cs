using System.Text.Json;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Stateless serialization mechanics shared by the plan model JSON converters. Writes a named
    /// property by emitting its name and delegating the value to <see cref="JsonSerializer"/>, in the
    /// same category as <see cref="Serialization.PlanNodeDiscriminator{T}"/>. Carries no domain branch.
    /// </summary>
    internal static class PlanJsonWriter
    {
        internal static void WriteProperty<T>(
            Utf8JsonWriter writer,
            JsonSerializerOptions options,
            string name,
            T value)
        {
            writer.WritePropertyName(name);
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
