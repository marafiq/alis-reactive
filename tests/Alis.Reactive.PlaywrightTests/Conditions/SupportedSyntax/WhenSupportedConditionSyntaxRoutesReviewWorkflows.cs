using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.SupportedSyntax;

namespace Alis.Reactive.PlaywrightTests.Conditions.SupportedSyntax;

[TestFixture]
public class WhenSupportedConditionSyntaxRoutesReviewWorkflows : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/SupportedSyntax";

    private PagePlan<SupportedSyntaxModel> _plan = null!;

    private ILocator PriorityReviewButton => Page.Locator("#btn-trigger-priority");
    private ILocator StandardReviewButton => Page.Locator("#btn-trigger-standard");
    private ILocator DeferredReviewButton => Page.Locator("#btn-trigger-deferred");

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

    private NumericTextBoxLocator RiskScore => _plan.NumericTextBox(m => m.RiskScore);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#btn-trigger-priority");
        _plan = await PagePlan<SupportedSyntaxModel>.FromPage(Page);
    }

    private async Task AssertTriggerPathAsync(string expectedResult, string? expectedBadge)
    {
        await Expect(TriggerPrecheck).ToHaveTextAsync("Review started");
        await Expect(TriggerAudit).ToHaveTextAsync("Audit logged");
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

        await Expect(TriggerSummary).ToHaveTextAsync("Workflow complete");
    }

    private async Task AssertReactivePathAsync(string expectedResult, string? expectedBadge)
    {
        await Expect(ReactivePrecheck).ToHaveTextAsync("Risk evaluated", new() { Timeout = 5000 });
        await Expect(ReactiveAudit).ToHaveTextAsync("Audit logged", new() { Timeout = 5000 });
        await Expect(ReactiveResult).ToHaveTextAsync(expectedResult, new() { Timeout = 5000 });
        if (expectedBadge is null)
        {
            await Expect(ReactiveBadge).ToBeHiddenAsync();
        }
        else
        {
            await Expect(ReactiveBadge).ToBeVisibleAsync();
            await Expect(ReactiveBadge).ToHaveTextAsync(expectedBadge);
        }

        await Expect(ReactiveSummary).ToHaveTextAsync("Assessment complete");
    }

    [Test]
    public async Task priority_review_takes_the_first_branch_of_the_trigger_ladder()
    {
        await NavigateAndBoot();

        await ClickWhenStable(PriorityReviewButton);

        await AssertTriggerPathAsync("Priority review", "Priority");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task standard_review_takes_the_elseif_branch_of_the_trigger_ladder()
    {
        await NavigateAndBoot();

        await ClickWhenStable(StandardReviewButton);

        await AssertTriggerPathAsync("Standard review", "Standard");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task deferred_review_falls_through_to_the_final_else_branch()
    {
        await NavigateAndBoot();

        await ClickWhenStable(DeferredReviewButton);

        await AssertTriggerPathAsync("Deferred review", null);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task trigger_ladder_recomputes_without_leaving_a_stale_badge_visible()
    {
        await NavigateAndBoot();

        await ClickWhenStable(PriorityReviewButton);
        await AssertTriggerPathAsync("Priority review", "Priority");

        await ClickWhenStable(DeferredReviewButton);
        await AssertTriggerPathAsync("Deferred review", null);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task high_risk_scores_route_to_the_first_reactive_branch()
    {
        await NavigateAndBoot();

        await RiskScore.FillAndBlur("95");

        await AssertReactivePathAsync("Urgent follow-up", "Urgent");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task mid_range_scores_route_to_the_reactive_elseif_branch()
    {
        await NavigateAndBoot();

        await RiskScore.FillAndBlur("75");

        await AssertReactivePathAsync("Standard follow-up", "Standard");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task low_scores_route_to_the_reactive_else_branch()
    {
        await NavigateAndBoot();

        await RiskScore.FillAndBlur("40");

        await AssertReactivePathAsync("Routine follow-up", null);
        AssertNoConsoleErrors();
    }
}
