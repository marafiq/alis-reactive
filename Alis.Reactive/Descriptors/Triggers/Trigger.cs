using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Triggers
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(DomReadyTrigger), "dom-ready")]
    [JsonDerivedType(typeof(CustomEventTrigger), "custom-event")]
    public abstract class Trigger
    {
    }

    public sealed class DomReadyTrigger : Trigger
    {
    }

    public sealed class CustomEventTrigger : Trigger
    {
        public string Event { get; }

        public CustomEventTrigger(string @event)
        {
            Event = @event;
        }
    }
}
