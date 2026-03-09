using System.Text.Json.Serialization;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// A single validation rule (one of 11 types: required, minLength, maxLength,
    /// email, regex, url, range, min, max, equalTo, atLeastOne).
    /// </summary>
    public sealed class ValidationRule
    {
        /// <summary>Rule type string (e.g. "required", "minLength").</summary>
        public string Rule { get; }

        /// <summary>Error message displayed when validation fails.</summary>
        public string Message { get; }

        /// <summary>
        /// Rule constraint value. Type depends on rule:
        /// number (minLength, maxLength, min, max), string (regex, equalTo),
        /// [number,number] (range), bool (required, email, url, atLeastOne).
        /// Null when not applicable.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Constraint { get; }

        /// <summary>
        /// Optional condition — rule only applies when condition is met.
        /// Used for conditional validation (e.g. "required when IsEmployed is truthy").
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ValidationCondition? When { get; }

        public ValidationRule(string rule, string message, object? constraint = null, ValidationCondition? when = null)
        {
            Rule = rule;
            Message = message;
            Constraint = constraint;
            When = when;
        }
    }
}
