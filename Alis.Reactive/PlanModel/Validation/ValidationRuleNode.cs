using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Validation;

namespace Alis.Reactive.PlanModel
{
    [JsonConverter(typeof(ValidationRuleNodeJsonConverter))]
    internal sealed class ValidationRuleNode
    {
        private readonly RuleName _name;
        private readonly ValidationMessage _message;
        private readonly ValidationRuleExecution _execution;

        public string Name => _name.Value;
        public string Message => _message.Value;
        internal ValidationRuleExecution Execution => _execution;

        internal ValidationRuleNode(
            RuleName name,
            ValidationMessage message,
            ValidationRuleExecution execution)
        {
            _name = name ?? throw new System.ArgumentNullException(nameof(name));
            _message = message ?? throw new System.ArgumentNullException(nameof(message));
            _execution = execution ?? throw new System.ArgumentNullException(nameof(execution));
        }
    }

    internal sealed class ValidationRuleNodeJsonConverter : JsonConverter<ValidationRuleNode>
    {
        public override void Write(Utf8JsonWriter writer, ValidationRuleNode value, JsonSerializerOptions options)
        {
            if (value == null) throw new System.ArgumentNullException(nameof(value));

            writer.WriteStartObject();
            writer.WriteString("name", value.Name);
            writer.WriteString("message", value.Message);
            WriteExecution(writer, options, value.Execution);
            writer.WriteEndObject();
        }

        public override ValidationRuleNode Read(
            ref Utf8JsonReader reader,
            System.Type typeToConvert,
            JsonSerializerOptions options) =>
            throw new System.NotSupportedException("Plan types are write-only.");

        private static void WriteExecution(
            Utf8JsonWriter writer,
            JsonSerializerOptions options,
            ValidationRuleExecution execution)
        {
            writer.WritePropertyName("execution");
            execution.WriteTo(writer, options);
        }
    }
}
