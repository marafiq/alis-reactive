using Alis.Reactive.PlanModel;

namespace Alis.Reactive.UnitTests.Conditions;

[TestFixture]
public class WhenBuildingConditionalBranches : PlanTestBase
{
    public sealed class Payload
    {
        public string Status { get; set; } = "";
    }

    [Test]
    public void dangling_when_requires_then_branch()
    {
        var plan = CreatePlan();

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            Trigger(plan).CustomEvent<Payload>("changed", (args, p) =>
            {
                p.When(args, x => x.Status).Eq("active");
            });
        });

        Assert.That(ex!.Message, Does.Contain("no executable cases"));
        Assert.That(ex.Message, Does.Contain("Call Then"));
    }

    [Test]
    public void dangling_confirm_requires_then_branch()
    {
        var plan = CreatePlan();

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Confirm("Delete?");
            });
        });

        Assert.That(ex!.Message, Does.Contain("no executable cases"));
        Assert.That(ex.Message, Does.Contain("Confirm"));
    }

    [Test]
    public void composite_conditions_require_at_least_one_term()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Alis.Reactive.PlanModel.Condition.All());

        Assert.That(exception!.Message, Does.Contain("all"));
        Assert.That(exception.Message, Does.Contain("at least one term"));
    }

    [Test]
    public void composite_field_conditions_require_at_least_one_term()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Alis.Reactive.Validation.FieldCondition.Any());

        Assert.That(exception!.Message, Does.Contain("any"));
        Assert.That(exception.Message, Does.Contain("at least one term"));
    }

    [Test]
    public void branch_default_case_must_be_last()
    {
        var defaultCase = BranchCase.Default(Reaction.Dispatch("default"));
        var conditionalCase = BranchCase.Of(
            Condition.Confirm("Continue?"),
            Reaction.Dispatch("continue"));

        var exception = Assert.Throws<InvalidOperationException>(() =>
            Reaction.Branch(new List<BranchCase> { defaultCase, conditionalCase }));

        Assert.That(exception!.Message, Does.Contain("default"));
        Assert.That(exception.Message, Does.Contain("last"));
    }

    [Test]
    public void branch_can_have_only_one_default_case()
    {
        var firstDefault = BranchCase.Default(Reaction.Dispatch("first"));
        var secondDefault = BranchCase.Default(Reaction.Dispatch("second"));

        var exception = Assert.Throws<InvalidOperationException>(() =>
            Reaction.Branch(new List<BranchCase> { firstDefault, secondDefault }));

        Assert.That(exception!.Message, Does.Contain("only one default"));
    }
}
