namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// Exercises NativeTextBox SetValue, Value reads, Changed-event conditions,
/// and component-read conditions.
/// </summary>
[TestFixture]
public class WhenTextInputChanges : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/NativeTextBox";
    private const string ModelIdPrefix = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_NativeTextBoxModel__";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("NativeTextBox — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_sets_initial_resident_name()
    {
        await NavigateAndBoot();

        var input = Page.Locator($"#{ModelIdPrefix}ResidentName");
        await Expect(input).ToHaveValueAsync("Jane Doe");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task value_echoed_from_component_read()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#value-echo");
        await Expect(echo).ToHaveTextAsync("Jane Doe", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_with_condition_shows_contact_status()
    {
        await NavigateAndBoot();

        var input = Page.Locator($"#{ModelIdPrefix}EmergencyContact");

        await input.FillAsync("John Smith");
        // FillAsync dispatches input, but this test needs the change event.
        await input.BlurAsync();

        var status = Page.Locator("#contact-status");
        await Expect(status).ToHaveTextAsync("contact provided", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_with_condition_shows_warning_when_cleared()
    {
        await NavigateAndBoot();

        var input = Page.Locator($"#{ModelIdPrefix}EmergencyContact");

        await input.FillAsync("John Smith");
        await input.BlurAsync();
        await input.ClearAsync();
        await input.BlurAsync();

        var status = Page.Locator("#contact-status");
        await Expect(status).ToHaveTextAsync("contact required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_warns_when_empty()
    {
        await NavigateAndBoot();

        var input = Page.Locator($"#{ModelIdPrefix}ResidentName");
        await input.ClearAsync();

        await Page.Locator("#check-name-btn").ClickAsync();

        var warning = Page.Locator("#name-warning");
        await Expect(warning).ToHaveTextAsync("name is required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_confirms_when_filled()
    {
        await NavigateAndBoot();

        // DomReady already set the value under test.
        await Page.Locator("#check-name-btn").ClickAsync();

        var warning = Page.Locator("#name-warning");
        await Expect(warning).ToHaveTextAsync("name set", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task typing_then_clearing_then_retyping_fires_condition_each_time()
    {
        await NavigateAndBoot();

        var input = Page.Locator($"#{ModelIdPrefix}EmergencyContact");
        var status = Page.Locator("#contact-status");

        await input.FillAsync("Alice Johnson");
        await input.BlurAsync();
        await Expect(status).ToHaveTextAsync("contact provided", new() { Timeout = 3000 });

        await input.ClearAsync();
        await input.BlurAsync();
        await Expect(status).ToHaveTextAsync("contact required", new() { Timeout = 3000 });

        await input.FillAsync("Bob Martinez");
        await input.BlurAsync();
        await Expect(status).ToHaveTextAsync("contact provided", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task overwriting_domready_value_updates_component_read()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#value-echo");
        await Expect(echo).ToHaveTextAsync("Jane Doe", new() { Timeout = 3000 });

        var input = Page.Locator($"#{ModelIdPrefix}ResidentName");
        await input.ClearAsync();
        await input.FillAsync("John Smith");

        // Component-read condition must use the current DOM value, not the DomReady value.
        await Page.Locator("#check-name-btn").ClickAsync();

        var warning = Page.Locator("#name-warning");
        await Expect(warning).ToHaveTextAsync("name set", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
