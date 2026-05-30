using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

[TestFixture]
public class WhenUsingFusionSidebar : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionSidebar";

    private FusionSidebarLocator Sidebar => new(Page, "resident-sidebar");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#is-open-echo", "pending");
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionSidebar — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_contains_typed_sidebar_members()
    {
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();

        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""));
        Assert.That(planJson, Does.Contain("resident-sidebar"));
        Assert.That(planJson, Does.Contain("\"isOpen\""));
        Assert.That(planJson, Does.Contain("\"show\""));
        Assert.That(planJson, Does.Contain("\"hide\""));
        Assert.That(planJson, Does.Contain("\"toggle\""));
        Assert.That(planJson, Does.Contain("\"open\""));
        Assert.That(planJson, Does.Contain("\"close\""));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task dom_ready_reads_is_open_source()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#is-open-echo")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        await Expect(Sidebar.ClosedRoot).ToHaveCountAsync(1);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task show_hide_and_toggle_methods_update_state_and_payloads()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-btn").ClickAsync();
        await Expect(Page.Locator("#open-state")).ToHaveTextAsync("opened", new() { Timeout = 5000 });
        await Expect(Page.Locator("#open-interacted")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        await Expect(Page.Locator("#panel-title")).ToHaveTextAsync("Resident navigation", new() { Timeout = 5000 });
        await Expect(Page.Locator("#panel-load-state")).ToHaveTextAsync("workflow opened resident navigation panel", new() { Timeout = 5000 });
        await Expect(Page.Locator("#panel-open-mode")).ToHaveTextAsync("workflow opened", new() { Timeout = 5000 });
        await Expect(Page.Locator("#is-open-echo")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Sidebar.OpenRoot).ToHaveCountAsync(1);

        await Page.Locator("#hide-btn").ClickAsync();
        await Expect(Page.Locator("#close-state")).ToHaveTextAsync("closed", new() { Timeout = 5000 });
        await Expect(Page.Locator("#close-interacted")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        await Expect(Page.Locator("#is-open-echo")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        await Expect(Sidebar.ClosedRoot).ToHaveCountAsync(1);

        await Page.Locator("#toggle-btn").ClickAsync();
        await Expect(Page.Locator("#command-state")).ToHaveTextAsync("toggle", new() { Timeout = 5000 });
        await Expect(Page.Locator("#is-open-echo")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Sidebar.OpenRoot).ToHaveCountAsync(1);

        AssertNoConsoleErrors();
    }
}
