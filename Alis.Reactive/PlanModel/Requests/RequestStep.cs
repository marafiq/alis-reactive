using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel.Reactions;
using Alis.Reactive.PlanModel.Values;
using Alis.Reactive.PlanModel.Writing;
using Alis.Reactive.WireTerms;

namespace Alis.Reactive.PlanModel.Requests
{
    internal sealed class RequestStep : Step
    {
        private readonly string _method;
        private readonly string _url;
        private readonly HashSet<string> _authored = new HashSet<string>(StringComparer.Ordinal);
        private string _format = "json";
        private GatherInputDraft _gather = new GatherInputDraft();
        private Block _whileLoading = new Block();
        private Block _finally = new Block();
        private ResponseRoutes _routes = new ResponseRoutes();

        internal RequestStep(string method, string url)
        {
            if (string.IsNullOrWhiteSpace(method)) throw new ArgumentException("HTTP method required.", nameof(method));
            _method = method;
            _url = url ?? throw new ArgumentNullException(nameof(url));
        }

        internal FormValidation? Validation { get; private set; }

        internal void Format(string format)
        {
            Author("format", "A request has one body format: call AsJson() or AsFormData() once.");
            _format = format;
        }

        internal void GatherFrom(GatherInputDraft gather)
        {
            Author("gather", "A request has one Gather; set every member and header of it in that one callback.");
            _gather = gather;
        }

        internal void LoadWith(Block whileLoading)
        {
            Author("while-loading", "A request has one WhileLoading; write every step of it in that one callback.");
            _whileLoading = whileLoading;
        }

        internal void FinishWith(Block cleanup)
        {
            Author("finally", "A request has one Finally; write every step of it in that one callback.");
            _finally = cleanup;
        }

        internal void RouteBy(ResponseRoutes routes)
        {
            Author("response", "A request has one Response; write every route of it in that one callback.");
            _routes = routes;
        }

        internal void ValidateBy(FormValidation validation)
        {
            Author("validate", "A request validates one form: call Validate<TValidator>(formId) once.");
            Validation = validation;
        }

        private void Author(string part, string refusal)
        {
            if (!_authored.Add(part)) throw new InvalidOperationException(refusal);
        }

        internal override void WriteTo(List<Statement> block, Scope scope) => block.AddRange(Write(scope));

        private IReadOnlyList<Statement> Write(Scope around)
        {
            var scope = Validation == null ? around : around.ForBlock();
            var method = _method;
            var input = _gather.Resolve(method, _url, Validation);
            var response = scope.Fresh("res");
            var routeScope = scope.WithResponse(_routes.Handle, response);

            var sentence = new List<Statement>();
            sentence.AddRange(_whileLoading.Write(scope));
            var cleanup = _finally.Write(scope);

            var exchange = scope.WithHoisted(() => new[]
            {
                G.Let(response, G.Call(G.Obj(scope.Objects.Http()), PathMembers.Required, "send", SendArgument(method, input, scope)), awaits: true),
            }).ToList();
            exchange.AddRange(_routes.Write(response, routeScope));

            if (cleanup.Count == 0)
                sentence.AddRange(exchange);
            else
                sentence.Add(G.Try(exchange, cleanup));

            if (_routes.Chained != null)
                sentence.Add(G.If(Ok(response), _routes.Chained.Write(routeScope.ForBlock()), G.NoStatements));

            if (Validation is { } validation)
            {
                var valid = G.Call(G.Obj(scope.Objects.Form(validation.Form)), PathMembers.Required, "validate");
                return new[] { G.If(valid, sentence, G.NoStatements) };
            }

            return sentence;
        }

        internal static Expr Ok(string response) => G.Get(G.Ref(response), new[] { "ok" }, Shape.Any, PathMembers.Optional);

        private Expr SendArgument(string method, GatheredInput input, Scope scope)
        {
            if (input.IsEmpty && _format != "json")
            {
                throw new InvalidOperationException(
                    $"The {method} request to '{_url}' gathers nothing, so it has no form data to send: remove AsFormData() or gather a value.");
            }

            var format = _format;
            var nested = method != "GET" && format == "json";
            var body = nested ? Nested(input.Payload).Write(scope) : G.Record(FlatValues(input.Payload, scope));

            var arrays = G.List(input.Payload
                .Where(member => member.Value.OutputShape.DescribesArray)
                .Select(member => (Expr)G.Lit(member.Name.Value)));
            if (input.Form is { } form)
            {
                var inputs = G.Obj(scope.Objects.Inputs(form));
                body = nested ? G.Call(inputs, PathMembers.Required, "fill", body) : G.Op(Operator.Merge, body, G.Get(inputs, PathMembers.Required, "fields"));
                arrays = G.Call(arrays, PathMembers.Optional, "concat", G.Get(inputs, new[] { "arrays" }, Shape.Any, PathMembers.Required));
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

        private static Expr Named(IReadOnlyList<(string Name, ValueExpression Value)> members, Scope scope)
            => G.Record(members.Select(member => new RecordField(member.Name, member.Value.Write(scope))));

        private static List<RecordField> FlatValues(IReadOnlyList<(BindingPath Name, ValueExpression Value)> payload, Scope scope)
            => payload.Select(member => new RecordField(member.Name.Value, member.Value.Write(scope))).ToList();

        private static ValueExpression Nested(IReadOnlyList<(BindingPath Name, ValueExpression Value)> payload)
        {
            var draft = new RecordDraft();
            foreach (var member in payload)
                draft.Assign(member.Name.Path, member.Value);
            return draft.ToValueExpression();
        }
    }

    internal sealed class ResponseRoutes
    {
        private readonly List<(int Status, Block Route)> _errorStatuses = new List<(int, Block)>();

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
            {
                throw new InvalidOperationException(
                    "A response has one OnError without a status; write every step of it in that one callback, and route " +
                    "a status of its own with OnError(status, ..).");
            }

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
            {
                throw new InvalidOperationException(
                    "A response has one Chained request; chain the next request from that one's own Response(r => r.Chained(..)).");
            }

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

        private static IReadOnlyList<Statement> Route(
            IReadOnlyList<(int Status, Block Route)> statuses, Block? catchAll, string response, Scope routeScope)
        {
            var status = G.Get(G.Ref(response), new[] { "status" }, Shape.Any, PathMembers.Optional);
            var arms = statuses
                .Select(route => new Arm(G.Eq(status, G.Lit(route.Status)), route.Route.Write(routeScope)))
                .ToList();
            var otherwise = catchAll == null ? G.NoStatements : catchAll.Write(routeScope);
            return arms.Count == 0 ? otherwise : new[] { G.If(arms, otherwise) };
        }
    }

    internal sealed class FormValidation
    {
        internal FormValidation(
            Func<IBrowserRuleSource, IReadOnlyList<BrowserField>> rules,
            Type validator,
            string form,
            string? ruleSet)
        {
            Rules = rules ?? throw new ArgumentNullException(nameof(rules));
            Validator = validator ?? throw new ArgumentNullException(nameof(validator));
            Form = form ?? throw new ArgumentNullException(nameof(form));
            RuleSet = ruleSet;
        }

        internal Func<IBrowserRuleSource, IReadOnlyList<BrowserField>> Rules { get; }

        internal Type Validator { get; }

        internal string Form { get; }

        internal string? RuleSet { get; }

        internal string DescribedRuleSet => RuleSet == null ? "no rule set" : $"rule set '{RuleSet}'";
    }
}
