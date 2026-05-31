using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    internal sealed class GatherInputDraft
    {
        private readonly List<RequestInputAssignment> _assignments = new List<RequestInputAssignment>();

        internal RegisteredInputSelection RegisteredInputs { get; private set; } =
            RegisteredInputSelection.ExplicitAssignments;

        internal RequestInput BuildRequestInput(RequestBodyFormat bodyFormat, RequestUrl url)
        {
            RequestRouteTemplate
                .For(url)
                .RequireRouteParameters(RouteParameterNames());

            var hasGatheredInput =
                _assignments.Count > 0
                || RegisteredInputs.SelectsRegisteredInputs;
            if (!hasGatheredInput)
                return RequestInput.None;

            return GatherRequestInput.From(
                _assignments,
                bodyFormat,
                RegisteredInputs);
        }

        internal void IncludeAllRegisteredInputs()
        {
            RegisteredInputs = RegisteredInputSelection.AllRegisteredInputs;
        }

        internal void AddAssignment(RequestInputAssignment assignment)
        {
            _assignments.Add(assignment);
        }

        internal void AddPayload(BindingPath path, ValueExpression value)
        {
            _assignments.Add(RequestInputAssignment.Payload(path, value));
        }

        internal void AddHeader(HeaderName name, ValueExpression value)
        {
            _assignments.Add(RequestInputAssignment.Header(name, value));
        }

        internal void AddRouteParameter(RouteParameterName name, ValueExpression value)
        {
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
