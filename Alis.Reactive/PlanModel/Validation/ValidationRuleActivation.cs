using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.PlanModel
{
    [JsonConverter(typeof(ValidationRuleActivationJsonConverter))]
    internal abstract class ValidationRuleActivation
    {
        private protected ValidationRuleActivation() { }

        internal static ValidationRuleActivation Always { get; } =
            new AlwaysActiveValidationRule();

        public abstract string Kind { get; }
        internal abstract void WritePayload(Utf8JsonWriter writer, JsonSerializerOptions options);

        internal static ValidationRuleActivation When(ConditionGraph condition) =>
            new ConditionallyActiveValidationRule(condition);

        private sealed class AlwaysActiveValidationRule : ValidationRuleActivation
        {
            public override string Kind => "always";

            internal override void WritePayload(Utf8JsonWriter writer, JsonSerializerOptions options)
            {
            }
        }

        private sealed class ConditionallyActiveValidationRule : ValidationRuleActivation
        {
            private readonly ConditionGraph _condition;

            internal ConditionallyActiveValidationRule(ConditionGraph condition)
            {
                _condition = condition ?? throw new System.ArgumentNullException(nameof(condition));
            }

            public override string Kind => "when";
            internal override void WritePayload(Utf8JsonWriter writer, JsonSerializerOptions options) =>
                PlanJsonWriter.WriteProperty(writer, options, "condition", _condition);
        }
    }

    internal sealed class ValidationRuleActivationJsonConverter : JsonConverter<ValidationRuleActivation>
    {
        public override void Write(Utf8JsonWriter writer, ValidationRuleActivation value, JsonSerializerOptions options)
        {
            if (value == null) throw new System.ArgumentNullException(nameof(value));

            writer.WriteStartObject();
            writer.WriteString("kind", value.Kind);
            value.WritePayload(writer, options);
            writer.WriteEndObject();
        }

        public override ValidationRuleActivation Read(
            ref Utf8JsonReader reader,
            System.Type typeToConvert,
            JsonSerializerOptions options) =>
            throw new System.NotSupportedException("Plan types are write-only.");
    }
}
