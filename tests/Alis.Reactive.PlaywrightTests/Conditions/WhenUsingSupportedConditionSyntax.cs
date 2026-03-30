using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Conditions;

/// <summary>
/// As a care coordinator
/// I want the supported condition DSL to route trigger and component-driven workflows consistently
/// So that every branch recomputes correctly without stale UI state
/// </summary>
[TestFixture]
public class WhenSupportedConditionSyntaxRoutesReviewWorkflows : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/SupportedSyntax";

    private PagePlan<SupportedSyntaxModel> _plan = null!;

    private ILocator PriorityReviewButton => Page.Locator("#btn-trigger-priority");
    private ILocator StandardReviewButton => Page.Locator("#btn-trigger-standard");
    private ILocator IsolationReviewButton => Page.Locator("#btn-escalation-isolation");
    private ILocator OverrideReviewButton => Page.Locator("#btn-escalation-override");
    private ILocator RoutineReviewButton => Page.Locator("#btn-escalation-routine");

    private ILocator TriggerPrecheck => _plan.Element("trigger-precheck");
    private ILocator TriggerAudit => _plan.Element("trigger-audit");
    private ILocator TriggerResult => _plan.Element("trigger-result");
    private ILocator TriggerBadge => _plan.Element("trigger-badge");
    private ILocator TriggerSummary => _plan.Element("trigger-summary");

    private ILocator ReactivePrecheck => _plan.Element("reactive-precheck");
    private ILocator ReactiveAudit => _plan.Element("reactive-audit");
    private ILocator ReactiveResult => _plan.Element("reactive-result");
    private ILocator ReactiveBadge => _plan.Element("reactive-badge");
    private ILocator ReactiveSummary => _plan.Element("reactive-summary");

    private ILocator EscalationPrecheck => _plan.Element("escalation-precheck");
    private ILocator EscalationAudit => _plan.Element("escalation-audit");
    private ILocator EscalationResult => _plan.Element("escalation-result");
    private ILocator EscalationBadge => _plan.Element("escalation-badge");
    private ILocator EscalationSummary => _plan.Element("escalation-summary");

    private ILocator CompositePrecheck => _plan.Element("composite-precheck");
    private ILocator CompositeAudit => _plan.Element("composite-audit");
    private ILocator CompositeResult => _plan.Element("composite-result");
    private ILocator CompositeBadge => _plan.Element("composite-badge");
    private ILocator CompositeSummary => _plan.Element("composite-summary");

    private NumericTextBoxLocator RiskScore => _plan.NumericTextBox(m => m.RiskScore);
    private NumericTextBoxLocator AssessmentScore => _plan.NumericTextBox(m => m.AssessmentScore);
    private SwitchLocator SupervisorOverride => _plan.Switch(m => m.SupervisorOverride);
    private DropDownListLocator CareTrack => _plan.DropDownList(m => m.CareTrack);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#btn-trigger-priority");
        _plan = await PagePlan<SupportedSyntaxModel>.FromPage(Page);
    }

    private async Task AssertPriorityReviewAsync()
    {
        await Expect(TriggerPrecheck).ToHaveTextAsync("Review started");
        await Expect(TriggerAudit).ToHaveTextAsync("Audit logged");
        await Expect(TriggerResult).ToHaveTextAsync("Priority review");
        await Expect(TriggerBadge).ToBeVisibleAsync();
        await Expect(TriggerBadge).ToHaveTextAsync("Priority");
        await Expect(TriggerSummary).ToHaveTextAsync("Workflow complete");
    }

    private async Task AssertStandardReviewAsync()
    {
        await Expect(TriggerPrecheck).ToHaveTextAsync("Review started");
        await Expect(TriggerAudit).ToHaveTextAsync("Audit logged");
        await Expect(TriggerResult).ToHaveTextAsync("Standard review");
        await Expect(TriggerBadge).ToBeHiddenAsync();
        await Expect(TriggerSummary).ToHaveTextAsync("Workflow complete");
    }

    private async Task AssertUrgentFollowUpAsync()
    {
        await Expect(ReactivePrecheck).ToHaveTextAsync("Risk evaluated", new() { Timeout = 5000 });
        await Expect(ReactiveAudit).ToHaveTextAsync("Audit logged", new() { Timeout = 5000 });
        await Expect(ReactiveResult).ToHaveTextAsync("Urgent follow-up", new() { Timeout = 5000 });
        await Expect(ReactiveBadge).ToBeVisibleAsync();
        await Expect(ReactiveBadge).ToHaveTextAsync("Urgent");
        await Expect(ReactiveSummary).ToHaveTextAsync("Assessment complete");
    }

    private async Task AssertRoutineFollowUpAsync()
    {
        await Expect(ReactivePrecheck).ToHaveTextAsync("Risk evaluated", new() { Timeout = 5000 });
        await Expect(ReactiveAudit).ToHaveTextAsync("Audit logged", new() { Timeout = 5000 });
        await Expect(ReactiveResult).ToHaveTextAsync("Routine follow-up", new() { Timeout = 5000 });
        await Expect(ReactiveBadge).ToBeHiddenAsync();
        await Expect(ReactiveSummary).ToHaveTextAsync("Assessment complete");
    }

    private async Task AssertIsolationReviewAsync()
    {
        await Expect(EscalationPrecheck).ToHaveTextAsync("Escalation reviewed");
        await Expect(EscalationAudit).ToHaveTextAsync("Escalation audit logged");
        await Expect(EscalationResult).ToHaveTextAsync("Isolation review");
        await Expect(EscalationBadge).ToBeVisibleAsync();
        await Expect(EscalationBadge).ToHaveTextAsync("Isolation");
        await Expect(EscalationSummary).ToHaveTextAsync("Escalation complete");
    }

    private async Task AssertExpeditedReviewAsync()
    {
        await Expect(EscalationPrecheck).ToHaveTextAsync("Escalation reviewed");
        await Expect(EscalationAudit).ToHaveTextAsync("Escalation audit logged");
        await Expect(EscalationResult).ToHaveTextAsync("Expedited review");
        await Expect(EscalationBadge).ToBeVisibleAsync();
        await Expect(EscalationBadge).ToHaveTextAsync("Expedited");
        await Expect(EscalationSummary).ToHaveTextAsync("Escalation complete");
    }

    private async Task AssertRoutineEscalationAsync()
    {
        await Expect(EscalationPrecheck).ToHaveTextAsync("Escalation reviewed");
        await Expect(EscalationAudit).ToHaveTextAsync("Escalation audit logged");
        await Expect(EscalationResult).ToHaveTextAsync("Routine review");
        await Expect(EscalationBadge).ToBeHiddenAsync();
        await Expect(EscalationSummary).ToHaveTextAsync("Escalation complete");
    }

    private async Task AssertEscalateNowAsync()
    {
        await Expect(CompositePrecheck).ToHaveTextAsync("Escalation evaluated", new() { Timeout = 5000 });
        await Expect(CompositeAudit).ToHaveTextAsync("Escalation audit logged", new() { Timeout = 5000 });
        await Expect(CompositeResult).ToHaveTextAsync("Escalate now", new() { Timeout = 5000 });
        await Expect(CompositeBadge).ToBeVisibleAsync();
        await Expect(CompositeBadge).ToHaveTextAsync("Escalate");
        await Expect(CompositeSummary).ToHaveTextAsync("Escalation decision complete");
    }

    private async Task AssertMonitorCloselyAsync()
    {
        await Expect(CompositePrecheck).ToHaveTextAsync("Escalation evaluated", new() { Timeout = 5000 });
        await Expect(CompositeAudit).ToHaveTextAsync("Escalation audit logged", new() { Timeout = 5000 });
        await Expect(CompositeResult).ToHaveTextAsync("Monitor closely", new() { Timeout = 5000 });
        await Expect(CompositeBadge).ToBeVisibleAsync();
        await Expect(CompositeBadge).ToHaveTextAsync("Monitor");
        await Expect(CompositeSummary).ToHaveTextAsync("Escalation decision complete");
    }

    private async Task AssertRoutineCompositeAsync()
    {
        await Expect(CompositePrecheck).ToHaveTextAsync("Escalation evaluated", new() { Timeout = 5000 });
        await Expect(CompositeAudit).ToHaveTextAsync("Escalation audit logged", new() { Timeout = 5000 });
        await Expect(CompositeResult).ToHaveTextAsync("Routine follow-up", new() { Timeout = 5000 });
        await Expect(CompositeBadge).ToBeHiddenAsync();
        await Expect(CompositeSummary).ToHaveTextAsync("Escalation decision complete");
    }

    [Test]
    public async Task priority_review_marks_the_case_for_priority_follow_up()
    {
        await NavigateAndBoot();

        await ClickWhenStable(PriorityReviewButton);

        await AssertPriorityReviewAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task standard_review_keeps_the_case_in_standard_follow_up()
    {
        await NavigateAndBoot();

        await ClickWhenStable(StandardReviewButton);

        await AssertStandardReviewAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_review_priority_recomputes_the_trigger_path_without_stale_priority_state()
    {
        await NavigateAndBoot();

        await ClickWhenStable(PriorityReviewButton);
        await AssertPriorityReviewAsync();

        await ClickWhenStable(StandardReviewButton);

        await AssertStandardReviewAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task high_risk_assessment_flags_the_resident_for_urgent_follow_up()
    {
        await NavigateAndBoot();

        await RiskScore.FillAndBlur("95");

        await AssertUrgentFollowUpAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task low_risk_assessment_keeps_the_resident_on_routine_follow_up()
    {
        await NavigateAndBoot();

        await RiskScore.FillAndBlur("40");

        await AssertRoutineFollowUpAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_the_risk_score_recomputes_follow_up_without_leaving_stale_urgent_state()
    {
        await NavigateAndBoot();

        await RiskScore.FillAndBlur("95");
        await AssertUrgentFollowUpAsync();

        await RiskScore.FillAndBlur("40");
        await AssertRoutineFollowUpAsync();

        await RiskScore.FillAndBlur("95");
        await AssertUrgentFollowUpAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task isolation_review_requires_both_extreme_score_and_isolation_flag()
    {
        await NavigateAndBoot();

        await ClickWhenStable(IsolationReviewButton);

        await AssertIsolationReviewAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task manual_override_review_uses_the_expedited_path_when_the_primary_branch_fails()
    {
        await NavigateAndBoot();

        await ClickWhenStable(OverrideReviewButton);

        await AssertExpeditedReviewAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task routine_review_falls_through_the_final_branch_when_no_condition_matches()
    {
        await NavigateAndBoot();

        await ClickWhenStable(RoutineReviewButton);

        await AssertRoutineEscalationAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task trigger_ladder_recomputes_cleanly_across_isolation_expedited_and_routine_outcomes()
    {
        await NavigateAndBoot();

        await ClickWhenStable(IsolationReviewButton);
        await AssertIsolationReviewAsync();

        await ClickWhenStable(OverrideReviewButton);
        await AssertExpeditedReviewAsync();

        await ClickWhenStable(RoutineReviewButton);
        await AssertRoutineEscalationAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task high_score_with_supervisor_override_escalates_immediately()
    {
        await NavigateAndBoot();

        await AssessmentScore.FillAndBlur("95");
        await AssertMonitorCloselyAsync();

        await SupervisorOverride.Toggle();

        await AssertEscalateNowAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task high_score_with_memory_care_escalates_even_without_manual_override()
    {
        await NavigateAndBoot();

        await AssessmentScore.FillAndBlur("95");
        await AssertMonitorCloselyAsync();

        await CareTrack.Select("Memory Care");

        await AssertEscalateNowAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task mid_score_without_override_stays_on_the_monitored_path()
    {
        await NavigateAndBoot();

        await AssessmentScore.FillAndBlur("70");

        await AssertMonitorCloselyAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task low_score_returns_to_routine_follow_up()
    {
        await NavigateAndBoot();

        await AssessmentScore.FillAndBlur("40");

        await AssertRoutineCompositeAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_override_and_care_track_recomputes_component_source_logic_without_stale_state()
    {
        await NavigateAndBoot();

        await AssessmentScore.FillAndBlur("95");
        await AssertMonitorCloselyAsync();

        await SupervisorOverride.Toggle();
        await AssertEscalateNowAsync();

        await SupervisorOverride.Toggle();
        await AssertMonitorCloselyAsync();

        await CareTrack.Select("Memory Care");
        await AssertEscalateNowAsync();

        await AssessmentScore.FillAndBlur("40");
        await AssertRoutineCompositeAsync();
        AssertNoConsoleErrors();
    }
}
