using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    internal sealed class GatherDraft
    {
        private readonly List<RequestPayloadAssignment> _payloadAssignments = new List<RequestPayloadAssignment>();
        private readonly Dictionary<string, ValueProducer> _headers =
            new Dictionary<string, ValueProducer>();
        private readonly Dictionary<string, ValueProducer> _routeParameters =
            new Dictionary<string, ValueProducer>();

        internal IReadOnlyList<RequestPayloadAssignment> PayloadAssignments => _payloadAssignments;

        internal GatherSourceSelection SourceSelection { get; private set; } =
            GatherSourceSelection.ExplicitPayloadAssignments;

        internal bool ReadsRequestInput =>
            _payloadAssignments.Count > 0
            || SourceSelection.SelectsRegisteredInputs;

        internal RequestInput BuildRequestInput(RequestBodyFormat bodyFormat)
        {
            if (!ReadsRequestInput)
                return RequestInput.None;

            return GatherInput.From(
                _payloadAssignments,
                bodyFormat,
                SourceSelection);
        }

        internal void IncludeAllRegisteredInputs()
        {
            SourceSelection = GatherSourceSelection.AllRegisteredInputs;
        }

        internal void AddPayloadAssignment(RequestPayloadAssignment payloadAssignment)
        {
            if (payloadAssignment == null) throw new ArgumentNullException(nameof(payloadAssignment));
            _payloadAssignments.Add(payloadAssignment);
        }

        internal void AddPayloadAssignment(BindingPath path, ValueProducer value)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (value == null) throw new ArgumentNullException(nameof(value));
            var payloadAssignment = RequestPayloadAssignment.Of(path, value);
            _payloadAssignments.Add(payloadAssignment);
        }

        internal void AddHeader(HeaderName name, ValueProducer value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (value == null) throw new ArgumentNullException(nameof(value));
            _headers[name.Value] = value;
        }

        internal RouteParameterName RegisterRouteParameter(string name)
        {
            var routeParameter = RouteParameterName.Of(name);
            var routeParameterAlreadyExists = _routeParameters.ContainsKey(routeParameter.Value);
            if (routeParameterAlreadyExists)
                throw new InvalidOperationException(
                    $"Route param '{name}' is already defined. Each route param can only be set once.");

            return routeParameter;
        }

        internal void AddRouteParameter(RouteParameterName name, ValueProducer value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (value == null) throw new ArgumentNullException(nameof(value));
            _routeParameters[name.Value] = value;
        }

        internal IReadOnlyDictionary<string, ValueProducer> HeadersForRequest()
        {
            return new Dictionary<string, ValueProducer>(_headers);
        }

        internal IReadOnlyDictionary<string, ValueProducer> RouteParametersFor(RequestUrl url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            return RequestRouteTemplate
                .For(url)
                .Bind(_routeParameters);
        }

    }
}
