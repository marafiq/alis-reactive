using System;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    [System.Text.Json.Serialization.JsonConverter(typeof(WriteOnlyPolymorphicConverter<Source>))]
    public abstract class Source
    {
        private protected Source() { }
    }

    public sealed class ComponentSource : Source
    {
        public string Kind => "component";
        public string Component { get; }

        internal ComponentSource(string component)
        {
            Component = component ?? throw new ArgumentNullException(nameof(component));
        }

        internal static ComponentSource Of(string component) => new ComponentSource(component);
    }

    public sealed class PayloadSource : Source
    {
        public string Kind => "payload";
        public string Scope { get; }
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
}
