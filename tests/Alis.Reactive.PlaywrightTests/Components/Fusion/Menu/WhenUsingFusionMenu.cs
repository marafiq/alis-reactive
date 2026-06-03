using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Menu;

[TestFixture]
public class WhenUsingFusionMenu : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionMenu";

    private FusionMenuLocator Menu => new(Page, "resident-menu");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionMenu — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_contains_typed_menu_members()
    {
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();

        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""));
        Assert.That(planJson, Does.Contain("resident-menu"));
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
    public async Task render_payload_is_projected_into_the_trace()
    {
        await NavigateAndBoot();

        await Menu.OpenButton.ClickAsync();
        await Menu.Item("Projects").ClickAsync();
        await Expect(Page.Locator("#before-item-state")).ToHaveTextAsync("rendered", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-item-text")).ToHaveTextAsync("Archive", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-item-id")).ToHaveTextAsync("archive-item", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-item-url")).ToHaveTextAsync("/archive", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-item-icon-css")).ToHaveTextAsync("e-icons e-archive", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-item-separator")).ToHaveTextAsync("false", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task open_select_surfaces_root_and_submenu_payloads()
    {
        await NavigateAndBoot();

        await Menu.OpenButton.ClickAsync();
        await Expect(Page.Locator("#before-open-state")).ToHaveTextAsync("rendered", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-open-kind")).ToHaveTextAsync("root", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-open-top")).Not.ToHaveTextAsync("pending", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-open-left")).Not.ToHaveTextAsync("pending", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-open-focused")).ToHaveTextAsync(new System.Text.RegularExpressions.Regex("^(true|false)$"), new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-open-first-item")).ToHaveTextAsync("Dashboard", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-open-parent")).ToHaveTextAsync("root", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-open-cancel-value")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        await Expect(Page.Locator("#open-state")).ToHaveTextAsync("opened", new() { Timeout = 5000 });
        await Expect(Page.Locator("#open-kind")).ToHaveTextAsync("root", new() { Timeout = 5000 });
        await Expect(Page.Locator("#open-first-item")).ToHaveTextAsync("Dashboard", new() { Timeout = 5000 });
        await Expect(Page.Locator("#open-parent")).ToHaveTextAsync("root", new() { Timeout = 5000 });
        await Expect(Page.Locator("#menu-visibility")).ToHaveTextAsync("visible", new() { Timeout = 5000 });

        await Menu.Item("Projects").ClickAsync();
        await Expect(Page.Locator("#select-state")).ToHaveTextAsync("selected", new() { Timeout = 5000 });
        await Expect(Page.Locator("#select-text")).ToHaveTextAsync("Projects", new() { Timeout = 5000 });
        await Expect(Page.Locator("#select-id")).ToHaveTextAsync("projects-item", new() { Timeout = 5000 });
        await Expect(Page.Locator("#select-url")).ToHaveTextAsync(string.Empty, new() { Timeout = 5000 });
        await Expect(Page.Locator("#select-icon-css")).ToHaveTextAsync("e-icons e-folder", new() { Timeout = 5000 });
        await Expect(Page.Locator("#select-separator")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        await Expect(Page.Locator("#select-first-child")).ToHaveTextAsync("Reports", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-item-state")).ToHaveTextAsync("rendered", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-item-text")).ToHaveTextAsync("Archive", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-item-id")).ToHaveTextAsync("archive-item", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-open-kind")).ToHaveTextAsync("submenu", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-open-first-item")).ToHaveTextAsync("Reports", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-open-parent")).ToHaveTextAsync("Projects", new() { Timeout = 5000 });
        await Expect(Page.Locator("#open-first-item")).ToHaveTextAsync("Dashboard", new() { Timeout = 5000 });
        await Expect(Page.Locator("#open-parent")).ToHaveTextAsync("root", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task close_method_surfaces_close_payloads()
    {
        await NavigateAndBoot();

        await Menu.OpenButton.ClickAsync();
        await Menu.CloseButton.ClickAsync();
        await Expect(Page.Locator("#before-close-state")).ToHaveTextAsync("rendered", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-close-first-item")).ToHaveTextAsync("Dashboard", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-close-parent")).ToHaveTextAsync("root", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-close-cancel")).ToHaveTextAsync("allowed", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-close-cancel-value")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        await Expect(Page.Locator("#close-state")).ToHaveTextAsync("closed", new() { Timeout = 5000 });
        await Expect(Page.Locator("#close-first-item")).ToHaveTextAsync("Dashboard", new() { Timeout = 5000 });
        await Expect(Page.Locator("#close-parent")).ToHaveTextAsync("root", new() { Timeout = 5000 });
        await Expect(Page.Locator("#menu-visibility")).ToHaveTextAsync("hidden", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task open_cancel_prevents_opening_and_close_cancel_preserves_visible_state()
    {
        await NavigateAndBoot();

        await Page.GetByLabel("Block open").CheckAsync();
        await Menu.OpenButton.ClickAsync();
        await Expect(Page.Locator("#before-open-cancel")).ToHaveTextAsync("cancelled", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-open-cancel-value")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Page.Locator("#open-state")).ToHaveTextAsync("pending", new() { Timeout = 5000 });
        await Expect(Page.Locator("#menu-visibility")).ToHaveTextAsync("pending", new() { Timeout = 5000 });

        await Page.GetByLabel("Block open").UncheckAsync();
        await Menu.OpenButton.ClickAsync();
        await Expect(Page.Locator("#menu-visibility")).ToHaveTextAsync("visible", new() { Timeout = 5000 });

        await Page.GetByLabel("Block close").CheckAsync();
        await Menu.CloseButton.ClickAsync();
        await Expect(Page.Locator("#before-close-cancel")).ToHaveTextAsync("cancelled", new() { Timeout = 5000 });
        await Expect(Page.Locator("#before-close-cancel-value")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Page.Locator("#close-state")).ToHaveTextAsync("pending", new() { Timeout = 5000 });
        await Expect(Page.Locator("#menu-visibility")).ToHaveTextAsync("visible", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
