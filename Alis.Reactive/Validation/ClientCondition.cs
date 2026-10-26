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
        public static ClientCondition Is(LambdaExpression member, ClientValueTest test)
        {
            var valueTest = ValueTestOf(test);
            return new FieldTest(FieldReference.Of(member), (left, shape) => ConditionGraph.Unary(valueTest, left, shape));
        }

        /// <summary>Compares the member with a value.</summary>
        /// <param name="member">The member, as a path from the model.</param>
        /// <param name="comparison">The comparison.</param>
        /// <param name="value">The value, a constant or a captured variable, read as the member's type: an enum compared through its number is its member.</param>
        public static ClientCondition Compare(LambdaExpression member, ClientComparison comparison, Expression value)
        {
            var field = FieldReference.Of(member);
            var op = OperatorOf(comparison);
            var fixedValue = LiteralOf(field.Type, Expressions.Evaluate(value)
                ?? throw new ArgumentNullException(nameof(value), "Test a missing value with ClientValueTest.Null."));
            return new FieldTest(field, (left, shape) => ConditionGraph.Ordered(op, left, ValueExpression.LiteralFor(fixedValue, shape), shape));
        }

        /// <summary>The member is one of the values.</summary>
        /// <param name="member">The member, as a path from the model.</param>
        /// <param name="values">The values, a constant or captured collection, each read as the member's type.</param>
        public static ClientCondition In(LambdaExpression member, Expression values)
        {
            var field = FieldReference.Of(member);
            var fixedValues = Expressions.Evaluate(values) as IEnumerable
                ?? throw new ArgumentException("The values must be a collection.", nameof(values));
            var members = fixedValues.Cast<object?>().Select(value => LiteralOf(field.Type, value)).ToArray();
            return new FieldTest(field, (left, shape) =>
                ConditionGraph.Membership(left, members.Select(value => ValueExpression.LiteralFor(value, shape)).ToList(), negated: false, shape));
        }

        /// <summary>Tests a text member against fixed text.</summary>
        /// <param name="member">The text member, as a path from the model.</param>
        /// <param name="test">The test.</param>
        /// <param name="text">The text, or the pattern for <see cref="ClientTextTest.Matches"/>.</param>
        public static ClientCondition Text(LambdaExpression member, ClientTextTest test, string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            var textTest = TextTestOf(test);
            return new FieldTest(FieldReference.Of(member), (left, shape) => ConditionGraph.Text(textTest, left, text, shape));
        }

        /// <summary>The collection member holds the item.</summary>
        /// <param name="collection">The collection member, as a path from the model.</param>
        /// <param name="item">The item, a constant or a captured variable, read as its own type.</param>
        public static ClientCondition Holds(LambdaExpression collection, Expression item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            var itemType = Expressions.TypeOf(item);
            var itemShape = Shape.FromClrType(itemType);
            var fixedItem = LiteralOf(itemType, Expressions.Evaluate(item));
            return new FieldTest(FieldReference.Of(collection), (left, shape) =>
                ConditionGraph.ArrayContains(left, ValueExpression.LiteralFor(fixedItem, itemShape), shape, itemShape));
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
        // how MVC binds it; anything else is written as it is.
        private static object? LiteralOf(Type memberType, object? value)
        {
            if (value == null) return null;

            var enumType = System.Nullable.GetUnderlyingType(memberType) ?? memberType;
            var asMemberType = enumType.IsEnum && !value.GetType().IsEnum ? Enum.ToObject(enumType, value) : value;
            return asMemberType.GetType().IsEnum ? asMemberType.ToString() : asMemberType;
        }

        private static ValueTest ValueTestOf(ClientValueTest test) => test switch
        {
            ClientValueTest.Truthy => ValueTest.Truthy,
            ClientValueTest.Falsy => ValueTest.Falsy,
            ClientValueTest.Null => ValueTest.IsNull,
            ClientValueTest.NotNull => ValueTest.NotNull,
            ClientValueTest.Empty => ValueTest.IsEmpty,
            ClientValueTest.NotEmpty => ValueTest.NotEmpty,
            _ => throw new ArgumentOutOfRangeException(nameof(test), test, "Unknown value test."),
        };

        private static CompareOperator OperatorOf(ClientComparison comparison) => comparison switch
        {
            ClientComparison.Equal => CompareOperator.Eq,
            ClientComparison.NotEqual => CompareOperator.Neq,
            ClientComparison.GreaterThan => CompareOperator.Gt,
            ClientComparison.GreaterThanOrEqual => CompareOperator.Gte,
            ClientComparison.LessThan => CompareOperator.Lt,
            ClientComparison.LessThanOrEqual => CompareOperator.Lte,
            _ => throw new ArgumentOutOfRangeException(nameof(comparison), comparison, "Unknown comparison."),
        };

        private static TextTest TextTestOf(ClientTextTest test) => test switch
        {
            ClientTextTest.Contains => TextTest.Contains,
            ClientTextTest.StartsWith => TextTest.StartsWith,
            ClientTextTest.EndsWith => TextTest.EndsWith,
            ClientTextTest.Matches => TextTest.Matches,
            _ => throw new ArgumentOutOfRangeException(nameof(test), test, "Unknown text test."),
        };

        private sealed class AlwaysCondition : ClientCondition
        {
            internal override bool IsAlways => true;

            internal override ClientCondition PrefixedBy(string prefix) => this;

            internal override IEnumerable<FieldReference> Fields => Array.Empty<FieldReference>();

            internal override ConditionGraph ToGraph(FormFieldValues fields) => ConditionGraph.True;
        }

        // A test of one member: when the form is written, the member is read as MVC binds it and tested in
        // the type its input holds, by the graph the test builds for that read and shape.
        private sealed class FieldTest : ClientCondition
        {
            private readonly FieldReference _field;
            private readonly Func<ValueExpression, Shape, ConditionGraph> _graph;

            internal FieldTest(FieldReference field, Func<ValueExpression, Shape, ConditionGraph> graph)
            {
                _field = field;
                _graph = graph;
            }

            internal override ClientCondition PrefixedBy(string prefix) => new FieldTest(_field.PrefixedBy(prefix), _graph);

            internal override IEnumerable<FieldReference> Fields => new[] { _field };

            internal override ConditionGraph ToGraph(FormFieldValues fields)
            {
                var input = fields.Resolve(_field);
                return _graph(ValueExpression.ReadBound(input.Id, Shape.None), input.Shape);
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
            var body = Expressions.StripConvert(member.Body);
            if (member.Parameters.Count != 1
                || !ExpressionPathHelper.TryMemberName(body, member.Parameters[0], out var name)
                || name.Length == 0)
                throw new ArgumentException(
                    $"A browser rule names a member of the model, like x => x.Address.City; {member} does not.", nameof(member));
            return new FieldReference(name, body.Type, Shape.FromClrType(body.Type));
        }

        /// <summary>The item of a collection member itself, with no path of its own: the member's element.</summary>
        internal static FieldReference ItemOf(FieldReference collection)
        {
            var itemType = ItemTypeOf(collection.Type)
                ?? throw new ArgumentException($"'{collection.Path}' is not a collection.", nameof(collection));
            return new FieldReference("", itemType, Shape.FromClrType(itemType));
        }

        internal FieldReference PrefixedBy(string prefix) =>
            prefix.Length == 0 ? this
            : Path.Length == 0 ? new FieldReference(prefix, Type, Shape)
            : new FieldReference(prefix + "." + Path, Type, Shape);

        private static Type? ItemTypeOf(Type type)
        {
            if (type.IsArray) return type.GetElementType();
            foreach (var candidate in new[] { type }.Concat(type.GetInterfaces()))
                if (candidate.IsGenericType && candidate.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    return candidate.GetGenericArguments()[0];
            return null;
        }
    }
}
