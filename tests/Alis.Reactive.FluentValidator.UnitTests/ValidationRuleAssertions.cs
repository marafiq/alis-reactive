using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator.UnitTests;

internal static class ValidationRuleAssertions
{
    internal static void HasNoOperand(this ValidationRule rule)
    {
        Assert.That(rule.Operand, Is.InstanceOf<NoValidationRuleOperand>());
    }

    internal static LiteralValidationRuleOperand LiteralOperand(this ValidationRule rule)
    {
        Assert.That(rule.Operand, Is.InstanceOf<LiteralValidationRuleOperand>());
        return (LiteralValidationRuleOperand)rule.Operand;
    }

    internal static RangeValidationRuleOperand RangeOperand(this ValidationRule rule)
    {
        Assert.That(rule.Operand, Is.InstanceOf<RangeValidationRuleOperand>());
        return (RangeValidationRuleOperand)rule.Operand;
    }

    internal static PeerFieldValidationRuleOperand PeerOperand(this ValidationRule rule)
    {
        Assert.That(rule.Operand, Is.InstanceOf<PeerFieldValidationRuleOperand>());
        return (PeerFieldValidationRuleOperand)rule.Operand;
    }

    internal static FieldCondition WhenCondition(this ValidationRule rule)
    {
        Assert.That(rule.Activation, Is.InstanceOf<ConditionalValidationRuleActivation>());
        return ((ConditionalValidationRuleActivation)rule.Activation).Condition;
    }

    internal static void IsAlwaysActive(this ValidationRule rule)
    {
        Assert.That(rule.Activation, Is.InstanceOf<AlwaysValidationRuleActivation>());
    }

    internal static void HasSameOperandAs(this ValidationRule actual, ValidationRule expected)
    {
        Assert.That(actual.Operand.Kind, Is.EqualTo(expected.Operand.Kind));

        switch (actual.Operand)
        {
            case NoValidationRuleOperand:
                break;
            case LiteralValidationRuleOperand actualLiteral:
                Assert.That(actualLiteral.Value, Is.EqualTo(((LiteralValidationRuleOperand)expected.Operand).Value));
                Assert.That(actualLiteral.Shape, Is.EqualTo(((LiteralValidationRuleOperand)expected.Operand).Shape));
                break;
            case RangeValidationRuleOperand actualRange:
                var expectedRange = (RangeValidationRuleOperand)expected.Operand;
                Assert.That(actualRange.LowerBound, Is.EqualTo(expectedRange.LowerBound));
                Assert.That(actualRange.UpperBound, Is.EqualTo(expectedRange.UpperBound));
                Assert.That(actualRange.EndpointShape, Is.EqualTo(expectedRange.EndpointShape));
                break;
            case PeerFieldValidationRuleOperand actualPeer:
                var expectedPeer = (PeerFieldValidationRuleOperand)expected.Operand;
                Assert.That(actualPeer.FieldName, Is.EqualTo(expectedPeer.FieldName));
                Assert.That(actualPeer.Shape, Is.EqualTo(expectedPeer.Shape));
                break;
            default:
                throw new AssertionException(
                    "Unknown validation rule operand '" + actual.Operand.GetType().FullName + "'.");
        }
    }
}
