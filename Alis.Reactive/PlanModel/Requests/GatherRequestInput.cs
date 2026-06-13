using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    internal sealed class GatherRequestInput : RequestInput
    {
        public string Kind => "gather";
        public IReadOnlyList<RequestInputAssignment> Assignments { get; }
        public string BodyFormat => RequestBodyFormat.Value;
        public RegisteredInputSelection RegisteredInputs { get; }

        private RequestBodyFormat RequestBodyFormat { get; }

        private GatherRequestInput(
            IReadOnlyList<RequestInputAssignment> assignments,
            RequestBodyFormat bodyFormat,
            RegisteredInputSelection registeredInputs)
        {
            Assignments = assignments;
            RequestBodyFormat = bodyFormat;
            RegisteredInputs = registeredInputs;
        }

        internal static GatherRequestInput From(
            IEnumerable<RequestInputAssignment> assignments,
            RequestBodyFormat bodyFormat,
            RegisteredInputSelection registeredInputs) =>
            new GatherRequestInput(
                assignments.ToList(),
                bodyFormat,
                registeredInputs);
    }

    internal sealed class RegisteredInputSelection
    {
        private RegisteredInputSelection(string kind, bool selectsRegisteredInputs)
        {
            Kind = kind;
            SelectsRegisteredInputs = selectsRegisteredInputs;
        }

        internal static RegisteredInputSelection ExplicitAssignments { get; } =
            new RegisteredInputSelection("explicit", false);

        internal static RegisteredInputSelection AllRegisteredInputs { get; } =
            new RegisteredInputSelection("all-registered-inputs", true);

        public string Kind { get; }
        internal bool SelectsRegisteredInputs { get; }
    }

    internal sealed class RequestInputAssignment
    {
        public RequestInputTarget Target { get; }
        public ValueExpression Source { get; }

        private RequestInputAssignment(RequestInputTarget target, ValueExpression source)
        {
            Target = target;
            Source = source;
        }

        internal static RequestInputAssignment Payload(BindingPath payloadPath, ValueExpression source)
            => new RequestInputAssignment(RequestPayloadTarget.For(payloadPath), source);

        internal static RequestInputAssignment Header(HeaderName name, ValueExpression source)
            => new RequestInputAssignment(RequestHeaderTarget.For(name), source);

        internal static RequestInputAssignment RouteParameter(RouteParameterName name, ValueExpression source)
            => new RequestInputAssignment(RequestRouteParameterTarget.For(name), source);
    }

    [JsonConverter(typeof(PlanNodeDiscriminator<RequestInputTarget>))]
    public abstract class RequestInputTarget
    {
        private protected RequestInputTarget() { }

        public abstract string Kind { get; }
    }

    internal sealed class RequestPayloadTarget : RequestInputTarget
    {
        private readonly BindingPath _path;

        private RequestPayloadTarget(BindingPath path)
        {
            _path = path;
        }

        public override string Kind => "payload";
        public string Name => _path.Value;
        public Path Path => _path.Path;

        internal static RequestPayloadTarget For(BindingPath path) =>
            new RequestPayloadTarget(path);
    }

    internal sealed class RequestHeaderTarget : RequestInputTarget
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

    internal sealed class RequestRouteParameterTarget : RequestInputTarget
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
