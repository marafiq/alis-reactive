namespace Alis.Reactive
{
    /// <summary>What a vendor's event listener receives, and so what a handler's payload is.</summary>
    public enum VendorEventPayload
    {
        /// <summary>The listener's first argument, as the vendor passes it; its members read as missing when it passes none.</summary>
        Argument,

        /// <summary>A DOM event: a <c>CustomEvent</c>'s <c>detail</c>, the event itself when it has none, otherwise the element that raised it.</summary>
        DomEvent,
    }
}
