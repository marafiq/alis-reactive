using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    // The members a gather sets, by binding name, its headers, and the form whose inputs it sends. Where a member
    // goes is decided when the request is written, because only then is the URL known: MVC's route-value rule sends
    // a member named like a {placeholder} to the path, and every other member to the query or body. Which form
    // Form() sends is decided then too, because only then is the form the request validates known.
    internal sealed class GatherInputDraft
    {
        private readonly List<(BindingPath Member, ValueExpression Value)> _members = new List<(BindingPath, ValueExpression)>();
        private readonly List<(string Name, ValueExpression Value)> _headers = new List<(string, ValueExpression)>();
        private SentForm? _form;

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

        internal void SendForm(SentForm form)
        {
            if (_form != null)
                throw new InvalidOperationException("A request sends the inputs of one form: call Form() or Form(formId) once.");
            _form = form;
        }

        internal GatheredInput Resolve(string method, string url, FormValidation? validation)
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
            return new GatheredInput(payload, route, _headers, _form?.ContainerFor(method, url, validation));
        }
    }

    /// <summary>The element whose inputs a gather's <c>Form</c> sends: the form its request validates, or an element it names.</summary>
    internal sealed class SentForm
    {
        private readonly string? _formId;

        private SentForm(string? formId)
        {
            _formId = formId;
        }

        /// <summary>The form the request validates.</summary>
        internal static SentForm Validated { get; } = new SentForm(null);

        /// <summary>The element with this id.</summary>
        internal static SentForm Named(string formId) => new SentForm(formId);

        // A form is an element, so Form() names the one the request checks; a request that checks none names none.
        internal string ContainerFor(string method, string url, FormValidation? validation) =>
            _formId ?? validation?.Form ?? throw new InvalidOperationException(
                $"The {method} request to '{url}' sends Form(), the inputs of the form it validates, but it validates no form. " +
                "Name the element whose inputs it sends, Form(\"formId\"), or validate that form first, Validate<TValidator>(\"formId\").");
    }

    /// <summary>A gather resolved against its URL: body or query members, route values, headers and the form whose inputs it sends.</summary>
    internal sealed class GatheredInput
    {
        internal GatheredInput(
            IReadOnlyList<(BindingPath Name, ValueExpression Value)> payload,
            IReadOnlyList<(string Name, ValueExpression Value)> route,
            IReadOnlyList<(string Name, ValueExpression Value)> headers,
            string? form)
        {
            Payload = payload;
            Route = route;
            Headers = headers;
            Form = form;
        }

        internal IReadOnlyList<(BindingPath Name, ValueExpression Value)> Payload { get; }

        internal IReadOnlyList<(string Name, ValueExpression Value)> Route { get; }

        internal IReadOnlyList<(string Name, ValueExpression Value)> Headers { get; }

        /// <summary>The id of the element whose mounted inputs are sent; null when the gather sends no form.</summary>
        internal string? Form { get; }

        internal bool IsEmpty => Payload.Count == 0 && Route.Count == 0 && Headers.Count == 0 && Form == null;
    }
}
