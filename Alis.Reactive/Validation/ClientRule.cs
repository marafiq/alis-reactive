using System;
using System.Globalization;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>What a browser rule checks.</summary>
    public enum ClientRuleKind
    {
        /// <summary>A value is entered, or bound.</summary>
        Required,

        /// <summary>A list has at least one item.</summary>
        AtLeastOne,

        /// <summary>Nothing is entered.</summary>
        Empty,

        /// <summary>Text at least a number of characters long.</summary>
        MinLength,

        /// <summary>Text at most a number of characters long.</summary>
        MaxLength,

        /// <summary>An email address.</summary>
        Email,

        /// <summary>An http or https URL.</summary>
        Url,

        /// <summary>A card number that passes the Luhn check.</summary>
        CreditCard,

        /// <summary>Text a regular expression matches.</summary>
        Pattern,

        /// <summary>The value compares with a fixed value or with another member.</summary>
        Comparison,

        /// <summary>The value lies within two bounds, bounds included.</summary>
        Range,

        /// <summary>The value lies strictly between two bounds.</summary>
        ExclusiveRange,
    }

    /// <summary>
    /// One check a validator gives the browser, with the message it shows: the browser's form of a rule
    /// the server's validator decides.
    /// </summary>
    /// <remarks>
    /// A validation integration builds one for each rule the browser can evaluate and adds it to a
    /// <see cref="ClientRuleSet"/>. A component whose own JavaScript validates (a grid's cell editor, say)
    /// reads a field's rules back through <see cref="Kind"/> and the typed operands to translate them into
    /// its native shape.
    /// </remarks>
    public sealed class ClientRule
    {
        private ClientRule(
            ClientRuleKind kind,
            string name,
            string message,
            object? constraint,
            FieldReference? peer,
            ClientComparison? comparison,
            ClientCondition condition,
            Shape comparisonShape,
            ShapeDependence dependence)
        {
            Kind = kind;
            Name = name;
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Constraint = constraint;
            Peer = peer;
            Comparison = comparison;
            Condition = condition;
            ComparisonShape = comparisonShape;
            Dependence = dependence;
        }

        /// <summary>What the rule checks.</summary>
        public ClientRuleKind Kind { get; }

        /// <summary>The message shown when the rule fails.</summary>
        public string Message { get; }

        /// <summary>Whether the rule always applies, rather than only when a condition holds.</summary>
        public bool IsUnconditional => Condition.IsAlways;

        /// <summary>How a <see cref="ClientRuleKind.Comparison"/> compares; <see langword="null"/> for any other kind.</summary>
        public ClientComparison? Comparison { get; }

        /// <summary>Whether a comparison compares with another member of the model rather than a fixed value.</summary>
        public bool ComparesWithMember => Peer != null;

        /// <summary>The length a <see cref="ClientRuleKind.MinLength"/> or <see cref="ClientRuleKind.MaxLength"/> rule checks; <see langword="null"/> for any other kind.</summary>
        public int? Length => Kind == ClientRuleKind.MinLength || Kind == ClientRuleKind.MaxLength ? (int?)Constraint : null;

        /// <summary>
        /// The JavaScript pattern, with no flags, the browser tests the text with: a pattern rule's own, or the
        /// one an email or a URL rule is decided by. <see langword="null"/> for any other kind, and for a
        /// pattern with flags.
        /// </summary>
        public string? TextPattern => IsPattern && Constraint is object[] pair && (string)pair[1] == ""
            ? (string)pair[0]
            : null;

        /// <summary>The number a comparison compares with, when it compares with a fixed number; <see langword="null"/> otherwise.</summary>
        public decimal? Number => Kind == ClientRuleKind.Comparison && Peer == null ? AsNumber(Constraint) : null;

        /// <summary>The bounds of a <see cref="ClientRuleKind.Range"/> or <see cref="ClientRuleKind.ExclusiveRange"/> rule, when both are numbers; <see langword="null"/> otherwise.</summary>
        public (decimal From, decimal To)? NumericRange =>
            IsRange && Constraint is object[] bounds && AsNumber(bounds[0]) is decimal from && AsNumber(bounds[1]) is decimal to
                ? (from, to)
                : null;

        // ── the rules ────────────────────────────────────────────────────────────

        /// <summary>A value is entered: not blank, and not the type's default. A list has at least one item.</summary>
        /// <param name="message">The message shown when the rule fails.</param>
        public static ClientRule NotEmpty(string message) =>
            new ClientRule(ClientRuleKind.Required, Required, message, null, null, null, ClientCondition.Always, Shape.None, ShapeDependence.NotEmpty);

        /// <summary>A value is bound. A list is never null in the browser, so the rule stays on the server for a list.</summary>
        /// <param name="message">The message shown when the rule fails.</param>
        public static ClientRule NotNull(string message) =>
            new ClientRule(ClientRuleKind.Required, Required, message, null, null, null, ClientCondition.Always, Shape.None, ShapeDependence.NotNull);

        /// <summary>Nothing is entered.</summary>
        /// <param name="message">The message shown when the rule fails.</param>
        public static ClientRule Empty(string message) => Plain(ClientRuleKind.Empty, "empty", message);

        /// <summary>Text at least <paramref name="length"/> characters long.</summary>
        /// <param name="length">The fewest characters.</param>
        /// <param name="message">The message shown when the rule fails.</param>
        public static ClientRule MinLength(int length, string message) =>
            new ClientRule(ClientRuleKind.MinLength, "minLength", message, length, null, null, ClientCondition.Always, Shape.None, ShapeDependence.None);

        /// <summary>Text at most <paramref name="length"/> characters long.</summary>
        /// <param name="length">The most characters.</param>
        /// <param name="message">The message shown when the rule fails.</param>
        public static ClientRule MaxLength(int length, string message) =>
            new ClientRule(ClientRuleKind.MaxLength, "maxLength", message, length, null, null, ClientCondition.Always, Shape.None, ShapeDependence.None);

        /// <summary>An email address, as <c>EmailAddress()</c> reads one: one @, neither first nor last.</summary>
        /// <param name="message">The message shown when the rule fails.</param>
        public static ClientRule Email(string message) => PatternRule(ClientRuleKind.Email, "email", EmailPattern, message);

        /// <summary>
        /// An <c>http</c> or <c>https</c> URL, its scheme in any case, as the server's absolute URL reads it.
        /// The browser checks the scheme; the server also checks the rest.
        /// </summary>
        /// <param name="message">The message shown when the rule fails.</param>
        public static ClientRule Url(string message) => PatternRule(ClientRuleKind.Url, "url", UrlPattern, message);

        /// <summary>A card number that passes the Luhn check.</summary>
        /// <param name="message">The message shown when the rule fails.</param>
        public static ClientRule CreditCard(string message) => Plain(ClientRuleKind.CreditCard, "creditCard", message);

        /// <summary>Text the JavaScript regular expression <paramref name="pattern"/> matches.</summary>
        /// <param name="pattern">A pattern that reads the same in JavaScript and .NET.</param>
        /// <param name="flags">JavaScript flags: <c>i</c>, <c>m</c>, <c>s</c>; empty for none.</param>
        /// <param name="message">The message shown when the rule fails.</param>
        public static ClientRule Pattern(string pattern, string flags, string message)
        {
            if (string.IsNullOrEmpty(pattern)) throw new ArgumentException("A browser pattern needs its text.", nameof(pattern));
            return new ClientRule(ClientRuleKind.Pattern, "regex", message, new object[] { pattern, flags ?? "" }, null, null, ClientCondition.Always, Shape.None, ShapeDependence.None);
        }

        // One @, with something on each side: what EmailAddress() accepts.
        private const string EmailPattern = "^[^@]+@[^@]+$";

        // The scheme the server's absolute URL lower-cases, written without a flag so that a component
        // testing a pattern with no flags tests the same one.
        private const string UrlPattern = "^[Hh][Tt][Tt][Pp][Ss]?://.+";

        private static ClientRule PatternRule(ClientRuleKind kind, string name, string pattern, string message) =>
            new ClientRule(kind, name, message, new object[] { pattern, "" }, null, null, ClientCondition.Always, Shape.None, ShapeDependence.None);

        /// <summary>The value compares with a fixed value: <c>GreaterThanOrEqual</c> is a minimum, <c>LessThanOrEqual</c> a maximum.</summary>
        /// <typeparam name="TValue">The value's type, which is the member's.</typeparam>
        /// <param name="comparison">The comparison.</param>
        /// <param name="value">The fixed value.</param>
        /// <param name="message">The message shown when the rule fails.</param>
        public static ClientRule Compare<TValue>(ClientComparison comparison, TValue value, string message)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            var shape = Shape.FromClrType(value.GetType());
            return new ClientRule(
                ClientRuleKind.Comparison, NameOf(comparison, peer: false), message,
                value, null, comparison, ClientCondition.Always, shape, ShapeDependence.None);
        }

        /// <summary>The value compares with another member of the model, read as the page binds it.</summary>
        /// <param name="comparison">The comparison.</param>
        /// <param name="peer">The other member, as a path from the model: <c>x =&gt; x.AdmitDate</c>.</param>
        /// <param name="message">The message shown when the rule fails.</param>
        public static ClientRule CompareWith(ClientComparison comparison, LambdaExpression peer, string message)
        {
            var reference = FieldReference.Of(peer);
            return new ClientRule(
                ClientRuleKind.Comparison, NameOf(comparison, peer: true), message,
                null, reference, comparison, ClientCondition.Always, reference.Shape, ShapeDependence.None);
        }

        /// <summary>The value lies between two bounds of its own type.</summary>
        /// <typeparam name="TValue">The bounds' type, which is the member's.</typeparam>
        /// <param name="from">The lower bound.</param>
        /// <param name="to">The upper bound.</param>
        /// <param name="inclusive">Whether the bounds themselves pass.</param>
        /// <param name="message">The message shown when the rule fails.</param>
        public static ClientRule Between<TValue>(TValue from, TValue to, bool inclusive, string message)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (to == null) throw new ArgumentNullException(nameof(to));
            var shape = Shape.FromClrType(from.GetType());
            if (!shape.Equals(Shape.FromClrType(to.GetType())))
                throw new ArgumentException(
                    "Client validation range bounds must have the same shape. " +
                    $"Lower bound is '{shape.Kind}', upper bound is '{Shape.FromClrType(to.GetType()).Kind}'.");
            var bounds = new object[] { from, to };
            return new ClientRule(
                inclusive ? ClientRuleKind.Range : ClientRuleKind.ExclusiveRange,
                inclusive ? "range" : "exclusiveRange",
                message, bounds, null, null, ClientCondition.Always, shape, ShapeDependence.None);
        }

        /// <summary>The same rule, applied only while <paramref name="condition"/> holds.</summary>
        /// <param name="condition">The condition.</param>
        public ClientRule When(ClientCondition condition) =>
            new ClientRule(Kind, Name, Message, Constraint, Peer, Comparison, condition ?? throw new ArgumentNullException(nameof(condition)), ComparisonShape, Dependence);

        // ── what the form writes ─────────────────────────────────────────────────

        internal string Name { get; }

        internal object? Constraint { get; }

        internal FieldReference? Peer { get; }

        internal ClientCondition Condition { get; }

        internal Shape ComparisonShape { get; }

        private ShapeDependence Dependence { get; }

        private bool IsRange => Kind == ClientRuleKind.Range || Kind == ClientRuleKind.ExclusiveRange;

        private bool IsPattern => Kind == ClientRuleKind.Pattern || Kind == ClientRuleKind.Email || Kind == ClientRuleKind.Url;

        // The rule as the field's shape makes it: NotNull has no browser form on a list, and NotEmpty on a
        // list asks for at least one item.
        internal bool TryFor(Shape fieldShape, out ClientRule rule)
        {
            rule = this;
            var isList = fieldShape.Kind == "array";
            if (Dependence == ShapeDependence.NotNull) return !isList;
            if (Dependence == ShapeDependence.NotEmpty && isList)
                rule = new ClientRule(ClientRuleKind.AtLeastOne, "atLeastOne", Message, null, null, null, Condition, ComparisonShape, ShapeDependence.None);
            return true;
        }

        internal ClientRule PrefixedBy(string prefix, ClientCondition outer) =>
            new ClientRule(
                Kind,
                Name,
                Message,
                Constraint,
                Peer?.PrefixedBy(prefix),
                Comparison,
                ClientCondition.Combine(outer, Condition.PrefixedBy(prefix)),
                ComparisonShape,
                Dependence);

        private const string Required = "required";

        private static ClientRule Plain(ClientRuleKind kind, string name, string message) =>
            new ClientRule(kind, name, message, null, null, null, ClientCondition.Always, Shape.None, ShapeDependence.None);

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

        private static decimal? AsNumber(object? value) => value switch
        {
            byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal =>
                Convert.ToDecimal(value, CultureInfo.InvariantCulture),
            _ => null,
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
