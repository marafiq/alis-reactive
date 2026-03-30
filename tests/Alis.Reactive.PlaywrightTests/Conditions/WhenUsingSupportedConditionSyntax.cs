using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Conditions;

/// <summary>
/// As a care coordinator
/// I want review routing and risk follow-up to branch consistently
/// So that the page always shows the right workflow outcome
/// </summary>
[TestFixture]
public class WhenSupportedConditionSyntaxRoutesReviewWorkflows : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/SupportedSyntax";

    private PagePlan<SupportedSyntaxModel> _plan = null!;

    private ILocator PriorityReviewButton => Page.Locator("#btn-trigger-priority");
    private ILocator StandardReviewButton => Page.Locator("#btn-trigger-standard");
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

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<SupportedSyntaxModel>.FromPage(Page);
    }

    private async Task AssertPriorityReviewAsync()
    {
        await Expect(TriggerPrecheck).ToHaveTextAsync("Review started");
        await Expect(TriggerAudit).ToHaveTextAsync("Audit logged");
        await Expect(TriggerResult).ToHaveTextAsync("Priority review");
        await Expect(TriggerBadge).ToBeVisibleAsync();
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

    [Test]
    public async Task priority_review_marks_the_case_for_priority_follow_up()
    {
        await NavigateAndBoot();

        await PriorityReviewButton.ClickAsync();

        await AssertPriorityReviewAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task standard_review_keeps_the_case_in_standard_follow_up()
    {
        await NavigateAndBoot();

        await StandardReviewButton.ClickAsync();

        await AssertStandardReviewAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_review_priority_recomputes_the_trigger_path_without_stale_priority_state()
    {
        await NavigateAndBoot();

        await PriorityReviewButton.ClickAsync();
        await AssertPriorityReviewAsync();

        await StandardReviewButton.ClickAsync();

        await AssertStandardReviewAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task high_risk_assessment_flags_the_resident_for_urgent_follow_up()
    {
        await NavigateAndBoot();

        await _plan.NumericTextBox(m => m.RiskScore).FillAndBlur("95");

        await AssertUrgentFollowUpAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task low_risk_assessment_keeps_the_resident_on_routine_follow_up()
    {
        await NavigateAndBoot();

        await _plan.NumericTextBox(m => m.RiskScore).FillAndBlur("40");

        await AssertRoutineFollowUpAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_the_risk_score_recomputes_follow_up_without_leaving_stale_urgent_state()
    {
        await NavigateAndBoot();

        var riskScore = _plan.NumericTextBox(m => m.RiskScore);

        await riskScore.FillAndBlur("95");
        await AssertUrgentFollowUpAsync();

        await riskScore.FillAndBlur("40");
        await AssertRoutineFollowUpAsync();

        await riskScore.FillAndBlur("95");
        await AssertUrgentFollowUpAsync();
        AssertNoConsoleErrors();
    }
}
