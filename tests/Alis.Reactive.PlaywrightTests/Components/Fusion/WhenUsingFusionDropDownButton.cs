using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

[TestFixture]
public class WhenUsingFusionDropDownButton : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionDropDownButton";
    private const string MenuId = "resident-actions-menu";

    private FusionDropDownButtonLocator Menu => new(Page, MenuId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#content-echo");
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionDropDownButton — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_contains_typed_dropdown_button_members()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();

        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""));
        Assert.That(planJson, Does.Contain(MenuId));
        Assert.That(planJson, Does.Contain("\"content\""));
        Assert.That(planJson, Does.Contain("\"disabled\""));
        Assert.That(planJson, Does.Contain("\"cssClass\""));
        Assert.That(planJson, Does.Contain("\"dataBind\""));
        Assert.That(planJson, Does.Contain("\"toggle\""));
        Assert.That(planJson, Does.Contain("\"focusIn\""));
        Assert.That(planJson, Does.Contain("\"removeItems\""));
        Assert.That(planJson, Does.Contain("\"select\""));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_sets_visible_content_and_reads_content_source()
    {
        await NavigateAndBoot();

        await Expect(Menu.Button).ToContainTextAsync("Ready Actions", new() { Timeout = 5000 });
        await Expect(Page.Locator("#content-echo")).ToHaveTextAsync("Ready Actions", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task set_content_and_disabled_update_visible_state_sources_and_condition()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-content-btn").ClickAsync();
        await Expect(Menu.Button).ToContainTextAsync("Discharge Actions", new() { Timeout = 5000 });
        await Expect(Page.Locator("#content-echo")).ToHaveTextAsync("Discharge Actions", new() { Timeout = 5000 });

        await Page.Locator("#check-disabled-btn").ClickAsync();
        await Expect(Page.Locator("#disabled-state")).ToHaveTextAsync("enabled", new() { Timeout = 5000 });

        await Page.Locator("#disable-menu-btn").ClickAsync();
        await Expect(Menu.Button).ToBeDisabledAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#disabled-echo")).ToHaveTextAsync("true", new() { Timeout = 5000 });

        await Page.Locator("#check-disabled-btn").ClickAsync();
        await Expect(Page.Locator("#disabled-state")).ToHaveTextAsync("disabled", new() { Timeout = 5000 });

        await Page.Locator("#enable-menu-btn").ClickAsync();
        await Expect(Menu.Button).ToBeEnabledAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#disabled-echo")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task set_css_class_updates_button_popup_and_css_source()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-css-btn").ClickAsync();

        Assert.That(await Menu.HasClass("e-success"), Is.True);
        Assert.That(await Menu.HasClass("extra-class"), Is.True);
        await Expect(Page.Locator("#css-echo")).ToHaveTextAsync("e-success extra-class", new() { Timeout = 5000 });

        await Page.Locator("#toggle-menu-btn").ClickAsync();
        await Expect(Menu.Button).ToHaveAttributeAsync("aria-expanded", "true", new() { Timeout = 5000 });
        Assert.That(await Menu.IsPopupOpen(), Is.True);
        Assert.That(await Menu.PopupHasClass("e-success"), Is.True);
        Assert.That(await Menu.PopupHasClass("extra-class"), Is.True);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task toggle_method_opens_and_closes_popup()
    {
        await NavigateAndBoot();

        await Page.Locator("#toggle-menu-btn").ClickAsync();
        await Expect(Page.Locator("#toggle-state")).ToHaveTextAsync("toggle called", new() { Timeout = 5000 });
        await Expect(Menu.Button).ToHaveAttributeAsync("aria-expanded", "true", new() { Timeout = 5000 });
        Assert.That(await Menu.IsPopupOpen(), Is.True);

        await Page.Locator("#toggle-menu-btn").DispatchEventAsync("click");
        await Expect(Menu.Button).ToHaveAttributeAsync("aria-expanded", "false", new() { Timeout = 5000 });
        Assert.That(await Menu.IsPopupOpen(), Is.False);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task focusin_method_focuses_dropdown_button()
    {
        await NavigateAndBoot();

        await Page.Locator("#focus-menu-btn").ClickAsync();

        await Expect(Page.Locator("#focus-state")).ToHaveTextAsync("focus called", new() { Timeout = 5000 });
        await Expect(Menu.Button).ToBeFocusedAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task remove_items_methods_remove_popup_items_by_text_and_id()
    {
        await NavigateAndBoot();

        await Page.Locator("#remove-assign-btn").ClickAsync();
        await Expect(Page.Locator("#remove-state")).ToHaveTextAsync("assign removed", new() { Timeout = 5000 });
        await Menu.Button.ClickAsync();
        await Expect(Menu.Item("Assign nurse")).ToHaveCountAsync(0, new() { Timeout = 5000 });
        await Expect(Menu.Item("Place hold")).ToHaveCountAsync(1, new() { Timeout = 5000 });

        await Page.Locator("#remove-hold-btn").ClickAsync();
        await Expect(Page.Locator("#remove-state")).ToHaveTextAsync("hold removed", new() { Timeout = 5000 });
        await Menu.Button.ClickAsync();
        await Expect(Menu.Item("Place hold")).ToHaveCountAsync(0, new() { Timeout = 5000 });
        await Expect(Menu.Item("View profile")).ToHaveCountAsync(1, new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task select_event_reads_typed_item_payload_and_condition()
    {
        await NavigateAndBoot();

        await Menu.Button.ClickAsync();
        await Menu.Item("Assign nurse").ClickAsync();

        await Expect(Page.Locator("#selected-id")).ToHaveTextAsync("assign", new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-text")).ToHaveTextAsync("Assign nurse", new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-disabled")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-condition")).ToHaveTextAsync("assign selected", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task gather_consumes_dropdown_button_sources()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-content-btn").ClickAsync();
        await Page.Locator("#set-css-btn").ClickAsync();
        await Page.Locator("#disable-menu-btn").ClickAsync();
        await Page.Locator("#gather-btn").ClickAsync();

        await Expect(Page.Locator("#gather-content")).ToHaveTextAsync("Discharge Actions", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-disabled")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-css")).ToHaveTextAsync("e-success extra-class", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-summary")).ToHaveTextAsync("Discharge Actions:True:e-success extra-class", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
