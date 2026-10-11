using System;
using System.Text.RegularExpressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// Where a vendor's components keep their JavaScript object and how they raise events, as data
    /// the runtime follows. The runtime holds no code for any vendor; it reads this from the plan.
    /// </summary>
    /// <remarks>
    /// A component declaration names its vendor once, and every component from that vendor shares it:
    /// <code>
    /// public static readonly ComponentVendor TomSelect = ComponentVendor.Define(
    ///     "tomselect", objectPath: "tomselect", subscribe: "on", unsubscribe: "off",
    ///     payload: VendorEventPayload.Argument);
    /// </code>
    /// </remarks>
    public sealed class ComponentVendor : IEquatable<ComponentVendor>
    {
        private static readonly Regex NamePattern =
            new Regex(@"^[a-zA-Z][a-zA-Z0-9_-]*$", RegexOptions.Compiled);

        private readonly Path _objectPath;
        private readonly MemberName _subscribe;
        private readonly MemberName _unsubscribe;
        private readonly VendorEventPayload _payload;

        private ComponentVendor(
            string name,
            Path objectPath,
            MemberName subscribe,
            MemberName unsubscribe,
            VendorEventPayload payload)
        {
            Name = name;
            _objectPath = objectPath;
            _subscribe = subscribe;
            _unsubscribe = unsubscribe;
            _payload = payload;
        }

        /// <summary>The vendor's name, written on every component it ships, for example <c>"fusion"</c>.</summary>
        public string Name { get; }

        /// <summary>
        /// The browser itself: the object is the element the framework rendered, events are DOM
        /// events subscribed with <c>addEventListener</c>.
        /// </summary>
        public static ComponentVendor Native { get; } = new ComponentVendor(
            "native",
            Path.None,
            MemberName.Of("addEventListener"),
            MemberName.Of("removeEventListener"),
            VendorEventPayload.DomEvent);

        /// <summary>Declares a vendor by where its objects live and how they raise events.</summary>
        /// <param name="name">A name unique among the page's vendors: a letter, then letters, digits, <c>_</c> or <c>-</c>.</param>
        /// <param name="objectPath">
        /// The dotted path from the rendered element to the component's object, for example
        /// <c>"ej2_instances.0"</c> or <c>"tomselect"</c>; empty when the element is the object.
        /// </param>
        /// <param name="subscribe">The object's method that adds an event listener, for example <c>"on"</c>.</param>
        /// <param name="unsubscribe">The object's method that removes an event listener, for example <c>"off"</c>.</param>
        /// <param name="payload">What a listener receives, and so what <c>args</c> reads.</param>
        public static ComponentVendor Define(
            string name,
            string objectPath,
            string subscribe,
            string unsubscribe,
            VendorEventPayload payload)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (objectPath == null) throw new ArgumentNullException(nameof(objectPath));
            if (!NamePattern.IsMatch(name))
                throw new ArgumentException(
                    "Vendor name '" + name + "' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen.",
                    nameof(name));

            return new ComponentVendor(
                name,
                Path.Parse(objectPath),
                MemberName.Of(subscribe),
                MemberName.Of(unsubscribe),
                payload);
        }

        internal string Value => Name;

        internal VendorContract Contract =>
            new VendorContract(_objectPath, _subscribe.Value, _unsubscribe.Value, VendorContract.PayloadToken(_payload));

        /// <inheritdoc />
        public bool Equals(ComponentVendor? other) =>
            other != null
            && Name == other.Name
            && _objectPath.Equals(other._objectPath)
            && _subscribe.Equals(other._subscribe)
            && _unsubscribe.Equals(other._unsubscribe)
            && _payload == other._payload;

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as ComponentVendor);

        /// <inheritdoc />
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Name);

        /// <inheritdoc />
        public override string ToString() => Name;
    }

    /// <summary>What a vendor's event listener receives, and so what a reaction's <c>args</c> reads.</summary>
    public enum VendorEventPayload
    {
        /// <summary>The listener's first argument; an empty object when the vendor passes none.</summary>
        Argument,

        /// <summary>A DOM event: a <c>CustomEvent</c>'s <c>detail</c>, otherwise the element that raised it.</summary>
        DomEvent,
    }
}
