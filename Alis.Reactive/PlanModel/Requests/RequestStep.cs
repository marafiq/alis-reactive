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

        // Each part is null until written: a request has one of each, and an empty part cannot tell a second
        // call from the first.
        private GatherInputDraft? _gather;
        private Block? _whileLoading;
        private ResponseRoutes? _routes;
        private Block? _finally;
        private string? _format;

        internal RequestStep(string method, string url)
        {
            if (string.IsNullOrWhiteSpace(method)) throw new ArgumentException("HTTP method required.", nameof(method));
            _method = method;
            _url = url ?? throw new ArgumentNullException(nameof(url));
        }

        internal FormValidation? Validation { get; private set; }

        internal string Url => _url;

        internal void Gathers(GatherInputDraft gather) =>
            _gather = Once(_gather, gather, "A request has one Gather; gather every member it sends in that one callback.");

        internal void ValidatedBy(FormValidation validation) =>
            Validation = Once(Validation, validation, "A request checks one form before it is sent; call Validate once, naming that form.");

        internal void LoadsWith(Block steps) =>
            _whileLoading = Once(_whileLoading, steps, "A request has one WhileLoading; write every step of it in that one callback.");

        internal void RoutesBy(ResponseRoutes routes) =>
            _routes = Once(_routes, routes, "A request has one Response; write every route of it in that one callback.");

        internal void FinishesWith(Block steps) =>
            _finally = Once(_finally, steps, "A request has one Finally; write every step of it in that one callback.");

        internal void Format(string format) =>
            _format = Once(_format, format, "A request has one body format; call AsJson() or AsFormData() once.");

        private static T Once<T>(T? written, T part, string refusal) where T : class =>
            written == null ? part : throw new InvalidOperationException(refusal);

        internal override void WriteTo(List<Statement> block, Scope scope) => block.AddRange(Write(scope));

        private IReadOnlyList<Statement> Write(Scope around)
        {
            // A request its form checks first runs inside the check, so what it binds is its own.
            var scope = Validation == null ? around : around.ForBlock();
            var method = _method;
            var input = (_gather ?? new GatherInputDraft()).Resolve(method, _url, Validation);
            var routes = _routes ?? new ResponseRoutes();
            var response = scope.Fresh("res");
            var routeScope = scope.WithResponse(routes.Handle, response);

            // WhileLoading runs before the request is built, and Finally whether or not it was built, sent or
            // answered, so neither reads what building and sending it binds: both are written before it. Finally
            // reads no response of its own request, as C# refuses a finally that reads what its try assigned.
            var sentence = new List<Statement>();
            if (_whileLoading != null) sentence.AddRange(_whileLoading.Write(scope));
            var cleanup = _finally == null ? G.NoStatements : _finally.Write(scope);

            // A gathered value that returns a promise settles just before the request is sent.
            var exchange = scope.WithHoisted(() => new[]
            {
                G.Let(response, G.Call(G.Obj(scope.Objects.Http()), "send", SendArgument(method, input, scope)), awaits: true),
            }).ToList();
            exchange.AddRange(routes.Write(response, routeScope));

            if (cleanup.Count == 0)
                sentence.AddRange(exchange);
            else
                sentence.Add(G.Try(exchange, cleanup));

            if (routes.Chained != null)
                sentence.Add(G.If(Ok(response), routes.Chained.Write(routeScope.ForBlock()), G.NoStatements));

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
            // JSON is the body format a request names none for.
            var format = _format ?? "json";
            if (input.IsEmpty && format != "json")
                throw new InvalidOperationException(
                    $"The {method} request to '{_url}' gathers nothing, so it has no form data to send: remove AsFormData() or gather a value.");
            var nested = method != "GET" && format == "json";
            var body = nested ? Nested(input.Payload).Write(scope) : G.Record(FlatValues(input.Payload, scope));

            // Form() writes each mounted input inside its form at its own key: nested, leaf by leaf into the
            // authored record (fill), so an authored value beside an input survives; flat, merged in by binding name.
            Expr arrays = G.List(input.Payload
                .Where(member => member.Value.OutputShape.DescribesArray)
                .Select(member => (Expr)G.Lit(member.Name.Value)));
            if (input.Form is { } form)
            {
                var inputs = G.Obj(scope.Objects.Inputs(form));
                body = nested ? G.Call(inputs, "fill", body) : G.Op(Operator.Merge, body, G.Get(inputs, "fields"));
                arrays = G.Call(arrays, "concat", G.Get(inputs, new[] { "arrays" }, Shape.None));
            }

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
                throw new InvalidOperationException(
                    "A response has one OnError without a status; write every step of it in that one callback, and route " +
                    "a status of its own with OnError(status, ..).");
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
                    "A response has one Chained request; chain the next request from that one's own Response(r => r.Chained(..)).");
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

    // The form a request validates before it is sent, the validator the request names, and the browser rules
    // read from it.
    internal sealed class FormValidation
    {
        internal FormValidation(
            Func<IBrowserRuleSource, IReadOnlyList<BrowserField>> rules,
            Type validator,
            string form)
        {
            Rules = rules ?? throw new ArgumentNullException(nameof(rules));
            Validator = validator ?? throw new ArgumentNullException(nameof(validator));
            Form = form ?? throw new ArgumentNullException(nameof(form));
        }

        internal Func<IBrowserRuleSource, IReadOnlyList<BrowserField>> Rules { get; }

        internal Type Validator { get; }

        internal string Form { get; }
    }
}
