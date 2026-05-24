using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator.UnitTests;

internal static class ValidationIntentAssertions
{
    internal static object? ConstraintValue(this ValidationRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        return rule.Execution.Constraint switch
        {
            NoValidationRuleOperandIntent => null,
            LiteralValidationRuleOperandIntent literal => literal.Value,
            RangeValidationRuleOperandIntent range => range.Bounds.ToArray(),
            PeerFieldValidationRuleOperandIntent peerField => peerField.Field,
            _ => throw new InvalidOperationException(
                $"Unknown validation constraint operand '{rule.Execution.Constraint.Kind}'.")
        };
    }

    internal static bool HasConstraintOperand(this ValidationRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        return rule.Execution.Constraint is not NoValidationRuleOperandIntent;
    }

    internal static string? PeerFieldName(this ValidationRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        return rule.Execution.OtherValue switch
        {
            NoValidationRuleOperandIntent => null,
            PeerFieldValidationRuleOperandIntent peerField => peerField.Field,
            _ => throw new InvalidOperationException(
                $"Expected peer-field operand, found '{rule.Execution.OtherValue.Kind}'.")
        };
    }

    internal static FieldCondition? Condition(this ValidationRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        return rule.Execution.Activation switch
        {
            AlwaysValidationRuleActivationIntent => null,
            ConditionalValidationRuleActivationIntent activation => activation.Condition,
            _ => throw new InvalidOperationException(
                $"Unknown validation activation '{rule.Execution.Activation.Kind}'.")
        };
    }

    internal static object? OperandValue(this FieldCompare compare)
    {
        ArgumentNullException.ThrowIfNull(compare);

        return compare.Operand switch
        {
            NoFieldComparisonOperand => null,
            LiteralFieldComparisonOperand literal => literal.Value,
            ArrayFieldComparisonOperand array => array.Values.ToArray(),
            _ => throw new InvalidOperationException(
                $"Unknown field comparison operand '{compare.Operand.Kind}'.")
        };
    }
}
