using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.PlanModel
{
    [JsonConverter(typeof(ValidationConditionJsonConverter))]
    internal sealed class ValidationCondition
    {
        private readonly Condition _predicate;

        private ValidationCondition(Condition condition)
        {
            _predicate = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        internal void WriteTo(Utf8JsonWriter writer, JsonSerializerOptions options) =>
            JsonSerializer.Serialize(writer, _predicate, options);

        internal static ValidationCondition FromResolvedFieldCondition(Condition condition)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            return new ValidationCondition(condition);
        }
    }

    internal sealed class ValidationConditionJsonConverter : JsonConverter<ValidationCondition>
    {
        public override void Write(Utf8JsonWriter writer, ValidationCondition value, JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            value.WriteTo(writer, options);
        }

        public override ValidationCondition Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
            throw new NotSupportedException("Plan types are write-only.");
    }
}
