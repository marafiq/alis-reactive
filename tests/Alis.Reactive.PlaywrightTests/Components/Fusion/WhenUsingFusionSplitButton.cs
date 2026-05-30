using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

[TestFixture]
public class WhenUsingFusionSplitButton : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionSplitButton";
    private const string MenuId = "resident-split-actions";

    private FusionSplitButtonLocator Menu => new(Page, MenuId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#content-echo");
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionSplitButton — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_contains_typed_split_button_members()
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
        Assert.That(planJson, Does.Contain("\"click\""));
        Assert.That(planJson, Does.Contain("\"select\""));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_sets_visible_content_and_reads_content_source()
    {
        await NavigateAndBoot();

        await Expect(Menu.Primary).ToContainTextAsync("Ready Review", new() { Timeout = 5000 });
        await Expect(Page.Locator("#content-echo")).ToHaveTextAsync("Ready Review", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task primary_click_event_runs_notification_pipeline()
    {
        await NavigateAndBoot();

        await Menu.Primary.ClickAsync();

        await Expect(Page.Locator("#primary-click-state")).ToHaveTextAsync("primary clicked", new() { Timeout = 5000 });
        await Expect(Page.Locator("#command-state")).ToHaveTextAsync("primary clicked", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task set_content_and_disabled_update_visible_state_sources_and_condition()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-content-btn").ClickAsync();
        await Expect(Menu.Primary).ToContainTextAsync("Discharge Review", new() { Timeout = 5000 });
        await Expect(Page.Locator("#content-echo")).ToHaveTextAsync("Discharge Review", new() { Timeout = 5000 });

        await Page.Locator("#check-disabled-btn").ClickAsync();
        await Expect(Page.Locator("#disabled-state")).ToHaveTextAsync("enabled", new() { Timeout = 5000 });

        await Page.Locator("#disable-split-btn").ClickAsync();
        await Expect(Menu.Primary).ToBeDisabledAsync(new() { Timeout = 5000 });
        await Expect(Menu.Secondary).ToBeDisabledAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#disabled-echo")).ToHaveTextAsync("true", new() { Timeout = 5000 });

        await Page.Locator("#check-disabled-btn").ClickAsync();
        await Expect(Page.Locator("#disabled-state")).ToHaveTextAsync("disabled", new() { Timeout = 5000 });

        await Page.Locator("#enable-split-btn").ClickAsync();
        await Expect(Menu.Primary).ToBeEnabledAsync(new() { Timeout = 5000 });
        await Expect(Menu.Secondary).ToBeEnabledAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#disabled-echo")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task set_css_class_updates_buttons_wrapper_popup_and_css_source()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-css-btn").ClickAsync();

        Assert.That(await Menu.PrimaryHasClass("e-success"), Is.True);
        Assert.That(await Menu.PrimaryHasClass("extra-class"), Is.True);
        Assert.That(await Menu.SecondaryHasClass("e-success"), Is.True);
        Assert.That(await Menu.SecondaryHasClass("extra-class"), Is.True);
        Assert.That(await Menu.WrapperHasClass("e-success"), Is.True);
        Assert.That(await Menu.WrapperHasClass("extra-class"), Is.True);
        await Expect(Page.Locator("#css-echo")).ToHaveTextAsync("e-success extra-class", new() { Timeout = 5000 });

        await Page.Locator("#toggle-split-btn").ClickAsync();
        Assert.That(await Menu.IsPopupOpen(), Is.True);
        Assert.That(await Menu.PopupHasClass("e-success"), Is.True);
        Assert.That(await Menu.PopupHasClass("extra-class"), Is.True);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task toggle_method_opens_and_closes_secondary_popup()
    {
        await NavigateAndBoot();

        await Page.Locator("#toggle-split-btn").ClickAsync();
        await Expect(Page.Locator("#toggle-state")).ToHaveTextAsync("toggle called", new() { Timeout = 5000 });
        Assert.That(await Menu.IsPopupOpen(), Is.True);

        await Page.Locator("#toggle-split-btn").DispatchEventAsync("click");
        Assert.That(await Menu.IsPopupOpen(), Is.False);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task focusin_method_focuses_primary_button()
    {
        await NavigateAndBoot();

        await Page.Locator("#focus-split-btn").ClickAsync();

        await Expect(Page.Locator("#focus-state")).ToHaveTextAsync("focus called", new() { Timeout = 5000 });
        await Expect(Menu.Primary).ToBeFocusedAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task remove_items_methods_remove_popup_items_by_text_and_id()
    {
        await NavigateAndBoot();

        await Page.Locator("#remove-schedule-btn").ClickAsync();
        await Expect(Page.Locator("#remove-state")).ToHaveTextAsync("schedule removed", new() { Timeout = 5000 });
        await Menu.Secondary.ClickAsync();
        await Expect(Menu.Item("Schedule visit")).ToHaveCountAsync(0, new() { Timeout = 5000 });
        await Expect(Menu.Item("Place hold")).ToHaveCountAsync(1, new() { Timeout = 5000 });

        await Page.Locator("#remove-hold-btn").ClickAsync();
        await Expect(Page.Locator("#remove-state")).ToHaveTextAsync("hold removed", new() { Timeout = 5000 });
        await Menu.Secondary.ClickAsync();
        await Expect(Menu.Item("Place hold")).ToHaveCountAsync(0, new() { Timeout = 5000 });
        await Expect(Menu.Item("Assign nurse")).ToHaveCountAsync(1, new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task select_event_reads_typed_item_payload_and_condition()
    {
        await NavigateAndBoot();

        await Menu.Secondary.ClickAsync();
        await Menu.Item("Assign nurse").ClickAsync();

        await Expect(Page.Locator("#selected-id")).ToHaveTextAsync("assign", new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-text")).ToHaveTextAsync("Assign nurse", new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-disabled")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-condition")).ToHaveTextAsync("assign selected", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task gather_consumes_split_button_sources()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-content-btn").ClickAsync();
        await Page.Locator("#set-css-btn").ClickAsync();
        await Page.Locator("#disable-split-btn").ClickAsync();
        await Page.Locator("#gather-btn").ClickAsync();

        await Expect(Page.Locator("#gather-content")).ToHaveTextAsync("Discharge Review", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-disabled")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-css")).ToHaveTextAsync("e-success extra-class", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-summary")).ToHaveTextAsync("Discharge Review:True:e-success extra-class", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
