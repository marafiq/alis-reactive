using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Commands
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(DispatchCommand), "dispatch")]
    [JsonDerivedType(typeof(MutateElementCommand), "mutate-element")]
    public abstract class Command
    {
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
        public string Action { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Value { get; }

        public MutateElementCommand(string target, string action, string? value = null)
        {
            Target = target;
            Action = action;
            Value = value;
        }
    }
}
