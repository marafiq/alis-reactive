using Microsoft.Playwright;
using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

[TestFixture]
public class WhenUsingFusionContextMenu : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/ContextMenu";

    private FusionContextMenuLocator ContextMenu => new(Page, "resident-context-menu");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionContextMenu — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_contains_typed_context_menu_members()
    {
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();

        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""));
        Assert.That(planJson, Does.Contain("resident-context-menu"));
        Assert.That(planJson, Does.Contain("\"beforeItemRender\""));
        Assert.That(planJson, Does.Contain("\"beforeOpen\""));
        Assert.That(planJson, Does.Contain("\"onOpen\""));
        Assert.That(planJson, Does.Contain("\"beforeClose\""));
        Assert.That(planJson, Does.Contain("\"onClose\""));
        Assert.That(planJson, Does.Contain("\"select\""));
        Assert.That(planJson, Does.Contain("\"open\""));
        Assert.That(planJson, Does.Contain("\"close\""));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task open_method_exposes_root_payloads_and_programmatic_state()
    {
        await NavigateAndBoot();

        await ContextMenu.OpenButton.ClickAsync();
        await Expect(Page.Locator("#before-open-kind")).ToHaveTextAsync("root", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-open-parent")).ToHaveTextAsync("root", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-open-top")).ToHaveTextAsync("96", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-open-left")).ToHaveTextAsync("168", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-open-cancel")).ToHaveTextAsync("allowed", new() { Timeout = 5000 });
        await Expect(Page.Locator("#open-state")).ToHaveTextAsync("opened", new() { Timeout = 5000 });
        await Expect(Page.Locator("#open-kind")).ToHaveTextAsync("root", new() { Timeout = 5000 });
        await Expect(Page.Locator("#menu-visibility")).ToHaveTextAsync("visible", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task submenu_render_and_select_payloads_are_projected()
    {
        await NavigateAndBoot();

        await ContextMenu.OpenButton.ClickAsync();
        await ContextMenu.Item("Projects").HoverAsync();
        await Expect(ContextMenu.Item("Archive")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Expect(Page.Locator("#before-item-state")).ToHaveTextAsync("rendered", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-item-text")).ToHaveTextAsync("Archive", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-item-id")).ToHaveTextAsync("archive-item", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-open-kind")).ToHaveTextAsync("submenu", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-open-parent")).ToHaveTextAsync("Projects", new() { Timeout = 5000 });

        await ContextMenu.Item("Archive").ClickAsync();
        await Expect(Page.Locator("#select-state")).ToHaveTextAsync("selected", new() { Timeout = 5000 });
        await Expect(Page.Locator("#select-text")).ToHaveTextAsync("Archive", new() { Timeout = 5000 });
        await Expect(Page.Locator("#select-id")).ToHaveTextAsync("archive-item", new() { Timeout = 5000 });
        await Expect(Page.Locator("#select-url")).ToHaveTextAsync(string.Empty, new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task close_method_and_cancels_are_exercised()
    {
        await NavigateAndBoot();

        await ContextMenu.OpenButton.ClickAsync();
        await ContextMenu.CloseButton.ClickAsync();
        await Expect(Page.Locator("#before-close-state")).ToHaveTextAsync("rendered", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-close-cancel")).ToHaveTextAsync("allowed", new() { Timeout = 5000 });
        await Expect(Page.Locator("#close-state")).ToHaveTextAsync("closed", new() { Timeout = 5000 });
        await Expect(Page.Locator("#menu-visibility")).ToHaveTextAsync("hidden", new() { Timeout = 5000 });

        await Page.GetByLabel("Block close").CheckAsync();
        await ContextMenu.OpenButton.ClickAsync();
        await ContextMenu.CloseButton.ClickAsync();
        await Expect(Page.Locator("#before-close-cancel")).ToHaveTextAsync("cancelled", new() { Timeout = 5000 });
        await Expect(Page.Locator("#close-state")).ToHaveTextAsync("closed", new() { Timeout = 5000 });
        await Expect(Page.Locator("#menu-visibility")).ToHaveTextAsync("visible", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task open_cancel_prevents_opening_from_a_clean_state()
    {
        await NavigateAndBoot();

        await Page.GetByLabel("Block open").CheckAsync();
        await ContextMenu.OpenButton.ClickAsync();
        await Expect(Page.Locator("#before-open-cancel")).ToHaveTextAsync("cancelled", new() { Timeout = 5000 });
        await Expect(Page.Locator("#open-state")).ToHaveTextAsync("pending", new() { Timeout = 5000 });
        await Expect(Page.Locator("#menu-visibility")).ToHaveTextAsync("pending", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task right_click_target_opens_the_context_menu()
    {
        await NavigateAndBoot();

        await ContextMenu.Target.ClickAsync(new() { Button = MouseButton.Right });
        await Expect(Page.Locator("#menu-visibility")).ToHaveTextAsync("visible", new() { Timeout = 5000 });
        await Expect(Page.Locator("#open-kind")).ToHaveTextAsync("root", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
