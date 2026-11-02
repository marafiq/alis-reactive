using System.Text.Json.Serialization;
using Alis.Reactive.Grammar;

namespace Alis.Reactive
{
    /// <summary>What a vendor's event listener receives, and so what a handler's payload is. The plan carries the member's camelCase name.</summary>
    [JsonConverter(typeof(CamelCaseEnum<VendorEventPayload>))]
    public enum VendorEventPayload
    {
        /// <summary>The listener's first argument, as the vendor passes it; its members read as missing when it passes none.</summary>
        Argument,

        /// <summary>The element that raised a DOM event; a <c>CustomEvent</c>'s <c>detail</c>, or the event itself when it has none.</summary>
        Element,
    }
}
