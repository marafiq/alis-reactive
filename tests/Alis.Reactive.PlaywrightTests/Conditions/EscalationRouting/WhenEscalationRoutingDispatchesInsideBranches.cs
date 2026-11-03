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

    [Test]
    public async Task trigger_branch_dispatches_the_isolation_route()
    {
        await NavigateAndBoot();

        await ClickWhenStable(IsolationReviewButton);

        await Expect(TriggerPrecheck).ToHaveTextAsync("Escalation reviewed");
        await Expect(TriggerAudit).ToHaveTextAsync("Isolation route dispatched");
        await Expect(TriggerResult).ToHaveTextAsync("Isolation review");
        await Expect(TriggerBadge).ToBeVisibleAsync();
        await Expect(TriggerBadge).ToHaveTextAsync("Isolation");
        await Expect(TriggerSummary).ToHaveTextAsync("Escalation complete");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task trigger_branch_dispatches_the_expedited_route_when_override_wins()
    {
        await NavigateAndBoot();

        await ClickWhenStable(OverrideReviewButton);

        await Expect(TriggerPrecheck).ToHaveTextAsync("Escalation reviewed");
        await Expect(TriggerAudit).ToHaveTextAsync("Expedited route dispatched");
        await Expect(TriggerResult).ToHaveTextAsync("Expedited review");
        await Expect(TriggerBadge).ToBeVisibleAsync();
        await Expect(TriggerBadge).ToHaveTextAsync("Expedited");
        await Expect(TriggerSummary).ToHaveTextAsync("Escalation complete");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task trigger_rerouting_to_routine_clears_the_stale_badge()
    {
        await NavigateAndBoot();

        await ClickWhenStable(IsolationReviewButton);
        await Expect(TriggerPrecheck).ToHaveTextAsync("Escalation reviewed");
        await Expect(TriggerAudit).ToHaveTextAsync("Isolation route dispatched");
        await Expect(TriggerResult).ToHaveTextAsync("Isolation review");
        await Expect(TriggerBadge).ToBeVisibleAsync();
        await Expect(TriggerBadge).ToHaveTextAsync("Isolation");
        await Expect(TriggerSummary).ToHaveTextAsync("Escalation complete");

        await ClickWhenStable(RoutineReviewButton);
        await Expect(TriggerPrecheck).ToHaveTextAsync("Escalation reviewed");
        await Expect(TriggerAudit).ToHaveTextAsync("Routine route dispatched");
        await Expect(TriggerResult).ToHaveTextAsync("Routine review");
        await Expect(TriggerBadge).ToBeHiddenAsync();
        await Expect(TriggerSummary).ToHaveTextAsync("Escalation complete");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_branch_dispatches_isolation_when_override_is_enabled()
    {
        await NavigateAndBoot();

        await AssessmentScore.FillAndBlur("95");
        await Expect(ComponentPrecheck).ToHaveTextAsync("Escalation evaluated", new() { Timeout = 5000 });
        await Expect(ComponentAudit).ToHaveTextAsync("Monitor route dispatched", new() { Timeout = 5000 });
        await Expect(ComponentResult).ToHaveTextAsync("Monitor closely", new() { Timeout = 5000 });
        await Expect(ComponentBadge).ToBeVisibleAsync();
        await Expect(ComponentBadge).ToHaveTextAsync("Monitor");
        await Expect(ComponentSummary).ToHaveTextAsync("Escalation decision complete");

        await SupervisorOverride.Toggle();

        await Expect(ComponentPrecheck).ToHaveTextAsync("Escalation evaluated", new() { Timeout = 5000 });
        await Expect(ComponentAudit).ToHaveTextAsync("Isolation route dispatched", new() { Timeout = 5000 });
        await Expect(ComponentResult).ToHaveTextAsync("Escalate now", new() { Timeout = 5000 });
        await Expect(ComponentBadge).ToBeVisibleAsync();
        await Expect(ComponentBadge).ToHaveTextAsync("Escalate");
        await Expect(ComponentSummary).ToHaveTextAsync("Escalation decision complete");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_branch_dispatches_isolation_when_memory_care_wins_the_nested_or()
    {
        await NavigateAndBoot();

        await AssessmentScore.FillAndBlur("95");
        await Expect(ComponentPrecheck).ToHaveTextAsync("Escalation evaluated", new() { Timeout = 5000 });
        await Expect(ComponentAudit).ToHaveTextAsync("Monitor route dispatched", new() { Timeout = 5000 });
        await Expect(ComponentResult).ToHaveTextAsync("Monitor closely", new() { Timeout = 5000 });
        await Expect(ComponentBadge).ToBeVisibleAsync();
        await Expect(ComponentBadge).ToHaveTextAsync("Monitor");
        await Expect(ComponentSummary).ToHaveTextAsync("Escalation decision complete");

        await CareTrack.Select("Memory Care");

        await Expect(ComponentPrecheck).ToHaveTextAsync("Escalation evaluated", new() { Timeout = 5000 });
        await Expect(ComponentAudit).ToHaveTextAsync("Isolation route dispatched", new() { Timeout = 5000 });
        await Expect(ComponentResult).ToHaveTextAsync("Escalate now", new() { Timeout = 5000 });
        await Expect(ComponentBadge).ToBeVisibleAsync();
        await Expect(ComponentBadge).ToHaveTextAsync("Escalate");
        await Expect(ComponentSummary).ToHaveTextAsync("Escalation decision complete");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_rerouting_to_routine_clears_the_stale_monitor_badge()
    {
        await NavigateAndBoot();

        await AssessmentScore.FillAndBlur("70");
        await Expect(ComponentPrecheck).ToHaveTextAsync("Escalation evaluated", new() { Timeout = 5000 });
        await Expect(ComponentAudit).ToHaveTextAsync("Monitor route dispatched", new() { Timeout = 5000 });
        await Expect(ComponentResult).ToHaveTextAsync("Monitor closely", new() { Timeout = 5000 });
        await Expect(ComponentBadge).ToBeVisibleAsync();
        await Expect(ComponentBadge).ToHaveTextAsync("Monitor");
        await Expect(ComponentSummary).ToHaveTextAsync("Escalation decision complete");

        await AssessmentScore.FillAndBlur("40");

        await Expect(ComponentPrecheck).ToHaveTextAsync("Escalation evaluated", new() { Timeout = 5000 });
        await Expect(ComponentAudit).ToHaveTextAsync("Routine route dispatched", new() { Timeout = 5000 });
        await Expect(ComponentResult).ToHaveTextAsync("Routine follow-up", new() { Timeout = 5000 });
        await Expect(ComponentBadge).ToBeHiddenAsync();
        await Expect(ComponentSummary).ToHaveTextAsync("Escalation decision complete");
        AssertNoConsoleErrors();
    }
}
