using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>A comparison of a member with a value the condition fixes.</summary>
    public enum ClientComparison
    {
        /// <summary>The member equals the value.</summary>
        Equal,

        /// <summary>The member differs from the value.</summary>
        NotEqual,

        /// <summary>The member is greater than the value.</summary>
        GreaterThan,

        /// <summary>The member is at least the value.</summary>
        GreaterThanOrEqual,

        /// <summary>The member is less than the value.</summary>
        LessThan,

        /// <summary>The member is at most the value.</summary>
        LessThanOrEqual,
    }

    /// <summary>A test of a member's value alone.</summary>
    public enum ClientValueTest
    {
        /// <summary>The value is truthy: not null, false, zero or empty.</summary>
        Truthy,

        /// <summary>The value is falsy.</summary>
        Falsy,

        /// <summary>Nothing is bound.</summary>
        Null,

        /// <summary>A value is bound.</summary>
        NotNull,

        /// <summary>The value is null, empty text or an empty list.</summary>
        Empty,

        /// <summary>The value is not empty.</summary>
        NotEmpty,
    }

    /// <summary>A test of a text member against text the condition fixes.</summary>
    public enum ClientTextTest
    {
        /// <summary>The member contains the text.</summary>
        Contains,

        /// <summary>The member starts with the text.</summary>
        StartsWith,

        /// <summary>The member ends with the text.</summary>
        EndsWith,

        /// <summary>The member matches the text as a regular expression.</summary>
        Matches,
    }

    /// <summary>
    /// A condition a browser rule applies under: a test of the model's members against values the
    /// condition fixes, joined by all-of, any-of and not. A member is named as a path from the model,
    /// <c>x =&gt; x.Address.City</c>, and typed by its C# type.
    /// </summary>
    /// <remarks>
    /// A validator builds one from a rule's own condition: <c>ReactiveValidator</c>'s
    /// <c>When(x =&gt; x.IsVeteran)</c> is <c>ClientCondition.Is(x =&gt; x.IsVeteran, ClientValueTest.Truthy)</c>.
    /// </remarks>
    public abstract class ClientCondition
    {
        private protected ClientCondition() { }

        /// <summary>No condition: the rule always applies.</summary>
        public static ClientCondition Always { get; } = new AlwaysCondition();

        /// <summary>Tests the member's value alone.</summary>
        /// <param name="member">The member, as a path from the model: <c>x =&gt; x.IsVeteran</c>.</param>
        /// <param name="test">The test.</param>
        public static ClientCondition Is(LambdaExpression member, ClientValueTest test) =>
            new FieldTest(FieldReference.Of(member), UnaryOperator(test), TestValue.None);

        /// <summary>Compares the member with a value.</summary>
        /// <param name="member">The member, as a path from the model.</param>
        /// <param name="comparison">The comparison.</param>
        /// <param name="value">The value, a constant or a captured variable, read as the member's type: an enum compared through its number is its member.</param>
        public static ClientCondition Compare(LambdaExpression member, ClientComparison comparison, Expression value)
        {
            var field = FieldReference.Of(member);
            var fixedValue = ConditionValue.Evaluate(value)
                ?? throw new ArgumentNullException(nameof(value), "Test a missing value with ClientValueTest.Null.");
            return new FieldTest(field, ComparisonOperator(comparison), TestValue.Literal(LiteralOf(field.Type, fixedValue)));
        }

        /// <summary>The member is one of the values.</summary>
        /// <param name="member">The member, as a path from the model.</param>
        /// <param name="values">The values, a constant or captured collection, each read as the member's type.</param>
        public static ClientCondition In(LambdaExpression member, Expression values)
        {
            var field = FieldReference.Of(member);
            var fixedValues = ConditionValue.Evaluate(values) as IEnumerable
                ?? throw new ArgumentException("The values must be a collection.", nameof(values));
            return new FieldTest(
                field,
                CompareOperator.In,
                TestValue.List(fixedValues.Cast<object?>().Select(value => LiteralOf(field.Type, value)).ToArray()));
        }

        /// <summary>Tests a text member against fixed text.</summary>
        /// <param name="member">The text member, as a path from the model.</param>
        /// <param name="test">The test.</param>
        /// <param name="text">The text, or the pattern for <see cref="ClientTextTest.Matches"/>.</param>
        public static ClientCondition Text(LambdaExpression member, ClientTextTest test, string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            return new FieldTest(FieldReference.Of(member), TextOperator(test), TestValue.Text(text));
        }

        /// <summary>The collection member holds the item.</summary>
        /// <param name="collection">The collection member, as a path from the model.</param>
        /// <param name="item">The item, a constant or a captured variable, read as its own type.</param>
        public static ClientCondition Holds(LambdaExpression collection, Expression item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            var itemType = ConditionValue.TypeOf(item);
            return new FieldTest(
                FieldReference.Of(collection),
                CompareOperator.ArrayContains,
                TestValue.Item(LiteralOf(itemType, ConditionValue.Evaluate(item)), Shape.FromClrType(itemType)));
        }

        /// <summary>Every term holds.</summary>
        /// <param name="terms">The terms, at least two.</param>
        public static ClientCondition All(params ClientCondition[] terms) => new AllTerms(Required(terms));

        /// <summary>At least one term holds.</summary>
        /// <param name="terms">The terms, at least two.</param>
        public static ClientCondition Any(params ClientCondition[] terms) => new AnyTerms(Required(terms));

        /// <summary>The term does not hold.</summary>
        /// <param name="term">The term.</param>
        public static ClientCondition Not(ClientCondition term) =>
            new NotTerm(term ?? throw new ArgumentNullException(nameof(term)));

        internal virtual bool IsAlways => false;

        /// <summary>Both conditions: the outer one, then this one.</summary>
        internal static ClientCondition Combine(ClientCondition outer, ClientCondition inner)
        {
            if (outer.IsAlways) return inner;
            if (inner.IsAlways) return outer;
            return new AllTerms(new[] { outer, inner });
        }

        /// <summary>The condition with every member it reads relocated under <paramref name="prefix"/>.</summary>
        internal abstract ClientCondition PrefixedBy(string prefix);

        /// <summary>The members the condition reads, left to right.</summary>
        internal abstract IEnumerable<FieldReference> Fields { get; }

        /// <summary>The condition over the inputs that hold the members it reads.</summary>
        internal abstract ConditionGraph ToGraph(FormFieldValues fields);

        private static IReadOnlyList<ClientCondition> Required(ClientCondition[] terms)
        {
            if (terms == null) throw new ArgumentNullException(nameof(terms));
            if (terms.Any(term => term == null)) throw new ArgumentException("A condition term must not be null.", nameof(terms));
            return terms.ToArray();
        }

        // A value the condition fixes, as the member's own type: an enum is its member's name, which is
        // how MVC binds it; a date is its ISO text.
        private static object? LiteralOf(Type memberType, object? value)
        {
            if (value == null) return null;

            var enumType = System.Nullable.GetUnderlyingType(memberType) ?? memberType;
            var asMemberType = enumType.IsEnum && !value.GetType().IsEnum ? Enum.ToObject(enumType, value) : value;
            return asMemberType.GetType().IsEnum
                ? asMemberType.ToString()
                : ValidationDateLiteral.From(asMemberType, Shape.FromClrType(asMemberType.GetType()));
        }

        private static CompareOperator UnaryOperator(ClientValueTest test) => test switch
        {
            ClientValueTest.Truthy => CompareOperator.Truthy,
            ClientValueTest.Falsy => CompareOperator.Falsy,
            ClientValueTest.Null => CompareOperator.IsNull,
            ClientValueTest.NotNull => CompareOperator.NotNull,
            ClientValueTest.Empty => CompareOperator.IsEmpty,
            ClientValueTest.NotEmpty => CompareOperator.NotEmpty,
            _ => throw new ArgumentOutOfRangeException(nameof(test), test, "Unknown value test."),
        };

        private static CompareOperator ComparisonOperator(ClientComparison comparison) => comparison switch
        {
            ClientComparison.Equal => CompareOperator.Eq,
            ClientComparison.NotEqual => CompareOperator.Neq,
            ClientComparison.GreaterThan => CompareOperator.Gt,
            ClientComparison.GreaterThanOrEqual => CompareOperator.Gte,
            ClientComparison.LessThan => CompareOperator.Lt,
            ClientComparison.LessThanOrEqual => CompareOperator.Lte,
            _ => throw new ArgumentOutOfRangeException(nameof(comparison), comparison, "Unknown comparison."),
        };

        private static CompareOperator TextOperator(ClientTextTest test) => test switch
        {
            ClientTextTest.Contains => CompareOperator.Contains,
            ClientTextTest.StartsWith => CompareOperator.StartsWith,
            ClientTextTest.EndsWith => CompareOperator.EndsWith,
            ClientTextTest.Matches => CompareOperator.Matches,
            _ => throw new ArgumentOutOfRangeException(nameof(test), test, "Unknown text test."),
        };

        private sealed class AlwaysCondition : ClientCondition
        {
            internal override bool IsAlways => true;

            internal override ClientCondition PrefixedBy(string prefix) => this;

            internal override IEnumerable<FieldReference> Fields => Array.Empty<FieldReference>();

            internal override ConditionGraph ToGraph(FormFieldValues fields) =>
                throw new InvalidOperationException("A rule that always applies has no condition.");
        }

        private sealed class FieldTest : ClientCondition
        {
            private readonly FieldReference _field;
            private readonly CompareOperator _op;
            private readonly TestValue _value;

            internal FieldTest(FieldReference field, CompareOperator op, TestValue value)
            {
                _field = field;
                _op = op;
                _value = value;
            }

            internal override ClientCondition PrefixedBy(string prefix) =>
                new FieldTest(_field.PrefixedBy(prefix), _op, _value);

            internal override IEnumerable<FieldReference> Fields => new[] { _field };

            // The member is read as MVC binds it, then tested in the type its input holds.
            internal override ConditionGraph ToGraph(FormFieldValues fields)
            {
                var input = fields.Resolve(_field);
                return _value.Compare(_op, ValueExpression.ReadBound(input.Id, Shape.None), input.Shape);
            }
        }

        private sealed class AllTerms : ClientCondition
        {
            private readonly IReadOnlyList<ClientCondition> _terms;

            internal AllTerms(IReadOnlyList<ClientCondition> terms)
            {
                _terms = terms;
            }

            internal override ClientCondition PrefixedBy(string prefix) =>
                new AllTerms(_terms.Select(term => term.PrefixedBy(prefix)).ToArray());

            internal override IEnumerable<FieldReference> Fields => _terms.SelectMany(term => term.Fields);

            internal override ConditionGraph ToGraph(FormFieldValues fields) =>
                ConditionGraph.All(_terms.Select(term => term.ToGraph(fields)).ToArray());
        }

        private sealed class AnyTerms : ClientCondition
        {
            private readonly IReadOnlyList<ClientCondition> _terms;

            internal AnyTerms(IReadOnlyList<ClientCondition> terms)
            {
                _terms = terms;
            }

            internal override ClientCondition PrefixedBy(string prefix) =>
                new AnyTerms(_terms.Select(term => term.PrefixedBy(prefix)).ToArray());

            internal override IEnumerable<FieldReference> Fields => _terms.SelectMany(term => term.Fields);

            internal override ConditionGraph ToGraph(FormFieldValues fields) =>
                ConditionGraph.Any(_terms.Select(term => term.ToGraph(fields)).ToArray());
        }

        private sealed class NotTerm : ClientCondition
        {
            private readonly ClientCondition _term;

            internal NotTerm(ClientCondition term)
            {
                _term = term;
            }

            internal override ClientCondition PrefixedBy(string prefix) => new NotTerm(_term.PrefixedBy(prefix));

            internal override IEnumerable<FieldReference> Fields => _term.Fields;

            internal override ConditionGraph ToGraph(FormFieldValues fields) => ConditionGraph.Not(_term.ToGraph(fields));
        }

        // What a test compares the member with: nothing, a literal in the member's type, text, a list
        // of literals, or one item of the member's collection.
        private abstract class TestValue
        {
            internal static TestValue None { get; } = new NoValue();

            internal static TestValue Literal(object? value) => new LiteralValue(value);

            internal static TestValue Text(string text) => new TextValue(text);

            internal static TestValue List(IReadOnlyList<object?> items) => new ListValue(items);

            internal static TestValue Item(object? item, Shape itemShape) => new ItemValue(item, itemShape);

            internal abstract ConditionGraph Compare(CompareOperator op, ValueExpression left, Shape fieldShape);

            private sealed class NoValue : TestValue
            {
                internal override ConditionGraph Compare(CompareOperator op, ValueExpression left, Shape fieldShape) =>
                    ConditionGraph.Unary(op, left, fieldShape);
            }

            private sealed class LiteralValue : TestValue
            {
                private readonly object? _value;

                internal LiteralValue(object? value)
                {
                    _value = value;
                }

                internal override ConditionGraph Compare(CompareOperator op, ValueExpression left, Shape fieldShape) =>
                    ConditionGraph.Binary(op, left, ValueExpression.LiteralRaw(_value, fieldShape), fieldShape);
            }

            private sealed class TextValue : TestValue
            {
                private readonly string _text;

                internal TextValue(string text)
                {
                    _text = text;
                }

                internal override ConditionGraph Compare(CompareOperator op, ValueExpression left, Shape fieldShape) =>
                    ConditionGraph.Binary(op, left, ValueExpression.LiteralRaw(_text, Shape.String), fieldShape);
            }

            private sealed class ListValue : TestValue
            {
                private readonly IReadOnlyList<object?> _items;

                internal ListValue(IReadOnlyList<object?> items)
                {
                    _items = items;
                }

                internal override ConditionGraph Compare(CompareOperator op, ValueExpression left, Shape fieldShape)
                {
                    var items = _items.Select(item => ValueExpression.LiteralRaw(item, fieldShape)).ToList();
                    return ConditionGraph.Binary(op, left, ValueExpression.Array(items, Shape.ArrayOf(fieldShape.IsNone ? Shape.Any : fieldShape)), fieldShape);
                }
            }

            private sealed class ItemValue : TestValue
            {
                private readonly object? _item;
                private readonly Shape _itemShape;

                internal ItemValue(object? item, Shape itemShape)
                {
                    _item = item;
                    _itemShape = itemShape;
                }

                internal override ConditionGraph Compare(CompareOperator op, ValueExpression left, Shape fieldShape) =>
                    ConditionGraph.CollectionItem(op, left, ValueExpression.LiteralRaw(_item, _itemShape), fieldShape, _itemShape);
            }
        }
    }

    /// <summary>
    /// A member of the validator's model, by its path (<c>"Address.City"</c>, <c>"Lines[0].Qty"</c>) and its
    /// value's shape.
    /// </summary>
    /// <summary>A member of the validator's model: its path and the shape of its value.</summary>
    internal sealed class FieldReference
    {
        private FieldReference(string path, Type type, Shape shape)
        {
            Path = path;
            Type = type;
            Shape = shape;
        }

        internal string Path { get; }

        internal Type Type { get; }

        internal Shape Shape { get; }

        // x => x.Address.City is "Address.City", typed string. Anything but a chain of members from the
        // model's parameter names no member.
        internal static FieldReference Of(LambdaExpression member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            var body = ConditionValue.StripConvert(member.Body);
            var names = new List<string>();
            var current = body;
            while (current is MemberExpression chain && chain.Expression != null)
            {
                names.Insert(0, chain.Member.Name);
                current = ConditionValue.StripConvert(chain.Expression);
            }

            var isMemberOfTheModel = member.Parameters.Count == 1 && current == member.Parameters[0] && names.Count > 0;
            if (!isMemberOfTheModel)
                throw new ArgumentException(
                    $"A browser rule names a member of the model, like x => x.Address.City; {member} does not.", nameof(member));
            return new FieldReference(string.Join(".", names), body.Type, Shape.FromClrType(body.Type));
        }

        internal FieldReference PrefixedBy(string prefix) =>
            prefix.Length == 0 ? this : new FieldReference(prefix + "." + Path, Type, Shape);
    }

    // A value a condition fixes: a constant, or a variable the lambda captured, evaluated once at render.
    internal static class ConditionValue
    {
        internal static object? Evaluate(Expression value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return Expression.Lambda<Func<object?>>(Expression.Convert(value, typeof(object))).Compile()();
        }

        internal static Type TypeOf(Expression value) => StripConvert(value).Type;

        internal static Expression StripConvert(Expression expression)
        {
            while (expression is UnaryExpression unary
                   && (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked))
                expression = unary.Operand;
            return expression;
        }
    }
}
