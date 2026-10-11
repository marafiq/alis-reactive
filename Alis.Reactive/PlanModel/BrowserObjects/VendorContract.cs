using System;
using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// A vendor as the runtime reads it: the path from a rendered element to the component's object,
    /// the object's subscribe and unsubscribe methods, and what an event hands the listener.
    /// </summary>
    internal sealed class VendorContract
    {
        private const string ArgumentPayload = "argument";
        private const string DomEventPayload = "dom-event";

        internal VendorContract(Path objectPath, string subscribe, string unsubscribe, string payload)
        {
            ObjectPath = objectPath ?? throw new ArgumentNullException(nameof(objectPath));
            Subscribe = subscribe ?? throw new ArgumentNullException(nameof(subscribe));
            Unsubscribe = unsubscribe ?? throw new ArgumentNullException(nameof(unsubscribe));
            Payload = payload ?? throw new ArgumentNullException(nameof(payload));
        }

        /// <summary>Path from the rendered element to the component's object; empty when the element is the object.</summary>
        public Path ObjectPath { get; }

        /// <summary>The object's method that adds a listener: <c>(eventName, listener)</c>.</summary>
        public string Subscribe { get; }

        /// <summary>The object's method that removes a listener: <c>(eventName, listener)</c>.</summary>
        public string Unsubscribe { get; }

        /// <summary>What a listener receives: <c>argument</c> or <c>dom-event</c>.</summary>
        public string Payload { get; }

        internal static string PayloadToken(VendorEventPayload payload)
        {
            switch (payload)
            {
                case VendorEventPayload.Argument:
                    return ArgumentPayload;
                case VendorEventPayload.DomEvent:
                    return DomEventPayload;
                default:
                    throw new ArgumentOutOfRangeException(nameof(payload), payload, "Unknown vendor event payload.");
            }
        }
    }
}
