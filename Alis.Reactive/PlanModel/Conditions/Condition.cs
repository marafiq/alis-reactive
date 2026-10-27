using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Alis.Reactive.Grammar;
using Alis.Reactive.Validation;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// A condition as the plan evaluates it: one node per comparison family, each carrying exactly its
    /// operands, composed with All, Any and Not. A validator writes one over the members of its model
    /// (<c>Condition.Is(x =&gt; x.IsVeteran, ValueTest.Truthy)</c>), and the form binds each member to the
    /// input that holds it when the page describes its rules; a reaction writes one over the values the
    /// plan reads. A condition that waits (a method result that is a promise, a user decision) guards its
    /// branches with the statements that settle it first.
    /// </summary>
    public abstract class Condition
    {
        private protected Condition() { }

        /// <summary>No condition: it always holds, and a rule under it always applies.</summary>
        public static Condition Always { get; } = new TrueCondition();

        // ── over the members of a validator's model ──────────────────────────────

        /// <summary>Tests the member's value alone.</summary>
        /// <param name="member">The member, as a path from the model: <c>x =&gt; x.IsVeteran</c>.</param>
        /// <param name="test">The test.</param>
        public static Condition Is(LambdaExpression member, ValueTest test)
        {
            if (test == null) throw new ArgumentNullException(nameof(test));
            return new MemberTest(FieldReference.Of(member), (left, shape) => Unary(test, left, shape));
        }

        /// <summary>Compares the member with a value.</summary>
        /// <param name="member">The member, as a path from the model.</param>
        /// <param name="comparison">The comparison.</param>
        /// <param name="value">The value, a constant or a captured variable, read as the member's type: an enum compared through its number is its member.</param>
        public static Condition Compare(LambdaExpression member, Comparison comparison, Expression value)
        {
            if (comparison == null) throw new ArgumentNullException(nameof(comparison));
            var field = FieldReference.Of(member);
            var fixedValue = LiteralOf(field.Type, Expressions.Evaluate(value)
                ?? throw new ArgumentNullException(nameof(value), "Test a missing value with ValueTest.IsNull."));
            return new MemberTest(field, (left, shape) => Ordered(comparison, left, ValueExpression.LiteralFor(fixedValue, shape), shape));
        }

        /// <summary>The member is one of the values.</summary>
        /// <param name="member">The member, as a path from the model.</param>
        /// <param name="values">The values, a constant or captured collection, each read as the member's type.</param>
        public static Condition In(LambdaExpression member, Expression values)
        {
            var field = FieldReference.Of(member);
            var fixedValues = Expressions.Evaluate(values) as IEnumerable
                ?? throw new ArgumentException("The values must be a collection.", nameof(values));
            var members = fixedValues.Cast<object?>().Select(value => LiteralOf(field.Type, value)).ToArray();
            return new MemberTest(field, (left, shape) =>
                Membership(left, members.Select(value => ValueExpression.LiteralFor(value, shape)).ToList(), negated: false, shape));
        }

        /// <summary>Tests a text member against fixed text.</summary>
        /// <param name="member">The text member, as a path from the model.</param>
        /// <param name="test">The test.</param>
        /// <param name="text">The text, or the pattern for <see cref="TextTest.Matches"/>.</param>
        public static Condition Text(LambdaExpression member, TextTest test, string text)
        {
            if (test == null) throw new ArgumentNullException(nameof(test));
            if (text == null) throw new ArgumentNullException(nameof(text));
            return new MemberTest(FieldReference.Of(member), (left, shape) => Text(test, left, text, shape));
        }

        /// <summary>The collection member holds the item.</summary>
        /// <param name="collection">The collection member, as a path from the model.</param>
        /// <param name="item">The item, a constant or a captured variable, read as its own type.</param>
        public static Condition Holds(LambdaExpression collection, Expression item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            var itemType = Expressions.TypeOf(item);
            var itemShape = Shape.FromClrType(itemType);
            var fixedItem = LiteralOf(itemType, Expressions.Evaluate(item));
            return new MemberTest(FieldReference.Of(collection), (left, shape) =>
                ArrayContains(left, ValueExpression.LiteralFor(fixedItem, itemShape), shape, itemShape));
        }

        /// <summary>Every term holds.</summary>
        /// <param name="terms">The terms, at least one.</param>
        public static Condition All(params Condition[] terms) => new AllCondition(Required(terms));

        /// <summary>At least one term holds.</summary>
        /// <param name="terms">The terms, at least one.</param>
        public static Condition Any(params Condition[] terms) => new AnyCondition(Required(terms));

        /// <summary>The term does not hold.</summary>
        /// <param name="term">The term.</param>
        public static Condition Not(Condition term) => new NotCondition(term ?? throw new ArgumentNullException(nameof(term)));

        // ── over the values a plan reads ─────────────────────────────────────────

        internal static Condition Unary(ValueTest test, ValueExpression left, Shape shape) =>
            new UnaryCondition(test, left, shape);

        internal static Condition Ordered(Comparison comparison, ValueExpression left, ValueExpression right, Shape shape) =>
            new OrderedCondition(comparison, left, right, shape);

        internal static Condition Membership(ValueExpression left, IReadOnlyList<ValueExpression> items, bool negated, Shape shape) =>
            new MembershipCondition(left, items, negated, shape);

        internal static Condition Range(ValueExpression left, ValueExpression low, ValueExpression high, Shape shape) =>
            new RangeCondition(left, low, high, shape);

        internal static Condition ArrayContains(ValueExpression left, ValueExpression item, Shape collectionShape, Shape itemShape) =>
            new ArrayContainsCondition(left, item, collectionShape, itemShape);

        internal static Condition Text(TextTest test, ValueExpression left, string text, Shape shape) =>
            new TextCondition(test, left, text, shape);

        internal static Condition MinLength(ValueExpression left, int length, Shape shape) =>
            new MinLengthCondition(left, length, shape);

        internal static Condition Confirm(string message) => new ConfirmCondition(message);

        /// <summary>Both conditions: the outer one, then this one.</summary>
        internal static Condition Combine(Condition outer, Condition inner)
        {
            if (outer.IsAlways) return inner;
            if (inner.IsAlways) return outer;
            return new AllCondition(new[] { outer, inner });
        }

        internal virtual bool IsAlways => false;

        /// <summary>The members of the validator's model the condition reads, left to right.</summary>
        internal virtual IEnumerable<FieldReference> Fields => Array.Empty<FieldReference>();

        /// <summary>The condition with every member it reads relocated under <paramref name="prefix"/>.</summary>
        internal virtual Condition PrefixedBy(string prefix) => this;

        /// <summary>The condition over the inputs that hold the members it reads.</summary>
        internal virtual Condition Bound(FormFieldValues fields) => this;

        internal virtual bool IsSynchronous => true;

        internal abstract Expr Write(Scope scope);

        internal virtual IReadOnlyList<Statement> Guard(IReadOnlyList<Statement> then, IReadOnlyList<Statement> otherwise, Scope scope) =>
            new[] { G.If(Write(scope), then, otherwise) };

        private static IReadOnlyList<Condition> Required(Condition[] terms)
        {
            if (terms == null) throw new ArgumentNullException(nameof(terms));
            if (terms.Length == 0) throw new ArgumentException("A condition over terms needs at least one.", nameof(terms));
            if (terms.Any(term => term == null)) throw new ArgumentException("A condition term must not be null.", nameof(terms));
            return terms.ToArray();
        }

        // A value the condition fixes, as the member's own type: an enum is its member's name, which is
        // how MVC binds it; anything else is written as it is.
        private static object? LiteralOf(Type memberType, object? value)
        {
            if (value == null) return null;

            var enumType = System.Nullable.GetUnderlyingType(memberType) ?? memberType;
            var asMemberType = enumType.IsEnum && !value.GetType().IsEnum ? Enum.ToObject(enumType, value) : value;
            return asMemberType.GetType().IsEnum ? asMemberType.ToString() : asMemberType;
        }
    }

    /// <summary>
    /// A test of one member of a validator's model. When the form binds it, the member is read as MVC
    /// binds it and tested in the shape its input holds, by the condition the test builds for that read.
    /// </summary>
    internal sealed class MemberTest : Condition
    {
        private readonly FieldReference _field;
        private readonly Func<ValueExpression, Shape, Condition> _test;

        internal MemberTest(FieldReference field, Func<ValueExpression, Shape, Condition> test)
        {
            _field = field ?? throw new ArgumentNullException(nameof(field));
            _test = test ?? throw new ArgumentNullException(nameof(test));
        }

        internal override IEnumerable<FieldReference> Fields => new[] { _field };

        internal override Condition PrefixedBy(string prefix) => new MemberTest(_field.PrefixedBy(prefix), _test);

        internal override Condition Bound(FormFieldValues fields)
        {
            var input = fields.Resolve(_field);
            return _test(ValueExpression.ReadBound(input.Id, Shape.None), input.Shape);
        }

        internal override Expr Write(Scope scope) =>
            throw new InvalidOperationException($"The test of '{_field.Path}' is bound to the form's inputs before it is written.");
    }

    /// <summary>A comparison of one value: it reads the value, converts it to the condition's shape, and tests it.</summary>
    internal abstract class ValueCondition : Condition
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
        private readonly Comparison _comparison;
        private readonly ValueExpression _right;

        internal OrderedCondition(Comparison comparison, ValueExpression left, ValueExpression right, Shape shape) : base(left, shape)
        {
            _comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));
            _right = right ?? throw new ArgumentNullException(nameof(right));
        }

        private protected override bool ReadsAwaited => base.ReadsAwaited || _right.ReadsAwaited;

        internal override Expr Write(Scope scope) => G.Op(_comparison.Operation, Left(scope), _right.WriteAs(Shape, scope));
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
            var some = G.Call(items, new[] { "some" }, new Expr[] { G.Fn(G.Eq(G.Ref(item), G.Ref(value)), item) }, Shape.Boolean);
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
            var some = G.Call(G.Ref(items), new[] { "some" }, new Expr[] { matches }, Shape.None);
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
            return G.Op(Operator.Truthy, G.Call(G.Op(Operator.ToText, left), new[] { _test.Method }, new Expr[] { G.Lit(_text) }, Shape.None));
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

    internal sealed class TrueCondition : Condition
    {
        internal override bool IsAlways => true;

        internal override Expr Write(Scope scope) => G.True;
    }

    internal sealed class AllCondition : Condition
    {
        internal AllCondition(IReadOnlyList<Condition> terms)
        {
            Terms = terms;
        }

        internal IReadOnlyList<Condition> Terms { get; }

        internal override IEnumerable<FieldReference> Fields => Terms.SelectMany(term => term.Fields);

        internal override Condition PrefixedBy(string prefix) => new AllCondition(Terms.Select(term => term.PrefixedBy(prefix)).ToList());

        internal override Condition Bound(FormFieldValues fields) => new AllCondition(Terms.Select(term => term.Bound(fields)).ToList());

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

    internal sealed class AnyCondition : Condition
    {
        internal AnyCondition(IReadOnlyList<Condition> terms)
        {
            Terms = terms;
        }

        internal IReadOnlyList<Condition> Terms { get; }

        internal override IEnumerable<FieldReference> Fields => Terms.SelectMany(term => term.Fields);

        internal override Condition PrefixedBy(string prefix) => new AnyCondition(Terms.Select(term => term.PrefixedBy(prefix)).ToList());

        internal override Condition Bound(FormFieldValues fields) => new AnyCondition(Terms.Select(term => term.Bound(fields)).ToList());

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

    internal sealed class NotCondition : Condition
    {
        private readonly Condition _term;

        internal NotCondition(Condition term)
        {
            _term = term ?? throw new ArgumentNullException(nameof(term));
        }

        internal override IEnumerable<FieldReference> Fields => _term.Fields;

        internal override Condition PrefixedBy(string prefix) => new NotCondition(_term.PrefixedBy(prefix));

        internal override Condition Bound(FormFieldValues fields) => new NotCondition(_term.Bound(fields));

        internal override bool IsSynchronous => _term.IsSynchronous;

        internal override Expr Write(Scope scope) => G.Not(_term.Write(scope));

        internal override IReadOnlyList<Statement> Guard(IReadOnlyList<Statement> then, IReadOnlyList<Statement> otherwise, Scope scope) =>
            IsSynchronous ? base.Guard(then, otherwise, scope) : _term.Guard(otherwise, then, scope);
    }

    /// <summary>A user decision: the confirm asks, and the branches wait for the answer.</summary>
    internal sealed class ConfirmCondition : Condition
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
                G.Let(accepted, G.Call(G.Obj(scope.Objects.Confirm()), "ask", G.Lit(_message)), awaits: true),
                G.If(G.Ref(accepted), then, otherwise),
            };
        }
    }
}
