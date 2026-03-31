using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.EscalationRouting;

namespace Alis.Reactive.PlaywrightTests.Conditions.EscalationRouting;

[TestFixture]
public class WhenEscalationRoutingDispatchesInsideBranches : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/EscalationRouting";

    private PagePlan<EscalationRoutingModel> _plan = null!;

    private ILocator IsolationReviewButton => Page.Locator("#btn-escalation-isolation");
    private ILocator OverrideReviewButton => Page.Locator("#btn-escalation-override");
    private ILocator RoutineReviewButton => Page.Locator("#btn-escalation-routine");

    private ILocator TriggerPrecheck => _plan.Element("trigger-precheck");
    private ILocator TriggerAudit => _plan.Element("trigger-audit");
    private ILocator TriggerResult => _plan.Element("trigger-result");
    private ILocator TriggerBadge => _plan.Element("trigger-badge");
    private ILocator TriggerSummary => _plan.Element("trigger-summary");

    private ILocator ComponentPrecheck => _plan.Element("component-precheck");
    private ILocator ComponentAudit => _plan.Element("component-audit");
    private ILocator ComponentResult => _plan.Element("component-result");
    private ILocator ComponentBadge => _plan.Element("component-badge");
    private ILocator ComponentSummary => _plan.Element("component-summary");

    private NumericTextBoxLocator AssessmentScore => _plan.NumericTextBox(m => m.AssessmentScore);
    private SwitchLocator SupervisorOverride => _plan.Switch(m => m.SupervisorOverride);
    private DropDownListLocator CareTrack => _plan.DropDownList(m => m.CareTrack);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#btn-escalation-isolation");
        _plan = await PagePlan<EscalationRoutingModel>.FromPage(Page);
    }

    private async Task AssertTriggerRouteAsync(string expectedAudit, string expectedResult, string? expectedBadge)
    {
        await Expect(TriggerPrecheck).ToHaveTextAsync("Escalation reviewed");
        await Expect(TriggerAudit).ToHaveTextAsync(expectedAudit);
        await Expect(TriggerResult).ToHaveTextAsync(expectedResult);
        if (expectedBadge is null)
        {
            await Expect(TriggerBadge).ToBeHiddenAsync();
        }
        else
        {
            await Expect(TriggerBadge).ToBeVisibleAsync();
            await Expect(TriggerBadge).ToHaveTextAsync(expectedBadge);
        }

        await Expect(TriggerSummary).ToHaveTextAsync("Escalation complete");
    }

    private async Task AssertComponentRouteAsync(string expectedAudit, string expectedResult, string? expectedBadge)
    {
        await Expect(ComponentPrecheck).ToHaveTextAsync("Escalation evaluated", new() { Timeout = 5000 });
        await Expect(ComponentAudit).ToHaveTextAsync(expectedAudit, new() { Timeout = 5000 });
        await Expect(ComponentResult).ToHaveTextAsync(expectedResult, new() { Timeout = 5000 });
        if (expectedBadge is null)
        {
            await Expect(ComponentBadge).ToBeHiddenAsync();
        }
        else
        {
            await Expect(ComponentBadge).ToBeVisibleAsync();
            await Expect(ComponentBadge).ToHaveTextAsync(expectedBadge);
        }

        await Expect(ComponentSummary).ToHaveTextAsync("Escalation decision complete");
    }

    [Test]
    public async Task trigger_branch_dispatches_the_isolation_route()
    {
        await NavigateAndBoot();

        await ClickWhenStable(IsolationReviewButton);

        await AssertTriggerRouteAsync("Isolation route dispatched", "Isolation review", "Isolation");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task trigger_branch_dispatches_the_expedited_route_when_override_wins()
    {
        await NavigateAndBoot();

        await ClickWhenStable(OverrideReviewButton);

        await AssertTriggerRouteAsync("Expedited route dispatched", "Expedited review", "Expedited");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task trigger_rerouting_to_routine_clears_the_stale_badge()
    {
        await NavigateAndBoot();

        await ClickWhenStable(IsolationReviewButton);
        await AssertTriggerRouteAsync("Isolation route dispatched", "Isolation review", "Isolation");

        await ClickWhenStable(RoutineReviewButton);
        await AssertTriggerRouteAsync("Routine route dispatched", "Routine review", null);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_branch_dispatches_isolation_when_override_is_enabled()
    {
        await NavigateAndBoot();

        await AssessmentScore.FillAndBlur("95");
        await AssertComponentRouteAsync("Monitor route dispatched", "Monitor closely", "Monitor");

        await SupervisorOverride.Toggle();

        await AssertComponentRouteAsync("Isolation route dispatched", "Escalate now", "Escalate");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_branch_dispatches_isolation_when_memory_care_wins_the_nested_or()
    {
        await NavigateAndBoot();

        await AssessmentScore.FillAndBlur("95");
        await AssertComponentRouteAsync("Monitor route dispatched", "Monitor closely", "Monitor");

        await CareTrack.Select("Memory Care");

        await AssertComponentRouteAsync("Isolation route dispatched", "Escalate now", "Escalate");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_rerouting_to_routine_clears_the_stale_monitor_badge()
    {
        await NavigateAndBoot();

        await AssessmentScore.FillAndBlur("70");
        await AssertComponentRouteAsync("Monitor route dispatched", "Monitor closely", "Monitor");

        await AssessmentScore.FillAndBlur("40");

        await AssertComponentRouteAsync("Routine route dispatched", "Routine follow-up", null);
        AssertNoConsoleErrors();
    }
}
