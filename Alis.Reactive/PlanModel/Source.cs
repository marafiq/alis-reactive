using System;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Base class for value source identifiers in a reactive plan. Not constructed in application code.
    /// </summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(WriteOnlyPolymorphicConverter<Source>))]
    public abstract class Source
    {
        private protected Source() { }
    }

    /// <summary>Identifies a browser object whose declared properties and methods can be evaluated at runtime.</summary>
    public abstract class RuntimeObjectSource : Source
    {
        private protected RuntimeObjectSource() { }
    }

    /// <summary>Identifies a registered UI component as the value source.</summary>
    public sealed class ComponentSource : RuntimeObjectSource
    {
        private readonly ComponentKey _component;

        /// <summary>Gets the kind. Always <c>"component"</c>.</summary>
        public string Kind => "component";
        /// <summary>Gets the registered component name.</summary>
        public string Component => _component.Value;

        internal ComponentSource(string component)
            : this(ComponentKey.Of(component))
        {
        }

        private ComponentSource(ComponentKey component)
        {
            _component = component ?? throw new ArgumentNullException(nameof(component));
        }

        internal static ComponentSource Of(string component) => new ComponentSource(component);
        internal static ComponentSource Of(ComponentKey component) => new ComponentSource(component);
    }

    /// <summary>Reads from the event or response payload of the current trigger.</summary>
    public sealed class PayloadSource : Source
    {
        private readonly PayloadScope _scope;
        private readonly PayloadContract _type;

        /// <summary>Gets the kind. Always <c>"payload"</c>.</summary>
        public string Kind => "payload";
        /// <summary>Gets the payload scope: event (trigger payload), success or error (HTTP response), request (outgoing body), dispatch (custom event data), or local (view model).</summary>
        public string Scope => _scope.Value;
        /// <summary>Gets the payload typing contract.</summary>
        public PayloadContract Type => _type;

        internal PayloadSource(string scope)
            : this(PayloadScope.From(scope), PayloadContract.Untyped)
        {
        }

        internal PayloadSource(string scope, string type)
            : this(PayloadScope.From(scope), PayloadContract.Named(type))
        {
        }

        private PayloadSource(PayloadScope scope, PayloadContract type)
        {
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
            _type = type ?? throw new ArgumentNullException(nameof(type));
        }

        internal static PayloadSource Event() => new PayloadSource(PayloadScope.Event, PayloadContract.Untyped);
        internal static PayloadSource Event(PayloadContract type) => new PayloadSource(PayloadScope.Event, type);
        internal static PayloadSource Event(string type) => Event(PayloadContract.Named(type));

        internal static PayloadSource Success() => new PayloadSource(PayloadScope.Success, PayloadContract.Untyped);
        internal static PayloadSource Success(PayloadContract type) => new PayloadSource(PayloadScope.Success, type);
        internal static PayloadSource Success(string type) => Success(PayloadContract.Named(type));

        internal static PayloadSource Error() => new PayloadSource(PayloadScope.Error, PayloadContract.Untyped);
        internal static PayloadSource Error(PayloadContract type) => new PayloadSource(PayloadScope.Error, type);
        internal static PayloadSource Error(string type) => Error(PayloadContract.Named(type));

        internal static PayloadSource Request() => new PayloadSource(PayloadScope.Request, PayloadContract.Untyped);
        internal static PayloadSource Request(PayloadContract type) => new PayloadSource(PayloadScope.Request, type);
        internal static PayloadSource Request(string type) => Request(PayloadContract.Named(type));

        internal static PayloadSource Dispatch() => new PayloadSource(PayloadScope.Dispatch, PayloadContract.Untyped);
        internal static PayloadSource Dispatch(PayloadContract type) => new PayloadSource(PayloadScope.Dispatch, type);
        internal static PayloadSource Dispatch(string type) => Dispatch(PayloadContract.Named(type));

        internal static PayloadSource Local() => new PayloadSource(PayloadScope.Local, PayloadContract.Untyped);

        /// <summary>The current array element under an array operation (top of the element scope stack).</summary>
        internal static PayloadSource Element() => new PayloadSource(PayloadScope.Element, PayloadContract.Untyped);
    }

    /// <summary>Reads a value from a named plugin object registered by the application.</summary>
    public sealed class PluginSource : RuntimeObjectSource
    {
        private readonly PluginName _name;
        private readonly TypeKey _type;

        /// <summary>Gets the kind. Always <c>"plugin"</c>.</summary>
        public string Kind => "plugin";
        /// <summary>Gets the browser plugin name.</summary>
        public string Name => _name.Value;
        /// <summary>Gets the plugin object contract type key.</summary>
        public string Type => _type.Value;
        private PluginSource(string name)
        {
            _name = PluginName.Of(name);
            _type = TypeKey.Plugin(_name);
        }
        internal static PluginSource Of(string name) => new PluginSource(name);
    }

    /// <summary>Reads a value from the browser's current URL query string.</summary>
    public sealed class UrlSource : Source
    {
        /// <summary>Gets the kind. Always <c>"url"</c>.</summary>
        public string Kind => "url";
        private UrlSource() { }
        internal static UrlSource Instance { get; } = new UrlSource();
    }
}
