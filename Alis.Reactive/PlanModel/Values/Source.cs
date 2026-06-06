using System;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Wire base for value source identifiers authored through DSL value reads.
    /// </summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(PlanNodeDiscriminator<Source>))]
    public abstract class Source
    {
        private protected Source() { }
    }

    /// <summary>Identifies a plan-declared object whose members can be evaluated at runtime.</summary>
    public abstract class RuntimeObjectSource : Source
    {
        private protected RuntimeObjectSource() { }
    }

    /// <summary>Identifies a plan-registered component as the value source.</summary>
    public sealed class ComponentSource : RuntimeObjectSource
    {
        private readonly ComponentKey _component;

        /// <summary>JSON discriminator for component sources. Always <c>"component"</c>.</summary>
        public string Kind => "component";
        /// <summary>Plan-registered component key used for runtime object lookup.</summary>
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

    /// <summary>Identifies an execution payload scope, such as event data or an HTTP response body.</summary>
    public sealed class PayloadSource : Source
    {
        private readonly PayloadScope _scope;
        private readonly PayloadContract _type;

        /// <summary>JSON discriminator for payload sources. Always <c>"payload"</c>.</summary>
        public string Kind => "payload";
        /// <summary>Payload scope wire term, for example event or success.</summary>
        public string Scope => _scope.Value;
        /// <summary>Payload typing contract used when authoring typed value paths.</summary>
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

    /// <summary>Identifies a plan-registered plugin object as the value source.</summary>
    public sealed class PluginSource : RuntimeObjectSource
    {
        private readonly PluginName _name;
        private readonly BrowserObjectId _type;

        /// <summary>JSON discriminator for plugin sources. Always <c>"plugin"</c>.</summary>
        public string Kind => "plugin";
        /// <summary>Plan-registered plugin name used for runtime object lookup.</summary>
        public string Name => _name.Value;
        /// <summary>Plugin object contract key declared in the generated plan.</summary>
        public string Type => _type.Value;
        private PluginSource(string name)
        {
            _name = PluginName.Of(name);
            _type = BrowserObjectId.Plugin(_name);
        }
        internal static PluginSource Of(string name) => new PluginSource(name);
    }

    /// <summary>Reads from the browser <c>window.location</c> query string.</summary>
    public sealed class UrlSource : Source
    {
        /// <summary>JSON discriminator for URL query sources. Always <c>"url"</c>.</summary>
        public string Kind => "url";
        private UrlSource() { }
        internal static UrlSource Instance { get; } = new UrlSource();
    }

    /// <summary>Identifies a DOM element (by id) whose members are read directly via getElementById.</summary>
    /// <remarks>
    /// A DOM element is a JavaScript object; its members are reached with the same RuntimePath
    /// primitive that resolves component/plugin members. Element IDs are plan-carried, and the
    /// runtime resolves them with <c>getElementById</c> only, without DOM scanning.
    /// </remarks>
    public sealed class DomSource : Source
    {
        private readonly string _element;

        /// <summary>JSON discriminator for direct DOM sources. Always <c>"dom"</c>.</summary>
        public string Kind => "dom";
        /// <summary>Element ID resolved via <c>getElementById</c>.</summary>
        public string Element => _element;

        private DomSource(string element)
        {
            _element = element ?? throw new ArgumentNullException(nameof(element));
        }

        internal static DomSource Of(string element) => new DomSource(element);
    }
}
