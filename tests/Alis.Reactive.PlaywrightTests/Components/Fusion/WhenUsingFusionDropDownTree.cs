using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

[TestFixture]
public class WhenUsingFusionDropDownTree : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionDropDownTree";
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_FusionDropDownTreeModel";
    private const string ResidentIdsId = Scope + "__ResidentIds";

    private FusionDropDownTreeLocator ResidentTree => new(Page, ResidentIdsId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#text-echo", "pending");
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionDropDownTree — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_sets_value_and_reads_text_and_value_condition()
    {
        await NavigateAndBoot();

        await Expect(ResidentTree.Input).ToHaveValueAsync("Alice", new() { Timeout = 5000 });
        await Expect(Page.Locator("#text-echo")).ToHaveTextAsync("Alice", new() { Timeout = 5000 });
        await Expect(Page.Locator("#value-state")).ToHaveTextAsync("has values", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task set_value_and_text_update_visible_state_sources()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-value-btn").ClickAsync();
        await Expect(ResidentTree.Input).ToHaveValueAsync("Bennett", new() { Timeout = 5000 });
        await Expect(Page.Locator("#text-echo")).ToHaveTextAsync("Bennett", new() { Timeout = 5000 });
        await Expect(Page.Locator("#value-state")).ToHaveTextAsync("has values", new() { Timeout = 5000 });

        await Page.Locator("#set-text-btn").ClickAsync();
        await Expect(ResidentTree.Input).ToHaveValueAsync("Carey", new() { Timeout = 5000 });
        await Expect(Page.Locator("#text-echo")).ToHaveTextAsync("Carey", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task showpopup_and_hidepopup_methods_control_popup()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();
        await Expect(ResidentTree.Popup).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(ResidentTree.TreeItemText("Carey")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Page.Locator("#hide-popup-btn").ClickAsync();
        await Expect(ResidentTree.Popup).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_payload_and_component_condition_update_trace()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();
        await ResidentTree.SelectItem("Bennett");

        await Expect(ResidentTree.Input).ToHaveValueAsync("Bennett", new() { Timeout = 5000 });
        await Expect(Page.Locator("#change-interacted")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Page.Locator("#change-value-state")).ToHaveTextAsync("has values", new() { Timeout = 5000 });
        await Expect(Page.Locator("#change-old-value-state")).ToHaveTextAsync("had old values", new() { Timeout = 5000 });
        await Expect(Page.Locator("#event-text")).ToHaveTextAsync("Bennett", new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator")).ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clear_method_resets_input_and_hides_component_condition()
    {
        await NavigateAndBoot();

        await Page.Locator("#show-popup-btn").ClickAsync();
        await ResidentTree.SelectItem("Carey");
        await Expect(Page.Locator("#selected-indicator")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Page.Locator("#clear-tree-btn").ClickAsync();
        await Expect(ResidentTree.Input).ToHaveValueAsync("", new() { Timeout = 5000 });
        await Expect(Page.Locator("#command-state")).ToHaveTextAsync("cleared", new() { Timeout = 5000 });
        await Expect(Page.Locator("#value-state")).ToHaveTextAsync("empty", new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator")).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task gather_consumes_value_and_text_sources()
    {
        await NavigateAndBoot();

        await Page.Locator("#set-value-btn").ClickAsync();
        await Page.Locator("#gather-btn").ClickAsync();

        await Expect(Page.Locator("#gather-value")).ToHaveTextAsync("bennett", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-text")).ToHaveTextAsync("Bennett", new() { Timeout = 5000 });
        await Expect(Page.Locator("#gather-summary")).ToHaveTextAsync("bennett:Bennett", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
