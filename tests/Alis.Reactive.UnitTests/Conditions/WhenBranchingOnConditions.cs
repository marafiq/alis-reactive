using static VerifyNUnit.Verifier;

namespace Alis.Reactive.UnitTests;

public class ScorePayload
{
    public int Score { get; set; }
    public string Status { get; set; } = "";
}

public class UserPayload
{
    public string? Name { get; set; }
    public string Role { get; set; } = "";
    public bool IsActive { get; set; }
}

[TestFixture]
public class WhenBranchingOnConditions : PlanTestBase
{
    [Test]
    public Task Simple_when_then_else()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<ScorePayload>("score-checked", (args, p) =>
            p.When(args, x => x.Score).Gte(90)
                .Then(then => then.Element("result").SetText("Pass"))
                .Else(else_ => else_.Element("result").SetText("Fail"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task When_then_without_else()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<UserPayload>("user-loaded", (args, p) =>
            p.When(args, x => x.Name).NotNull()
                .Then(then => then.Element("greeting").SetText("Welcome"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task And_composition()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<ScorePayload>("score-checked", (args, p) =>
            p.When(args, x => x.Score).Gte(90)
                .And(g => g.When(args, x => x.Status).Eq("active"))
                .Then(then => then.Element("result").SetText("Pass"))
                .Else(else_ => else_.Element("result").SetText("Fail"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Or_composition()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<UserPayload>("role-checked", (args, p) =>
            p.When(args, x => x.Role).Eq("admin")
                .Or(g => g.When(args, x => x.Role).Eq("superuser"))
                .Then(then => then.Element("panel").Show())
                .Else(else_ => else_.Element("panel").Hide())
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task ElseIf_multi_branch()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<ScorePayload>("grade-check", (args, p) =>
            p.When(args, x => x.Score).Gte(90)
                .Then(then => then.Element("grade").SetText("A"))
                .ElseIf(args, x => x.Score).Gte(80)
                .Then(then => then.Element("grade").SetText("B"))
                .ElseIf(args, x => x.Score).Gte(70)
                .Then(then => then.Element("grade").SetText("C"))
                .Else(else_ => else_.Element("grade").SetText("F"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Presence_operators()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<UserPayload>("status-check", (args, p) =>
            p.When(args, x => x.IsActive).Truthy()
                .Then(then =>
                {
                    then.Element("badge").AddClass("active");
                    then.Element("status").SetText("Online");
                })
                .Else(else_ =>
                {
                    else_.Element("badge").AddClass("inactive");
                    else_.Element("status").SetText("Offline");
                })
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
