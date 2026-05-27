using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    internal sealed class GatherDraft
    {
        private readonly List<RequestInputAssignment> _assignments = new List<RequestInputAssignment>();

        internal RequestInputSourceSelection SourceSelection { get; private set; } =
            RequestInputSourceSelection.ExplicitAssignments;

        internal RequestInput BuildRequestInput(RequestBodyFormat bodyFormat, RequestUrl url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));

            RequestRouteTemplate
                .For(url)
                .RequireRouteParameters(RouteParameterNames());

            var hasGatheredInput =
                _assignments.Count > 0
                || SourceSelection.SelectsRegisteredInputs;
            if (!hasGatheredInput)
                return RequestInput.None;

            return RequestInputProjection.From(
                _assignments,
                bodyFormat,
                SourceSelection);
        }

        internal void IncludeAllRegisteredInputs()
        {
            SourceSelection = RequestInputSourceSelection.AllRegisteredInputs;
        }

        internal void AddAssignment(RequestInputAssignment assignment)
        {
            if (assignment == null) throw new ArgumentNullException(nameof(assignment));
            _assignments.Add(assignment);
        }

        internal void AddPayload(BindingPath path, ValueProducer value)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (value == null) throw new ArgumentNullException(nameof(value));
            _assignments.Add(RequestInputAssignment.Payload(path, value));
        }

        internal void AddHeader(HeaderName name, ValueProducer value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (value == null) throw new ArgumentNullException(nameof(value));
            _assignments.Add(RequestInputAssignment.Header(name, value));
        }

        internal void AddRouteParameter(RouteParameterName name, ValueProducer value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (value == null) throw new ArgumentNullException(nameof(value));
            _assignments.Add(RequestInputAssignment.RouteParameter(name, value));
        }

        private IEnumerable<string> RouteParameterNames()
        {
            foreach (var assignment in _assignments)
            {
                if (assignment.Target is RequestRouteParameterTarget routeParameter)
                    yield return routeParameter.Name;
            }
        }

    }
}
