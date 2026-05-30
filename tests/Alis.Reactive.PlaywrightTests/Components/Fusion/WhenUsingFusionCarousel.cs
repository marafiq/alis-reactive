using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

[TestFixture]
public class WhenUsingFusionCarousel : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionCarousel";

    private FusionCarouselLocator Carousel => new(Page, "resident-carousel");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#selected-index-echo", "pending");
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionCarousel — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_contains_typed_carousel_members()
    {
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();

        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""));
        Assert.That(planJson, Does.Contain("resident-carousel"));
        Assert.That(planJson, Does.Contain("\"selectedIndex\""));
        Assert.That(planJson, Does.Contain("\"next\""));
        Assert.That(planJson, Does.Contain("\"prev\""));
        Assert.That(planJson, Does.Contain("\"slideChanging\""));
        Assert.That(planJson, Does.Contain("\"slideChanged\""));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task dom_ready_reads_selected_index_source()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#selected-index-echo")).ToHaveTextAsync("1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-state")).ToHaveTextAsync("second slide", new() { Timeout = 5000 });
        await Expect(Carousel.ActiveSlide).ToContainTextAsync("Care Plan Review", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task next_and_previous_methods_update_selected_slide_and_source()
    {
        await NavigateAndBoot();

        await Page.Locator("#next-btn").ClickAsync();
        await Expect(Page.Locator("#selected-index-echo")).ToHaveTextAsync("2", new() { Timeout = 5000 });
        await Expect(Page.Locator("#changing-is-swiped")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        await Expect(Page.Locator("#changing-cancel")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        await Expect(Page.Locator("#changed-current")).ToHaveTextAsync("2", new() { Timeout = 5000 });
        await Expect(Page.Locator("#changed-previous")).ToHaveTextAsync("1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#changed-is-swiped")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        await Expect(Page.Locator("#changed-direction")).ToHaveTextAsync("Next", new() { Timeout = 5000 });
        await Expect(Page.Locator("#review-persisted")).ToHaveTextAsync("Saved review slide 2: discharge readiness", new() { Timeout = 5000 });
        await Expect(Page.Locator("#review-section")).ToHaveTextAsync("discharge readiness", new() { Timeout = 5000 });
        await Expect(Page.Locator("#review-direction")).ToHaveTextAsync("Next", new() { Timeout = 5000 });
        await Expect(Carousel.ActiveSlide).ToContainTextAsync("Discharge Readiness", new() { Timeout = 5000 });

        await Page.Locator("#prev-btn").ClickAsync();
        await Expect(Page.Locator("#selected-index-echo")).ToHaveTextAsync("1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#changing-is-swiped")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        await Expect(Page.Locator("#changing-cancel")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        await Expect(Page.Locator("#changed-current")).ToHaveTextAsync("1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#changed-previous")).ToHaveTextAsync("2", new() { Timeout = 5000 });
        await Expect(Page.Locator("#changed-is-swiped")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        await Expect(Page.Locator("#changed-direction")).ToHaveTextAsync("Previous", new() { Timeout = 5000 });
        await Expect(Page.Locator("#review-persisted")).ToHaveTextAsync("Saved review slide 1: care plan review", new() { Timeout = 5000 });
        await Expect(Page.Locator("#review-section")).ToHaveTextAsync("care plan review", new() { Timeout = 5000 });
        await Expect(Page.Locator("#review-direction")).ToHaveTextAsync("Previous", new() { Timeout = 5000 });
        await Expect(Carousel.ActiveSlide).ToContainTextAsync("Care Plan Review", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task slide_changing_can_cancel_the_final_previous_transition()
    {
        await NavigateAndBoot();

        await Page.Locator("#next-btn").ClickAsync();
        await Page.Locator("#prev-btn").ClickAsync();
        await Page.Locator("#prev-btn").ClickAsync();

        await Expect(Page.Locator("#cancel-state")).ToHaveTextAsync("cancelled", new() { Timeout = 5000 });
        await Expect(Page.Locator("#changing-is-swiped")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        await Expect(Page.Locator("#changing-cancel")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-index-echo")).ToHaveTextAsync("1", new() { Timeout = 5000 });
        await Expect(Page.Locator("#changed-state")).ToHaveTextAsync("changed", new() { Timeout = 5000 });
        await Expect(Page.Locator("#review-persisted")).ToHaveTextAsync("Saved review slide 1: care plan review", new() { Timeout = 5000 });
        await Expect(Carousel.ActiveSlide).ToContainTextAsync("Care Plan Review", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
