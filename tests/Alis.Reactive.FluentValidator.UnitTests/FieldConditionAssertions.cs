using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator.UnitTests;

internal static class FieldConditionAssertions
{
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
