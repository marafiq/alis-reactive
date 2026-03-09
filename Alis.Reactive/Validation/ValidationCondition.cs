using System.Text.Json.Serialization;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// A simple condition for conditional validation rules.
    /// Operators: "truthy", "falsy", "eq", "neq".
    /// </summary>
    public sealed class ValidationCondition
    {
        /// <summary>Property name to check (e.g. "IsEmployed").</summary>
        public string Field { get; }

        /// <summary>Operator: "truthy", "falsy", "eq", "neq".</summary>
        public string Op { get; }

        /// <summary>Comparison value for "eq" and "neq" operators.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Value { get; }

        public ValidationCondition(string field, string op, object? value = null)
        {
            Field = field;
            Op = op;
            Value = value;
        }
    }
}
