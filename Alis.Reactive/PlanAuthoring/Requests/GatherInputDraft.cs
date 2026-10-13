using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    // The members a gather sets, by binding name, and its headers. Where a member goes is decided when the
    // request is written, because only then is the URL known: MVC's route-value rule sends a member named like
    // a {placeholder} to the path, and every other member to the query or body.
    internal sealed class GatherInputDraft
    {
        private readonly List<(BindingPath Member, ValueExpression Value)> _members = new List<(BindingPath, ValueExpression)>();
        private readonly List<(string Name, ValueExpression Value)> _headers = new List<(string, ValueExpression)>();
        private bool _includesForm;

        internal void Add(BindingPath member, ValueExpression value)
        {
            var setTwice = _members.Any(existing => string.Equals(existing.Member.Value, member.Value, StringComparison.OrdinalIgnoreCase));
            if (setTwice)
                throw new InvalidOperationException($"The request member '{member.Value}' is gathered twice; set each member once.");
            _members.Add((member, value));
        }

        internal void AddHeader(string name, ValueExpression value)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Header name required.", nameof(name));
            _headers.Add((name, value));
        }

        internal void IncludeForm()
        {
            _includesForm = true;
        }

        internal GatheredInput Resolve(string url)
        {
            var template = RequestRouteTemplate.For(url);
            var payload = new List<(BindingPath, ValueExpression)>();
            var route = new List<(string, ValueExpression)>();
            foreach (var (member, value) in _members)
            {
                var placeholder = template.PlaceholderFor(member);
                if (placeholder == null)
                {
                    payload.Add((member, value));
                    continue;
                }

                RequestScalarTarget.RouteParameter(placeholder, value.OutputShape);
                route.Add((placeholder, value));
            }

            template.RequireEveryPlaceholderFilled(route.Select(member => member.Item1));
            return new GatheredInput(payload, route, _headers, _includesForm);
        }
    }

    /// <summary>A gather resolved against its URL: body or query members, route values and headers.</summary>
    internal sealed class GatheredInput
    {
        internal GatheredInput(
            IReadOnlyList<(BindingPath Name, ValueExpression Value)> payload,
            IReadOnlyList<(string Name, ValueExpression Value)> route,
            IReadOnlyList<(string Name, ValueExpression Value)> headers,
            bool includesForm)
        {
            Payload = payload;
            Route = route;
            Headers = headers;
            IncludesForm = includesForm;
        }

        internal IReadOnlyList<(BindingPath Name, ValueExpression Value)> Payload { get; }

        internal IReadOnlyList<(string Name, ValueExpression Value)> Route { get; }

        internal IReadOnlyList<(string Name, ValueExpression Value)> Headers { get; }

        internal bool IncludesForm { get; }

        internal bool IsEmpty => Payload.Count == 0 && Route.Count == 0 && Headers.Count == 0 && !IncludesForm;
    }
}
