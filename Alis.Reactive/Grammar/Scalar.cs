using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Grammar
{
    /// <summary>
    /// The value a literal carries (grammar.md 1.2): text, a number, a boolean, or null, written as it is. A
    /// value of any other type is written as the record or the list it is, never as a literal.
    /// </summary>
    [JsonConverter(typeof(ScalarConverter))]
    internal sealed class Scalar
    {
        internal static readonly Scalar Null = new Scalar(null);

        private Scalar(object? value)
        {
            Value = value;
        }

        internal object? Value { get; }

        internal static Scalar Of(object? value)
        {
            if (value == null) return Null;
            if (value is string || value is bool || IsNumber(value)) return new Scalar(value);
            throw new ArgumentException(
                $"A literal is text, a number, a boolean or null; a {value.GetType().Name} is written as a record or a list.",
                nameof(value));
        }

        internal static bool IsNumber(object value) =>
            value is byte || value is sbyte || value is short || value is ushort || value is int || value is uint
            || value is long || value is ulong || value is float || value is double || value is decimal;
    }

    /// <summary>Writes a scalar as the JSON value it is.</summary>
    internal sealed class ScalarConverter : JsonConverter<Scalar>
    {
        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, Scalar value, JsonSerializerOptions options)
        {
            if (value.Value == null) writer.WriteNullValue();
            else JsonSerializer.Serialize(writer, value.Value, value.Value.GetType(), options);
        }

        /// <inheritdoc/>
        public override Scalar Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotSupportedException("Plan types are write-only.");
    }
}
