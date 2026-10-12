using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Grammar.Lowering
{
    /// <summary>
    /// Lowers the plan model's values and synchronous conditions to grammar expressions: every read is
    /// a <c>get</c> or <c>call</c> on a located object or a binding, every array operation a lambda over
    /// <c>toArray</c>, every comparison an operator of the closed table.
    /// </summary>
    internal sealed class ValueLowering
    {
        private readonly LoweringContext _context;

        // The awaited reads of the statement being lowered; null where nothing can wait for one.
        private List<Statement>? _awaitedReads;

        internal ValueLowering(LoweringContext context)
        {
            _context = context;
        }

        // ── awaited reads ────────────────────────────────────────────────────────

        /// <summary>
        /// Lowers one statement. A read of a member that returns a promise becomes
        /// <c>let r = await call(…)</c> before the statement, which reads <c>ref r</c> (grammar.md 5).
        /// </summary>
        internal IReadOnlyList<Statement> WithAwaitedReads(Func<IReadOnlyList<Statement>> lower)
        {
            var outer = _awaitedReads;
            var awaited = new List<Statement>();
            _awaitedReads = awaited;
            try
            {
                var statements = lower();
                return awaited.Count == 0 ? statements : awaited.Concat(statements).ToList();
            }
            finally
            {
                _awaitedReads = outer;
            }
        }

        private T WithoutAwaiting<T>(Func<T> lower)
        {
            var outer = _awaitedReads;
            _awaitedReads = null;
            try
            {
                return lower();
            }
            finally
            {
                _awaitedReads = outer;
            }
        }

        private Expr Awaited(Expr call, string member)
        {
            if (_awaitedReads == null)
                throw new InvalidOperationException(
                    $"Plan '{_context.PlanId}' reads '{member}', which returns a promise, inside an array operation or a " +
                    "validation rule, where nothing waits for it. Read it in a reaction step instead.");

            var settled = _context.Fresh("r");
            _awaitedReads.Add(G.Let(settled, call, awaits: true));
            return G.Ref(settled);
        }

        internal Expr Value(ValueExpression value, LoweringScope scope)
        {
            switch (value)
            {
                case LiteralExpression literal:
                    return G.Lit(literal.Value, literal.Shape);
                case ReadExpression read:
                    return Read(read, scope);
                case ObjectExpression record:
                    return G.Record(record.Fields.Select(field => (RecordEntry)new FieldEntry(field.Key, Value(field.Value, scope))));
                case ArrayExpression array:
                    return G.List(array.Items.Select(item => Value(item, scope)));
                case ArrayOperationExpression operation:
                    return ArrayOperation(operation, scope);
                default:
                    throw new InvalidOperationException("No lowering for value " + value.GetType().Name + ".");
            }
        }

        // ── reads ────────────────────────────────────────────────────────────────

        private Expr Read(ReadExpression read, LoweringScope scope)
        {
            switch (read.From)
            {
                case ComponentSource component:
                    return ComponentRead(component.Component, read, scope);
                case PluginSource plugin:
                    return MemberRead(
                        G.Obj(_context.Objects.Plugin(plugin.Name)),
                        _context.PluginContractOf(plugin.Name),
                        "plugin " + plugin.Name,
                        read,
                        scope);
                case UrlSource _:
                    // MVC binds a scalar from a key's first value and an array from all of them.
                    var values = DescribesArray(read.Shape) ? "queryAll" : "query";
                    return G.Get(G.Get(G.Obj(_context.Objects.Document()), new[] { values }, Shape.None), new[] { read.Member }, read.Shape);
                case PayloadSource payload:
                    return PayloadRead(payload, read, scope);
                case FormFieldSource field:
                    return G.Call(G.Obj(_context.Objects.Model()), new[] { "boundValueOf" }, new Expr[] { G.Lit(field.Input) }, G.NoShapes, read.Shape);
                case DomSource dom:
                    return G.Get(G.Obj(_context.Objects.Element(dom.Element)), G.PathOf(read.Path), read.Shape);
                default:
                    throw new InvalidOperationException("No lowering for value source " + read.From.GetType().Name + ".");
            }
        }

        // A field another plan rendered (a partial's input, validated by the form of the view that
        // loads it) is read through the model, which every plan of the composition joins. A validation
        // rule reads each input it names through the model too, as MVC binds it.
        private Expr ComponentRead(string componentKey, ReadExpression read, LoweringScope scope)
        {
            if (scope.ReadsInputsAsBound)
                return G.Call(G.Obj(_context.Objects.Model()), new[] { "boundValueOf" }, new Expr[] { G.Lit(componentKey) }, G.NoShapes, read.Shape);

            if (!_context.IsDeclared(componentKey))
            {
                if (read.Access is MethodValueReadAccess)
                    throw new InvalidOperationException(
                        $"Plan '{_context.PlanId}' calls '{read.Member}' on '{componentKey}', which it does not render.");
                return G.Call(G.Obj(_context.Objects.Model()), new[] { "valueOf" }, new Expr[] { G.Lit(componentKey) }, G.NoShapes, read.Shape);
            }

            return MemberRead(
                G.Obj(_context.Objects.Element(componentKey)),
                _context.ContractOf(componentKey),
                componentKey,
                read,
                scope);
        }

        private Expr MemberRead(Expr target, BrowserObjectContract contract, string owner, ReadExpression read, LoweringScope scope)
        {
            if (read.Access is MethodValueReadAccess method)
            {
                var declared = _context.Method(contract, read.Member, owner);
                var call = G.Call(target, G.PathOf(declared.Path), Arguments(method.Args, scope), ArgumentShapes(declared), Requested(read.Shape, declared.Returns));
                return declared.Signature.Awaits ? Awaited(call, read.Member) : call;
            }

            var property = _context.Property(contract, read.Member, owner);
            return _context.Read(target, property.Path, Requested(read.Shape, property.Shape));
        }

        private Expr PayloadRead(PayloadSource payload, ReadExpression read, LoweringScope scope)
        {
            var path = G.PathOf(read.Path);
            var (root, rootPath) = PayloadRoot(payload, scope);

            if (read.Access is MethodValueReadAccess method)
                return G.Call(root, rootPath.Concat(path).ToList(), Arguments(method.Args, scope), G.NoShapes, read.Shape);

            var fullPath = rootPath.Concat(path).ToList();
            return fullPath.Count == 0 ? G.As(root, read.Shape) : G.Get(root, fullPath, read.Shape);
        }

        internal (Expr Root, IReadOnlyList<string> Path) PayloadRoot(PayloadSource payload, LoweringScope scope)
        {
            switch (payload.Scope)
            {
                case "event":
                    return (G.Ref(scope.EventParam), G.NoPath);
                case "success":
                case "error":
                    var response = scope.ResponseOf(payload.Response)
                        ?? throw new InvalidOperationException(
                            $"Plan '{_context.PlanId}' reads a response body outside the routes of the request that receives it.");
                    return (G.Ref(response), new[] { "body" });
                case "element":
                    return (G.Ref(scope.Element), G.NoPath);
                default:
                    throw new InvalidOperationException(
                        $"Plan '{_context.PlanId}' reads the '{payload.Scope}' payload, which no builder can author.");
            }
        }

        internal IReadOnlyList<Expr> Arguments(IReadOnlyList<ValueExpression> args, LoweringScope scope) =>
            args.Select(arg => Value(arg, scope)).ToList();

        internal static IReadOnlyList<Shape> ArgumentShapes(ObjectMethod method) =>
            method.Arguments is ExactMethodArgumentContract exact ? exact.Shapes : G.NoShapes;

        internal static Shape Requested(Shape requested, Shape declared) => requested.IsNone ? declared : requested;

        private static bool DescribesArray(Shape shape) =>
            shape.TryGetNullableInnerShape(out var inner) ? DescribesArray(inner) : shape.Kind == "array";

        // ── arrays ───────────────────────────────────────────────────────────────

        // The source may be awaited; a lambda body runs once per item, where nothing waits.
        private Expr ArrayOperation(ArrayOperationExpression operation, LoweringScope scope)
        {
            var items = G.Op(Operator.ToArray, Value(operation.Source, scope));
            return WithoutAwaiting(() => ArrayOperation(operation, items, scope));
        }

        private Expr ArrayOperation(ArrayOperationExpression operation, Expr items, LoweringScope scope)
        {
            switch (operation.Op)
            {
                case "count":
                    return G.Get(items, new[] { "length" }, Shape.Number);
                case "filter":
                    return G.Call(items, new[] { "filter" }, new Expr[] { Predicate(operation, scope) }, G.NoShapes, operation.Shape);
                case "map":
                    return G.Call(items, new[] { "map" }, new Expr[] { Projection(operation, scope) }, G.NoShapes, operation.Shape);
                case "sum":
                    return Sum(items, operation, scope);
                case "any":
                    return operation.Predicate == null
                        ? G.Op(Operator.Gt, G.Get(items, new[] { "length" }, Shape.Number), G.Lit(0))
                        : G.Call(items, new[] { "some" }, new Expr[] { Predicate(operation, scope) }, G.NoShapes, Shape.Boolean);
                case "all":
                    return G.Call(items, new[] { "every" }, new Expr[] { Predicate(operation, scope) }, G.NoShapes, Shape.Boolean);
                case "find":
                    return Find(items, operation, scope);
                case "orderBy":
                    return OrderBy(items, operation, scope, descending: false);
                case "orderByDescending":
                    return OrderBy(items, operation, scope, descending: true);
                default:
                    throw new InvalidOperationException("No lowering for array operation '" + operation.Op + "'.");
            }
        }

        private FnExpr Predicate(ArrayOperationExpression operation, LoweringScope scope)
        {
            var predicate = operation.Predicate
                ?? throw new InvalidOperationException("Array operation '" + operation.Op + "' needs a predicate.");
            var item = _context.Fresh("x");
            return G.Fn(Condition(predicate, scope.WithElement(item)), item);
        }

        private FnExpr Projection(ArrayOperationExpression operation, LoweringScope scope)
        {
            var projection = operation.Projection
                ?? throw new InvalidOperationException("Array operation '" + operation.Op + "' needs a selector.");
            var item = _context.Fresh("x");
            return G.Fn(Value(projection, scope.WithElement(item)), item);
        }

        // Sum adds each selected value as a finite number, so text never concatenates.
        private Expr Sum(Expr items, ArrayOperationExpression operation, LoweringScope scope)
        {
            var total = _context.Fresh("s");
            var item = _context.Fresh("x");
            var term = operation.Projection == null
                ? (Expr)G.Ref(item)
                : Value(operation.Projection, scope.WithElement(item));
            var add = G.Fn(G.Op(Operator.Add, G.Ref(total), G.Op(Operator.Finite, term)), total, item);
            return G.Call(items, new[] { "reduce" }, new Expr[] { add, G.Lit(0) }, G.NoShapes, Shape.Number);
        }

        // Find gives null when nothing matches, then applies the selector to what it found.
        private Expr Find(Expr items, ArrayOperationExpression operation, LoweringScope scope)
        {
            var found = G.Op(Operator.Coalesce, G.Call(items, new[] { "find" }, new Expr[] { Predicate(operation, scope) }, G.NoShapes, Shape.None), G.Null);
            if (operation.Projection == null) return G.As(found, operation.Shape);

            var match = _context.Fresh("x");
            var selected = Value(operation.Projection, scope.WithElement(match));
            var select = G.Fn(G.Op(Operator.Cond, G.Eq(G.Ref(match), G.Null), G.Null, selected), match);
            return G.As(G.Apply(select, found), operation.Shape);
        }

        private Expr OrderBy(Expr items, ArrayOperationExpression operation, LoweringScope scope, bool descending)
        {
            var key = operation.Projection
                ?? throw new InvalidOperationException("Array operation '" + operation.Op + "' needs a key selector.");
            var left = _context.Fresh("a");
            var right = _context.Fresh("b");
            Expr compare = G.Op(Operator.Order, Value(key, scope.WithElement(left)), Value(key, scope.WithElement(right)));
            if (descending) compare = G.Op(Operator.Neg, compare);
            return G.Call(items, new[] { "toSorted" }, new Expr[] { G.Fn(compare, left, right) }, G.NoShapes, operation.Shape);
        }

        // ── synchronous conditions ───────────────────────────────────────────────

        /// <summary>A condition with no user decision in it, as one boolean expression.</summary>
        internal Expr Condition(ConditionGraph condition, LoweringScope scope)
        {
            switch (condition)
            {
                case CompareCondition compare:
                    return Compare(compare, scope);
                case AllCondition all:
                    return G.And(all.Terms.Select(term => Condition(term, scope)).ToList());
                case AnyCondition any:
                    return G.Or(any.Terms.Select(term => Condition(term, scope)).ToList());
                case NotCondition not:
                    return G.Not(Condition(not.Term, scope));
                case ConfirmCondition _:
                    throw new InvalidOperationException("A user decision cannot be part of a synchronous condition.");
                default:
                    throw new InvalidOperationException("No lowering for condition " + condition.GetType().Name + ".");
            }
        }

        /// <summary>
        /// A condition that is one boolean expression: no user decision and no read that returns a
        /// promise, so it never waits and an `if` can evaluate it in place.
        /// </summary>
        internal bool IsSynchronous(ConditionGraph condition)
        {
            switch (condition)
            {
                case ConfirmCondition _:
                    return false;
                case AllCondition all:
                    return all.Terms.All(IsSynchronous);
                case AnyCondition any:
                    return any.Terms.All(IsSynchronous);
                case NotCondition not:
                    return IsSynchronous(not.Term);
                case CompareCondition compare:
                    return !ReadsAwaited(compare.Left)
                        && !(compare.RightOperand is PresentComparisonRightOperand right && ReadsAwaited(right.Value));
                default:
                    return true;
            }
        }

        private bool ReadsAwaited(ValueExpression value)
        {
            switch (value)
            {
                case ReadExpression read:
                    var arguments = read.Access is MethodValueReadAccess method ? method.Args : Array.Empty<ValueExpression>();
                    return IsAwaitedRead(read) || arguments.Any(ReadsAwaited);
                case ObjectExpression record:
                    return record.Fields.Values.Any(ReadsAwaited);
                case ArrayExpression array:
                    return array.Items.Any(ReadsAwaited);
                case ArrayOperationExpression operation:
                    return ReadsAwaited(operation.Source);
                default:
                    return false;
            }
        }

        private bool IsAwaitedRead(ReadExpression read) =>
            read.From is ComponentSource component
            && read.Access is MethodValueReadAccess
            && _context.IsDeclared(component.Component)
            && _context.Method(_context.ContractOf(component.Component), read.Member, component.Component).Signature.Awaits;

        // The left value is read, then converted to the condition's shape; a literal on the right is
        // written in that shape. A missing value stays missing on both sides (grammar.md 2.2).
        private Expr Compare(CompareCondition compare, LoweringScope scope)
        {
            var raw = Value(compare.Left, scope);
            var left = G.As(raw, compare.Shape);
            switch (compare.Op)
            {
                case CompareOp.Eq: return G.Eq(left, Right(compare, compare.Shape, scope));
                case CompareOp.Neq: return G.Op(Operator.Neq, left, Right(compare, compare.Shape, scope));
                case CompareOp.Gt: return G.Op(Operator.Gt, left, Right(compare, compare.Shape, scope));
                case CompareOp.Gte: return G.Op(Operator.Gte, left, Right(compare, compare.Shape, scope));
                case CompareOp.Lt: return G.Op(Operator.Lt, left, Right(compare, compare.Shape, scope));
                case CompareOp.Lte: return G.Op(Operator.Lte, left, Right(compare, compare.Shape, scope));
                case CompareOp.Truthy: return G.Op(Operator.Truthy, left);
                case CompareOp.Falsy: return G.Not(G.Op(Operator.Truthy, left));
                case CompareOp.IsNull: return G.Eq(raw, G.Null);
                case CompareOp.NotNull: return G.Op(Operator.Neq, raw, G.Null);
                case CompareOp.IsEmpty: return G.Op(Operator.Empty, raw);
                case CompareOp.NotEmpty: return G.Not(G.Op(Operator.Empty, raw));
                case CompareOp.In: return Membership(left, compare, scope);
                case CompareOp.NotIn: return G.Not(Membership(left, compare, scope));
                case CompareOp.Between: return Between(left, compare, scope);
                case CompareOp.ArrayContains: return ArrayContains(left, compare, scope);
                case CompareOp.Contains: return TextMethod(left, "includes", compare);
                case CompareOp.StartsWith: return TextMethod(left, "startsWith", compare);
                case CompareOp.EndsWith: return TextMethod(left, "endsWith", compare);
                case CompareOp.Matches:
                    return G.Op(Operator.Matches, left, G.Lit(LiteralText(compare)), G.Lit(""));
                case CompareOp.MinLength:
                    return G.Op(Operator.Gte, G.Get(G.Op(Operator.ToText, left), new[] { "length" }, Shape.None), G.Lit(LiteralNumber(compare)));
                default:
                    throw new InvalidOperationException("No lowering for comparison '" + compare.Op + "'.");
            }
        }

        private Expr Right(CompareCondition compare, Shape shape, LoweringScope scope)
        {
            var operand = RightOperand(compare);
            return operand is LiteralExpression literal
                ? G.Lit(literal.Value, shape)
                : G.As(Value(operand, scope), shape);
        }

        private static ValueExpression RightOperand(CompareCondition compare) =>
            compare.RightOperand is PresentComparisonRightOperand present
                ? present.Value
                : throw new InvalidOperationException("Comparison '" + compare.Op + "' needs a right operand.");

        private IReadOnlyList<Expr> RightItems(CompareCondition compare, Shape shape, LoweringScope scope)
        {
            var operand = RightOperand(compare);
            var items = operand is ArrayExpression array
                ? array.Items
                : throw new InvalidOperationException("Comparison '" + compare.Op + "' needs a list of values.");
            return items
                .Select(item => item is LiteralExpression literal ? G.Lit(literal.Value, shape) : G.As(Value(item, scope), shape))
                .ToList();
        }

        // The left value is read once and compared with each item, as C#'s Contains does.
        private Expr Membership(Expr left, CompareCondition compare, LoweringScope scope)
        {
            var value = _context.Fresh("v");
            var item = _context.Fresh("i");
            var items = G.List(RightItems(compare, compare.Shape, scope));
            var some = G.Call(items, new[] { "some" }, new Expr[] { G.Fn(G.Eq(G.Ref(item), G.Ref(value)), item) }, G.NoShapes, Shape.Boolean);
            return G.Apply(G.Fn(some, value), left);
        }

        private Expr Between(Expr left, CompareCondition compare, LoweringScope scope)
        {
            var bounds = RightItems(compare, compare.Shape, scope);
            if (bounds.Count != 2) throw new InvalidOperationException("Between needs a lower and an upper bound.");
            var value = _context.Fresh("v");
            var inRange = G.And(
                G.Op(Operator.Gte, G.Ref(value), bounds[0]),
                G.Op(Operator.Lte, G.Ref(value), bounds[1]));
            return G.Apply(G.Fn(inRange, value), left);
        }

        private Expr ArrayContains(Expr left, CompareCondition compare, LoweringScope scope)
        {
            var items = _context.Fresh("c");
            var item = _context.Fresh("i");
            var right = Right(compare, compare.ItemShape, scope);
            var matches = G.Fn(G.Eq(G.As(G.Ref(item), compare.ItemShape), right), item);
            var some = G.Call(G.Ref(items), new[] { "some" }, new Expr[] { matches }, G.NoShapes, Shape.None);
            return G.Apply(G.Fn(G.Op(Operator.Truthy, some), items), left);
        }

        private static Expr TextMethod(Expr left, string method, CompareCondition compare) =>
            G.Op(Operator.Truthy, G.Call(G.Op(Operator.ToText, left), new[] { method }, new Expr[] { G.Lit(LiteralText(compare)) }, G.NoShapes, Shape.None));

        private static string LiteralText(CompareCondition compare) =>
            RightOperand(compare) is LiteralExpression literal && literal.Value is string text
                ? text
                : throw new InvalidOperationException("Comparison '" + compare.Op + "' needs literal text.");

        private static object LiteralNumber(CompareCondition compare) =>
            RightOperand(compare) is LiteralExpression literal && literal.Value != null
                ? literal.Value
                : throw new InvalidOperationException("Comparison '" + compare.Op + "' needs a literal number.");
    }
}
