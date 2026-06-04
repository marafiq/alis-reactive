using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.ComboBox;

[TestFixture]
public class WhenUsingFusionComboBox : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionComboBox";
    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_FusionComboBoxModel";
    private const string ResidentId = GeneratedTypeScope + "__Resident";

    private FusionComboBoxLocator Resident => new(Page, ResidentId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#value-echo", "pending");
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionComboBox — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_contains_typed_combo_box_members()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();

        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""));
        Assert.That(planJson, Does.Contain(ResidentId));
        Assert.That(planJson, Does.Contain("\"value\""));
        Assert.That(planJson, Does.Contain("\"text\""));
        Assert.That(planJson, Does.Contain("\"index\""));
        Assert.That(planJson, Does.Contain("\"dataBind\""));
        Assert.That(planJson, Does.Contain("\"showPopup\""));
        Assert.That(planJson, Does.Contain("\"hidePopup\""));
        Assert.That(planJson, Does.Contain("\"focusIn\""));
        Assert.That(planJson, Does.Contain("\"focusOut\""));
        Assert.That(planJson, Does.Contain("\"clear\""));
        Assert.That(planJson, Does.Contain("\"change\""));
        Assert.That(planJson, Does.Contain("\"focus\""));
        Assert.That(planJson, Does.Contain("\"blur\""));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_sets_value_and_reads_value_text_index_sources()
    {
        await NavigateAndBoot();

        await Expect(Resident.Input).ToHaveValueAsync("Alice", new() { Timeout = 5000 });
        await Expect(Page.Locator("#value-echo")).ToHaveTextAsync("alice", new() { Timeout = 5000 });
        await Expect(Page.Locator("#text-echo")).ToHaveTextAsync("Alice", new() { Timeout = 5000 });
        await Expect(Page.Locator("#index-echo")).ToHaveTextAsync("0", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task set_value_text_and_index_update_visible_state_and_sources()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-value-btn").ClickAsync();
        await Expect(Resident.Input).ToHaveValueAsync("Bennett", new() { Timeout = 5000 });
        await Expect(Page.Locator("#value-echo")).ToHaveTextAsync("bennett", new() { Timeout = 5000 });
        await Expect(Page.Locator("#text-echo")).ToHaveTextAsync("Bennett", new() { Timeout = 5000 });
        await Expect(Page.Locator("#index-echo")).ToHaveTextAsync("1", new() { Timeout = 5000 });

        await Page.Locator("#set-text-btn").ClickAsync();
        await Expect(Resident.Input).ToHaveValueAsync("Carey", new() { Timeout = 5000 });
        await Expect(Page.Locator("#value-echo")).ToHaveTextAsync("carey", new() { Timeout = 5000 });
        await Expect(Page.Locator("#text-echo")).ToHaveTextAsync("Carey", new() { Timeout = 5000 });
        await Expect(Page.Locator("#index-echo")).ToHaveTextAsync("2", new() { Timeout = 5000 });

        await Page.Locator("#set-index-btn").ClickAsync();
        await Expect(Resident.Input).ToHaveValueAsync("Dawson", new() { Timeout = 5000 });
        await Expect(Page.Locator("#value-echo")).ToHaveTextAsync("dawson", new() { Timeout = 5000 });
        await Expect(Page.Locator("#text-echo")).ToHaveTextAsync("Dawson", new() { Timeout = 5000 });
        await Expect(Page.Locator("#index-echo")).ToHaveTextAsync("3", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task showpopup_and_hidepopup_methods_control_popup()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(Resident.Popup).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Page.Locator("#hide-popup-btn").ClickAsync();
        await Expect(Resident.Popup).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task focusin_and_focusout_methods_control_focus_and_fire_events()
    {
        await NavigateAndBoot();

        await Page.Locator("#focus-combo-btn").ClickAsync();
        await Expect(Resident.Input).ToBeFocusedAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#focus-state")).ToHaveTextAsync("focused", new() { Timeout = 5000 });

        await Page.Locator("#blur-combo-btn").ClickAsync();
        await Expect(Resident.Input).Not.ToBeFocusedAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#blur-state")).ToHaveTextAsync("blurred", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_payload_and_conditions_update_trace()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Resident.SelectItem("Bennett");

        await Expect(Page.Locator("#change-value")).ToHaveTextAsync("bennett", new() { Timeout = 5000 });
        await Expect(Page.Locator("#change-interacted")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Page.Locator("#event-text")).ToHaveTextAsync("Bennett", new() { Timeout = 5000 });
        await Expect(Page.Locator("#args-condition")).ToHaveTextAsync("other resident", new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Resident.SelectItem("Alice");
        await Expect(Page.Locator("#args-condition")).ToHaveTextAsync("alice selected", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clear_method_resets_input_and_hides_component_condition()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Resident.SelectItem("Carey");
        await Expect(Page.Locator("#selected-indicator")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Page.Locator("#clear-combo-btn").ClickAsync();
        await Expect(Resident.Input).ToHaveValueAsync("", new() { Timeout = 5000 });
        await Expect(Page.Locator("#command-state")).ToHaveTextAsync("cleared", new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator")).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_index_source_supports_conditions()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-index-btn").ClickAsync();
        await Page.Locator("#check-index-btn").ClickAsync();

        await Expect(Page.Locator("#index-condition")).ToHaveTextAsync("index three", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task gather_consumes_value_text_and_index_sources()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-index-btn").ClickAsync();
        await Page.Locator("#gather-btn").ClickAsync();

        await Expect(Page.Locator("#gather-value")).ToHaveTextAsync("dawson", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-text")).ToHaveTextAsync("Dawson", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-index")).ToHaveTextAsync("3", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-summary")).ToHaveTextAsync("dawson:Dawson:3", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
