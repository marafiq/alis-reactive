namespace Alis.Reactive.UnitTests;

public class MixedPayload
{
    public string? Value { get; set; }
    public int Count { get; set; }
}

/// <summary>
/// Tests that a single pipeline supports any sequence of commands and conditions:
///   command → condition → command → condition → command
/// Each When().Then().Else() block is independent — both evaluate, not first-match-wins.
/// </summary>
[TestFixture]
public class WhenMixingCommandsAndConditions : PlanTestBase
{
    [Test]
    public Task Two_independent_when_blocks_both_produce_entries()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.When(args, x => x.Value).Eq("hello")
                .Then(t => t.Element("r1").SetText("yes"))
                .Else(e => e.Element("r1").SetText("no"));
            p.When(args, x => x.Count).Gt(5)
                .Then(t => t.Element("r2").SetText(">5"))
                .Else(e => e.Element("r2").SetText("<=5"));
        });

        var json = plan.Render();
        AssertSchemaValid(json);

        // Both conditions must be in the plan — r1 AND r2
        Assert.That(json, Does.Contain("r1"), "First condition (r1) must not be overwritten");
        Assert.That(json, Does.Contain("r2"), "Second condition (r2) must be present");
        Assert.That(json, Does.Contain("hello"), "First guard operand must be in plan");
        return VerifyJson(json);
    }

    [Test]
    public Task Commands_before_first_when()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Element("echo").SetText("before");
            p.When(args, x => x.Value).Eq("hello")
                .Then(t => t.Element("r1").SetText("matched"))
                .Else(e => e.Element("r1").SetText("nope"));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("before"));
        Assert.That(json, Does.Contain("r1"));
        return VerifyJson(json);
    }

    [Test]
    public Task Commands_after_last_when()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.When(args, x => x.Value).Eq("hello")
                .Then(t => t.Element("r1").SetText("matched"))
                .Else(e => e.Element("r1").SetText("nope"));
            p.Element("footer").SetText("after");
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("r1"));
        Assert.That(json, Does.Contain("after"));
        return VerifyJson(json);
    }

    [Test]
    public Task Full_mix_commands_and_conditions()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Element("echo").SetText("before");
            p.When(args, x => x.Value).Eq("hello")
                .Then(t => t.Element("r1").SetText("yes"))
                .Else(e => e.Element("r1").SetText("no"));
            p.When(args, x => x.Count).Gt(5)
                .Then(t => t.Element("r2").SetText(">5"))
                .Else(e => e.Element("r2").SetText("<=5"));
            p.Element("footer").SetText("after");
        });

        var json = plan.Render();
        AssertSchemaValid(json);

        // All four pieces must be present
        Assert.That(json, Does.Contain("before"), "Pre-command present");
        Assert.That(json, Does.Contain("r1"), "First condition present");
        Assert.That(json, Does.Contain("r2"), "Second condition present");
        Assert.That(json, Does.Contain("after"), "Post-command present");
        return VerifyJson(json);
    }

    [Test]
    public Task Single_when_still_works()
    {
        // Regression — existing single-when pattern must not break
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Element("echo").SetText("before");
            p.When(args, x => x.Value).Eq("hello")
                .Then(t => t.Element("r1").SetText("yes"))
                .Else(e => e.Element("r1").SetText("no"));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("before"));
        Assert.That(json, Does.Contain("r1"));
        return VerifyJson(json);
    }

    [Test]
    public Task Commands_only_still_produces_sequential()
    {
        // Regression — no conditions = sequential reaction
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.Element("a").SetText("one");
            p.Element("b").SetText("two");
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("sequential"));
        Assert.That(json, Does.Not.Contain("conditional"));
        return VerifyJson(json);
    }

    [Test]
    public Task ElseIf_chain_is_one_block_not_multiple()
    {
        // ElseIf stays in the same conditional block — not split
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MixedPayload>("test", (args, p) =>
        {
            p.When(args, x => x.Count).Gt(100)
                .Then(t => t.Element("tier").SetText("gold"))
                .ElseIf(args, x => x.Count).Gt(50)
                .Then(t => t.Element("tier").SetText("silver"))
                .Else(e => e.Element("tier").SetText("bronze"));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("gold"));
        Assert.That(json, Does.Contain("silver"));
        Assert.That(json, Does.Contain("bronze"));
        return VerifyJson(json);
    }
}
