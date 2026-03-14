using System.Text.Json.Serialization;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Guards;

namespace Alis.Reactive.Descriptors.Commands
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(DispatchCommand), "dispatch")]
    [JsonDerivedType(typeof(MutateElementCommand), "mutate-element")]
    [JsonDerivedType(typeof(ValidationErrorsCommand), "validation-errors")]
    [JsonDerivedType(typeof(IntoCommand), "into")]
    public abstract class Command
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guard? When { get; internal set; }
    }

    public sealed class DispatchCommand : Command
    {
        public string Event { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Payload { get; }

        public DispatchCommand(string @event, object? payload = null)
        {
            Event = @event;
            Payload = payload;
        }
    }

    public sealed class MutateElementCommand : Command
    {
        public string Target { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Prop { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Method { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Chain { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Coerce { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object[]? Args { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Value { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public BindSource? Source { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Vendor { get; }

        public MutateElementCommand(
            string target,
            string? prop = null,
            string? method = null,
            string? chain = null,
            string? coerce = null,
            object[]? args = null,
            string? value = null,
            BindSource? source = null,
            string? vendor = null)
        {
            Target = target;
            Prop = prop;
            Method = method;
            Chain = chain;
            Coerce = coerce;
            Args = args;
            Value = value;
            Source = source;
            Vendor = vendor;
        }
    }

    public sealed class ValidationErrorsCommand : Command
    {
        public string FormId { get; }

        public ValidationErrorsCommand(string formId)
        {
            FormId = formId;
        }
    }

    /// <summary>
    /// Injects the HTTP response body as innerHTML of the target element.
    /// Used for loading partial views via GET requests.
    /// </summary>
    public sealed class IntoCommand : Command
    {
        public string Target { get; }

        public IntoCommand(string target)
        {
            Target = target;
        }
    }
}
