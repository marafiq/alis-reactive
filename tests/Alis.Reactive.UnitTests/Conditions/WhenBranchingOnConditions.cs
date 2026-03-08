using System;
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

public class LongPayload
{
    public long Balance { get; set; }
}

public class DoublePayload
{
    public double Temperature { get; set; }
}

public class FloatPayload
{
    public float Rate { get; set; }
}

public class DatePayload
{
    public DateTime Deadline { get; set; }
}

public class NullablePayload
{
    public int? NullableScore { get; set; }
    public DateTime? NullableDate { get; set; }
    public bool? NullableBool { get; set; }
}

[TestFixture]
public class WhenBranchingOnConditions : PlanTestBase
{
    // ── int ──

    [Test]
    public Task Int_gte_with_elseif()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<ScorePayload>("score-checked", (args, p) =>
            p.When(args, x => x.Score).Gte(90)
                .Then(then => then.Element("grade").SetText("A"))
                .ElseIf(args, x => x.Score).Gte(80)
                .Then(then => then.Element("grade").SetText("B"))
                .Else(else_ => else_.Element("grade").SetText("F"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ── long ──

    [Test]
    public Task Long_gt_threshold()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<LongPayload>("balance-check", (args, p) =>
            p.When(args, x => x.Balance).Gt(1000000L)
                .Then(then => then.Element("result").SetText("High Value"))
                .Else(else_ => else_.Element("result").SetText("Standard"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ── double ──

    [Test]
    public Task Double_gt_comparison()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<DoublePayload>("temp-check", (args, p) =>
            p.When(args, x => x.Temperature).Gt(98.6)
                .Then(then => then.Element("result").SetText("Fever"))
                .Else(else_ => else_.Element("result").SetText("Normal"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ── float ──

    [Test]
    public Task Float_lte_comparison()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<FloatPayload>("rate-check", (args, p) =>
            p.When(args, x => x.Rate).Lte(0.5f)
                .Then(then => then.Element("result").SetText("Low"))
                .Else(else_ => else_.Element("result").SetText("High"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ── bool ──

    [Test]
    public Task Bool_truthy_falsy()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<UserPayload>("status-check", (args, p) =>
            p.When(args, x => x.IsActive).Truthy()
                .Then(then => then.Element("status").SetText("Online"))
                .Else(else_ => else_.Element("status").SetText("Offline"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ── string ──

    [Test]
    public Task String_eq_comparison()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<UserPayload>("role-check", (args, p) =>
            p.When(args, x => x.Role).Eq("admin")
                .Then(then => then.Element("panel").Show())
                .Else(else_ => else_.Element("panel").Hide())
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ── string? (nullable ref type) ──

    [Test]
    public Task Nullable_string_not_null()
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

    // ── DateTime ──

    [Test]
    public Task DateTime_gt_comparison()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<DatePayload>("deadline-check", (args, p) =>
            p.When(args, x => x.Deadline).Gt(new DateTime(2026, 6, 1))
                .Then(then => then.Element("result").SetText("On Time"))
                .Else(else_ => else_.Element("result").SetText("Overdue"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ── int? (nullable value type) ──

    [Test]
    public Task Nullable_int_is_null()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<NullablePayload>("nullable-check", (args, p) =>
            p.When(args, x => x.NullableScore).IsNull()
                .Then(then => then.Element("result").SetText("No Score"))
                .Else(else_ => else_.Element("result").SetText("Has Score"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ── DateTime? (nullable date) ──

    [Test]
    public Task Nullable_datetime_is_null()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<NullablePayload>("date-nullable-check", (args, p) =>
            p.When(args, x => x.NullableDate).IsNull()
                .Then(then => then.Element("result").SetText("No Date"))
                .Else(else_ => else_.Element("result").SetText("Has Date"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ── AND composition (int + string — mixed types) ──

    [Test]
    public Task And_mixed_types()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<ScorePayload>("compound-check", (args, p) =>
            p.When(args, x => x.Score).Gte(90)
                .And(g => g.When(args, x => x.Status).Eq("active"))
                .Then(then => then.Element("result").SetText("Pass"))
                .Else(else_ => else_.Element("result").SetText("Fail"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ── OR composition (string alternatives) ──

    [Test]
    public Task Or_string_alternatives()
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
}
