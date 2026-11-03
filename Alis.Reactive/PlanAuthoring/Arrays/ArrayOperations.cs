using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel.Values;
using Alis.Reactive.PlanModel.Writing;
using Alis.Reactive.PlanModel.Conditions;

namespace Alis.Reactive.PlanAuthoring.Arrays
{
    internal sealed class HoldsExpression : ValueExpression
    {
        private readonly Condition _condition;

        internal HoldsExpression(Condition condition)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        internal override Shape OutputShape => Shape.Boolean;

        internal override Expr Write(Scope scope) => _condition.Write(scope);
    }

    /// <summary>
    /// An operation over a list, written as JavaScript's own method with a lambda: the source is read as
    /// an array, and the lambdas run per item with hoisting off, so a method result read inside one is
    /// refused where it is written.
    /// </summary>
    internal abstract class ArrayOperation : ValueExpression
    {
        internal static ValueExpression Count(ValueExpression source) => new CountOperation(source);

        internal static ValueExpression Filter(ValueExpression source, Condition predicate, Shape itemShape) =>
            new FilterOperation(source, predicate, Shape.ArrayOf(itemShape));

        internal static ValueExpression Map(ValueExpression source, ValueExpression projection, Shape resultItemShape) =>
            new MapOperation(source, projection, Shape.ArrayOf(resultItemShape));

        internal static ValueExpression Sum(ValueExpression source, ValueExpression projection) =>
            new SumOperation(source, projection);

        internal static ValueExpression Any(ValueExpression source) => new HasItemsOperation(source);

        internal static ValueExpression Any(ValueExpression source, Condition predicate) =>
            new SomeOperation(source, predicate);

        internal static ValueExpression Every(ValueExpression source, Condition predicate) =>
            new EveryOperation(source, predicate);

        internal static ValueExpression Find(ValueExpression source, Condition predicate, Shape itemShape) =>
            new FindOperation(source, predicate, itemShape);

        internal static ValueExpression Find(
            ValueExpression source, Condition predicate, ValueExpression projection, Shape resultShape) =>
            new FindSelectOperation(source, predicate, projection, resultShape);

        internal static ValueExpression OrderBy(ValueExpression source, ValueExpression key, Shape itemShape, bool descending) =>
            new OrderByOperation(source, key, Shape.ArrayOf(itemShape), descending);

        private readonly ValueExpression _source;
        private readonly Shape _shape;

        private protected ArrayOperation(ValueExpression source, Shape shape)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal override Shape OutputShape => _shape;

        internal override bool ReadsAwaited => _source.ReadsAwaited;

        internal override Expr Write(Scope scope)
        {
            var items = G.Op(Operator.ToArray, _source.Write(scope));
            return Operate(items, scope);
        }

        private protected abstract Expr Operate(Expr items, Scope scope);

        private protected static FnExpr PerItem(Condition predicate, Scope scope)
        {
            var item = scope.Fresh("x");
            return G.Fn(predicate.Write(scope.WithItem(item)), item);
        }

        private protected static FnExpr PerItem(ValueExpression projection, Scope scope)
        {
            var item = scope.Fresh("x");
            return G.Fn(projection.Write(scope.WithItem(item)), item);
        }

        private protected static Expr Length(Expr items) => G.Get(items, new[] { "length" }, Shape.Number);
    }

    internal sealed class CountOperation : ArrayOperation
    {
        internal CountOperation(ValueExpression source) : base(source, Shape.Number) { }

        private protected override Expr Operate(Expr items, Scope scope) => Length(items);
    }

    internal sealed class FilterOperation : ArrayOperation
    {
        private readonly Condition _predicate;

        internal FilterOperation(ValueExpression source, Condition predicate, Shape shape) : base(source, shape)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        private protected override Expr Operate(Expr items, Scope scope) =>
            G.Call(items, new[] { "filter" }, new Expr[] { PerItem(_predicate, scope) }, OutputShape);
    }

    internal sealed class MapOperation : ArrayOperation
    {
        private readonly ValueExpression _projection;

        internal MapOperation(ValueExpression source, ValueExpression projection, Shape shape) : base(source, shape)
        {
            _projection = projection ?? throw new ArgumentNullException(nameof(projection));
        }

        private protected override Expr Operate(Expr items, Scope scope) =>
            G.Call(items, new[] { "map" }, new Expr[] { PerItem(_projection, scope) }, OutputShape);
    }

    /// <summary>Sum adds each selected value as a finite number, so text never concatenates.</summary>
    internal sealed class SumOperation : ArrayOperation
    {
        private readonly ValueExpression _projection;

        internal SumOperation(ValueExpression source, ValueExpression projection) : base(source, Shape.Number)
        {
            _projection = projection ?? throw new ArgumentNullException(nameof(projection));
        }

        private protected override Expr Operate(Expr items, Scope scope)
        {
            var total = scope.Fresh("s");
            var item = scope.Fresh("x");
            var term = _projection.Write(scope.WithItem(item));
            var add = G.Fn(G.Op(Operator.Add, G.Ref(total), G.Op(Operator.Finite, term)), total, item);
            return G.Call(items, new[] { "reduce" }, new Expr[] { add, G.Lit(0) }, Shape.Number);
        }
    }

    internal sealed class HasItemsOperation : ArrayOperation
    {
        internal HasItemsOperation(ValueExpression source) : base(source, Shape.Boolean) { }

        private protected override Expr Operate(Expr items, Scope scope) => G.Op(Operator.Gt, Length(items), G.Lit(0));
    }

    internal sealed class SomeOperation : ArrayOperation
    {
        private readonly Condition _predicate;

        internal SomeOperation(ValueExpression source, Condition predicate) : base(source, Shape.Boolean)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        private protected override Expr Operate(Expr items, Scope scope) =>
            G.Call(items, new[] { "some" }, new Expr[] { PerItem(_predicate, scope) }, Shape.Boolean);
    }

    internal sealed class EveryOperation : ArrayOperation
    {
        private readonly Condition _predicate;

        internal EveryOperation(ValueExpression source, Condition predicate) : base(source, Shape.Boolean)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        private protected override Expr Operate(Expr items, Scope scope) =>
            G.Call(items, new[] { "every" }, new Expr[] { PerItem(_predicate, scope) }, Shape.Boolean);
    }

    /// <summary>Find gives null when nothing matches.</summary>
    internal sealed class FindOperation : ArrayOperation
    {
        private readonly Condition _predicate;

        internal FindOperation(ValueExpression source, Condition predicate, Shape itemShape) : base(source, itemShape)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        private protected override Expr Operate(Expr items, Scope scope) =>
            G.As(Found(items, _predicate, scope), OutputShape);

        internal static Expr Found(Expr items, Condition predicate, Scope scope) =>
            G.Op(Operator.Coalesce, G.Call(items, new[] { "find" }, new Expr[] { PerItem(predicate, scope) }, Shape.Any), G.Null);
    }

    /// <summary>Find gives null when nothing matches, then applies the selector to what it found.</summary>
    internal sealed class FindSelectOperation : ArrayOperation
    {
        private readonly Condition _predicate;
        private readonly ValueExpression _projection;

        internal FindSelectOperation(ValueExpression source, Condition predicate, ValueExpression projection, Shape resultShape)
            : base(source, resultShape)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            _projection = projection ?? throw new ArgumentNullException(nameof(projection));
        }

        private protected override Expr Operate(Expr items, Scope scope)
        {
            var found = FindOperation.Found(items, _predicate, scope);
            var match = scope.Fresh("x");
            var selected = _projection.Write(scope.WithItem(match));
            var select = G.Fn(G.Op(Operator.Cond, G.Eq(G.Ref(match), G.Null), G.Null, selected), match);
            return G.As(G.Apply(select, found), OutputShape);
        }
    }

    internal sealed class OrderByOperation : ArrayOperation
    {
        private readonly ValueExpression _key;
        private readonly bool _descending;

        internal OrderByOperation(ValueExpression source, ValueExpression key, Shape shape, bool descending) : base(source, shape)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _descending = descending;
        }

        private protected override Expr Operate(Expr items, Scope scope)
        {
            var left = scope.Fresh("a");
            var right = scope.Fresh("b");
            Expr compare = G.Op(Operator.Order, _key.Write(scope.WithItem(left)), _key.Write(scope.WithItem(right)));
            if (_descending) compare = G.Op(Operator.Neg, compare);
            return G.Call(items, new[] { "toSorted" }, new Expr[] { G.Fn(compare, left, right) }, OutputShape);
        }
    }
}
