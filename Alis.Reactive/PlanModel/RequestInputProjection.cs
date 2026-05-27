using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    internal sealed class RequestInputProjection : RequestInput
    {
        public string Kind => "gather";
        public IReadOnlyList<RequestInputAssignment> Assignments { get; }
        public string BodyFormat => RequestBodyFormat.Value;
        public RequestInputSourceSelection SourceSelection { get; }

        private RequestBodyFormat RequestBodyFormat { get; }

        private RequestInputProjection(
            IReadOnlyList<RequestInputAssignment> assignments,
            RequestBodyFormat bodyFormat,
            RequestInputSourceSelection sourceSelection)
        {
            Assignments = assignments;
            RequestBodyFormat = bodyFormat;
            SourceSelection = sourceSelection;
        }

        internal static RequestInputProjection From(
            IEnumerable<RequestInputAssignment> assignments,
            RequestBodyFormat bodyFormat,
            RequestInputSourceSelection sourceSelection) =>
            new RequestInputProjection(
                assignments.ToList(),
                bodyFormat,
                sourceSelection);
    }

    internal sealed class RequestInputSourceSelection
    {
        private RequestInputSourceSelection(string kind, bool selectsRegisteredInputs)
        {
            Kind = kind;
            SelectsRegisteredInputs = selectsRegisteredInputs;
        }

        internal static RequestInputSourceSelection ExplicitAssignments { get; } =
            new RequestInputSourceSelection("explicit", false);

        internal static RequestInputSourceSelection AllRegisteredInputs { get; } =
            new RequestInputSourceSelection("all-registered-inputs", true);

        public string Kind { get; }
        internal bool SelectsRegisteredInputs { get; }
    }

    internal sealed class RequestInputAssignment
    {
        public RequestInputTarget Target { get; }
        public ValueProducer Source { get; }

        private RequestInputAssignment(RequestInputTarget target, ValueProducer source)
        {
            Target = target;
            Source = source;
        }

        internal static RequestInputAssignment Payload(string payloadPath, ValueProducer source)
            => Payload(BindingPath.Of(payloadPath), source);

        internal static RequestInputAssignment Payload(BindingPath payloadPath, ValueProducer source)
            => new RequestInputAssignment(RequestPayloadTarget.For(payloadPath), source);

        internal static RequestInputAssignment Header(HeaderName name, ValueProducer source)
            => new RequestInputAssignment(RequestHeaderTarget.For(name), source);

        internal static RequestInputAssignment RouteParameter(RouteParameterName name, ValueProducer source)
            => new RequestInputAssignment(RequestRouteParameterTarget.For(name), source);
    }

    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<RequestInputTarget>))]
    public abstract class RequestInputTarget
    {
        private protected RequestInputTarget() { }

        public abstract string Kind { get; }
    }

    public sealed class RequestPayloadTarget : RequestInputTarget
    {
        private readonly BindingPath _path;

        private RequestPayloadTarget(BindingPath path)
        {
            _path = path;
        }

        public override string Kind => "payload";
        public string Name => _path.Value;
        public Path Path => _path.Path;

        internal static RequestPayloadTarget For(string path) =>
            For(BindingPath.Of(path));

        internal static RequestPayloadTarget For(BindingPath path) =>
            new RequestPayloadTarget(path);
    }

    public sealed class RequestHeaderTarget : RequestInputTarget
    {
        private readonly HeaderName _name;

        private RequestHeaderTarget(HeaderName name)
        {
            _name = name;
        }

        public override string Kind => "header";
        public string Name => _name.Value;

        internal static RequestHeaderTarget For(HeaderName name) =>
            new RequestHeaderTarget(name);
    }

    public sealed class RequestRouteParameterTarget : RequestInputTarget
    {
        private readonly RouteParameterName _name;

        private RequestRouteParameterTarget(RouteParameterName name)
        {
            _name = name;
        }

        public override string Kind => "route-param";
        public string Name => _name.Value;

        internal static RequestRouteParameterTarget For(RouteParameterName name) =>
            new RequestRouteParameterTarget(name);
    }
}
