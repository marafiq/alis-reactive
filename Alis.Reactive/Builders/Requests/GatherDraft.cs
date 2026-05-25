using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    internal sealed class GatherDraft
    {
        private readonly List<GatherPayloadField> _payloadFields = new List<GatherPayloadField>();
        private readonly List<GatherPayloadField> _supplementalFields =
            new List<GatherPayloadField>();
        private readonly HashSet<string> _supplementalPayloadPaths =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, ValueProducer> _headerFields =
            new Dictionary<string, ValueProducer>();
        private readonly Dictionary<string, ValueProducer> _routeParameterFields =
            new Dictionary<string, ValueProducer>();

        internal IReadOnlyList<GatherPayloadField> PayloadFields => _payloadFields;
        internal IEnumerable<string> SupplementalPayloadPaths
        {
            get
            {
                foreach (var payloadField in _supplementalFields)
                    yield return payloadField.PayloadPath;
            }
        }

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
            var payloadField = GatherPayloadField.Of(path.Value, value);
            var payloadPathAlreadyExists = !_supplementalPayloadPaths.Add(payloadField.PayloadPath);
            if (payloadPathAlreadyExists)
                throw new InvalidOperationException(
                    $"Supplemental gather payload path '{payloadField.PayloadPath}' is already declared.");

            _supplementalFields.Add(payloadField);
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

        private IReadOnlyList<GatherPayloadField> CopySupplementalFields()
        {
            return new List<GatherPayloadField>(_supplementalFields);
        }
    }
}
