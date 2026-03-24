namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// Exercises NativeButton API end-to-end in the browser:
/// click events, SetText mutations, FocusIn calls, and dispatch chains.
///
/// Page under test: /Sandbox/Components/NativeButton
/// </summary>
[TestFixture]
public class WhenButtonFires : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/NativeButton";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
    }

    // ── Page loads ──

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("NativeButton — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    // ── Section 1: Click event updates status text ──

    [Test]
    public async Task click_event_updates_status_text()
    {
        await NavigateAndBoot();

        await Page.Locator("#btn-admit").ClickAsync();

        var status = Page.Locator("#admit-status");
        await Expect(status).ToHaveTextAsync("Admit Resident clicked", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 2: DomReady sets button text ──

    [Test]
    public async Task domready_sets_button_text()
    {
        await NavigateAndBoot();

        var button = Page.Locator("#btn-admit-text");
        await Expect(button).ToHaveTextAsync("Admit Resident", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 4: Click dispatches event to another listener ──

    [Test]
    public async Task click_dispatches_event_to_another_listener()
    {
        await NavigateAndBoot();

        await Page.Locator("#btn-transfer").ClickAsync();

        var status = Page.Locator("#transfer-status");
        await Expect(status).ToHaveTextAsync("transfer confirmed", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Multi-click stability ──

    [Test]
    public async Task clicking_multiple_buttons_in_sequence_updates_each_status()
    {
        await NavigateAndBoot();

        // Click Admit → verify status updates
        await Page.Locator("#btn-admit").ClickAsync();
        var admitStatus = Page.Locator("#admit-status");
        await Expect(admitStatus).ToHaveTextAsync("Admit Resident clicked", new() { Timeout = 3000 });

        // Click Transfer → verify transfer chain fires
        await Page.Locator("#btn-transfer").ClickAsync();
        var transferStatus = Page.Locator("#transfer-status");
        await Expect(transferStatus).ToHaveTextAsync("transfer confirmed", new() { Timeout = 3000 });

        // Click Admit again → verify status still updates (handlers not disconnected)
        // First reset the text to confirm it actually changes on re-click
        await admitStatus.EvaluateAsync("el => el.textContent = 'reset'");
        await Page.Locator("#btn-admit").ClickAsync();
        await Expect(admitStatus).ToHaveTextAsync("Admit Resident clicked", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Dispatch chain: button Click → Dispatch() → CustomEvent listener → DOM mutation ──

    [Test]
    public async Task dispatch_chain_from_button_reaches_custom_event_listener()
    {
        await NavigateAndBoot();

        // Before click: transfer-status shows the initial dash
        var transferStatus = Page.Locator("#transfer-status");
        await Expect(transferStatus).Not.ToHaveTextAsync("transfer confirmed");

        // Click Transfer button → dispatches "resident-transferred" →
        // CustomEvent listener picks it up → sets text + swaps classes
        await Page.Locator("#btn-transfer").ClickAsync();

        // Verify text mutation from the custom-event listener
        await Expect(transferStatus).ToHaveTextAsync("transfer confirmed", new() { Timeout = 3000 });

        // Verify class mutations from the custom-event listener
        await Expect(transferStatus).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));

        AssertNoConsoleErrors();
    }
}
