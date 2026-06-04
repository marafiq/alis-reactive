using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Rating;

[TestFixture]
public class WhenUsingFusionRating : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Rating";
    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_RatingModel";
    private const string SatisfactionScoreId = GeneratedTypeScope + "__SatisfactionScore";

    private FusionRatingLocator SatisfactionScore => new(Page, SatisfactionScoreId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#value-echo");
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionRating — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_contains_typed_rating_members()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();

        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""));
        Assert.That(planJson, Does.Contain(SatisfactionScoreId));
        Assert.That(planJson, Does.Contain("\"value\""));
        Assert.That(planJson, Does.Contain("\"dataBind\""));
        Assert.That(planJson, Does.Contain("\"reset\""));
        Assert.That(planJson, Does.Contain("\"valueChanged\""));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_sets_visible_value_and_reads_value_source()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#value-echo")).ToHaveTextAsync("3", new() { Timeout = 5000 });
        Assert.That(await SatisfactionScore.ValueAttribute(), Is.EqualTo("3"));
        Assert.That(await SatisfactionScore.AriaValue(), Is.EqualTo("3"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task set_value_updates_visible_rating_and_value_source()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-rating-btn").ClickAsync();

        await Expect(Page.Locator("#command-state")).ToHaveTextAsync("rating set", new() { Timeout = 5000 });
        await Expect(Page.Locator("#value-echo")).ToHaveTextAsync("4", new() { Timeout = 5000 });
        Assert.That(await SatisfactionScore.ValueAttribute(), Is.EqualTo("4"));
        Assert.That(await SatisfactionScore.AriaValue(), Is.EqualTo("4"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reset_method_resets_rating_and_valuechanged_event_exposes_payload()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-rating-btn").ClickAsync();
        await Page.Locator("#reset-rating-btn").ClickAsync();

        await Expect(Page.Locator("#command-state")).ToHaveTextAsync("rating reset", new() { Timeout = 5000 });
        await Expect(Page.Locator("#value-echo")).ToHaveTextAsync("0", new() { Timeout = 5000 });
        Assert.That(await SatisfactionScore.ValueAttribute(), Is.EqualTo("0"));
        Assert.That(await SatisfactionScore.AriaValue(), Is.EqualTo("0"));
        await Expect(Page.Locator("#changed-value")).ToHaveTextAsync("0", new() { Timeout = 5000 });
        await Expect(Page.Locator("#changed-previous")).ToHaveTextAsync("4", new() { Timeout = 5000 });
        await Expect(Page.Locator("#changed-interacted")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        await Expect(Page.Locator("#args-condition")).ToHaveTextAsync("reset", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_reads_current_value()
    {
        await NavigateAndBoot();

        await Page.Locator("#check-rating-btn").ClickAsync();
        await Expect(Page.Locator("#rating-state")).ToHaveTextAsync("satisfied", new() { Timeout = 5000 });

        await Page.Locator("#reset-rating-btn").ClickAsync();
        await Page.Locator("#check-rating-btn").ClickAsync();
        await Expect(Page.Locator("#rating-state")).ToHaveTextAsync("needs follow-up", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task gather_consumes_rating_value_source()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-rating-btn").ClickAsync();
        await Page.Locator("#gather-btn").ClickAsync();

        await Expect(Page.Locator("#gather-score")).ToHaveTextAsync("4", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-summary")).ToHaveTextAsync("score:4", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
