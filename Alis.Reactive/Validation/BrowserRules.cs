using System;
using System.Collections.Generic;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// A value is entered: not blank, and not the type's default. On a list, at least one item; on an
    /// enum, a member other than the default one, and on a nullable enum, whose default is null, any member.
    /// </summary>
    public sealed class NotEmptyRule : BrowserRule
    {
        internal NotEmptyRule(string message, Condition condition)
            : base(message, condition)
        {
        }

        internal override string Name => "required";

        internal override bool TryFor(FieldReference field, out BrowserRule rule)
        {
            rule = field.Shape is ArrayShape ? new AtLeastOneRule(Message, Condition)
                : DefaultMemberOf(field.Type) is string member ? new NotDefaultMemberRule(member, Message, Condition)
                : this;
            return true;
        }

        // The member an enum's default binds as (its member with value 0), which the server's NotEmpty refuses.
        // A nullable enum's default is null, which binds as blank, so its member with value 0 is an answer.
        private static string? DefaultMemberOf(Type memberType) =>
            memberType.IsEnum ? EnumMember.Default(memberType)?.ToString() : null;

        internal override Expr Check(Expr value, Shape fieldShape, FormFieldValues values, Scope scope) =>
            G.Not(BlankOrDefault(value, fieldShape));
    }

    /// <summary>
    /// A value is bound: text that is not blank, or a nullable with a value. A list is never null in the
    /// browser, and a number, a boolean, a date or an enum that cannot be null always binds, so on those
    /// the rule stays with the server, which always passes it.
    /// </summary>
    public sealed class NotNullRule : BrowserRule
    {
        internal NotNullRule(string message, Condition condition)
            : base(message, condition)
        {
        }

        internal override string Name => "required";

        internal override bool TryFor(FieldReference field, out BrowserRule rule)
        {
            rule = this;
            return field.Shape is not ArrayShape && (!field.Type.IsValueType || Nullable.GetUnderlyingType(field.Type) != null);
        }

        internal override Expr Check(Expr value, Shape fieldShape, FormFieldValues values, Scope scope) => G.Not(Blank(value));
    }

    /// <summary>An enum member other than the type's default one: what <see cref="BrowserRule.NotEmpty"/> asks of an enum.</summary>
    public sealed class NotDefaultMemberRule : BrowserRule
    {
        internal NotDefaultMemberRule(string defaultMember, string message, Condition condition)
            : base(message, condition)
        {
            DefaultMember = defaultMember;
        }

        /// <summary>The member the type's default binds as, which the rule refuses.</summary>
        public string DefaultMember { get; }

        internal override string Name => "required";

        internal override Expr Check(Expr value, Shape fieldShape, FormFieldValues values, Scope scope) =>
            G.Not(G.Or(Blank(value), G.Eq(G.Op(Operator.ToText, value), G.Lit(DefaultMember))));
    }

    /// <summary>A list has at least one item: what <see cref="BrowserRule.NotEmpty"/> asks of a list.</summary>
    public sealed class AtLeastOneRule : BrowserRule
    {
        internal AtLeastOneRule(string message, Condition condition)
            : base(message, condition)
        {
        }

        internal override string Name => "atLeastOne";

        // The value is the list as the field's shape reads it: an array, or the list the input keeps when it is
        // no array (a file input's FileList), each counting its items; a missing one has none.
        internal override Expr Check(Expr value, Shape fieldShape, FormFieldValues values, Scope scope) =>
            G.Op(Operator.Gt, G.Get(value, new[] { "length" }, Shape.None), G.Lit(0));
    }

    /// <summary>Nothing is entered. On a list, no items.</summary>
    public sealed class EmptyRule : BrowserRule
    {
        internal EmptyRule(string message, Condition condition)
            : base(message, condition)
        {
        }

        internal override string Name => "empty";

        internal override bool TryFor(FieldReference field, out BrowserRule rule)
        {
            rule = field.Shape is ArrayShape ? new NoItemsRule(Message, Condition) : this;
            return true;
        }

        internal override Expr Check(Expr value, Shape fieldShape, FormFieldValues values, Scope scope) =>
            BlankOrDefault(value, fieldShape);
    }

    /// <summary>A list has no items: what <see cref="BrowserRule.Empty"/> asks of a list.</summary>
    public sealed class NoItemsRule : BrowserRule
    {
        internal NoItemsRule(string message, Condition condition)
            : base(message, condition)
        {
        }

        internal override string Name => "noItems";

        // The list as AtLeastOneRule reads it, an array or the input's own list (a FileList), with no items
        // counted; a missing one has none, as the server's Empty passes null.
        internal override Expr Check(Expr value, Shape fieldShape, FormFieldValues values, Scope scope) =>
            G.Not(G.Op(Operator.Gt, G.Get(value, new[] { "length" }, Shape.None), G.Lit(0)));
    }

    /// <summary>Text at least <see cref="Length"/> characters long.</summary>
    public sealed class MinLengthRule : BrowserRule
    {
        internal MinLengthRule(int length, string message, Condition condition)
            : base(message, condition)
        {
            Length = length;
        }

        /// <summary>The fewest characters.</summary>
        public int Length { get; }

        internal override string Name => "minLength";

        internal override Expr Check(Expr value, Shape fieldShape, FormFieldValues values, Scope scope) =>
            G.Or(Absent(value), G.Op(Operator.Gte, TextLength(value), G.Lit(Length)));
    }

    /// <summary>Text at most <see cref="Length"/> characters long.</summary>
    public sealed class MaxLengthRule : BrowserRule
    {
        internal MaxLengthRule(int length, string message, Condition condition)
            : base(message, condition)
        {
            Length = length;
        }

        /// <summary>The most characters.</summary>
        public int Length { get; }

        internal override string Name => "maxLength";

        internal override Expr Check(Expr value, Shape fieldShape, FormFieldValues values, Scope scope) =>
            G.Or(Absent(value), G.Op(Operator.Lte, TextLength(value), G.Lit(Length)));
    }

    /// <summary>
    /// Text a JavaScript regular expression matches: the validator's own pattern, or the one an email
    /// address or a URL is decided by.
    /// </summary>
    public sealed class PatternRule : BrowserRule
    {
        private readonly string _name;

        internal PatternRule(string name, string pattern, string flags, string message, Condition condition)
            : base(message, condition)
        {
            _name = name;
            Pattern = pattern;
            Flags = flags;
        }

        /// <summary>The pattern, which reads the same in JavaScript and .NET.</summary>
        public string Pattern { get; }

        /// <summary>JavaScript flags: <c>i</c>, <c>m</c>, <c>s</c>; empty for none.</summary>
        public string Flags { get; }

        internal override string Name => _name;

        internal override Expr Check(Expr value, Shape fieldShape, FormFieldValues values, Scope scope) =>
            G.Or(Absent(value), G.Op(Operator.Matches, value, G.Lit(Pattern), G.Lit(Flags)));
    }

    /// <summary>A card number that passes the Luhn check.</summary>
    public sealed class CreditCardRule : BrowserRule
    {
        internal CreditCardRule(string message, Condition condition)
            : base(message, condition)
        {
        }

        internal override string Name => "creditCard";

        internal override Expr Check(Expr value, Shape fieldShape, FormFieldValues values, Scope scope) =>
            G.Or(Absent(value), CardNumber(value, scope));

        // The server's card check: dashes and spaces dropped, every other character a digit, and
        // the Luhn sum a multiple of ten.
        private static Expr CardNumber(Expr v, Scope scope)
        {
            var digits = scope.Fresh("d");
            var character = scope.Fresh("c");
            var sum = scope.Fresh("s");
            var digit = scope.Fresh("c");
            var index = scope.Fresh("i");

            var separator = G.Or(G.Eq(G.Ref(character), G.Lit("-")), G.Eq(G.Ref(character), G.Lit(" ")));
            var withoutSeparators = G.Call(
                G.Call(G.Call(G.Op(Operator.ToText, v), "split", G.Lit("")), "filter", G.Fn(G.Not(separator), character)),
                "join",
                G.Lit(""));
            var digitValue = G.Op(Operator.ToNumber, G.Ref(digit));
            var doubled = G.Op(Operator.Mul, digitValue, G.Lit(2));
            var weighted = G.Op(Operator.Cond,
                G.Eq(G.Op(Operator.Mod, G.Ref(index), G.Lit(2)), G.Lit(1)),
                G.Op(Operator.Cond, G.Op(Operator.Gt, doubled, G.Lit(9)), G.Op(Operator.Sub, doubled, G.Lit(9)), doubled),
                digitValue);
            var total = G.Call(
                G.Call(G.Call(G.Ref(digits), "split", G.Lit("")), "reverse"),
                "reduce",
                G.Fn(G.Op(Operator.Add, G.Ref(sum), weighted), sum, digit, index),
                G.Lit(0));
            var valid = G.And(
                Matches(G.Ref(digits), "^\\d*$"),
                G.Eq(G.Op(Operator.Mod, total, G.Lit(10)), G.Lit(0)));
            return G.Apply(G.Fn(valid, digits), withoutSeparators);
        }
    }

    /// <summary>
    /// The value compares with a target under the target's shape: a fixed value of the member's type,
    /// or another member of the model.
    /// </summary>
    public abstract class ComparisonRule : BrowserRule
    {
        private protected ComparisonRule(Comparison comparison, Shape shape, string message, Condition condition)
            : base(message, condition)
        {
            Comparison = comparison;
            Shape = shape;
        }

        /// <summary>How the value compares with the target.</summary>
        public Comparison Comparison { get; }

        /// <summary>The shape both sides are converted to.</summary>
        internal Shape Shape { get; }

        internal override string Name =>
            Comparison == Comparison.Equal ? "equalTo"
            : Comparison == Comparison.NotEqual ? (ComparesWithMember ? "notEqualTo" : "notEqual")
            : Comparison == Comparison.GreaterThan ? "gt"
            : Comparison == Comparison.GreaterThanOrEqual ? "min"
            : Comparison == Comparison.LessThan ? "lt"
            : "max";

        // Equality has no skip for nothing bound, as the server's Equal and NotEqual; an ordering passes
        // on nothing bound, as the server's comparisons do.
        internal override Expr Check(Expr value, Shape fieldShape, FormFieldValues values, Scope scope)
        {
            var target = Target(values, scope);
            if (Comparison == Comparison.Equal) return Same(value, target);
            if (Comparison == Comparison.NotEqual) return G.Not(Same(value, target));
            return Ordered(value, target, Comparison.Operation, scope);
        }

        private protected abstract bool ComparesWithMember { get; }

        // Equality has a browser form under every shape; order only under a shape the browser orders as the server does.
        internal override bool TryFor(FieldReference field, out BrowserRule rule)
        {
            rule = this;
            return Comparison.IsEquality || IsOrdered(Shape);
        }

        /// <summary>The target as the check reads it when it runs.</summary>
        private protected abstract Expr Target(FormFieldValues values, Scope scope);

        private Expr Same(Expr value, Expr target) => G.Eq(Canonical(value, Shape), Canonical(target, Shape));

        // The value and its target as numbers under the shape; a pair that cannot be compared (a peer
        // with nothing bound) fails.
        private Expr Ordered(Expr value, Expr target, Operator op, Scope scope)
        {
            var a = scope.Fresh("a");
            var b = scope.Fresh("b");
            var holds = G.And(IsNumber(G.Ref(a)), IsNumber(G.Ref(b)), G.Op(op, G.Ref(a), G.Ref(b)));
            var compared = G.Apply(G.Fn(holds, a, b), Comparable(value, Shape), Comparable(target, Shape));
            return G.Or(Absent(value), compared);
        }
    }

    /// <summary>The value compares with a fixed value of the member's type.</summary>
    public sealed class FixedComparisonRule : ComparisonRule
    {
        private readonly object _value;

        internal FixedComparisonRule(Comparison comparison, object value, Shape shape, string message, Condition condition)
            : base(comparison, shape, message, condition)
        {
            _value = value;
        }

        /// <summary>The fixed value as a number, which is what a component's numeric minimum and maximum take; null for a date or text.</summary>
        public decimal? Number => AsNumber(_value);

        private protected override bool ComparesWithMember => false;

        private protected override Expr Target(FormFieldValues values, Scope scope) => G.Lit(_value);
    }

    /// <summary>The value compares with another member of the model, read as the page binds it.</summary>
    public sealed class MemberComparisonRule : ComparisonRule
    {
        private readonly FieldReference _peer;

        internal MemberComparisonRule(Comparison comparison, FieldReference peer, string message, Condition condition)
            : base(comparison, peer.Shape, message, condition)
        {
            _peer = peer;
        }

        internal override IEnumerable<FieldReference> Fields => new[] { _peer };

        internal override BrowserRule PrefixedBy(string prefix, Condition outer) =>
            new MemberComparisonRule(Comparison, _peer.PrefixedBy(prefix), Message, Condition.Combine(outer, Condition.PrefixedBy(prefix)));

        private protected override bool ComparesWithMember => true;

        private protected override Expr Target(FormFieldValues values, Scope scope) => values.Value(_peer).Write(scope);
    }

    /// <summary>The value lies between two bounds of its own type, both included or both excluded.</summary>
    public sealed class RangeRule : BrowserRule
    {
        private readonly object _from;
        private readonly object _to;
        private readonly Shape _shape;

        internal RangeRule(object from, object to, bool inclusive, Shape shape, string message, Condition condition)
            : base(message, condition)
        {
            _from = from;
            _to = to;
            _shape = shape;
            Inclusive = inclusive;
        }

        /// <summary>Whether the bounds themselves pass.</summary>
        public bool Inclusive { get; }

        /// <summary>The bounds as numbers, which is what a component's numeric range takes; null for dates or text.</summary>
        public (decimal From, decimal To)? NumericBounds =>
            AsNumber(_from) is decimal from && AsNumber(_to) is decimal to ? (from, to) : null;

        internal override string Name => Inclusive ? "range" : "exclusiveRange";

        // A range of text stays with the server, which orders text by its culture.
        internal override bool TryFor(FieldReference field, out BrowserRule rule)
        {
            rule = this;
            return IsOrdered(_shape);
        }

        // The value and both bounds as numbers under the bounds' shape; nothing bound passes, as the
        // server's range check does.
        internal override Expr Check(Expr value, Shape fieldShape, FormFieldValues values, Scope scope)
        {
            var a = scope.Fresh("a");
            var low = scope.Fresh("l");
            var high = scope.Fresh("h");
            var inRange = G.And(
                IsNumber(G.Ref(a)), IsNumber(G.Ref(low)), IsNumber(G.Ref(high)),
                G.Op(Inclusive ? Operator.Gte : Operator.Gt, G.Ref(a), G.Ref(low)),
                G.Op(Inclusive ? Operator.Lte : Operator.Lt, G.Ref(a), G.Ref(high)));
            var compared = G.Apply(
                G.Fn(inRange, a, low, high),
                Comparable(value, _shape),
                Comparable(G.Lit(_from), _shape),
                Comparable(G.Lit(_to), _shape));
            return G.Or(Absent(value), compared);
        }
    }
}
