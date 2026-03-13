using static VerifyNUnit.Verifier;

namespace Alis.Reactive.UnitTests;

public class WorkflowPayload
{
    public string Role { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal Total { get; set; }
    public bool IsActive { get; set; }
    public bool IsAdmin { get; set; }
}

[TestFixture]
public class WhenExecutingAConditionalWorkflow : PlanTestBase
{
    [Test]
    public Task IfElseBranchesExecuteCorrectly()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<WorkflowPayload>("role-check", (args, p) =>
            p.When(args, x => x.Role).Eq("admin")
                .Then(t => t.Element("panel").Show())
                .Else(e => e.Element("panel").Hide())
        );
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    [Test]
    public Task IfElseIfElseBranchesExecuteCorrectly()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<WorkflowPayload>("tier-check", (args, p) =>
            p.When(args, x => x.Total).Gte(1000m)
                .Then(t => t.Element("tier").SetText("Gold"))
                .ElseIf(args, x => x.Total).Gte(500m)
                .Then(t => t.Element("tier").SetText("Silver"))
                .Else(e => e.Element("tier").SetText("Bronze"))
        );
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    [Test]
    public Task MultipleActionsExecuteInsideThen()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<WorkflowPayload>("role-check", (args, p) =>
            p.When(args, x => x.Role).Eq("admin")
                .Then(t =>
                {
                    t.Element("admin-panel").Show();
                    t.Element("delete-btn").Show();
                    t.Dispatch("admin-loaded");
                })
                .Else(e => e.Element("admin-panel").Hide())
        );
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    [Test]
    public Task MultipleActionsExecuteInsideElse()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<WorkflowPayload>("role-check", (args, p) =>
            p.When(args, x => x.Role).Eq("admin")
                .Then(t => t.Element("panel").Show())
                .Else(e =>
                {
                    e.Element("panel").Hide();
                    e.Element("notice").SetText("Contact admin for access");
                    e.Dispatch("access-denied");
                })
        );
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    [Test]
    public Task CompoundConditionsExecuteCorrectly()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<WorkflowPayload>("compound-check", (args, p) =>
            p.When(args, x => x.IsActive).Truthy()
                .And(args, x => x.IsAdmin).Truthy()
                .Then(t => t.Element("admin-panel").Show())
                .Else(e => e.Element("admin-panel").Hide())
        );
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    [Test]
    public Task UnconditionalActionsExecuteAlongsideAConditionBlock()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<WorkflowPayload>("workflow", (args, p) =>
        {
            p.Element("status").SetText("Processing...");
            p.Dispatch("workflow-started");
            p.When(args, x => x.Role).Eq("admin")
                .Then(t => t.Element("admin-panel").Show())
                .Else(e => e.Element("admin-panel").Hide());
        });
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    [Test]
    public Task AThenBranchCanExecuteAnHttpWorkflow()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<WorkflowPayload>("save-check", (args, p) =>
            p.When(args, x => x.Role).Eq("admin")
                .Then(t =>
                    t.Post("/api/admin/save")
                     .Response(r => r.OnSuccess(s => s.Dispatch("saved"))))
                .Else(e => e.Element("error").SetText("Unauthorized"))
        );
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    [Test]
    public Task AnElseBranchCanExecuteAnHttpWorkflow()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<WorkflowPayload>("save-check", (args, p) =>
            p.When(args, x => x.Role).Eq("admin")
                .Then(t =>
                    t.Post("/api/admin/save")
                     .Response(r => r.OnSuccess(s => s.Dispatch("admin-saved"))))
                .Else(e =>
                    e.Post("/api/user/save")
                     .Response(r => r.OnSuccess(s => s.Dispatch("user-saved"))))
        );
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    [Test]
    public Task ABranchCanChooseHttpVersusPlainUiActions()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<WorkflowPayload>("action-check", (args, p) =>
            p.When(args, x => x.IsActive).Truthy()
                .Then(t =>
                    t.Post("/api/process")
                     .Response(r => r.OnSuccess(s => s.Element("result").SetText("Processed"))))
                .Else(e =>
                {
                    e.Element("error").SetText("Account inactive");
                    e.Element("retry-btn").Show();
                })
        );
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    [Test]
    public Task ConfirmBranchesExecuteMultipleActionsCorrectly()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<WorkflowPayload>("delete-check", (args, p) =>
            p.Confirm("Are you sure you want to delete?")
                .Then(t =>
                {
                    t.Element("item").Hide();
                    t.Element("status").SetText("Deleted");
                    t.Dispatch("item-deleted");
                })
                .Else(e => e.Element("status").SetText("Cancelled"))
        );
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }
}
