namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Metadata for a conditional validation rule declared via IConditionalRuleProvider.
    /// </summary>
    public sealed class ConditionalRuleMetadata
    {
        public string PropertyName { get; }
        public string Rule { get; }
        public string Message { get; }
        public object? Constraint { get; }
        public ValidationCondition When { get; }

        public ConditionalRuleMetadata(
            string propertyName,
            string rule,
            string message,
            ValidationCondition when,
            object? constraint = null)
        {
            PropertyName = propertyName;
            Rule = rule;
            Message = message;
            When = when;
            Constraint = constraint;
        }
    }
}
