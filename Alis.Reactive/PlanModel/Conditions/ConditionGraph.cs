using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Grammar;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// A condition over values: a comparison, all of, any of, not, or a question the user answers. It
    /// writes itself as an expression where it is synchronous, and as the guard of an <c>if</c> where it
    /// waits: a question is asked, or a promise settles, only when the condition reaches it.
    /// </summary>
    internal abstract class ConditionGraph
    {
        private protected ConditionGraph() { }

        internal static ConditionGraph Unary(CompareOperator op, ValueExpression left, Shape shape) =>
            new UnaryCondition(op, left, shape);

        internal static ConditionGraph Binary(CompareOperator op, ValueExpression left, ValueExpression right, Shape shape) =>
            new BinaryCondition(op, left, right, shape, Shape.None);

        /// <summary>A collection compared with one item: <c>ArrayContains</c>.</summary>
        internal static ConditionGraph CollectionItem(CompareOperator op, ValueExpression left, ValueExpression item, Shape collectionShape, Shape itemShape) =>
            new BinaryCondition(op, left, item, collectionShape, itemShape);

        internal static ConditionGraph All(params ConditionGraph[] terms) => new AllCondition(terms.ToList());

        internal static ConditionGraph Any(params ConditionGraph[] terms) => new AnyCondition(terms.ToList());

        internal static ConditionGraph Not(ConditionGraph term) => new NotCondition(term);

        internal static ConditionGraph Confirm(string message) => new ConfirmCondition(message);

        internal virtual bool IsSynchronous => true;

        internal abstract Expr Write(Scope scope);

        /// <summary>The statements that run <paramref name="then"/> when the condition holds and <paramref name="otherwise"/> when not.</summary>
        internal virtual IReadOnlyList<Statement> Guard(IReadOnlyList<Statement> then, IReadOnlyList<Statement> otherwise, Scope scope) =>
            new[] { G.If(Write(scope), then, otherwise) };
    }

    /// <summary>A comparison with nothing on its right: truthy, null, empty and their negations.</summary>
    internal sealed class UnaryCondition : ConditionGraph
    {
        private readonly CompareOperator _op;
        private readonly ValueExpression _left;
        private readonly Shape _shape;

        internal UnaryCondition(CompareOperator op, ValueExpression left, Shape shape)
        {
            _op = op;
            _left = left ?? throw new ArgumentNullException(nameof(left));
            _shape = shape;
        }

        internal override bool IsSynchronous => !_left.ReadsAwaited;

        // The value is read, then converted to the condition's shape; null and empty test it unconverted.
        internal override Expr Write(Scope scope)
        {
            var raw = _left.Write(scope);
            var left = G.As(raw, _shape);
            if (_op == CompareOperator.Truthy) return G.Op(Operator.Truthy, left);
            if (_op == CompareOperator.Falsy) return G.Not(G.Op(Operator.Truthy, left));
            if (_op == CompareOperator.IsNull) return G.Eq(raw, G.Null);
            if (_op == CompareOperator.NotNull) return G.Op(Operator.Neq, raw, G.Null);
            if (_op == CompareOperator.IsEmpty) return G.Op(Operator.Empty, raw);
            if (_op == CompareOperator.NotEmpty) return G.Not(G.Op(Operator.Empty, raw));
            throw new InvalidOperationException("Comparison '" + _op + "' needs a right operand.");
        }

        // A comparison that reads a promise: its awaited reads settle, then the `if` runs.
        internal override IReadOnlyList<Statement> Guard(IReadOnlyList<Statement> then, IReadOnlyList<Statement> otherwise, Scope scope) =>
            IsSynchronous ? base.Guard(then, otherwise, scope) : scope.WithHoisted(() => base.Guard(then, otherwise, scope));
    }

    /// <summary>
    /// A comparison with a right operand: a value, a list (membership, a range), literal text (text tests,
    /// a pattern) or, for <c>ArrayContains</c>, one item of the collection on the left.
    /// </summary>
    internal sealed class BinaryCondition : ConditionGraph
    {
        private readonly CompareOperator _op;
        private readonly ValueExpression _left;
        private readonly ValueExpression _right;
        private readonly Shape _shape;
        private readonly Shape _itemShape;

        internal BinaryCondition(CompareOperator op, ValueExpression left, ValueExpression right, Shape shape, Shape itemShape)
        {
            _op = op;
            _left = left ?? throw new ArgumentNullException(nameof(left));
            _right = right ?? throw new ArgumentNullException(nameof(right));
            _shape = shape;
            _itemShape = itemShape;
        }

        internal override bool IsSynchronous => !_left.ReadsAwaited && !_right.ReadsAwaited;

        // The left value is read, then converted to the condition's shape; a literal on the right is written
        // in that shape. A missing value stays missing on both sides (grammar.md 2.2).
        internal override Expr Write(Scope scope)
        {
            var left = G.As(_left.Write(scope), _shape);
            if (_op == CompareOperator.Eq) return G.Eq(left, Right(_shape, scope));
            if (_op == CompareOperator.Neq) return G.Op(Operator.Neq, left, Right(_shape, scope));
            if (_op == CompareOperator.Gt) return G.Op(Operator.Gt, left, Right(_shape, scope));
            if (_op == CompareOperator.Gte) return G.Op(Operator.Gte, left, Right(_shape, scope));
            if (_op == CompareOperator.Lt) return G.Op(Operator.Lt, left, Right(_shape, scope));
            if (_op == CompareOperator.Lte) return G.Op(Operator.Lte, left, Right(_shape, scope));
            if (_op == CompareOperator.In) return Membership(left, scope);
            if (_op == CompareOperator.NotIn) return G.Not(Membership(left, scope));
            if (_op == CompareOperator.Between) return Between(left, scope);
            if (_op == CompareOperator.ArrayContains) return ArrayContains(left, scope);
            if (_op == CompareOperator.Contains) return TextMethod(left, "includes");
            if (_op == CompareOperator.StartsWith) return TextMethod(left, "startsWith");
            if (_op == CompareOperator.EndsWith) return TextMethod(left, "endsWith");
            if (_op == CompareOperator.Matches) return G.Op(Operator.Matches, left, G.Lit(LiteralText()), G.Lit(""));
            if (_op == CompareOperator.MinLength)
                return G.Op(Operator.Gte, G.Get(G.Op(Operator.ToText, left), new[] { "length" }, Shape.None), G.Lit(LiteralNumber()));
            throw new InvalidOperationException("Comparison '" + _op + "' takes no right operand.");
        }

        internal override IReadOnlyList<Statement> Guard(IReadOnlyList<Statement> then, IReadOnlyList<Statement> otherwise, Scope scope) =>
            IsSynchronous ? base.Guard(then, otherwise, scope) : scope.WithHoisted(() => base.Guard(then, otherwise, scope));

        private Expr Right(Shape shape, Scope scope) =>
            _right is LiteralExpression literal ? G.Lit(literal.Value, shape) : G.As(_right.Write(scope), shape);

        private IReadOnlyList<Expr> RightItems(Scope scope)
        {
            var items = _right is ArrayExpression array
                ? array.Items
                : throw new InvalidOperationException("Comparison '" + _op + "' needs a list of values.");
            return items
                .Select(item => item is LiteralExpression literal ? G.Lit(literal.Value, _shape) : G.As(item.Write(scope), _shape))
                .ToList();
        }

        // The left value is read once and compared with each item, as C#'s Contains does.
        private Expr Membership(Expr left, Scope scope)
        {
            var value = scope.Fresh("v");
            var item = scope.Fresh("i");
            var items = G.List(RightItems(scope));
            var some = G.Call(items, new[] { "some" }, new Expr[] { G.Fn(G.Eq(G.Ref(item), G.Ref(value)), item) }, G.NoShapes, Shape.Boolean);
            return G.Apply(G.Fn(some, value), left);
        }

        private Expr Between(Expr left, Scope scope)
        {
            var bounds = RightItems(scope);
            if (bounds.Count != 2) throw new InvalidOperationException("Between needs a lower and an upper bound.");
            var value = scope.Fresh("v");
            var inRange = G.And(
                G.Op(Operator.Gte, G.Ref(value), bounds[0]),
                G.Op(Operator.Lte, G.Ref(value), bounds[1]));
            return G.Apply(G.Fn(inRange, value), left);
        }

        private Expr ArrayContains(Expr left, Scope scope)
        {
            var items = scope.Fresh("c");
            var item = scope.Fresh("i");
            var right = Right(_itemShape, scope);
            var matches = G.Fn(G.Eq(G.As(G.Ref(item), _itemShape), right), item);
            var some = G.Call(G.Ref(items), new[] { "some" }, new Expr[] { matches }, G.NoShapes, Shape.None);
            return G.Apply(G.Fn(G.Op(Operator.Truthy, some), items), left);
        }

        private Expr TextMethod(Expr left, string method) =>
            G.Op(Operator.Truthy, G.Call(G.Op(Operator.ToText, left), new[] { method }, new Expr[] { G.Lit(LiteralText()) }, G.NoShapes, Shape.None));

        private string LiteralText() =>
            _right is LiteralExpression literal && literal.Value is string text
                ? text
                : throw new InvalidOperationException("Comparison '" + _op + "' needs literal text.");

        private object LiteralNumber() =>
            _right is LiteralExpression literal && literal.Value != null
                ? literal.Value
                : throw new InvalidOperationException("Comparison '" + _op + "' needs a literal number.");
    }

    internal sealed class AllCondition : ConditionGraph
    {
        internal AllCondition(IReadOnlyList<ConditionGraph> terms)
        {
            Terms = terms;
        }

        internal IReadOnlyList<ConditionGraph> Terms { get; }

        internal override bool IsSynchronous => Terms.All(term => term.IsSynchronous);

        internal override Expr Write(Scope scope) => G.And(Terms.Select(term => term.Write(scope)).ToList());

        internal override IReadOnlyList<Statement> Guard(IReadOnlyList<Statement> then, IReadOnlyList<Statement> otherwise, Scope scope) =>
            IsSynchronous ? base.Guard(then, otherwise, scope) : AllOf(0, then, otherwise, scope);

        // Consecutive synchronous terms share one `if`; a term that waits guards the rest.
        private IReadOnlyList<Statement> AllOf(int start, IReadOnlyList<Statement> then, IReadOnlyList<Statement> otherwise, Scope scope)
        {
            if (start >= Terms.Count) return then;
            var synchronous = Terms.Skip(start).TakeWhile(term => term.IsSynchronous).ToList();
            if (synchronous.Count > 0)
            {
                var guard = G.And(synchronous.Select(term => term.Write(scope)).ToList());
                return new[] { G.If(guard, AllOf(start + synchronous.Count, then, otherwise, scope), otherwise) };
            }

            return Terms[start].Guard(AllOf(start + 1, then, otherwise, scope), otherwise, scope);
        }
    }

    internal sealed class AnyCondition : ConditionGraph
    {
        internal AnyCondition(IReadOnlyList<ConditionGraph> terms)
        {
            Terms = terms;
        }

        internal IReadOnlyList<ConditionGraph> Terms { get; }

        internal override bool IsSynchronous => Terms.All(term => term.IsSynchronous);

        internal override Expr Write(Scope scope) => G.Or(Terms.Select(term => term.Write(scope)).ToList());

        internal override IReadOnlyList<Statement> Guard(IReadOnlyList<Statement> then, IReadOnlyList<Statement> otherwise, Scope scope) =>
            IsSynchronous ? base.Guard(then, otherwise, scope) : AnyOf(0, then, otherwise, scope);

        private IReadOnlyList<Statement> AnyOf(int start, IReadOnlyList<Statement> then, IReadOnlyList<Statement> otherwise, Scope scope)
        {
            if (start >= Terms.Count) return otherwise;
            var synchronous = Terms.Skip(start).TakeWhile(term => term.IsSynchronous).ToList();
            if (synchronous.Count > 0)
            {
                var guard = G.Or(synchronous.Select(term => term.Write(scope)).ToList());
                return new[] { G.If(guard, then, AnyOf(start + synchronous.Count, then, otherwise, scope)) };
            }

            return Terms[start].Guard(then, AnyOf(start + 1, then, otherwise, scope), scope);
        }
    }

    internal sealed class NotCondition : ConditionGraph
    {
        private readonly ConditionGraph _term;

        internal NotCondition(ConditionGraph term)
        {
            _term = term ?? throw new ArgumentNullException(nameof(term));
        }

        internal override bool IsSynchronous => _term.IsSynchronous;

        internal override Expr Write(Scope scope) => G.Not(_term.Write(scope));

        internal override IReadOnlyList<Statement> Guard(IReadOnlyList<Statement> then, IReadOnlyList<Statement> otherwise, Scope scope) =>
            IsSynchronous ? base.Guard(then, otherwise, scope) : _term.Guard(otherwise, then, scope);
    }

    /// <summary>A question the user answers in the confirm dialog: the steps after it wait for the answer.</summary>
    internal sealed class ConfirmCondition : ConditionGraph
    {
        private readonly string _message;

        internal ConfirmCondition(string message)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
        }

        internal override bool IsSynchronous => false;

        internal override Expr Write(Scope scope) =>
            throw new InvalidOperationException("A user decision cannot be part of a synchronous condition.");

        internal override IReadOnlyList<Statement> Guard(IReadOnlyList<Statement> then, IReadOnlyList<Statement> otherwise, Scope scope)
        {
            var accepted = scope.Fresh("ok");
            return new[]
            {
                G.Let(accepted, G.CallTyped(G.Obj(scope.Objects.Confirm()), "ask", Shape.Boolean, G.Lit(_message)), awaits: true),
                G.If(G.Ref(accepted), then, otherwise),
            };
        }
    }
}
