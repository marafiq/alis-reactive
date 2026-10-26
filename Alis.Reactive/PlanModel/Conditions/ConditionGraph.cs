using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Grammar;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// A condition as the plan evaluates it: one node per comparison family, each carrying exactly its
    /// operands, composed with All, Any and Not. A condition that waits (a method result that is a promise,
    /// a user decision) guards its branches with the statements that settle it first.
    /// </summary>
    internal abstract class ConditionGraph
    {
        private protected ConditionGraph() { }

        internal static ConditionGraph Unary(ValueTest test, ValueExpression left, Shape shape) =>
            new UnaryCondition(test, left, shape);

        internal static ConditionGraph Ordered(CompareOperator op, ValueExpression left, ValueExpression right, Shape shape) =>
            new OrderedCondition(op, left, right, shape);

        internal static ConditionGraph Membership(ValueExpression left, IReadOnlyList<ValueExpression> items, bool negated, Shape shape) =>
            new MembershipCondition(left, items, negated, shape);

        internal static ConditionGraph Range(ValueExpression left, ValueExpression low, ValueExpression high, Shape shape) =>
            new RangeCondition(left, low, high, shape);

        internal static ConditionGraph ArrayContains(ValueExpression left, ValueExpression item, Shape collectionShape, Shape itemShape) =>
            new ArrayContainsCondition(left, item, collectionShape, itemShape);

        internal static ConditionGraph Text(TextTest test, ValueExpression left, string text, Shape shape) =>
            new TextCondition(test, left, text, shape);

        internal static ConditionGraph MinLength(ValueExpression left, int length, Shape shape) =>
            new MinLengthCondition(left, length, shape);

        internal static ConditionGraph All(params ConditionGraph[] terms) => new AllCondition(terms.ToList());

        internal static ConditionGraph Any(params ConditionGraph[] terms) => new AnyCondition(terms.ToList());

        internal static ConditionGraph Not(ConditionGraph term) => new NotCondition(term);

        internal static ConditionGraph Confirm(string message) => new ConfirmCondition(message);

        internal virtual bool IsSynchronous => true;

        internal abstract Expr Write(Scope scope);

        internal virtual IReadOnlyList<Statement> Guard(IReadOnlyList<Statement> then, IReadOnlyList<Statement> otherwise, Scope scope) =>
            new[] { G.If(Write(scope), then, otherwise) };
    }

    /// <summary>A comparison of one value: it reads the value, converts it to the condition's shape, and tests it.</summary>
    internal abstract class ValueCondition : ConditionGraph
    {
        private readonly ValueExpression _left;

        private protected ValueCondition(ValueExpression left, Shape shape)
        {
            _left = left ?? throw new ArgumentNullException(nameof(left));
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        private protected Shape Shape { get; }

        private protected virtual bool ReadsAwaited => _left.ReadsAwaited;

        internal override bool IsSynchronous => !ReadsAwaited;

        private protected Expr Raw(Scope scope) => _left.Write(scope);

        private protected Expr Left(Scope scope) => G.As(Raw(scope), Shape);

        // A comparison that reads a promise: its awaited reads settle, then the `if` runs.
        internal override IReadOnlyList<Statement> Guard(IReadOnlyList<Statement> then, IReadOnlyList<Statement> otherwise, Scope scope) =>
            IsSynchronous ? base.Guard(then, otherwise, scope) : scope.WithHoisted(() => base.Guard(then, otherwise, scope));
    }

    internal sealed class UnaryCondition : ValueCondition
    {
        private readonly ValueTest _test;

        internal UnaryCondition(ValueTest test, ValueExpression left, Shape shape) : base(left, shape)
        {
            _test = test ?? throw new ArgumentNullException(nameof(test));
        }

        // Null and empty test the value unconverted; truthiness tests it in the condition's shape.
        internal override Expr Write(Scope scope)
        {
            var raw = Raw(scope);
            if (_test == ValueTest.Truthy) return G.Op(Operator.Truthy, G.As(raw, Shape));
            if (_test == ValueTest.Falsy) return G.Not(G.Op(Operator.Truthy, G.As(raw, Shape)));
            if (_test == ValueTest.IsNull) return G.Eq(raw, G.Null);
            if (_test == ValueTest.NotNull) return G.Op(Operator.Neq, raw, G.Null);
            if (_test == ValueTest.IsEmpty) return G.Op(Operator.Empty, raw);
            return G.Not(G.Op(Operator.Empty, raw));
        }
    }

    /// <summary>The value ordered against another of its shape, a literal or a value read when the condition runs.</summary>
    internal sealed class OrderedCondition : ValueCondition
    {
        private readonly CompareOperator _op;
        private readonly ValueExpression _right;

        internal OrderedCondition(CompareOperator op, ValueExpression left, ValueExpression right, Shape shape) : base(left, shape)
        {
            _op = op ?? throw new ArgumentNullException(nameof(op));
            _right = right ?? throw new ArgumentNullException(nameof(right));
        }

        private protected override bool ReadsAwaited => base.ReadsAwaited || _right.ReadsAwaited;

        internal override Expr Write(Scope scope) => G.Op(_op.Operation, Left(scope), _right.WriteAs(Shape, scope));
    }

    /// <summary>The value among a list of values, read once and compared with each, as C#'s Contains does.</summary>
    internal sealed class MembershipCondition : ValueCondition
    {
        private readonly IReadOnlyList<ValueExpression> _items;
        private readonly bool _negated;

        internal MembershipCondition(ValueExpression left, IReadOnlyList<ValueExpression> items, bool negated, Shape shape) : base(left, shape)
        {
            _items = items ?? throw new ArgumentNullException(nameof(items));
            _negated = negated;
        }

        private protected override bool ReadsAwaited => base.ReadsAwaited || _items.Any(item => item.ReadsAwaited);

        internal override Expr Write(Scope scope)
        {
            var left = Left(scope);
            var value = scope.Fresh("v");
            var item = scope.Fresh("i");
            var items = G.List(_items.Select(candidate => candidate.WriteAs(Shape, scope)).ToList());
            var some = G.Call(items, new[] { "some" }, new Expr[] { G.Fn(G.Eq(G.Ref(item), G.Ref(value)), item) }, G.NoShapes, Shape.Boolean);
            var among = G.Apply(G.Fn(some, value), left);
            return _negated ? G.Not(among) : among;
        }
    }

    /// <summary>The value between a lower and an upper bound, both included.</summary>
    internal sealed class RangeCondition : ValueCondition
    {
        private readonly ValueExpression _low;
        private readonly ValueExpression _high;

        internal RangeCondition(ValueExpression left, ValueExpression low, ValueExpression high, Shape shape) : base(left, shape)
        {
            _low = low ?? throw new ArgumentNullException(nameof(low));
            _high = high ?? throw new ArgumentNullException(nameof(high));
        }

        private protected override bool ReadsAwaited => base.ReadsAwaited || _low.ReadsAwaited || _high.ReadsAwaited;

        internal override Expr Write(Scope scope)
        {
            var left = Left(scope);
            var low = _low.WriteAs(Shape, scope);
            var high = _high.WriteAs(Shape, scope);
            var value = scope.Fresh("v");
            var inRange = G.And(G.Op(Operator.Gte, G.Ref(value), low), G.Op(Operator.Lte, G.Ref(value), high));
            return G.Apply(G.Fn(inRange, value), left);
        }
    }

    /// <summary>A list holding an item: each item converted to the item's shape and compared.</summary>
    internal sealed class ArrayContainsCondition : ValueCondition
    {
        private readonly ValueExpression _item;
        private readonly Shape _itemShape;

        internal ArrayContainsCondition(ValueExpression left, ValueExpression item, Shape collectionShape, Shape itemShape) : base(left, collectionShape)
        {
            _item = item ?? throw new ArgumentNullException(nameof(item));
            _itemShape = itemShape ?? throw new ArgumentNullException(nameof(itemShape));
        }

        private protected override bool ReadsAwaited => base.ReadsAwaited || _item.ReadsAwaited;

        internal override Expr Write(Scope scope)
        {
            var left = Left(scope);
            var items = scope.Fresh("c");
            var item = scope.Fresh("i");
            var matches = G.Fn(G.Eq(G.As(G.Ref(item), _itemShape), _item.WriteAs(_itemShape, scope)), item);
            var some = G.Call(G.Ref(items), new[] { "some" }, new Expr[] { matches }, G.NoShapes, Shape.None);
            return G.Apply(G.Fn(G.Op(Operator.Truthy, some), items), left);
        }
    }

    /// <summary>The value, as text, holding, starting with, ending with or matching the given text.</summary>
    internal sealed class TextCondition : ValueCondition
    {
        private readonly TextTest _test;
        private readonly string _text;

        internal TextCondition(TextTest test, ValueExpression left, string text, Shape shape) : base(left, shape)
        {
            _test = test ?? throw new ArgumentNullException(nameof(test));
            _text = text ?? throw new ArgumentNullException(nameof(text));
        }

        internal override Expr Write(Scope scope)
        {
            var left = Left(scope);
            if (_test.IsPattern) return G.Op(Operator.Matches, left, G.Lit(_text), G.Lit(""));
            return G.Op(Operator.Truthy, G.Call(G.Op(Operator.ToText, left), new[] { _test.Method }, new Expr[] { G.Lit(_text) }, G.NoShapes, Shape.None));
        }
    }

    /// <summary>The value, as text, at least so many characters long.</summary>
    internal sealed class MinLengthCondition : ValueCondition
    {
        private readonly int _length;

        internal MinLengthCondition(ValueExpression left, int length, Shape shape) : base(left, shape)
        {
            _length = length;
        }

        internal override Expr Write(Scope scope) =>
            G.Op(Operator.Gte, G.Get(G.Op(Operator.ToText, Left(scope)), new[] { "length" }, Shape.None), G.Lit(_length));
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

    /// <summary>A user decision: the confirm asks, and the branches wait for the answer.</summary>
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
