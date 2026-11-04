using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Grammar
{
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

    internal sealed class BodyStepConverter : JsonConverter<BodyStep>
    {
        public override void Write(Utf8JsonWriter writer, BodyStep value, JsonSerializerOptions options)
        {
            if (value.IsIndex) writer.WriteNumberValue(value.Index);
            else writer.WriteStringValue(value.Name);
        }

        public override BodyStep Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotSupportedException("Plan types are write-only.");
    }
}
