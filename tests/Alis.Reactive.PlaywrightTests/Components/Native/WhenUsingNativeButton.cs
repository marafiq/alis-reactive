namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// Exercises NativeButton API end-to-end in the browser:
/// click events, SetText mutations, FocusIn calls, and dispatch chains.
///
/// Page under test: /Sandbox/NativeButton
/// </summary>
[TestFixture]
public class WhenUsingNativeButton : PlaywrightTestBase
{
    private const string Path = "/Sandbox/NativeButton";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
    }

    // ── Page loads ──

    [Test]
    public async Task Page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("NativeButton — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    // ── Section 1: Click event updates status text ──

    [Test]
    public async Task Click_event_updates_status_text()
    {
        await NavigateAndBoot();

        await Page.Locator("#btn-admit").ClickAsync();

        var status = Page.Locator("#admit-status");
        await Expect(status).ToHaveTextAsync("Admit Resident clicked", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 2: DomReady sets button text ──

    [Test]
    public async Task DomReady_sets_button_text()
    {
        await NavigateAndBoot();

        var button = Page.Locator("#btn-admit-text");
        await Expect(button).ToHaveTextAsync("Admit Resident", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 4: Click dispatches event to another listener ──

    [Test]
    public async Task Click_dispatches_event_to_another_listener()
    {
        await NavigateAndBoot();

        await Page.Locator("#btn-transfer").ClickAsync();

        var status = Page.Locator("#transfer-status");
        await Expect(status).ToHaveTextAsync("transfer confirmed", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }
}
