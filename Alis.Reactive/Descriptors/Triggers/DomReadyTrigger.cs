using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Triggers
{
    public sealed class DomReadyTrigger : Trigger
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "dom-ready";

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders.
        /// </summary>
        internal DomReadyTrigger() { }
    }
}
