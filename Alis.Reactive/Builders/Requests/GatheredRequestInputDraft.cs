using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    internal sealed class GatheredRequestInputDraft
    {
        private readonly List<RequestInputAssignment> _assignments = new List<RequestInputAssignment>();

        internal RegisteredInputSelection SourceSelection { get; private set; } =
            RegisteredInputSelection.ExplicitAssignments;

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

            return GatheredRequestInput.From(
                _assignments,
                bodyFormat,
                SourceSelection);
        }

        internal void IncludeAllRegisteredInputs()
        {
            SourceSelection = RegisteredInputSelection.AllRegisteredInputs;
        }

        internal void AddAssignment(RequestInputAssignment assignment)
        {
            if (assignment == null) throw new ArgumentNullException(nameof(assignment));
            _assignments.Add(assignment);
        }

        internal void AddPayload(BindingPath path, ValueExpression value)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (value == null) throw new ArgumentNullException(nameof(value));
            _assignments.Add(RequestInputAssignment.Payload(path, value));
        }

        internal void AddHeader(HeaderName name, ValueExpression value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (value == null) throw new ArgumentNullException(nameof(value));
            _assignments.Add(RequestInputAssignment.Header(name, value));
        }

        internal void AddRouteParameter(RouteParameterName name, ValueExpression value)
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
