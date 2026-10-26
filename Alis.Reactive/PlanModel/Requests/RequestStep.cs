using System;
using System.Collections.Generic;
using System.Globalization;
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

        private IReadOnlyList<Statement> Write(Scope scope)
        {
            var method = _method;
            var input = Gather.Resolve(_url);
            var response = scope.Fresh("res");
            var routeScope = scope.WithResponse(Routes.Handle, response);

            // A gathered value that returns a promise settles just before the request is sent.
            var exchange = scope.WithHoisted(() => new[]
            {
                G.Let(response, G.CallTyped(G.Obj(scope.Objects.Http()), "send", Shape.None, SendArgument(method, input, scope)), awaits: true),
            }).ToList();
            exchange.AddRange(Routes.Write(response, routeScope));

            var sentence = new List<Statement>();
            sentence.AddRange(WhileLoading.Write(scope));
            // The response the try bound stays in scope after it (grammar.md 3.1), so Finally reads it too.
            var cleanup = Finally.Write(routeScope);
            if (cleanup.Count == 0)
                sentence.AddRange(exchange);
            else
                sentence.Add(G.Try(exchange, cleanup));

            if (Routes.Chained != null)
                sentence.Add(G.If(Ok(response), Routes.Chained.Write(routeScope), G.NoStatements));

            if (Validation is { } validation)
            {
                var valid = G.CallTyped(G.Obj(scope.Objects.Form(validation.Form)), "validate", Shape.Boolean);
                return new[] { G.If(valid, sentence, G.NoStatements) };
            }

            return sentence;
        }

        internal static Expr Ok(string response) => G.Get(G.Ref(response), new[] { "ok" }, Shape.None);

        // ── the request alis.http sends ──────────────────────────────────────────

        private Expr SendArgument(string method, GatheredInput input, Scope scope)
        {
            // A request that gathers nothing is sent as JSON, whatever format it named.
            var format = input.IsEmpty ? "json" : _format;
            var nested = method != "GET" && format == "json";
            var values = nested ? NestedValues(input.Payload, scope) : FlatValues(input.Payload, scope);
            Expr body = G.Record(values);

            // Form() writes each input at its own key: nested, leaf by leaf into the authored record
            // (model.fill), so an authored value beside an input survives; flat, by binding name.
            if (input.IncludesForm && nested)
                body = G.Call(G.Obj(scope.Objects.Model()), new[] { "fill" }, new[] { body }, G.NoShapes, Shape.None);
            else if (input.IncludesForm)
                body = G.Record(values.Append(new SpreadEntry(G.Get(G.Obj(scope.Objects.Model()), new[] { "fields" }, Shape.None))));

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
            G.Record(members.Select(member => (RecordEntry)new FieldEntry(member.Name, member.Value.Write(scope))));

        // A query string or form data is flat, keyed by MVC binding name.
        private static List<RecordEntry> FlatValues(IReadOnlyList<(BindingPath Name, ValueExpression Value)> payload, Scope scope) =>
            payload.Select(member => (RecordEntry)new FieldEntry(member.Name.Value, member.Value.Write(scope))).ToList();

        // A JSON body nests each value at its body path; an index segment (Lines[0]) makes an array.
        private static List<RecordEntry> NestedValues(IReadOnlyList<(BindingPath Name, ValueExpression Value)> payload, Scope scope)
        {
            var root = new BodyNode();
            foreach (var member in payload)
                root.Assign(member.Name.Path.Segments, 0, member.Value.Write(scope));
            return root.Entries();
        }

        // The body a request sends: each gathered value at its path, a name making an object and a
        // position making an array.
        private sealed class BodyNode
        {
            private readonly List<string> _order = new List<string>();
            private readonly Dictionary<string, BodyNode> _children = new Dictionary<string, BodyNode>(StringComparer.Ordinal);
            private readonly Dictionary<int, BodyNode> _items = new Dictionary<int, BodyNode>();
            private Expr? _leaf;

            internal void Assign(IReadOnlyList<PathSegment> segments, int at, Expr value)
            {
                if (at == segments.Count)
                {
                    _leaf = value;
                    _children.Clear();
                    _order.Clear();
                    _items.Clear();
                    return;
                }

                _leaf = null;
                var segment = segments[at];
                var child = segment.IsIndex ? ItemAt(segment.Index) : ChildNamed(segment.ToPathText());
                child.Assign(segments, at + 1, value);
            }

            internal List<RecordEntry> Entries() =>
                _order.Select(key => (RecordEntry)new FieldEntry(key, _children[key].ToExpr())).ToList();

            private BodyNode ChildNamed(string name)
            {
                if (_children.TryGetValue(name, out var child)) return child;
                child = new BodyNode();
                _children.Add(name, child);
                _order.Add(name);
                return child;
            }

            private BodyNode ItemAt(int index)
            {
                if (_items.TryGetValue(index, out var item)) return item;
                item = new BodyNode();
                _items.Add(index, item);
                return item;
            }

            private Expr ToExpr()
            {
                if (_leaf != null) return _leaf;
                if (_items.Count == 0 || _children.Count > 0) return G.Record(Entries().Concat(ItemEntries()));
                var count = _items.Keys.Max() + 1;
                var items = new List<Expr>(count);
                for (var index = 0; index < count; index++)
                    items.Add(_items.TryGetValue(index, out var item) ? item.ToExpr() : G.Null);
                return G.List(items);
            }

            // A node with both names and positions is an object; its positions are entries named by their digits.
            private IEnumerable<RecordEntry> ItemEntries() =>
                _items.OrderBy(item => item.Key)
                    .Select(item => (RecordEntry)new FieldEntry(item.Key.ToString(CultureInfo.InvariantCulture), item.Value.ToExpr()));
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
            Func<IClientValidationRuleSource, IReadOnlyList<ClientValidationField>> rules,
            string validatorName,
            string form)
        {
            Rules = rules ?? throw new ArgumentNullException(nameof(rules));
            ValidatorName = validatorName ?? throw new ArgumentNullException(nameof(validatorName));
            Form = form ?? throw new ArgumentNullException(nameof(form));
        }

        internal Func<IClientValidationRuleSource, IReadOnlyList<ClientValidationField>> Rules { get; }

        internal string ValidatorName { get; }

        internal string Form { get; }
    }
}
