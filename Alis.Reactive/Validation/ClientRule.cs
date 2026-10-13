using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// One check a validator gives the browser, with the message it shows: the browser's form of a rule
    /// the server's validator decides.
    /// </summary>
    /// <remarks>
    /// A validation integration builds one for each rule the browser can evaluate and adds it to a
    /// <see cref="ClientRuleSet"/>. A component whose own JavaScript validates (a grid's cell editor, say)
    /// reads a field's rules back to translate them into its native shape.
    /// </remarks>
    public sealed class ClientRule
    {
        private ClientRule(
            string name,
            string message,
            object? constraint,
            FieldReference? peer,
            ClientCondition condition,
            Shape comparisonShape,
            ShapeDependence dependence)
        {
            Name = name;
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Constraint = constraint;
            Peer = peer;
            Condition = condition;
            ComparisonShape = comparisonShape;
            Dependence = dependence;
        }

        /// <summary>The rule's name in the plan, for example <c>"required"</c>, <c>"minLength"</c> or <c>"range"</c>.</summary>
        public string Name { get; }

        /// <summary>The message shown when the rule fails.</summary>
        public string Message { get; }

        /// <summary>Whether the rule always applies, rather than only when a condition holds.</summary>
        public bool IsUnconditional => Condition.IsAlways;

        /// <summary>The rule's single operand, such as a length or a bound; <see langword="null"/> when it has none.</summary>
        public object? LiteralOperand => Constraint is object[] ? null : Constraint;

        /// <summary>
        /// The JavaScript pattern, with no flags, the browser tests the text with: a pattern rule's own, or the
        /// one an email or a URL rule is decided by. <see langword="null"/> for any other rule, and for a
        /// pattern with flags.
        /// </summary>
        public string? TextPattern => IsPattern && Constraint is object[] pair && (string)pair[1] == ""
            ? (string)pair[0]
            : null;

        /// <summary>The rule's two bounds for a range; <see langword="null"/> for any other rule.</summary>
        public object[]? RangeOperand => IsRange ? (object[]?)Constraint : null;

        // ── the rules ────────────────────────────────────────────────────────────

        /// <summary>A value is entered: not blank, and not the type's default. A list has at least one item.</summary>
        /// <param name="message">The message shown when the rule fails.</param>
        public static ClientRule NotEmpty(string message) =>
            new ClientRule(Required, message, null, null, ClientCondition.Always, Shape.None, ShapeDependence.NotEmpty);

        /// <summary>A value is bound. A list is never null in the browser, so the rule stays on the server for a list.</summary>
        /// <param name="message">The message shown when the rule fails.</param>
        public static ClientRule NotNull(string message) =>
            new ClientRule(Required, message, null, null, ClientCondition.Always, Shape.None, ShapeDependence.NotNull);

        /// <summary>Nothing is entered.</summary>
        /// <param name="message">The message shown when the rule fails.</param>
        public static ClientRule Empty(string message) => Plain("empty", message);

        /// <summary>Text at least <paramref name="length"/> characters long.</summary>
        /// <param name="length">The fewest characters.</param>
        /// <param name="message">The message shown when the rule fails.</param>
        public static ClientRule MinLength(int length, string message) =>
            new ClientRule("minLength", message, length, null, ClientCondition.Always, Shape.None, ShapeDependence.None);

        /// <summary>Text at most <paramref name="length"/> characters long.</summary>
        /// <param name="length">The most characters.</param>
        /// <param name="message">The message shown when the rule fails.</param>
        public static ClientRule MaxLength(int length, string message) =>
            new ClientRule("maxLength", message, length, null, ClientCondition.Always, Shape.None, ShapeDependence.None);

        /// <summary>An email address, as <c>EmailAddress()</c> reads one: one @, neither first nor last.</summary>
        /// <param name="message">The message shown when the rule fails.</param>
        public static ClientRule Email(string message) => PatternRule("email", EmailPattern, message);

        /// <summary>
        /// An <c>http</c> or <c>https</c> URL, its scheme in any case, as the server's absolute URL reads it.
        /// The browser checks the scheme; the server also checks the rest.
        /// </summary>
        /// <param name="message">The message shown when the rule fails.</param>
        public static ClientRule Url(string message) => PatternRule("url", UrlPattern, message);

        /// <summary>A card number that passes the Luhn check.</summary>
        /// <param name="message">The message shown when the rule fails.</param>
        public static ClientRule CreditCard(string message) => Plain("creditCard", message);

        /// <summary>Text the JavaScript regular expression <paramref name="pattern"/> matches.</summary>
        /// <param name="pattern">A pattern that reads the same in JavaScript and .NET.</param>
        /// <param name="flags">JavaScript flags: <c>i</c>, <c>m</c>, <c>s</c>; empty for none.</param>
        /// <param name="message">The message shown when the rule fails.</param>
        public static ClientRule Pattern(string pattern, string flags, string message)
        {
            if (string.IsNullOrEmpty(pattern)) throw new ArgumentException("A browser pattern needs its text.", nameof(pattern));
            return new ClientRule("regex", message, new object[] { pattern, flags ?? "" }, null, ClientCondition.Always, Shape.None, ShapeDependence.None);
        }

        // One @, with something on each side: what EmailAddress() accepts.
        private const string EmailPattern = "^[^@]+@[^@]+$";

        // The scheme the server's absolute URL lower-cases, written without a flag so that a component
        // testing a pattern with no flags tests the same one.
        private const string UrlPattern = "^[Hh][Tt][Tt][Pp][Ss]?://.+";

        private static ClientRule PatternRule(string name, string pattern, string message) =>
            new ClientRule(name, message, new object[] { pattern, "" }, null, ClientCondition.Always, Shape.None, ShapeDependence.None);

        /// <summary>The value compares with a fixed value: <c>GreaterThanOrEqual</c> is a minimum, <c>LessThanOrEqual</c> a maximum.</summary>
        /// <param name="comparison">The comparison.</param>
        /// <param name="value">The fixed value.</param>
        /// <param name="message">The message shown when the rule fails.</param>
        public static ClientRule Compare(ClientComparison comparison, object value, string message)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            var shape = Shape.FromClrType(value.GetType());
            return new ClientRule(NameOf(comparison, peer: false), message, ValidationDateLiteral.From(value, shape), null, ClientCondition.Always, shape, ShapeDependence.None);
        }

        /// <summary>The value compares with a sibling member's value.</summary>
        /// <param name="comparison">The comparison.</param>
        /// <param name="peer">The sibling member's path in the validator's model.</param>
        /// <param name="peerType">The sibling member's type.</param>
        /// <param name="message">The message shown when the rule fails.</param>
        public static ClientRule CompareWith(ClientComparison comparison, string peer, Type peerType, string message)
        {
            var reference = FieldReference.Of(peer, peerType);
            return new ClientRule(NameOf(comparison, peer: true), message, null, reference, ClientCondition.Always, reference.Shape, ShapeDependence.None);
        }

        /// <summary>The value lies between two bounds, inclusive or not.</summary>
        /// <param name="from">The lower bound.</param>
        /// <param name="to">The upper bound, of the lower bound's type.</param>
        /// <param name="inclusive">Whether a value equal to a bound passes.</param>
        /// <param name="message">The message shown when the rule fails.</param>
        public static ClientRule Between(object from, object to, bool inclusive, string message)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (to == null) throw new ArgumentNullException(nameof(to));
            var shape = Shape.FromClrType(from.GetType());
            if (!shape.Equals(Shape.FromClrType(to.GetType())))
                throw new ArgumentException(
                    "Client validation range bounds must have the same shape. " +
                    $"Lower bound is '{shape.Kind}', upper bound is '{Shape.FromClrType(to.GetType()).Kind}'.");

            var bounds = new[] { ValidationDateLiteral.From(from, shape), ValidationDateLiteral.From(to, shape) };
            return new ClientRule(inclusive ? "range" : "exclusiveRange", message, bounds, null, ClientCondition.Always, shape, ShapeDependence.None);
        }

        /// <summary>The rule, applied only when <paramref name="condition"/> holds.</summary>
        /// <param name="condition">The condition the rule is declared under; <see cref="ClientCondition.Always"/> for none.</param>
        public ClientRule When(ClientCondition condition) =>
            new ClientRule(Name, Message, Constraint, Peer, condition ?? throw new ArgumentNullException(nameof(condition)), ComparisonShape, Dependence);

        // ── what the form writes ─────────────────────────────────────────────────

        internal object? Constraint { get; }

        internal FieldReference? Peer { get; }

        internal ClientCondition Condition { get; }

        /// <summary>The type the rule compares in: its fixed value's, its peer's or its bounds'.</summary>
        internal Shape ComparisonShape { get; }

        private ShapeDependence Dependence { get; }

        private bool IsRange => Name == "range" || Name == "exclusiveRange";

        private bool IsPattern => Name == "regex" || Name == "email" || Name == "url";

        /// <summary>The rule a field of this shape has: a list has items or stays on the server for null.</summary>
        internal bool TryFor(Shape fieldShape, out ClientRule rule)
        {
            rule = this;
            var isList = fieldShape.Kind == "array";
            if (Dependence == ShapeDependence.NotNull) return !isList;
            if (Dependence == ShapeDependence.NotEmpty && isList)
                rule = new ClientRule("atLeastOne", Message, null, null, Condition, ComparisonShape, ShapeDependence.None);
            return true;
        }

        /// <summary>The rule of a child validator's field, under the member that holds it and its condition.</summary>
        internal ClientRule PrefixedBy(string prefix, ClientCondition outer) =>
            new ClientRule(
                Name,
                Message,
                Constraint,
                Peer?.PrefixedBy(prefix),
                ClientCondition.Combine(outer, Condition.PrefixedBy(prefix)),
                ComparisonShape,
                Dependence);

        private const string Required = "required";

        private static ClientRule Plain(string name, string message) =>
            new ClientRule(name, message, null, null, ClientCondition.Always, Shape.None, ShapeDependence.None);

        private static string NameOf(ClientComparison comparison, bool peer) => comparison switch
        {
            ClientComparison.Equal => "equalTo",
            ClientComparison.NotEqual => peer ? "notEqualTo" : "notEqual",
            ClientComparison.GreaterThan => "gt",
            ClientComparison.GreaterThanOrEqual => "min",
            ClientComparison.LessThan => "lt",
            ClientComparison.LessThanOrEqual => "max",
            _ => throw new ArgumentOutOfRangeException(nameof(comparison), comparison, "Unknown comparison."),
        };

        // NotEmpty and NotNull read the field's shape: a list has items, and is never null in the browser.
        private enum ShapeDependence
        {
            None,
            NotEmpty,
            NotNull,
        }
    }
}
