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

    /// <summary>Identifies a registered UI component as the value source.</summary>
    public sealed class ComponentSource : Source
    {
        /// <summary>Gets the kind. Always <c>"component"</c>.</summary>
        public string Kind => "component";
        /// <summary>Gets the registered component name.</summary>
        public string Component { get; }

        internal ComponentSource(string component)
        {
            Component = component ?? throw new ArgumentNullException(nameof(component));
        }

        internal static ComponentSource Of(string component) => new ComponentSource(component);
    }

    /// <summary>Reads from the event or response payload of the current trigger.</summary>
    public sealed class PayloadSource : Source
    {
        /// <summary>Gets the kind. Always <c>"payload"</c>.</summary>
        public string Kind => "payload";
        /// <summary>Gets the payload scope: event (trigger payload), success or error (HTTP response), request (outgoing body), dispatch (custom event data), or local (view model).</summary>
        public string Scope { get; }
        /// <summary>Gets the optional payload type tag, or <see langword="null"/> when untyped.</summary>
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string Type { get; }

        internal PayloadSource(string scope, string type = null)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            Type = type;
        }

        internal static PayloadSource Event(string type = null) => new PayloadSource("event", type);
        internal static PayloadSource Success(string type = null) => new PayloadSource("success", type);
        internal static PayloadSource Error(string type = null) => new PayloadSource("error", type);
        internal static PayloadSource Request(string type = null) => new PayloadSource("request", type);
        internal static PayloadSource Dispatch(string type = null) => new PayloadSource("dispatch", type);
        internal static PayloadSource Local() => new PayloadSource("local");
    }

    /// <summary>Reads a value from a named plugin object registered by the application.</summary>
    public sealed class PluginSource : Source
    {
        /// <summary>Gets the kind. Always <c>"plugin"</c>.</summary>
        public string Kind => "plugin";
        /// <summary>Gets the plugin registry name.</summary>
        public string Name { get; }
        private PluginSource(string name)
        {
            Name = name ?? throw new System.ArgumentNullException(nameof(name));
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
