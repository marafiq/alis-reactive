using System.Text.Json.Serialization;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// A single validation rule (one of 18 types: required, empty, minLength, maxLength,
    /// email, regex, url, creditCard, range, exclusiveRange, min, max, gt, lt,
    /// equalTo, notEqual, notEqualTo, atLeastOne).
    /// </summary>
    public sealed class ValidationRule
    {
        /// <summary>Rule type string (e.g. "required", "minLength").</summary>
        public string Rule { get; }

        /// <summary>Error message displayed when validation fails.</summary>
        public string Message { get; }

        /// <summary>
        /// Rule constraint value. Type depends on rule:
        /// number (minLength, maxLength, min, max), string (regex, notEqual),
        /// [value,value] (range, exclusiveRange), field name (equalTo, notEqualTo).
        /// Null when not applicable.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Constraint { get; }

        /// <summary>
        /// Cross-property binding path — when present, the comparison target is read
        /// from another field instead of using a fixed constraint value.
        /// Mutually exclusive with Constraint for comparison rules (min, max, gt, lt, equalTo, notEqualTo).
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Field { get; }

        /// <summary>
        /// Coercion type for comparison — derived from TProperty at extraction time.
        /// "number" for numeric types, "date" for DateTime/DateOnly, omitted for string comparison.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? CoerceAs { get; }

        /// <summary>
        /// Optional condition — rule only applies when condition is met.
        /// Used for conditional validation (e.g. "required when IsEmployed is truthy").
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ValidationCondition? When { get; }

        internal ValidationRule(string rule, string message, object? constraint = null,
            ValidationCondition? when = null, string? field = null, string? coerceAs = null)
        {
            Rule = rule;
            Message = message;
            Constraint = constraint;
            When = when;
            Field = field;
            CoerceAs = coerceAs;
        }
    }
}
