using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Grammar.Lowering
{
    /// <summary>
    /// Lowers a request to the request sentence (grammar.md 5.1):
    /// <code>
    /// if (form.validate()) {
    ///   …WhileLoading…
    ///   try {
    ///     let res = await http.send({ method, url, route, values, arrays, headers, format });
    ///     if (res.ok) { …OnSuccess, exact status first… } else { …OnError, exact status first… }
    ///   } finally { …Finally… }
    ///   if (res.ok) { …Chained… }
    /// }
    /// </code>
    /// The gathered values are built in C# from the declared targets; only the values themselves are
    /// read in the browser.
    /// </summary>
    internal sealed class RequestLowering
    {
        private readonly LoweringContext _context;
        private readonly ValueLowering _values;
        private readonly ReactionLowering _reactions;

        internal RequestLowering(LoweringContext context, ValueLowering values, ReactionLowering reactions)
        {
            _context = context;
            _values = values;
            _reactions = reactions;
        }

        internal IReadOnlyList<Statement> Request(RequestPlan request, LoweringScope scope)
        {
            var response = _context.Fresh("res");
            var routeScope = scope.WithResponse(request.Response, response);

            // A gathered value that returns a promise settles just before the request is sent.
            var exchange = _values.WithAwaitedReads(() => new[]
            {
                G.Let(response, G.CallTyped(G.Obj(_context.Objects.Http()), "send", Shape.None, SendArgument(request, scope)), awaits: true),
            }).ToList();
            exchange.AddRange(Routing(request, response, routeScope));

            var sentence = new List<Statement>();
            sentence.AddRange(_reactions.Block(request.WhileLoading, scope));
            if (request.Finally.Count == 0)
                sentence.AddRange(exchange);
            else
                sentence.Add(G.Try(exchange, _reactions.Block(request.Finally, scope)));

            if (request.Chain is FollowUpRequestChain chained)
                sentence.Add(G.If(Ok(response), Request(chained.Next, routeScope), G.NoStatements));

            if (request.Validation is ContainerRequestValidationTarget validation)
            {
                var valid = G.CallTyped(G.Obj(_context.Objects.Form(validation.Container)), "validate", Shape.Boolean);
                return new[] { G.If(valid, sentence, G.NoStatements) };
            }

            return sentence;
        }

        private static Expr Ok(string response) => G.Get(G.Ref(response), new[] { "ok" }, Shape.None);

        // ── routing ──────────────────────────────────────────────────────────────

        private IReadOnlyList<Statement> Routing(RequestPlan request, string response, LoweringScope routeScope)
        {
            var success = Routes(request.Success, response, routeScope);
            var error = Routes(request.Error, response, routeScope);
            if (success.Count == 0 && error.Count == 0) return G.NoStatements;
            if (success.Count == 0) return new[] { G.If(G.Not(Ok(response)), error, G.NoStatements) };
            return new[] { G.If(Ok(response), success, error) };
        }

        // An exact status runs its route; any other status runs the first catch-all route.
        private IReadOnlyList<Statement> Routes(IReadOnlyList<ResponseRoute> routes, string response, LoweringScope routeScope)
        {
            var status = G.Get(G.Ref(response), new[] { "status" }, Shape.None);
            var arms = routes
                .Where(route => route.Match is ExactResponseStatusMatch)
                .Select(route => new Arm(
                    G.Eq(status, G.Lit(((ExactResponseStatusMatch)route.Match).Status)),
                    _reactions.Block(route.Reaction, routeScope)))
                .ToList();
            var catchAll = routes.FirstOrDefault(route => route.Match is AnyResponseStatusMatch);
            var otherwise = catchAll == null ? G.NoStatements : _reactions.Block(catchAll.Reaction, routeScope);
            return arms.Count == 0 ? otherwise : new[] { G.If(arms, otherwise) };
        }

        // ── the request alis.http sends ──────────────────────────────────────────

        private Expr SendArgument(RequestPlan request, LoweringScope scope)
        {
            var gather = request.Input as GatherRequestInput;
            var assignments = gather?.Assignments ?? (IReadOnlyList<RequestInputAssignment>)Array.Empty<RequestInputAssignment>();
            var includesAll = gather != null && gather.RegisteredInputs.SelectsRegisteredInputs;
            var format = gather?.BodyFormat ?? "json";
            var nested = request.Method != "GET" && format == "json";

            var payload = assignments.Where(a => a.Target is RequestPayloadTarget).ToList();
            var values = nested ? NestedValues(payload, scope) : FlatValues(payload, scope);
            Expr body = G.Record(values);
            // IncludeAll writes each input at its own key: nested, leaf by leaf into the authored record
            // (model.fill), so an authored value beside an input survives; flat, by binding name.
            if (includesAll && nested)
                body = G.Call(G.Obj(_context.Objects.Model()), new[] { "fill" }, new[] { body }, G.NoShapes, Shape.None);
            else if (includesAll)
                body = G.Record(values.Append(new SpreadEntry(G.Get(G.Obj(_context.Objects.Model()), new[] { "fields" }, Shape.None))));

            Expr arrays = G.List(payload
                .Where(a => DescribesArray(a.Source.OutputShape))
                .Select(a => (Expr)G.Lit(((RequestPayloadTarget)a.Target).Name)));
            if (includesAll)
                arrays = G.Call(arrays, "concat", G.Get(G.Obj(_context.Objects.Model()), new[] { "arrays" }, Shape.None));

            return G.Record(
                ("method", G.Lit(request.Method)),
                ("url", G.Lit(request.Url)),
                ("route", Named<RequestRouteParameterTarget>(assignments, target => target.Name, scope)),
                ("values", body),
                ("arrays", arrays),
                ("headers", Named<RequestHeaderTarget>(assignments, target => target.Name, scope)),
                ("format", G.Lit(format)));
        }

        private Expr Named<TTarget>(IReadOnlyList<RequestInputAssignment> assignments, Func<TTarget, string> name, LoweringScope scope)
            where TTarget : RequestInputTarget =>
            G.Record(assignments
                .Where(a => a.Target is TTarget)
                .Select(a => (RecordEntry)new FieldEntry(name((TTarget)a.Target), _values.Value(a.Source, scope))));

        // A query string or form data is flat, keyed by MVC binding name.
        private List<RecordEntry> FlatValues(IReadOnlyList<RequestInputAssignment> payload, LoweringScope scope) =>
            payload
                .Select(a => (RecordEntry)new FieldEntry(((RequestPayloadTarget)a.Target).Name, _values.Value(a.Source, scope)))
                .ToList();

        // A JSON body nests each value at its body path; an index segment (Lines[0]) makes an array.
        private List<RecordEntry> NestedValues(IReadOnlyList<RequestInputAssignment> payload, LoweringScope scope)
        {
            var root = new BodyNode();
            foreach (var assignment in payload)
            {
                var target = (RequestPayloadTarget)assignment.Target;
                root.Assign(target.Path.Segments, 0, _values.Value(assignment.Source, scope));
            }

            return root.Entries();
        }

        private static bool DescribesArray(Shape shape) =>
            shape.TryGetNullableInnerShape(out var inner) ? DescribesArray(inner) : shape.Kind == "array";

        private sealed class BodyNode
        {
            private readonly List<string> _order = new List<string>();
            private readonly Dictionary<string, BodyNode> _children = new Dictionary<string, BodyNode>(StringComparer.Ordinal);
            private bool _indexed = true;
            private Expr? _leaf;

            internal void Assign(IReadOnlyList<PathSegment> segments, int at, Expr value)
            {
                if (at == segments.Count)
                {
                    _leaf = value;
                    _children.Clear();
                    _order.Clear();
                    return;
                }

                _leaf = null;
                var segment = segments[at];
                if (segment.Kind != "index") _indexed = false;
                var key = segment.ToPathText();
                if (!_children.TryGetValue(key, out var child))
                {
                    child = new BodyNode();
                    _children.Add(key, child);
                    _order.Add(key);
                }

                child.Assign(segments, at + 1, value);
            }

            internal List<RecordEntry> Entries() =>
                _order.Select(key => (RecordEntry)new FieldEntry(key, _children[key].ToExpr())).ToList();

            private Expr ToExpr()
            {
                if (_leaf != null) return _leaf;
                if (!_indexed || _children.Count == 0) return G.Record(Entries());

                var count = _children.Keys.Max(key => int.Parse(key, CultureInfo.InvariantCulture)) + 1;
                var items = new List<Expr>();
                for (var index = 0; index < count; index++)
                    items.Add(_children.TryGetValue(index.ToString(CultureInfo.InvariantCulture), out var item) ? item.ToExpr() : G.Null);
                return G.List(items);
            }
        }
    }
}
