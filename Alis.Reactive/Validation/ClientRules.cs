using System;
using System.Collections.Generic;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>A value is entered: not blank, and not the type's default. On a list, at least one item.</summary>
    public sealed class NotEmptyRule : ClientRule
    {
        internal NotEmptyRule(string message, ClientCondition condition)
            : base(message, condition)
        {
        }

        internal override string Name => "required";

        internal override bool TryFor(Shape fieldShape, out ClientRule rule)
        {
            rule = fieldShape.Kind == "array" ? new AtLeastOneRule(Message, Condition) : this;
            return true;
        }

        internal override Expr Check(Expr value, Shape fieldShape, FormFieldValues values, Scope scope) =>
            G.Not(BlankOrDefault(value, fieldShape));

        private protected override ClientRule Under(ClientCondition condition) => new NotEmptyRule(Message, condition);
    }

    /// <summary>A value is bound. A list is never null in the browser, so on a list the rule stays with the server.</summary>
    public sealed class NotNullRule : ClientRule
    {
        internal NotNullRule(string message, ClientCondition condition)
            : base(message, condition)
        {
        }

        internal override string Name => "required";

        internal override bool TryFor(Shape fieldShape, out ClientRule rule)
        {
            rule = this;
            return fieldShape.Kind != "array";
        }

        internal override Expr Check(Expr value, Shape fieldShape, FormFieldValues values, Scope scope) =>
            G.Not(BlankOrDefault(value, fieldShape));

        private protected override ClientRule Under(ClientCondition condition) => new NotNullRule(Message, condition);
    }

    /// <summary>A list has at least one item: what <see cref="ClientRule.NotEmpty"/> asks of a list.</summary>
    public sealed class AtLeastOneRule : ClientRule
    {
        internal AtLeastOneRule(string message, ClientCondition condition)
            : base(message, condition)
        {
        }

        internal override string Name => "atLeastOne";

        internal override Expr Check(Expr value, Shape fieldShape, FormFieldValues values, Scope scope)
        {
            var isArray = G.Eq(G.Get(value, new[] { "constructor", "name" }, Shape.None), G.Lit("Array"));
            var hasItems = G.Op(Operator.Gt, G.Get(value, new[] { "length" }, Shape.None), G.Lit(0));
            return G.Op(Operator.Cond, isArray, hasItems, G.Not(BlankOrDefault(value, fieldShape)));
        }

        private protected override ClientRule Under(ClientCondition condition) => new AtLeastOneRule(Message, condition);
    }

    /// <summary>Nothing is entered.</summary>
    public sealed class EmptyRule : ClientRule
    {
        internal EmptyRule(string message, ClientCondition condition)
            : base(message, condition)
        {
        }

        internal override string Name => "empty";

        internal override Expr Check(Expr value, Shape fieldShape, FormFieldValues values, Scope scope) =>
            BlankOrDefault(value, fieldShape);

        private protected override ClientRule Under(ClientCondition condition) => new EmptyRule(Message, condition);
    }

    /// <summary>Text at least <see cref="Length"/> characters long.</summary>
    public sealed class MinLengthRule : ClientRule
    {
        internal MinLengthRule(int length, string message, ClientCondition condition)
            : base(message, condition)
        {
            Length = length;
        }

        /// <summary>The fewest characters.</summary>
        public int Length { get; }

        internal override string Name => "minLength";

        internal override Expr Check(Expr value, Shape fieldShape, FormFieldValues values, Scope scope) =>
            G.Or(Absent(value), G.Op(Operator.Gte, TextLength(value), G.Lit(Length)));

        private protected override ClientRule Under(ClientCondition condition) => new MinLengthRule(Length, Message, condition);
    }

    /// <summary>Text at most <see cref="Length"/> characters long.</summary>
    public sealed class MaxLengthRule : ClientRule
    {
        internal MaxLengthRule(int length, string message, ClientCondition condition)
            : base(message, condition)
        {
            Length = length;
        }

        /// <summary>The most characters.</summary>
        public int Length { get; }

        internal override string Name => "maxLength";

        internal override Expr Check(Expr value, Shape fieldShape, FormFieldValues values, Scope scope) =>
            G.Or(Absent(value), G.Op(Operator.Lte, TextLength(value), G.Lit(Length)));

        private protected override ClientRule Under(ClientCondition condition) => new MaxLengthRule(Length, Message, condition);
    }

    /// <summary>
    /// Text a JavaScript regular expression matches: the validator's own pattern, or the one an email
    /// address or a URL is decided by.
    /// </summary>
    public sealed class PatternRule : ClientRule
    {
        private readonly string _name;

        internal PatternRule(string name, string pattern, string flags, string message, ClientCondition condition)
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

        private protected override ClientRule Under(ClientCondition condition) => new PatternRule(_name, Pattern, Flags, Message, condition);
    }

    /// <summary>A card number that passes the Luhn check.</summary>
    public sealed class CreditCardRule : ClientRule
    {
        internal CreditCardRule(string message, ClientCondition condition)
            : base(message, condition)
        {
        }

        internal override string Name => "creditCard";

        internal override Expr Check(Expr value, Shape fieldShape, FormFieldValues values, Scope scope) =>
            G.Or(Absent(value), CardNumber(value, scope));

        private protected override ClientRule Under(ClientCondition condition) => new CreditCardRule(Message, condition);

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
    public abstract class ComparisonRule : ClientRule
    {
        private protected ComparisonRule(ClientComparison comparison, Shape shape, string message, ClientCondition condition)
            : base(message, condition)
        {
            Comparison = comparison;
            Shape = shape;
        }

        /// <summary>How the value compares with the target.</summary>
        public ClientComparison Comparison { get; }

        /// <summary>The shape both sides are converted to.</summary>
        internal Shape Shape { get; }

        internal override string Name => Comparison switch
        {
            ClientComparison.Equal => "equalTo",
            ClientComparison.NotEqual => ComparesWithMember ? "notEqualTo" : "notEqual",
            ClientComparison.GreaterThan => "gt",
            ClientComparison.GreaterThanOrEqual => "min",
            ClientComparison.LessThan => "lt",
            ClientComparison.LessThanOrEqual => "max",
            _ => throw new ArgumentOutOfRangeException(nameof(Comparison), Comparison, "Unknown comparison."),
        };

        // Equality has no skip for nothing bound, as the server's Equal and NotEqual; an ordering passes
        // on nothing bound, as the server's comparisons do.
        internal override Expr Check(Expr value, Shape fieldShape, FormFieldValues values, Scope scope)
        {
            var target = Target(values, scope);
            switch (Comparison)
            {
                case ClientComparison.Equal: return Same(value, target);
                case ClientComparison.NotEqual: return G.Not(Same(value, target));
                case ClientComparison.GreaterThan: return Ordered(value, target, Operator.Gt, scope);
                case ClientComparison.GreaterThanOrEqual: return Ordered(value, target, Operator.Gte, scope);
                case ClientComparison.LessThan: return Ordered(value, target, Operator.Lt, scope);
                case ClientComparison.LessThanOrEqual: return Ordered(value, target, Operator.Lte, scope);
                default: throw new ArgumentOutOfRangeException(nameof(Comparison), Comparison, "Unknown comparison.");
            }
        }

        private protected abstract bool ComparesWithMember { get; }

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

        internal FixedComparisonRule(ClientComparison comparison, object value, Shape shape, string message, ClientCondition condition)
            : base(comparison, shape, message, condition)
        {
            _value = value;
        }

        /// <summary>The fixed value as a number, which is what a component's numeric minimum and maximum take; null for a date or text.</summary>
        public decimal? Number => AsNumber(_value);

        private protected override bool ComparesWithMember => false;

        private protected override Expr Target(FormFieldValues values, Scope scope) => G.Lit(_value);

        private protected override ClientRule Under(ClientCondition condition) =>
            new FixedComparisonRule(Comparison, _value, Shape, Message, condition);
    }

    /// <summary>The value compares with another member of the model, read as the page binds it.</summary>
    public sealed class MemberComparisonRule : ComparisonRule
    {
        private readonly FieldReference _peer;

        internal MemberComparisonRule(ClientComparison comparison, FieldReference peer, string message, ClientCondition condition)
            : base(comparison, peer.Shape, message, condition)
        {
            _peer = peer;
        }

        internal override IEnumerable<FieldReference> Fields => new[] { _peer };

        internal override ClientRule PrefixedBy(string prefix, ClientCondition outer) =>
            new MemberComparisonRule(Comparison, _peer.PrefixedBy(prefix), Message, ClientCondition.Combine(outer, Condition.PrefixedBy(prefix)));

        private protected override bool ComparesWithMember => true;

        private protected override Expr Target(FormFieldValues values, Scope scope) => values.Value(_peer).Write(scope);

        private protected override ClientRule Under(ClientCondition condition) =>
            new MemberComparisonRule(Comparison, _peer, Message, condition);
    }

    /// <summary>The value lies between two bounds of its own type, both included or both excluded.</summary>
    public sealed class RangeRule : ClientRule
    {
        private readonly object _from;
        private readonly object _to;
        private readonly Shape _shape;

        internal RangeRule(object from, object to, bool inclusive, Shape shape, string message, ClientCondition condition)
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

        private protected override ClientRule Under(ClientCondition condition) =>
            new RangeRule(_from, _to, Inclusive, _shape, Message, condition);
    }
}
