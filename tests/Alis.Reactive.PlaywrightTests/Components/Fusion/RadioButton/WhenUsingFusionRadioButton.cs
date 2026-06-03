using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.RadioButton;

[TestFixture]
public class WhenUsingFusionRadioButton : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionRadioButton";
    private const string PrivateId = "private-room-radio";
    private const string SharedId = "shared-room-radio";

    private FusionRadioButtonLocator PrivateRoom => new(Page, PrivateId);
    private FusionRadioButtonLocator SharedRoom => new(Page, SharedId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#selected-echo");
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionRadioButton — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_contains_typed_radio_button_members()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();

        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""));
        Assert.That(planJson, Does.Contain(PrivateId));
        Assert.That(planJson, Does.Contain(SharedId));
        Assert.That(planJson, Does.Contain("\"checked\""));
        Assert.That(planJson, Does.Contain("\"disabled\""));
        Assert.That(planJson, Does.Contain("\"dataBind\""));
        Assert.That(planJson, Does.Contain("\"getSelectedValue\""));
        Assert.That(planJson, Does.Contain("\"click\""));
        Assert.That(planJson, Does.Contain("\"focusIn\""));
        Assert.That(planJson, Does.Contain("\"change\""));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_sets_checked_state_and_selected_value_source()
    {
        await NavigateAndBoot();

        Assert.That(await SharedRoom.IsChecked(), Is.True);
        Assert.That(await PrivateRoom.IsChecked(), Is.False);
        await Expect(Page.Locator("#selected-echo")).ToHaveTextAsync("Shared", new() { Timeout = 5000 });
        await Expect(Page.Locator("#shared-checked-echo")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task set_checked_updates_group_selection_sources_and_condition()
    {
        await NavigateAndBoot();

        await Page.Locator("#check-selected-btn").ClickAsync();
        await Expect(Page.Locator("#selected-state")).ToHaveTextAsync("other selected", new() { Timeout = 5000 });

        await Page.Locator("#set-private-btn").ClickAsync();
        Assert.That(await PrivateRoom.IsChecked(), Is.True);
        Assert.That(await SharedRoom.IsChecked(), Is.False);
        await Expect(Page.Locator("#selected-echo")).ToHaveTextAsync("Private", new() { Timeout = 5000 });
        await Expect(Page.Locator("#private-checked-echo")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Page.Locator("#shared-checked-echo")).ToHaveTextAsync("false", new() { Timeout = 5000 });

        await Page.Locator("#check-selected-btn").ClickAsync();
        await Expect(Page.Locator("#selected-state")).ToHaveTextAsync("private selected", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task set_disabled_updates_visible_state_and_source()
    {
        await NavigateAndBoot();

        await Page.Locator("#disable-shared-btn").ClickAsync();
        await Expect(SharedRoom.Input).ToBeDisabledAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#disabled-echo")).ToHaveTextAsync("true", new() { Timeout = 5000 });

        await Page.Locator("#enable-shared-btn").ClickAsync();
        await Expect(SharedRoom.Input).ToBeEnabledAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#disabled-echo")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task click_method_selects_radio_and_change_event_reads_payload()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-private-btn").ClickAsync();
        await Page.Locator("#click-shared-btn").ClickAsync();

        await Expect(Page.Locator("#click-state")).ToHaveTextAsync("click called", new() { Timeout = 5000 });
        Assert.That(await SharedRoom.IsChecked(), Is.True);
        Assert.That(await PrivateRoom.IsChecked(), Is.False);
        await Expect(Page.Locator("#selected-echo")).ToHaveTextAsync("Shared", new() { Timeout = 5000 });
        await Expect(Page.Locator("#change-value")).ToHaveTextAsync("Shared", new() { Timeout = 5000 });
        await Expect(Page.Locator("#args-condition")).ToHaveTextAsync("shared", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task focusin_method_focuses_radio_button()
    {
        await NavigateAndBoot();

        await Page.Locator("#focus-private-btn").ClickAsync();

        await Expect(Page.Locator("#focus-state")).ToHaveTextAsync("focus called", new() { Timeout = 5000 });
        await Expect(PrivateRoom.Input).ToBeFocusedAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task gather_consumes_selected_value_and_state_sources()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-private-btn").ClickAsync();
        await Page.Locator("#disable-shared-btn").ClickAsync();
        await Page.Locator("#gather-btn").ClickAsync();

        await Expect(Page.Locator("#gather-selected")).ToHaveTextAsync("Private", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-private-checked")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-shared-checked")).ToHaveTextAsync("false", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-shared-disabled")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-summary")).ToHaveTextAsync("Private:True:False:True", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
