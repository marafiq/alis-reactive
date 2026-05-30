using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

[TestFixture]
public class WhenUsingFusionToolbar : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionToolbar";

    private FusionToolbarLocator Toolbar => new(Page, "resident-toolbar");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#toolbar-disabled", "pending");
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionToolbar — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_contains_typed_toolbar_members()
    {
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();

        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""));
        Assert.That(planJson, Does.Contain("resident-toolbar"));
        Assert.That(planJson, Does.Contain("\"disable\""));
        Assert.That(planJson, Does.Contain("\"clicked\""));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicked_event_exposes_typed_item_metadata()
    {
        await NavigateAndBoot();

        await Toolbar.SaveItem.ClickAsync();

        await Expect(Page.Locator("#clicked-state")).ToHaveTextAsync("clicked", new() { Timeout = 5000 });
        await Expect(Page.Locator("#clicked-id")).ToHaveTextAsync("toolbar-save", new() { Timeout = 5000 });
        await Expect(Page.Locator("#clicked-text")).ToHaveTextAsync("Save", new() { Timeout = 5000 });
        await Expect(Page.Locator("#clicked-disabled")).ToHaveTextAsync("false", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task disable_and_enable_toggle_the_toolbar_root_state()
    {
        await NavigateAndBoot();

        await Page.Locator("#disable-toolbar-btn").ClickAsync();
        await Expect(Page.Locator("#toolbar-disabled")).ToHaveTextAsync("true", new() { Timeout = 5000 });

        await Page.Locator("#enable-toolbar-btn").ClickAsync();
        await Expect(Page.Locator("#toolbar-disabled")).ToHaveTextAsync("false", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
