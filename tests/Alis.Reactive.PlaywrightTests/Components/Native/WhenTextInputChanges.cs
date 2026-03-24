namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// Exercises NativeTextBox API end-to-end in the browser:
/// property writes (SetValue), property reads (Value as source),
/// reactive events (Changed with typed condition), and component-read conditions.
///
/// Page under test: /Sandbox/Components/NativeTextBox
/// </summary>
[TestFixture]
public class WhenTextInputChanges : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/NativeTextBox";
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_NativeTextBoxModel__";

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
        await Expect(Page).ToHaveTitleAsync("NativeTextBox — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    // ── Section 1: Property Write — DomReady sets resident name ──

    [Test]
    public async Task domready_sets_initial_resident_name()
    {
        await NavigateAndBoot();

        var input = Page.Locator($"#{Scope}ResidentName");
        await Expect(input).ToHaveValueAsync("Jane Doe");
        AssertNoConsoleErrors();
    }

    // ── Section 2: Property Read — DomReady reads resident name into echo ──

    [Test]
    public async Task value_echoed_from_component_read()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#value-echo");
        await Expect(echo).ToHaveTextAsync("Jane Doe", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 3: Changed event with typed condition ──

    [Test]
    public async Task changed_event_with_condition_shows_contact_status()
    {
        await NavigateAndBoot();

        var input = Page.Locator($"#{Scope}EmergencyContact");

        // Type a contact name — triggers Changed with non-empty value
        await input.FillAsync("John Smith");
        // FillAsync dispatches "input" but we need "change" — blur the field
        await input.BlurAsync();

        var status = Page.Locator("#contact-status");
        await Expect(status).ToHaveTextAsync("contact provided", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_with_condition_shows_warning_when_cleared()
    {
        await NavigateAndBoot();

        var input = Page.Locator($"#{Scope}EmergencyContact");

        // First fill, then clear to trigger empty condition
        await input.FillAsync("John Smith");
        await input.BlurAsync();
        await input.ClearAsync();
        await input.BlurAsync();

        var status = Page.Locator("#contact-status");
        await Expect(status).ToHaveTextAsync("contact required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 4: Component value condition ──

    [Test]
    public async Task component_value_condition_warns_when_empty()
    {
        await NavigateAndBoot();

        // Clear the resident name that was set by DomReady
        var input = Page.Locator($"#{Scope}ResidentName");
        await input.ClearAsync();

        // Click the button that checks resident name
        await Page.Locator("#check-name-btn").ClickAsync();

        var warning = Page.Locator("#name-warning");
        await Expect(warning).ToHaveTextAsync("name is required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_confirms_when_filled()
    {
        await NavigateAndBoot();

        // DomReady already set the name — just click check
        await Page.Locator("#check-name-btn").ClickAsync();

        var warning = Page.Locator("#name-warning");
        await Expect(warning).ToHaveTextAsync("name set", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Deep BDD: reactive re-evaluation cycles ──

    [Test]
    public async Task typing_then_clearing_then_retyping_fires_condition_each_time()
    {
        await NavigateAndBoot();

        var input = Page.Locator($"#{Scope}EmergencyContact");
        var status = Page.Locator("#contact-status");

        // Cycle 1: type a contact → "contact provided"
        await input.FillAsync("Alice Johnson");
        await input.BlurAsync();
        await Expect(status).ToHaveTextAsync("contact provided", new() { Timeout = 3000 });

        // Cycle 2: clear → "contact required"
        await input.ClearAsync();
        await input.BlurAsync();
        await Expect(status).ToHaveTextAsync("contact required", new() { Timeout = 3000 });

        // Cycle 3: retype → "contact provided" again
        await input.FillAsync("Bob Martinez");
        await input.BlurAsync();
        await Expect(status).ToHaveTextAsync("contact provided", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task overwriting_domready_value_updates_component_read()
    {
        await NavigateAndBoot();

        // DomReady set "Jane Doe" — verify echo shows it
        var echo = Page.Locator("#value-echo");
        await Expect(echo).ToHaveTextAsync("Jane Doe", new() { Timeout = 3000 });

        // User overwrites with "John Smith"
        var input = Page.Locator($"#{Scope}ResidentName");
        await input.ClearAsync();
        await input.FillAsync("John Smith");

        // Click check button — component-read condition evaluates CURRENT DOM value
        await Page.Locator("#check-name-btn").ClickAsync();

        // Component reads "John Smith" (non-empty) → "name set"
        var warning = Page.Locator("#name-warning");
        await Expect(warning).ToHaveTextAsync("name set", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
