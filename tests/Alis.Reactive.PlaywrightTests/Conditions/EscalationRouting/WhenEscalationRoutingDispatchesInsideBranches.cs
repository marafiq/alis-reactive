using Alis.Reactive.PlaywrightTests.Support.Controls;

namespace Alis.Reactive.PlaywrightTests.Conditions.EscalationRouting;

[TestFixture]
public class WhenEscalationRoutingDispatchesInsideBranches : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/EscalationRouting";
    private EscalationRoutingPage _page = null!;

    private ILocator IsolationReviewButton => Page.Locator("#btn-escalation-isolation");
    private ILocator OverrideReviewButton => Page.Locator("#btn-escalation-override");
    private ILocator RoutineReviewButton => Page.Locator("#btn-escalation-routine");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#btn-escalation-isolation");
        _page = new EscalationRoutingPage(Page);
    }

    private async Task AssertTriggerRouteAsync(string expectedAudit, string expectedResult, string? expectedBadge)
    {
        await Expect(_page.TriggerPrecheck).ToHaveTextAsync("Escalation reviewed");
        await Expect(_page.TriggerAudit).ToHaveTextAsync(expectedAudit);
        await Expect(_page.TriggerResult).ToHaveTextAsync(expectedResult);
        if (expectedBadge is null)
        {
            await Expect(_page.TriggerBadge).ToBeHiddenAsync();
        }
        else
        {
            await Expect(_page.TriggerBadge).ToBeVisibleAsync();
            await Expect(_page.TriggerBadge).ToHaveTextAsync(expectedBadge);
        }

        await Expect(_page.TriggerSummary).ToHaveTextAsync("Escalation complete");
    }

    private async Task AssertComponentRouteAsync(string expectedAudit, string expectedResult, string? expectedBadge)
    {
        await Expect(_page.ComponentPrecheck).ToHaveTextAsync("Escalation evaluated", new() { Timeout = 5000 });
        await Expect(_page.ComponentAudit).ToHaveTextAsync(expectedAudit, new() { Timeout = 5000 });
        await Expect(_page.ComponentResult).ToHaveTextAsync(expectedResult, new() { Timeout = 5000 });
        if (expectedBadge is null)
        {
            await Expect(_page.ComponentBadge).ToBeHiddenAsync();
        }
        else
        {
            await Expect(_page.ComponentBadge).ToBeVisibleAsync();
            await Expect(_page.ComponentBadge).ToHaveTextAsync(expectedBadge);
        }

        await Expect(_page.ComponentSummary).ToHaveTextAsync("Escalation decision complete");
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

        await _page.AssessmentScore.FillAndBlur("95");
        await AssertComponentRouteAsync("Monitor route dispatched", "Monitor closely", "Monitor");

        await _page.SupervisorOverride.Toggle();

        await AssertComponentRouteAsync("Isolation route dispatched", "Escalate now", "Escalate");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_branch_dispatches_isolation_when_memory_care_wins_the_nested_or()
    {
        await NavigateAndBoot();

        await _page.AssessmentScore.FillAndBlur("95");
        await AssertComponentRouteAsync("Monitor route dispatched", "Monitor closely", "Monitor");

        await _page.CareTrack.Select("Memory Care");

        await AssertComponentRouteAsync("Isolation route dispatched", "Escalate now", "Escalate");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_rerouting_to_routine_clears_the_stale_monitor_badge()
    {
        await NavigateAndBoot();

        await _page.AssessmentScore.FillAndBlur("70");
        await AssertComponentRouteAsync("Monitor route dispatched", "Monitor closely", "Monitor");

        await _page.AssessmentScore.FillAndBlur("40");

        await AssertComponentRouteAsync("Routine route dispatched", "Routine follow-up", null);
        AssertNoConsoleErrors();
    }

    private sealed class EscalationRoutingPage
    {
        private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_Conditions_EscalationRouting_EscalationRoutingModel__";
        private readonly IPage _page;

        public EscalationRoutingPage(IPage page)
        {
            _page = page;
        }

        public ILocator TriggerPrecheck => _page.Locator("#trigger-precheck");
        public ILocator TriggerAudit => _page.Locator("#trigger-audit");
        public ILocator TriggerResult => _page.Locator("#trigger-result");
        public ILocator TriggerBadge => _page.Locator("#trigger-badge");
        public ILocator TriggerSummary => _page.Locator("#trigger-summary");

        public ILocator ComponentPrecheck => _page.Locator("#component-precheck");
        public ILocator ComponentAudit => _page.Locator("#component-audit");
        public ILocator ComponentResult => _page.Locator("#component-result");
        public ILocator ComponentBadge => _page.Locator("#component-badge");
        public ILocator ComponentSummary => _page.Locator("#component-summary");

        public NumericTextBoxLocator AssessmentScore => new(_page, Scope + "AssessmentScore");
        public SwitchLocator SupervisorOverride => new(_page, Scope + "SupervisorOverride");
        public DropDownListLocator CareTrack => new(_page, Scope + "CareTrack");
    }
}
