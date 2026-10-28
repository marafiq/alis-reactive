using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// One check a validator gives the browser, with the message it shows: the browser's form of a rule
    /// the server's validator decides. Each rule is a class of its own that carries exactly its operands
    /// and writes its own check: a lambda of the field's value that is true when the value passes.
    /// </summary>
    /// <remarks>
    /// A validation integration builds one for each rule the browser can evaluate and adds it to a
    /// <see cref="BrowserRuleSet"/>. A component whose own JavaScript validates (a grid's cell editor, say)
    /// reads a field's rules back by their classes (<see cref="MinLengthRule"/>, <see cref="PatternRule"/>,
    /// <see cref="FixedComparisonRule"/>, <see cref="RangeRule"/>) to translate them into its native shape.
    /// </remarks>
    public abstract class BrowserRule
    {
        private protected BrowserRule(string message, Condition condition)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        /// <summary>The message shown when the rule fails.</summary>
        public string Message { get; }

        /// <summary>Whether the rule always applies, rather than only when a condition holds.</summary>
        public bool IsUnconditional => Condition.IsAlways;

        // ── the rules ────────────────────────────────────────────────────────────

        /// <summary>A value is entered: not blank, and not the type's default. A list has at least one item.</summary>
        /// <param name="message">The message shown when the rule fails.</param>
        public static BrowserRule NotEmpty(string message) => new NotEmptyRule(message, Condition.Always);

        /// <summary>A value is bound. A list is never null in the browser, so the rule stays on the server for a list.</summary>
        /// <param name="message">The message shown when the rule fails.</param>
        public static BrowserRule NotNull(string message) => new NotNullRule(message, Condition.Always);

        /// <summary>Nothing is entered.</summary>
        /// <param name="message">The message shown when the rule fails.</param>
        public static BrowserRule Empty(string message) => new EmptyRule(message, Condition.Always);

        /// <summary>Text at least <paramref name="length"/> characters long.</summary>
        /// <param name="length">The fewest characters.</param>
        /// <param name="message">The message shown when the rule fails.</param>
        public static BrowserRule MinLength(int length, string message) => new MinLengthRule(length, message, Condition.Always);

        /// <summary>Text at most <paramref name="length"/> characters long.</summary>
        /// <param name="length">The most characters.</param>
        /// <param name="message">The message shown when the rule fails.</param>
        public static BrowserRule MaxLength(int length, string message) => new MaxLengthRule(length, message, Condition.Always);

        /// <summary>An email address, as <c>EmailAddress()</c> reads one: one @, neither first nor last.</summary>
        /// <param name="message">The message shown when the rule fails.</param>
        public static BrowserRule Email(string message) => new PatternRule("email", EmailPattern, "", message, Condition.Always);

        /// <summary>
        /// An <c>http</c> or <c>https</c> URL, its scheme in any case, as the server's absolute URL reads it.
        /// The browser checks the scheme; the server also checks the rest.
        /// </summary>
        /// <param name="message">The message shown when the rule fails.</param>
        public static BrowserRule Url(string message) => new PatternRule("url", UrlPattern, "", message, Condition.Always);

        /// <summary>A card number that passes the Luhn check.</summary>
        /// <param name="message">The message shown when the rule fails.</param>
        public static BrowserRule CreditCard(string message) => new CreditCardRule(message, Condition.Always);

        /// <summary>Text the JavaScript regular expression <paramref name="pattern"/> matches.</summary>
        /// <param name="pattern">A pattern that reads the same in JavaScript and .NET.</param>
        /// <param name="flags">JavaScript flags: <c>i</c>, <c>m</c>, <c>s</c>; empty for none.</param>
        /// <param name="message">The message shown when the rule fails.</param>
        public static BrowserRule Matches(string pattern, string flags, string message)
        {
            if (string.IsNullOrEmpty(pattern)) throw new ArgumentException("A browser pattern needs its text.", nameof(pattern));
            return new PatternRule("regex", pattern, flags ?? "", message, Condition.Always);
        }

        /// <summary>The value compares with a fixed value: <c>GreaterThanOrEqual</c> is a minimum, <c>LessThanOrEqual</c> a maximum.</summary>
        /// <typeparam name="TValue">The value's type, which is the member's.</typeparam>
        /// <param name="comparison">The comparison.</param>
        /// <param name="value">The fixed value.</param>
        /// <param name="message">The message shown when the rule fails.</param>
        public static BrowserRule Compare<TValue>(Comparison comparison, TValue value, string message)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return new FixedComparisonRule(comparison, value, Shape.FromClrType(value.GetType()), message, Condition.Always);
        }

        /// <summary>The value compares with another member of the model, read as the page binds it.</summary>
        /// <param name="comparison">The comparison.</param>
        /// <param name="peer">The other member, as a path from the model: <c>x =&gt; x.AdmitDate</c>.</param>
        /// <param name="message">The message shown when the rule fails.</param>
        public static BrowserRule CompareWith(Comparison comparison, LambdaExpression peer, string message) =>
            new MemberComparisonRule(comparison, FieldReference.Of(peer), message, Condition.Always);

        /// <summary>The value lies between two bounds of its own type.</summary>
        /// <typeparam name="TValue">The bounds' type, which is the member's.</typeparam>
        /// <param name="from">The lower bound.</param>
        /// <param name="to">The upper bound.</param>
        /// <param name="inclusive">Whether the bounds themselves pass.</param>
        /// <param name="message">The message shown when the rule fails.</param>
        public static BrowserRule Between<TValue>(TValue from, TValue to, bool inclusive, string message)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (to == null) throw new ArgumentNullException(nameof(to));
            var shape = Shape.FromClrType(from.GetType());
            if (!shape.Equals(Shape.FromClrType(to.GetType())))
                throw new ArgumentException(
                    "Client validation range bounds must have the same shape. " +
                    $"Lower bound is '{shape.Kind}', upper bound is '{Shape.FromClrType(to.GetType()).Kind}'.");
            return new RangeRule(from, to, inclusive, shape, message, Condition.Always);
        }

        /// <summary>The same rule, applied only while <paramref name="condition"/> holds.</summary>
        /// <param name="condition">The condition.</param>
        public BrowserRule When(Condition condition) => Under(condition ?? throw new ArgumentNullException(nameof(condition)));

        // ── what the form writes ─────────────────────────────────────────────────

        internal Condition Condition { get; private set; }

        /// <summary>The test's name in the plan.</summary>
        internal abstract string Name { get; }

        /// <summary>The other members the rule reads, besides the one it is on.</summary>
        internal virtual IEnumerable<FieldReference> Fields => Array.Empty<FieldReference>();

        /// <summary>The rule as the member makes it; false when the member leaves the rule to the server.</summary>
        internal virtual bool TryFor(FieldReference field, out BrowserRule rule)
        {
            rule = this;
            return true;
        }

        /// <summary>The rule relocated under <paramref name="prefix"/>, declared under <paramref name="outer"/> as well.</summary>
        internal virtual BrowserRule PrefixedBy(string prefix, Condition outer) =>
            Under(Condition.Combine(outer, Condition.PrefixedBy(prefix)));

        /// <summary>
        /// True when <paramref name="value"/>, the field's value as MVC binds it, passes: the server rule's own
        /// verdict on that value (grammar.md 7.4).
        /// </summary>
        internal abstract Expr Check(Expr value, Shape fieldShape, FormFieldValues values, Scope scope);

        // The same rule under another condition. A rule holds only its message, its condition and immutable
        // operands, so a shallow copy with the condition swapped is that rule.
        private protected BrowserRule Under(Condition condition)
        {
            var rule = (BrowserRule)MemberwiseClone();
            rule.Condition = condition;
            return rule;
        }

        // ── how a check reads a value ────────────────────────────────────────────

        private protected const string NetWhitespace = "\\t\\n\\v\\f\\r \\u0085\\u00A0\\u1680\\u2000-\\u200A\\u2028\\u2029\\u202F\\u205F\\u3000";

        // One @, with something on each side: what EmailAddress() accepts.
        private const string EmailPattern = "^[^@]+@[^@]+$";

        // The scheme the server's absolute URL lower-cases, written without a flag so that a component
        // testing a pattern with no flags tests the same one; an authority with no whitespace, which an
        // absolute URI never holds. The server checks the rest.
        private const string UrlPattern = "^[Hh][Tt][Tt][Pp][Ss]?://[^\\s/?#]+[^\\s]*$";

        // 0001-01-02T00:00:00Z: a date before it is DateTime.MinValue, in whatever zone it was read.
        private const long DayAfterEarliestDate = -62135510400000;

        // A bound value the server's null checks skip: nothing bound (blank text binds as null).
        private protected static Expr Absent(Expr v) => G.Eq(v, G.Null);

        private protected static Expr Matches(Expr v, string pattern) => G.Op(Operator.Matches, v, G.Lit(pattern), G.Lit(""));

        private protected static Expr TextLength(Expr v) => G.Get(G.Op(Operator.ToText, v), new[] { "length" }, Shape.None);

        // Nothing bound: nothing entered, or text of only whitespace, which MVC binds as null.
        private protected static Expr Blank(Expr v) => G.Or(NothingEntered(v), Matches(v, "^[" + NetWhitespace + "]*$"));

        // What NotEmpty rejects, as far as the field's shape tells: nothing entered, text of only
        // whitespace, and the type's default: false, 0, or 0001-01-01 for a date.
        private protected static Expr BlankOrDefault(Expr v, Shape fieldShape)
        {
            var terms = new List<Expr> { NothingEntered(v), Matches(v, "^[" + NetWhitespace + "]*$") };
            if (fieldShape is BooleanShape) terms.Add(G.Eq(v, G.False));
            if (fieldShape is NumberShape) terms.Add(G.Eq(v, G.Lit(0)));
            if (fieldShape is DateShape or InstantShape) terms.Add(G.Op(Operator.Lt, G.Op(Operator.ToNumber, v), G.Lit(DayAfterEarliestDate)));
            return G.Or(terms);
        }

        // Nothing was entered: a missing value, one whose text is "" (an empty list included), or one
        // with no text form. A pattern with one character of anything fails on exactly those.
        private static Expr NothingEntered(Expr v) => G.Not(Matches(v, "[\\s\\S]"));

        private protected static Expr IsNumber(Expr value) => G.Eq(G.Op(Operator.Finite, value), value);

        // The shapes the browser orders as the server does: numbers, flags and dates. The server orders text by
        // its culture, which the browser cannot match, so an order of text stays with the server.
        private protected static bool IsOrdered(Shape shape)
        {
            var orderedShape = shape.TryGetNullableInnerShape(out var inner) ? inner : shape;
            return orderedShape is NumberShape or BooleanShape or DateShape or InstantShape;
        }

        // A value as the number a comparison under an ordered shape (IsOrdered) orders it by.
        private protected static Expr Comparable(Expr value, Shape shape)
        {
            if (shape.TryGetNullableInnerShape(out var inner)) return Nullable(value, inner, Comparable);

            switch (shape.Kind)
            {
                case "number":
                    return G.Op(Operator.Cond, G.Eq(value, G.Null), G.Lit(0), G.Op(Operator.ToNumber, value));
                case "date":
                case "instant":
                    return G.Call(G.Op(Operator.ToDate, value), "getTime");
                case "boolean":
                    return G.Op(Operator.Cond, G.Op(Operator.ToBoolean, value), G.Lit(1), G.Lit(0));
                default:
                    throw new InvalidOperationException($"A value of shape '{shape.Kind}' has no order in the browser.");
            }
        }

        // A value as equality under the shape reads it, with no skip for nothing bound, as the server's
        // Equal and NotEqual: converted the way the shape converts a missing value too.
        private protected static Expr Canonical(Expr value, Shape shape)
        {
            if (shape.TryGetNullableInnerShape(out var inner)) return Nullable(value, inner, Canonical);

            switch (shape.Kind)
            {
                case "string":
                    return G.Op(Operator.Coalesce, G.Op(Operator.ToText, value), G.Lit(""));
                case "number":
                    return G.Op(Operator.Cond, G.Eq(value, G.Null), G.Lit(0), G.Op(Operator.ToNumber, value));
                case "boolean":
                    return G.Op(Operator.Cond, G.Eq(value, G.Null), G.False, G.Op(Operator.ToBoolean, value));
                case "date":
                case "instant":
                    return G.Call(G.Op(Operator.ToDate, value), "getTime");
                default:
                    return value;
            }
        }

        // A nullable as MVC binds it: a missing value stays missing, and for a number, boolean or date so
        // does blank text; anything else converts under the inner shape.
        private static Expr Nullable(Expr value, Shape inner, Func<Expr, Shape, Expr> convert)
        {
            var absent = G.Eq(value, G.Null);
            if (inner is NumberShape or BooleanShape or DateShape or InstantShape)
                absent = G.Or(absent, Matches(value, "^[" + NetWhitespace + "]*$"));
            return G.Op(Operator.Cond, absent, G.Null, convert(value, inner));
        }

        private protected static decimal? AsNumber(object? value) => value switch
        {
            byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal =>
                Convert.ToDecimal(value, CultureInfo.InvariantCulture),
            _ => null,
        };
    }
}
