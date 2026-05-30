using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

[TestFixture]
public class WhenUsingFusionBreadcrumb : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionBreadcrumb";

    private FusionBreadcrumbLocator Breadcrumb => new(Page, "navigation-breadcrumb");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#active-item-echo", "pending");
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionBreadcrumb — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_contains_typed_breadcrumb_members()
    {
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();

        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""));
        Assert.That(planJson, Does.Contain("navigation-breadcrumb"));
        Assert.That(planJson, Does.Contain("\"activeItem\""));
        Assert.That(planJson, Does.Contain("\"dataBind\""));
        Assert.That(planJson, Does.Contain("\"itemClick\""));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task dom_ready_reads_active_item_source_and_current_item()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#active-item-echo")).ToHaveTextAsync("/docs", new() { Timeout = 5000 });
        await Expect(Page.Locator("#active-state")).ToHaveTextAsync("has active item", new() { Timeout = 5000 });
        await Expect(Breadcrumb.CurrentItem).ToHaveTextAsync("Docs", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task set_active_item_updates_source_and_rendered_current_item()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-guide-btn").ClickAsync();

        await Expect(Page.Locator("#active-item-echo")).ToHaveTextAsync("/docs/guide", new() { Timeout = 5000 });
        await Expect(Breadcrumb.CurrentItem).ToHaveTextAsync("Guide", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task item_click_reads_typed_payload_and_updates_current_item()
    {
        await NavigateAndBoot();

        await Breadcrumb.ClickLink("Home");

        await Expect(Page.Locator("#clicked-text")).ToHaveTextAsync("Home", new() { Timeout = 5000 });
        await Expect(Page.Locator("#clicked-id")).ToHaveTextAsync("home", new() { Timeout = 5000 });
        await Expect(Page.Locator("#clicked-url")).ToHaveTextAsync("/home", new() { Timeout = 5000 });
        await Expect(Page.Locator("#clicked-icon-css")).ToHaveTextAsync("e-icons e-home", new() { Timeout = 5000 });
        await Expect(Page.Locator("#clicked-disabled")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        await Expect(Page.Locator("#route-message")).ToHaveTextAsync("Opening Home in workspace", new() { Timeout = 5000 });
        await Expect(Page.Locator("#route-category")).ToHaveTextAsync("workspace", new() { Timeout = 5000 });
        await Expect(Page.Locator("#route-trail")).ToHaveTextAsync("home:/home", new() { Timeout = 5000 });
        await Expect(Breadcrumb.CurrentItem).ToHaveTextAsync("Home", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
