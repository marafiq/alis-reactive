using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.Grammar;
using Alis.Reactive.Validation;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// One HTTP request and what happens around it, written as the sentence the runtime runs: the form
    /// checks itself, the loading steps run, the request is sent and its response routed, the finally
    /// steps run, then a chained request follows a success.
    /// </summary>
    internal sealed class RequestStep : Step
    {
        private readonly string _method;
        private readonly string _url;
        private string _format = "json";

        internal RequestStep(string method, string url)
        {
            if (string.IsNullOrWhiteSpace(method)) throw new ArgumentException("HTTP method required.", nameof(method));
            _method = method;
            _url = url ?? throw new ArgumentNullException(nameof(url));
        }

        internal GatherInputDraft Gather { get; set; } = new GatherInputDraft();

        internal Block WhileLoading { get; set; } = new Block();

        internal Block Finally { get; set; } = new Block();

        internal ResponseRoutes Routes { get; set; } = new ResponseRoutes();

        internal FormValidation? Validation { get; set; }

        internal string Url => _url;

        internal void Format(string format) => _format = format;

        internal override void WriteTo(List<Statement> block, Scope scope) => block.AddRange(Write(scope));

        private IReadOnlyList<Statement> Write(Scope around)
        {
            // A request its form checks first runs inside the check, so what it binds is its own.
            var scope = Validation == null ? around : around.ForBlock();
            var method = _method;
            var input = Gather.Resolve(_url);
            var response = scope.Fresh("res");
            var routeScope = scope.WithResponse(Routes.Handle, response);

            // WhileLoading runs before the request is built, and Finally whether or not it was built, sent or
            // answered, so neither reads what building and sending it binds: both are written before it. Finally
            // reads no response of its own request, as C# refuses a finally that reads what its try assigned.
            var sentence = new List<Statement>();
            sentence.AddRange(WhileLoading.Write(scope));
            var cleanup = Finally.Write(scope);

            // A gathered value that returns a promise settles just before the request is sent.
            var exchange = scope.WithHoisted(() => new[]
            {
                G.Let(response, G.Call(G.Obj(scope.Objects.Http()), "send", SendArgument(method, input, scope)), awaits: true),
            }).ToList();
            exchange.AddRange(Routes.Write(response, routeScope));

            if (cleanup.Count == 0)
                sentence.AddRange(exchange);
            else
                sentence.Add(G.Try(exchange, cleanup));

            if (Routes.Chained != null)
                sentence.Add(G.If(Ok(response), Routes.Chained.Write(routeScope.ForBlock()), G.NoStatements));

            if (Validation is { } validation)
            {
                var valid = G.Call(G.Obj(scope.Objects.Form(validation.Form)), "validate");
                return new[] { G.If(valid, sentence, G.NoStatements) };
            }

            return sentence;
        }

        internal static Expr Ok(string response) => G.Get(G.Ref(response), new[] { "ok" }, Shape.None);

        // ── the request alis.http sends ──────────────────────────────────────────

        private Expr SendArgument(string method, GatheredInput input, Scope scope)
        {
            if (input.IsEmpty && _format != "json")
                throw new InvalidOperationException(
                    $"The {method} request to '{_url}' gathers nothing, so it has no form data to send: remove AsFormData() or gather a value.");
            var format = _format;
            var nested = method != "GET" && format == "json";
            var body = nested ? Nested(input.Payload).Write(scope) : G.Record(FlatValues(input.Payload, scope));

            // Form() writes each input at its own key: nested, leaf by leaf into the authored record
            // (model.fill), so an authored value beside an input survives; flat, merged in by binding name.
            if (input.IncludesForm && nested)
                body = G.Call(G.Obj(scope.Objects.Model()), "fill", body);
            else if (input.IncludesForm)
                body = G.Op(Operator.Merge, body, G.Get(G.Obj(scope.Objects.Model()), "fields"));

            Expr arrays = G.List(input.Payload
                .Where(member => member.Value.OutputShape.DescribesArray)
                .Select(member => (Expr)G.Lit(member.Name.Value)));
            if (input.IncludesForm)
                arrays = G.Call(arrays, "concat", G.Get(G.Obj(scope.Objects.Model()), new[] { "arrays" }, Shape.None));

            return G.Record(
                ("method", G.Lit(method)),
                ("url", G.Lit(_url)),
                ("route", Named(input.Route, scope)),
                ("values", body),
                ("arrays", arrays),
                ("headers", Named(input.Headers, scope)),
                ("format", G.Lit(format)));
        }

        private static Expr Named(IReadOnlyList<(string Name, ValueExpression Value)> members, Scope scope) =>
            G.Record(members.Select(member => new RecordField(member.Name, member.Value.Write(scope))));

        // A query string or form data is flat, keyed by MVC binding name.
        private static List<RecordField> FlatValues(IReadOnlyList<(BindingPath Name, ValueExpression Value)> payload, Scope scope) =>
            payload.Select(member => new RecordField(member.Name.Value, member.Value.Write(scope))).ToList();

        // A JSON body nests each value at its body path; an index segment (Lines[0]) makes an array.
        private static ValueExpression Nested(IReadOnlyList<(BindingPath Name, ValueExpression Value)> payload)
        {
            var draft = new RecordDraft();
            foreach (var member in payload)
                draft.Assign(member.Name.Path, member.Value);
            return draft.ToValueExpression();
        }
    }

    /// <summary>
    /// Where a response goes: an exact status runs its route, any other success or error its catch-all,
    /// and a chained request follows a success.
    /// </summary>
    internal sealed class ResponseRoutes
    {
        private readonly List<(int Status, Block Route)> _errorStatuses = new List<(int, Block)>();

        // Null until declared: an empty route cannot tell a second OnSuccess from the first.
        private Block? _success;
        private Block? _error;

        internal ResponseHandle Handle { get; } = new ResponseHandle();

        internal RequestStep? Chained { get; private set; }

        internal void OnSuccess(Block route)
        {
            if (_success != null)
                throw new InvalidOperationException("A response has one OnSuccess; write every step of it in that one callback.");
            _success = route;
        }

        internal void OnError(Block route)
        {
            if (_error != null)
                throw new InvalidOperationException("A response has one OnError for every status; write every step of it in that one callback.");
            _error = route;
        }

        internal void OnError(int status, Block route)
        {
            var code = HttpResponseStatusCode.FromDeveloperStatus(status).Value;
            if (_errorStatuses.Any(existing => existing.Status == code))
                throw new InvalidOperationException($"A response has one OnError({code}); write every step of it in that one callback.");
            _errorStatuses.Add((code, route));
        }

        internal void ContinueWith(RequestStep request)
        {
            if (Chained != null)
                throw new InvalidOperationException(
                    "A response can declare only one chained request. " +
                    "To continue the sequence, attach the next Chained request to the existing follow-up request.");
            Chained = request;
        }

        internal IReadOnlyList<Statement> Write(string response, Scope routeScope)
        {
            var success = Route(Array.Empty<(int, Block)>(), _success, response, routeScope);
            var error = Route(_errorStatuses, _error, response, routeScope);
            if (success.Count == 0 && error.Count == 0) return G.NoStatements;
            if (success.Count == 0) return new[] { G.If(G.Not(RequestStep.Ok(response)), error, G.NoStatements) };
            return new[] { G.If(RequestStep.Ok(response), success, error) };
        }

        // An exact status runs its route; any other status runs the catch-all route.
        private static IReadOnlyList<Statement> Route(
            IReadOnlyList<(int Status, Block Route)> statuses, Block? catchAll, string response, Scope routeScope)
        {
            var status = G.Get(G.Ref(response), new[] { "status" }, Shape.None);
            var arms = statuses
                .Select(route => new Arm(G.Eq(status, G.Lit(route.Status)), route.Route.Write(routeScope)))
                .ToList();
            var otherwise = catchAll == null ? G.NoStatements : catchAll.Write(routeScope);
            return arms.Count == 0 ? otherwise : new[] { G.If(arms, otherwise) };
        }
    }

    // The form a request validates before it is sent, and the browser rules it reads from the validator the
    // request names.
    internal sealed class FormValidation
    {
        internal FormValidation(
            Func<IBrowserRuleSource, IReadOnlyList<BrowserField>> rules,
            string validatorName,
            string form)
        {
            Rules = rules ?? throw new ArgumentNullException(nameof(rules));
            ValidatorName = validatorName ?? throw new ArgumentNullException(nameof(validatorName));
            Form = form ?? throw new ArgumentNullException(nameof(form));
        }

        internal Func<IBrowserRuleSource, IReadOnlyList<BrowserField>> Rules { get; }

        internal string ValidatorName { get; }

        internal string Form { get; }
    }
}
