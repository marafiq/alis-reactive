using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    internal sealed class GatherDraft
    {
        private readonly List<GatherPayloadField> _payloadFields = new List<GatherPayloadField>();
        private readonly Dictionary<string, ValueProducer> _supplementalFields =
            new Dictionary<string, ValueProducer>();
        private readonly Dictionary<string, ValueProducer> _headerFields =
            new Dictionary<string, ValueProducer>();
        private readonly Dictionary<string, ValueProducer> _routeParameterFields =
            new Dictionary<string, ValueProducer>();

        internal IReadOnlyList<GatherPayloadField> PayloadFields => _payloadFields;
        internal IEnumerable<string> SupplementalPayloadPaths => _supplementalFields.Keys;
        internal GatherSelection Selection { get; private set; } = GatherSelection.ExplicitFields;

        internal void IncludeAllRegisteredInputs()
        {
            Selection = GatherSelection.AllRegisteredInputs;
        }

        internal void AddPayloadField(GatherPayloadField field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            _payloadFields.Add(field);
        }

        internal void AddSupplementalField(HttpPayloadPath path, ValueProducer value)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (value == null) throw new ArgumentNullException(nameof(value));
            var payloadPathAlreadyExists = _supplementalFields.ContainsKey(path.Value);
            if (payloadPathAlreadyExists)
                throw new InvalidOperationException(
                    $"Supplemental gather payload path '{path.Value}' is already declared.");

            _supplementalFields.Add(path.Value, value);
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

        internal SupplementalRequestFields ToSupplementalFields()
        {
            var hasNoSupplementalFields = _supplementalFields.Count == 0;
            if (hasNoSupplementalFields) return SupplementalRequestFields.Empty;

            return SupplementalRequestFields.From(CopySupplementalFields());
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

        private Dictionary<string, ValueProducer> CopySupplementalFields()
        {
            var copy = new Dictionary<string, ValueProducer>();
            foreach (var field in _supplementalFields)
                copy[field.Key] = field.Value;
            return copy;
        }
    }
}
