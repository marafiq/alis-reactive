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

public class MembershipPayload
{
    public string Category { get; set; } = "";
    public int Priority { get; set; }
}

public class TextPayload
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}

public class RangePayload
{
    public int Age { get; set; }
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

    // ── In / NotIn membership ──

    [Test]
    public Task In_membership()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MembershipPayload>("category-check", (args, p) =>
            p.When(args, x => x.Category).In("A", "B", "C")
                .Then(then => then.Element("result").SetText("In Group"))
                .Else(else_ => else_.Element("result").SetText("Not In Group"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task NotIn_membership()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<MembershipPayload>("category-exclude", (args, p) =>
            p.When(args, x => x.Category).NotIn("X", "Y")
                .Then(then => then.Element("result").SetText("Allowed"))
                .Else(else_ => else_.Element("result").SetText("Blocked"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ── Between range ──

    [Test]
    public Task Between_range()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<RangePayload>("age-check", (args, p) =>
            p.When(args, x => x.Age).Between(18, 65)
                .Then(then => then.Element("result").SetText("Working Age"))
                .Else(else_ => else_.Element("result").SetText("Outside Range"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ── Text operators ──

    [Test]
    public Task Contains_text()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<TextPayload>("text-check", (args, p) =>
            p.When(args, x => x.Name).Contains("admin")
                .Then(then => then.Element("result").SetText("Has admin"))
                .Else(else_ => else_.Element("result").SetText("No admin"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task StartsWith_text()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<TextPayload>("starts-check", (args, p) =>
            p.When(args, x => x.Email).StartsWith("admin@")
                .Then(then => then.Element("result").SetText("Admin email"))
                .Else(else_ => else_.Element("result").SetText("Other email"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Matches_regex()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<TextPayload>("regex-check", (args, p) =>
            p.When(args, x => x.Email).Matches(@"^[a-z]+@[a-z]+\.[a-z]+$")
                .Then(then => then.Element("result").SetText("Valid"))
                .Else(else_ => else_.Element("result").SetText("Invalid"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task MinLength_text()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<TextPayload>("minlen-check", (args, p) =>
            p.When(args, x => x.Name).MinLength(3)
                .Then(then => then.Element("result").SetText("Long enough"))
                .Else(else_ => else_.Element("result").SetText("Too short"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ── IsEmpty / NotEmpty ──

    [Test]
    public Task IsEmpty_presence()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<TextPayload>("empty-check", (args, p) =>
            p.When(args, x => x.Name).IsEmpty()
                .Then(then => then.Element("result").SetText("Empty"))
                .Else(else_ => else_.Element("result").SetText("Has value"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ── InvertGuard (NOT) ──

    [Test]
    public Task Not_inverts_guard()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<UserPayload>("not-check", (args, p) =>
            p.When(args, x => x.Role).Eq("admin").Not()
                .Then(then => then.Element("result").SetText("Not admin"))
                .Else(else_ => else_.Element("result").SetText("Is admin"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ── ConfirmGuard ──

    [Test]
    public Task Confirm_guard()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<ScorePayload>("delete-check", (args, p) =>
            p.Confirm("Are you sure you want to delete?")
                .Then(then => then.Dispatch("do-delete"))
                .Else(else_ => else_.Element("status").SetText("Cancelled"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ── Confirm composed with value guard ──

    [Test]
    public Task Confirm_composed_with_value_guard()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<ScorePayload>("composed-confirm", (args, p) =>
            p.When(args, x => x.Score).Gt(0)
                .And(g => g.Confirm("Delete this record?"))
                .Then(then => then.Dispatch("do-delete"))
                .Else(else_ => else_.Element("status").SetText("Aborted"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ── Per-action When guard ──

    [Test]
    public Task Per_action_when_guard()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<ScorePayload>("per-action-check", (args, p) =>
        {
            p.Element("result").SetText("Always runs");
            var el = p.Element("bonus");
            el.SetText("Bonus!");
            el.When(args, x => x.Score, csb => csb.Gte(90));
        });
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ── Direct And (matching ElseIf pattern) ──

    [Test]
    public Task Direct_and_syntax()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<ScorePayload>("direct-and-check", (args, p) =>
            p.When(args, x => x.Score).Gte(90)
                .And(args, x => x.Status).Eq("active")
                .Then(then => then.Element("result").SetText("Pass"))
                .Else(else_ => else_.Element("result").SetText("Fail"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    // ── Direct Or syntax ──

    [Test]
    public Task Direct_or_syntax()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<UserPayload>("direct-or-check", (args, p) =>
            p.When(args, x => x.Role).Eq("admin")
                .Or(args, x => x.Role).Eq("superuser")
                .Then(then => then.Element("panel").Show())
                .Else(else_ => else_.Element("panel").Hide())
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
