using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    internal sealed class GatherDraft
    {
        private readonly List<RequestPayloadAssignment> _fields = new List<RequestPayloadAssignment>();
        private readonly Dictionary<string, ValueProducer> _headerFields =
            new Dictionary<string, ValueProducer>();
        private readonly Dictionary<string, ValueProducer> _routeParameterFields =
            new Dictionary<string, ValueProducer>();

        internal IReadOnlyList<RequestPayloadAssignment> Fields => _fields;

        internal GatherSelection Selection { get; private set; } = GatherSelection.ExplicitFields;

        internal void IncludeAllRegisteredInputs()
        {
            Selection = GatherSelection.AllRegisteredInputs;
        }

        internal void AddField(RequestPayloadAssignment field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            _fields.Add(field);
        }

        internal void AddField(BindingPath path, ValueProducer value)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (value == null) throw new ArgumentNullException(nameof(value));
            var payloadField = RequestPayloadAssignment.Of(path, value);
            _fields.Add(payloadField);
        }

        internal void AddHeader(HeaderName name, ValueProducer value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (value == null) throw new ArgumentNullException(nameof(value));
            _headerFields[name.Value] = value;
        }

        internal RouteParameterName RegisterRouteParameter(string name)
        {
            var routeParameter = RouteParameterName.Of(name);
            var routeParameterAlreadyExists = _routeParameterFields.ContainsKey(routeParameter.Value);
            if (routeParameterAlreadyExists)
                throw new InvalidOperationException(
                    $"Route param '{name}' is already defined. Each route param can only be set once.");

            return routeParameter;
        }

        internal void AddRouteParameter(RouteParameterName name, ValueProducer value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (value == null) throw new ArgumentNullException(nameof(value));
            _routeParameterFields[name.Value] = value;
        }

        internal IReadOnlyDictionary<string, ValueProducer> HeadersForRequest()
        {
            var requestHasNoHeaders = _headerFields.Count == 0;
            if (requestHasNoHeaders)
                return new Dictionary<string, ValueProducer>();

            return new Dictionary<string, ValueProducer>(_headerFields);
        }

        internal IReadOnlyDictionary<string, ValueProducer> RouteParametersFor(RequestUrl url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            return RequestRouteTemplate
                .For(url)
                .Bind(_routeParameterFields);
        }

    }
}
