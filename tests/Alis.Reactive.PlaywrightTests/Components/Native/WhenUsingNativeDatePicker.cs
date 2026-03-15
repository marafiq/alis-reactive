namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// Exercises NativeDatePicker API end-to-end in the browser:
/// property writes (SetValue), property reads (Value as source),
/// reactive events (Changed with typed condition), and component-read conditions.
///
/// Page under test: /Sandbox/NativeDatePicker
/// </summary>
[TestFixture]
public class WhenUsingNativeDatePicker : PlaywrightTestBase
{
    private const string Path = "/Sandbox/NativeDatePicker";
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_NativeDatePickerModel__";

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
        await Expect(Page).ToHaveTitleAsync("NativeDatePicker — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    // ── Section 1: Property Write — DomReady sets admission date ──

    [Test]
    public async Task DomReady_sets_initial_date_value()
    {
        await NavigateAndBoot();

        var input = Page.Locator($"#{Scope}AdmissionDate");
        await Expect(input).ToHaveValueAsync("2026-03-15");
        AssertNoConsoleErrors();
    }

    // ── Section 2: Property Read — DomReady reads admission date into echo ──

    [Test]
    public async Task Value_echoed_from_component_read()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#value-echo");
        await Expect(echo).ToHaveTextAsync("2026-03-15", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 3: Changed event with typed condition ──

    [Test]
    public async Task Changed_event_with_condition_shows_status_when_date_selected()
    {
        await NavigateAndBoot();

        var input = Page.Locator($"#{Scope}BirthDate");
        await input.FillAsync("1945-06-15");

        var status = Page.Locator("#date-status");
        await Expect(status).ToHaveTextAsync("date selected", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 4: Component value condition ──

    [Test]
    public async Task Component_value_condition_shows_warning_when_empty()
    {
        await NavigateAndBoot();

        // Clear the admission date that was set by DomReady
        var input = Page.Locator($"#{Scope}AdmissionDate");
        await input.ClearAsync();

        // Click the button that checks admission date
        await Page.Locator("#check-date-btn").ClickAsync();

        var warning = Page.Locator("#admission-warning");
        await Expect(warning).ToHaveTextAsync("admission date is required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Component_value_condition_confirms_when_filled()
    {
        await NavigateAndBoot();

        // DomReady already set the date — just click check
        await Page.Locator("#check-date-btn").ClickAsync();

        var warning = Page.Locator("#admission-warning");
        await Expect(warning).ToHaveTextAsync("admission date set", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }
}
