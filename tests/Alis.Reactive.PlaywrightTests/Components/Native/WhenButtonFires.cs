namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// Exercises NativeButton API end-to-end in the browser:
/// click events, <c>SetText</c> mutations, and dispatch chains.
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

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("NativeButton — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task click_event_updates_status_text()
    {
        await NavigateAndBoot();

        await Page.Locator("#btn-admit").ClickAsync();

        var status = Page.Locator("#admit-status");
        await Expect(status).ToHaveTextAsync("Admit Resident clicked", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_sets_button_text()
    {
        await NavigateAndBoot();

        var button = Page.Locator("#btn-admit-text");
        await Expect(button).ToHaveTextAsync("Admit Resident", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task click_dispatches_event_to_another_listener()
    {
        await NavigateAndBoot();

        await Page.Locator("#btn-transfer").ClickAsync();

        var status = Page.Locator("#transfer-status");
        await Expect(status).ToHaveTextAsync("transfer confirmed", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_multiple_buttons_in_sequence_updates_each_status()
    {
        await NavigateAndBoot();

        await Page.Locator("#btn-admit").ClickAsync();
        var admitStatus = Page.Locator("#admit-status");
        await Expect(admitStatus).ToHaveTextAsync("Admit Resident clicked", new() { Timeout = 3000 });

        await Page.Locator("#btn-transfer").ClickAsync();
        var transferStatus = Page.Locator("#transfer-status");
        await Expect(transferStatus).ToHaveTextAsync("transfer confirmed", new() { Timeout = 3000 });

        // Reset status so the re-click must produce a visible mutation.
        await admitStatus.EvaluateAsync("el => el.textContent = 'reset'");
        await Page.Locator("#btn-admit").ClickAsync();
        await Expect(admitStatus).ToHaveTextAsync("Admit Resident clicked", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task dispatch_chain_from_button_reaches_custom_event_listener()
    {
        await NavigateAndBoot();

        var transferStatus = Page.Locator("#transfer-status");
        await Expect(transferStatus).Not.ToHaveTextAsync("transfer confirmed");

        // Button Click dispatches resident-transferred, then the CustomEvent listener mutates text and classes.
        await Page.Locator("#btn-transfer").ClickAsync();

        await Expect(transferStatus).ToHaveTextAsync("transfer confirmed", new() { Timeout = 3000 });

        await Expect(transferStatus).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));

        AssertNoConsoleErrors();
    }
}
