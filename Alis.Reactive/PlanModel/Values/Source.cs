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
    internal sealed class PayloadSource : Source
    {
        private readonly PayloadScope _scope;

        /// <summary>JSON discriminator for payload sources. Always <c>"payload"</c>.</summary>
        public string Kind => "payload";
        /// <summary>Payload scope wire term, for example event or success.</summary>
        public string Scope => _scope.Value;

        internal PayloadSource(string scope)
            : this(PayloadScope.From(scope))
        {
        }

        private PayloadSource(PayloadScope scope)
        {
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
        }

        internal static PayloadSource Event() => new PayloadSource(PayloadScope.Event);

        internal static PayloadSource Success() => new PayloadSource(PayloadScope.Success);

        internal static PayloadSource Error() => new PayloadSource(PayloadScope.Error);

        /// <summary>Request snapshot scope. Designed landing zone (owner-confirmed 2026-06-12):
        /// the runtime keeps the request snapshot for deterministic retry; the authoring door is future surface.</summary>
        internal static PayloadSource Request() => new PayloadSource(PayloadScope.Request);

        /// <summary>Pipeline-local variable scope, one flat scope per execution: hold a value read
        /// from a browser object so later steps reuse it (var x = read(); b = x). Designed landing
        /// zone (owner-confirmed 2026-06-12): the runtime resolver case exists; the authoring door
        /// and the context writer are future surface.</summary>
        internal static PayloadSource Local() => new PayloadSource(PayloadScope.Local);

        /// <summary>Current array element under an array operation (top of the element scope stack).</summary>
        internal static PayloadSource Element() => new PayloadSource(PayloadScope.Element);
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
    /// DOM element is a JavaScript object; its members are reached with the same RuntimePath
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
