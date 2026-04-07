using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// A single validation rule extracted from FluentValidation.
    /// </summary>
    public sealed class ValidationRule
    {
        public string Rule { get; }
        public string Message { get; }
        public object Constraint { get; }
        public string Field { get; }
        public Shape Shape { get; }
        public FieldCondition When { get; }

        internal ValidationRule(string rule, string message, object constraint = null,
            FieldCondition when = null, string field = null, Shape shape = null)
        {
            Rule = rule;
            Message = message;
            Constraint = constraint;
            When = when;
            Field = field;
            Shape = shape ?? Shape.None;
        }
    }
}
