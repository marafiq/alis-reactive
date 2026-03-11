using System.Text.Json.Serialization;
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
        public string JsEmit { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Value { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Source { get; }

        public MutateElementCommand(string target, string jsEmit, string? value = null, string? source = null)
        {
            Target = target;
            JsEmit = jsEmit;
            Value = value;
            Source = source;
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
