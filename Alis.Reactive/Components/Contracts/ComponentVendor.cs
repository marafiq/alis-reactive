using System;
using System.Text.RegularExpressions;
using Alis.Reactive.Components;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// Where a vendor's components keep their JavaScript object, how they take a listener, and what a
    /// listener receives, as data the runtime follows. The runtime holds no code for any vendor; it
    /// reads this from the plan.
    /// </summary>
    /// <remarks>
    /// A component declaration names its vendor once, and every component from that vendor shares it.
    /// An object that adds and removes listeners through a pair of methods:
    /// <code>
    /// public static readonly ComponentVendor TomSelect = ComponentVendor.Define(
    ///     "tomselect", objectPath: "tomselect", subscribe: "on", unsubscribe: "off",
    ///     payload: VendorEventPayload.Argument);
    /// </code>
    /// An object that takes each listener as a property named for the event, as a chart takes
    /// <c>options.onClick</c>:
    /// <code>
    /// public static readonly ComponentVendor CallBell = ComponentVendor.DefineCallbackProperties(
    ///     "callbell", objectPath: "callBell", payload: VendorEventPayload.Argument);
    /// </code>
    /// </remarks>
    public sealed class ComponentVendor : IEquatable<ComponentVendor>
    {
        private static readonly Regex NamePattern =
            new Regex(@"^[a-zA-Z][a-zA-Z0-9_-]*$", RegexOptions.Compiled);

        private readonly Path _objectPath;
        private readonly VendorListening _listening;
        private readonly VendorEventPayload _payload;

        private ComponentVendor(
            string name,
            Path objectPath,
            VendorListening listening,
            VendorEventPayload payload)
        {
            Name = name;
            _objectPath = objectPath;
            _listening = listening;
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
            new MethodListening("addEventListener", "removeEventListener"),
            VendorEventPayload.DomEvent);

        /// <summary>
        /// Declares a vendor whose objects add and remove a listener through a pair of methods:
        /// <c>object.on(event, fn)</c> and <c>object.off(event, fn)</c>.
        /// </summary>
        /// <param name="name">A name unique among the page's vendors: a letter, then letters, digits, <c>_</c> or <c>-</c>.</param>
        /// <param name="objectPath">
        /// The dotted path from the rendered element to the component's object, for example
        /// <c>"ej2_instances.0"</c> or <c>"tomselect"</c>; empty when the element is the object.
        /// </param>
        /// <param name="subscribe">The object's method that adds an event listener, for example <c>"on"</c>.</param>
        /// <param name="unsubscribe">The object's method that removes an event listener, for example <c>"off"</c>.</param>
        /// <param name="payload">What a listener receives, and so what <c>args</c> reads.</param>
        [TypedDslExemption("The vendor boundary: a library is described by the JavaScript names it uses for its object and its subscribe and unsubscribe methods.")]
        public static ComponentVendor Define(
            string name,
            string objectPath,
            string subscribe,
            string unsubscribe,
            VendorEventPayload payload)
        {
            if (objectPath == null) throw new ArgumentNullException(nameof(objectPath));

            return new ComponentVendor(
                ValidName(name),
                Path.Parse(objectPath),
                new MethodListening(MemberName.Of(subscribe).Value, MemberName.Of(unsubscribe).Value),
                payload);
        }

        /// <summary>
        /// Declares a vendor whose objects take each listener as a property named for the event:
        /// <c>chart.options.onClick = fn</c>, <c>widget.onChange = fn</c>. The event's JavaScript name is
        /// the property's name (<c>public Event&lt;PressArgs&gt; OnPressed =&gt; Event&lt;PressArgs&gt;();</c>
        /// assigns <c>onPressed</c>). Assigning adds the listener after the function the property already
        /// holds, which keeps running, and what the property held is put back when the handler's owner
        /// unloads.
        /// </summary>
        /// <param name="name">A name unique among the page's vendors: a letter, then letters, digits, <c>_</c> or <c>-</c>.</param>
        /// <param name="objectPath">
        /// The dotted path from the rendered element to the component's object, for example
        /// <c>"callBell"</c> or <c>"chart.options"</c>; empty when the element is the object.
        /// </param>
        /// <param name="payload">What a listener receives, and so what <c>args</c> reads.</param>
        [TypedDslExemption("The vendor boundary: a library is described by the JavaScript name of the path to its object.")]
        public static ComponentVendor DefineCallbackProperties(
            string name,
            string objectPath,
            VendorEventPayload payload)
        {
            if (objectPath == null) throw new ArgumentNullException(nameof(objectPath));

            return new ComponentVendor(
                ValidName(name),
                Path.Parse(objectPath),
                new AssignmentListening(),
                payload);
        }

        /// <summary>The vendor as the plan carries it; a DOM event hands the listener the element that raised it.</summary>
        internal Vendor ToGrammar() =>
            new Vendor(
                _objectPath.TargetText,
                _listening,
                Vendor.PayloadOf(_payload));

        /// <inheritdoc />
        public bool Equals(ComponentVendor? other) =>
            other != null
            && Name == other.Name
            && _objectPath.Equals(other._objectPath)
            && _listening.SameAs(other._listening)
            && _payload == other._payload;

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as ComponentVendor);

        /// <inheritdoc />
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Name);

        /// <inheritdoc />
        public override string ToString() => Name;

        private static string ValidName(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (!NamePattern.IsMatch(name))
                throw new ArgumentException(
                    "Vendor name '" + name + "' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen.",
                    nameof(name));
            return name;
        }
    }

    /// <summary>What a vendor's event listener receives, and so what a handler's payload is.</summary>
    public enum VendorEventPayload
    {
        /// <summary>The listener's first argument, as the vendor passes it; its members read as missing when it passes none.</summary>
        Argument,

        /// <summary>A DOM event: a <c>CustomEvent</c>'s <c>detail</c>, the event itself when it has none, otherwise the element that raised it.</summary>
        DomEvent,
    }
}
