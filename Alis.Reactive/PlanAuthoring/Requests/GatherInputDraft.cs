using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    // The members a gather sets, by binding name, and its headers. Where a member goes is decided when the
    // request is built, because only then is the URL known: MVC's route-value rule sends a member named like
    // a {placeholder} to the path, and every other member to the query or body.
    internal sealed class GatherInputDraft
    {
        private readonly List<(BindingPath Member, ValueExpression Value)> _members = new List<(BindingPath, ValueExpression)>();
        private readonly List<RequestInputAssignment> _headers = new List<RequestInputAssignment>();
        private bool _includesForm;

        internal void Add(BindingPath member, ValueExpression value)
        {
            var setTwice = _members.Any(existing => string.Equals(existing.Member.Value, member.Value, StringComparison.OrdinalIgnoreCase));
            if (setTwice)
                throw new InvalidOperationException($"The request member '{member.Value}' is gathered twice; set each member once.");

            _members.Add((member, value));
        }

        internal void AddHeader(HeaderName name, ValueExpression value)
        {
            _headers.Add(RequestInputAssignment.Header(name, value));
        }

        internal void IncludeForm()
        {
            _includesForm = true;
        }

        internal RequestInput BuildRequestInput(RequestBodyFormat bodyFormat, RequestUrl url)
        {
            var template = RequestRouteTemplate.For(url);
            var assignments = new List<RequestInputAssignment>();
            var filled = new List<string>();

            foreach (var (member, value) in _members)
            {
                var placeholder = template.PlaceholderFor(member);
                if (placeholder == null)
                {
                    assignments.Add(RequestInputAssignment.Payload(member, value));
                    continue;
                }

                RequestScalarTarget.RouteParameter(placeholder, value.OutputShape);
                assignments.Add(RequestInputAssignment.RouteParameter(RouteParameterName.Of(placeholder), value));
                filled.Add(placeholder);
            }

            template.RequireEveryPlaceholderFilled(filled);
            assignments.AddRange(_headers);

            if (assignments.Count == 0 && !_includesForm)
                return RequestInput.None;

            return GatherRequestInput.From(assignments, bodyFormat, _includesForm);
        }
    }
}
