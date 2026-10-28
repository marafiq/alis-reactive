namespace Alis.Reactive.Framework.PlaywrightTests.Conditions;

/// <summary>
/// When, ElseIf and Else: the first arm whose condition holds runs, in authored order, the Else only when
/// no arm holds, and a branch without an Else runs nothing when its condition fails while the steps after
/// it run. The care plan's arms are pain of 4 or more, then pain of 2 or more, then no action; its alarm
/// is raised at 5. The pain starts at 0.
/// </summary>
[TestFixture]
public sealed class WhenAPageBranches : AppPageTest
{
    private ILocator Line(string id) => Page.Locator($"#{id}");

    private Task PlanCare() => Page.GetByRole(AriaRole.Button, new() { Name = "Plan care", Exact = true }).ClickAsync();

    private Task Raise() => Page.Locator("#pain").GetByRole(AriaRole.Button, new() { Name = "Raise", Exact = true }).ClickAsync();

    private Task Lower() => Page.Locator("#pain").GetByRole(AriaRole.Button, new() { Name = "Lower", Exact = true }).ClickAsync();

    [Test]
    public async Task the_first_arm_that_holds_runs_though_a_later_arm_holds_too()
    {
        await Open("/Conditions/CarePlan");
        await Raise(); await Raise(); await Raise(); await Raise();

        await PlanCare();

        await Expect(Line("plan")).ToHaveTextAsync("Call the doctor");
    }

    [Test]
    public async Task an_else_if_runs_when_the_arms_before_it_do_not_hold_and_its_own_condition_does()
    {
        await Open("/Conditions/CarePlan");
        await Raise(); await Raise(); await Raise();

        await PlanCare();
        await Expect(Line("plan")).ToHaveTextAsync("Give pain relief");

        await Lower(); await Lower();
        await PlanCare();
        await Expect(Line("plan")).ToHaveTextAsync("No action");
    }

    [Test]
    public async Task else_runs_only_when_no_arm_holds()
    {
        await Open("/Conditions/CarePlan");

        await PlanCare();
        await Expect(Line("plan")).ToHaveTextAsync("No action");

        await Raise(); await Raise(); await Raise(); await Raise();
        await PlanCare();
        await Expect(Line("plan")).ToHaveTextAsync("Call the doctor");
    }

    [Test]
    public async Task a_branch_without_else_runs_nothing_when_its_condition_fails_and_the_steps_after_it_run()
    {
        await Open("/Conditions/CarePlan");
        await Raise(); await Raise(); await Raise(); await Raise();

        await PlanCare();
        await Expect(Line("planned-at")).ToHaveTextAsync("4");
        await Expect(Line("alarm")).ToHaveTextAsync("");

        await Raise();
        await PlanCare();
        await Expect(Line("planned-at")).ToHaveTextAsync("5");
        await Expect(Line("alarm")).ToHaveTextAsync("Raise the alarm");
    }
}
