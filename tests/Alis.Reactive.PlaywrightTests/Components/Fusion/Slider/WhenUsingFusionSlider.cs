using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Slider;

[TestFixture]
public class WhenUsingFusionSlider : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Slider";
    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_SliderModel";
    private const string PainScoreId = GeneratedTypeScope + "__PainScore";
    private const string PreferredRangeId = GeneratedTypeScope + "__PreferredRange";

    private FusionSliderLocator PainScore => new(Page, PainScoreId);
    private FusionSliderLocator PreferredRange => new(Page, PreferredRangeId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#value-echo");
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionSlider — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_contains_typed_slider_members()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();

        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""));
        Assert.That(planJson, Does.Contain(PainScoreId));
        Assert.That(planJson, Does.Contain(PreferredRangeId));
        Assert.That(planJson, Does.Contain("\"rangeValue\""));
        Assert.That(planJson, Does.Contain("\"dataBind\""));
        Assert.That(planJson, Does.Contain("\"change\""));
        Assert.That(planJson, Does.Contain("\"changed\""));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_sets_scalar_value_and_reads_value_source()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#value-echo")).ToHaveTextAsync("35", new() { Timeout = 5000 });
        Assert.That(await PainScore.ValueNow(), Is.EqualTo("35"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_sets_range_value_and_reads_range_source()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#range-echo")).ToHaveTextAsync("15,75", new() { Timeout = 5000 });
        Assert.That(await PreferredRange.ValueNow(0), Is.EqualTo("15"));
        Assert.That(await PreferredRange.ValueNow(1), Is.EqualTo("75"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task set_value_updates_visible_slider_and_change_payload()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-score-btn").ClickAsync();

        await Expect(Page.Locator("#command-state")).ToHaveTextAsync("score set", new() { Timeout = 5000 });
        await Expect(Page.Locator("#value-echo")).ToHaveTextAsync("65", new() { Timeout = 5000 });
        Assert.That(await PainScore.ValueNow(), Is.EqualTo("65"));
        await Expect(Page.Locator("#change-value")).ToHaveTextAsync("65", new() { Timeout = 5000 });
        await Expect(Page.Locator("#change-previous")).ToHaveTextAsync("35", new() { Timeout = 5000 });
        await Expect(Page.Locator("#change-text")).ToHaveTextAsync("65", new() { Timeout = 5000 });
        await Expect(Page.Locator("#change-action")).ToHaveTextAsync("change", new() { Timeout = 5000 });
        await Expect(Page.Locator("#change-interacted")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_exposes_value_previous_text_and_action()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-score-btn").ClickAsync();

        await Expect(Page.Locator("#changed-value")).ToHaveTextAsync("65", new() { Timeout = 5000 });
        await Expect(Page.Locator("#changed-previous")).ToHaveTextAsync("35", new() { Timeout = 5000 });
        await Expect(Page.Locator("#changed-text")).ToHaveTextAsync("65", new() { Timeout = 5000 });
        await Expect(Page.Locator("#changed-action")).ToHaveTextAsync("changed", new() { Timeout = 5000 });
        await Expect(Page.Locator("#args-condition")).ToHaveTextAsync("at least 50", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_reads_current_value()
    {
        await NavigateAndBoot();

        await Page.Locator("#check-score-btn").ClickAsync();
        await Expect(Page.Locator("#score-state")).ToHaveTextAsync("low", new() { Timeout = 5000 });

        await Page.Locator("#set-score-btn").ClickAsync();
        await Page.Locator("#check-score-btn").ClickAsync();
        await Expect(Page.Locator("#score-state")).ToHaveTextAsync("high", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task set_range_value_updates_visible_slider_and_range_source()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-range-btn").ClickAsync();

        await Expect(Page.Locator("#command-state")).ToHaveTextAsync("range set", new() { Timeout = 5000 });
        await Expect(Page.Locator("#range-echo")).ToHaveTextAsync("30,90", new() { Timeout = 5000 });
        Assert.That(await PreferredRange.ValueNow(0), Is.EqualTo("30"));
        Assert.That(await PreferredRange.ValueNow(1), Is.EqualTo("90"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task gather_consumes_scalar_and_range_value_sources()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-score-btn").ClickAsync();
        await Page.Locator("#set-range-btn").ClickAsync();
        await Page.Locator("#gather-btn").ClickAsync();

        await Expect(Page.Locator("#gather-score")).ToHaveTextAsync("65", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-range")).ToHaveTextAsync("30,90", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-summary")).ToHaveTextAsync("65:30,90", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
