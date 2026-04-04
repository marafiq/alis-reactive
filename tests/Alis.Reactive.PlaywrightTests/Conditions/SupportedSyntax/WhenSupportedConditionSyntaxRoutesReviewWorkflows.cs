using Alis.Reactive.PlaywrightTests.Support.Controls;

namespace Alis.Reactive.PlaywrightTests.Conditions.SupportedSyntax;

[TestFixture]
public class WhenSupportedConditionSyntaxRoutesReviewWorkflows : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/SupportedSyntax";
    private SupportedSyntaxPage _page = null!;

    private ILocator PriorityReviewButton => Page.Locator("#btn-trigger-priority");
    private ILocator StandardReviewButton => Page.Locator("#btn-trigger-standard");
    private ILocator DeferredReviewButton => Page.Locator("#btn-trigger-deferred");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#btn-trigger-priority");
        _page = new SupportedSyntaxPage(Page);
    }

    private async Task AssertTriggerPathAsync(string expectedResult, string? expectedBadge)
    {
        await Expect(_page.TriggerPrecheck).ToHaveTextAsync("Review started");
        await Expect(_page.TriggerAudit).ToHaveTextAsync("Audit logged");
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

        await Expect(_page.TriggerSummary).ToHaveTextAsync("Workflow complete");
    }

    private async Task AssertReactivePathAsync(string expectedResult, string? expectedBadge)
    {
        await Expect(_page.ReactivePrecheck).ToHaveTextAsync("Risk evaluated", new() { Timeout = 5000 });
        await Expect(_page.ReactiveAudit).ToHaveTextAsync("Audit logged", new() { Timeout = 5000 });
        await Expect(_page.ReactiveResult).ToHaveTextAsync(expectedResult, new() { Timeout = 5000 });
        if (expectedBadge is null)
        {
            await Expect(_page.ReactiveBadge).ToBeHiddenAsync();
        }
        else
        {
            await Expect(_page.ReactiveBadge).ToBeVisibleAsync();
            await Expect(_page.ReactiveBadge).ToHaveTextAsync(expectedBadge);
        }

        await Expect(_page.ReactiveSummary).ToHaveTextAsync("Assessment complete");
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

        await _page.RiskScore.FillAndBlur("95");

        await AssertReactivePathAsync("Urgent follow-up", "Urgent");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task mid_range_scores_route_to_the_reactive_elseif_branch()
    {
        await NavigateAndBoot();

        await _page.RiskScore.FillAndBlur("75");

        await AssertReactivePathAsync("Standard follow-up", "Standard");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task low_scores_route_to_the_reactive_else_branch()
    {
        await NavigateAndBoot();

        await _page.RiskScore.FillAndBlur("40");

        await AssertReactivePathAsync("Routine follow-up", null);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reactive_ladder_recomputes_without_leaving_a_stale_badge_visible()
    {
        await NavigateAndBoot();

        await _page.RiskScore.FillAndBlur("95");
        await AssertReactivePathAsync("Urgent follow-up", "Urgent");

        await _page.RiskScore.FillAndBlur("40");
        await AssertReactivePathAsync("Routine follow-up", null);

        await _page.RiskScore.FillAndBlur("95");
        await AssertReactivePathAsync("Urgent follow-up", "Urgent");
        AssertNoConsoleErrors();
    }

    private sealed class SupportedSyntaxPage
    {
        private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_Conditions_SupportedSyntax_SupportedSyntaxModel__";
        private readonly IPage _page;

        public SupportedSyntaxPage(IPage page)
        {
            _page = page;
        }

        public ILocator TriggerPrecheck => _page.Locator("#trigger-precheck");
        public ILocator TriggerAudit => _page.Locator("#trigger-audit");
        public ILocator TriggerResult => _page.Locator("#trigger-result");
        public ILocator TriggerBadge => _page.Locator("#trigger-badge");
        public ILocator TriggerSummary => _page.Locator("#trigger-summary");

        public ILocator ReactivePrecheck => _page.Locator("#reactive-precheck");
        public ILocator ReactiveAudit => _page.Locator("#reactive-audit");
        public ILocator ReactiveResult => _page.Locator("#reactive-result");
        public ILocator ReactiveBadge => _page.Locator("#reactive-badge");
        public ILocator ReactiveSummary => _page.Locator("#reactive-summary");

        public NumericTextBoxLocator RiskScore => new(_page, Scope + "RiskScore");
    }
}
