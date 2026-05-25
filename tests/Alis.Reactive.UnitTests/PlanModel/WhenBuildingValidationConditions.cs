using Alis.Reactive.PlanModel;

namespace Alis.Reactive.UnitTests.PlanModel;

[TestFixture]
public class WhenBuildingValidationConditions
{
    [Test]
    public void validation_condition_rejects_confirm_prompt_anywhere_in_the_tree()
    {
        var condition = Condition.All(
            Condition.Compare(
                CompareOperator.Truthy,
                ComparisonOperands.Unary(ValueProducer.Literal(true), Shape.Boolean)),
            Condition.Confirm("Continue?"));

        var exception = Assert.Throws<ArgumentException>(
            () => ValidationCondition.FromDeterministicCondition(condition));

        Assert.That(exception!.Message, Does.Contain("Validation activation conditions must be deterministic"));
        Assert.That(exception.Message, Does.Contain("Confirm"));
    }
}
