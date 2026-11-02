using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Grammar
{
    /// <summary>
    /// One step of the JSON body path an input binds under (grammar.md 6.3): a member by its name, which makes
    /// an object, or an item by its position, which makes an array. Written as the name or the position, and
    /// only C# tells the two apart: a dictionary keyed by a number names its entry <c>"1042"</c>, a list item
    /// is at <c>1042</c>.
    /// </summary>
    [JsonConverter(typeof(BodyStepConverter))]
    internal sealed class BodyStep
    {
        private readonly string? _name;
        private readonly int _index;

        private BodyStep(string? name, int index)
        {
            _name = name;
            _index = index;
        }

        internal static BodyStep Member(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Member name required.", nameof(name));
            return new BodyStep(name, -1);
        }

        internal static BodyStep At(int index)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), index, "A position is not negative.");
            return new BodyStep(null, index);
        }

        internal bool IsIndex => _name == null;

        internal string Name => _name ?? throw new InvalidOperationException($"Position {_index} names no member.");

        internal int Index => IsIndex ? _index : throw new InvalidOperationException($"'{_name}' names a member, not a position.");
    }

    /// <summary>Writes a body step as the member's name or the item's position.</summary>
    internal sealed class BodyStepConverter : JsonConverter<BodyStep>
    {
        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, BodyStep value, JsonSerializerOptions options)
        {
            if (value.IsIndex) writer.WriteNumberValue(value.Index);
            else writer.WriteStringValue(value.Name);
        }

        /// <inheritdoc/>
        public override BodyStep Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotSupportedException("Plan types are write-only.");
    }
}
