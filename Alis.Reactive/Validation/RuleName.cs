using System;
using System.Collections.Generic;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// The single source of the client validation rule names. Projected into the
    /// TypeScript <c>ValidationRuleName</c> union by the plan contract generator.
    /// </summary>
    internal sealed class RuleName : IEquatable<RuleName>
    {
        private static readonly Dictionary<string, RuleName> Known =
            new Dictionary<string, RuleName>(StringComparer.Ordinal)
            {
                { "required", new RuleName("required") },
                { "empty", new RuleName("empty") },
                { "minLength", new RuleName("minLength") },
                { "maxLength", new RuleName("maxLength") },
                { "email", new RuleName("email") },
                { "regex", new RuleName("regex") },
                { "url", new RuleName("url") },
                { "creditCard", new RuleName("creditCard") },
                { "range", new RuleName("range") },
                { "exclusiveRange", new RuleName("exclusiveRange") },
                { "min", new RuleName("min") },
                { "max", new RuleName("max") },
                { "gt", new RuleName("gt") },
                { "lt", new RuleName("lt") },
                { "equalTo", new RuleName("equalTo") },
                { "notEqual", new RuleName("notEqual") },
                { "notEqualTo", new RuleName("notEqualTo") },
                { "atLeastOne", new RuleName("atLeastOne") },
            };

        private RuleName(string value)
        {
            Value = value;
        }

        internal string Value { get; }

        internal static RuleName Required => Known["required"];
        internal static RuleName Empty => Known["empty"];
        internal static RuleName MinLength => Known["minLength"];
        internal static RuleName MaxLength => Known["maxLength"];
        internal static RuleName Email => Known["email"];
        internal static RuleName Regex => Known["regex"];
        internal static RuleName Url => Known["url"];
        internal static RuleName CreditCard => Known["creditCard"];
        internal static RuleName Range => Known["range"];
        internal static RuleName ExclusiveRange => Known["exclusiveRange"];
        internal static RuleName Min => Known["min"];
        internal static RuleName Max => Known["max"];
        internal static RuleName Gt => Known["gt"];
        internal static RuleName Lt => Known["lt"];
        internal static RuleName EqualTo => Known["equalTo"];
        internal static RuleName NotEqual => Known["notEqual"];
        internal static RuleName NotEqualTo => Known["notEqualTo"];
        internal static RuleName AtLeastOne => Known["atLeastOne"];
        internal static IReadOnlyCollection<string> Values => Known.Keys;

        internal static RuleName From(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (Known.TryGetValue(value, out var name)) return name;
            throw new ArgumentException(
                "Unknown validation rule '" + value + "'.",
                nameof(value));
        }

        public bool Equals(RuleName? other) =>
            other != null && string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object? obj) => Equals(obj as RuleName);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

        public override string ToString() => Value;
    }
}
