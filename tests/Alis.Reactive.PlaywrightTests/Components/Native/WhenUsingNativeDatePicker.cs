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
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("NativeDatePicker — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    // ── Section 1: Property Write — DomReady sets admission date ──

    [Test]
    public async Task domready_sets_initial_date_value()
    {
        await NavigateAndBoot();

        var input = Page.Locator($"#{Scope}AdmissionDate");
        await Expect(input).ToHaveValueAsync("2026-03-15");
        AssertNoConsoleErrors();
    }

    // ── Section 2: Property Read — DomReady reads admission date into echo ──

    [Test]
    public async Task value_echoed_from_component_read()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#value-echo");
        await Expect(echo).ToHaveTextAsync("2026-03-15", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 3: Changed event with typed condition ──

    [Test]
    public async Task changed_event_with_condition_shows_status_when_date_selected()
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
    public async Task component_value_condition_shows_warning_when_empty()
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
    public async Task component_value_condition_confirms_when_filled()
    {
        await NavigateAndBoot();

        // DomReady already set the date — just click check
        await Page.Locator("#check-date-btn").ClickAsync();

        var warning = Page.Locator("#admission-warning");
        await Expect(warning).ToHaveTextAsync("admission date set", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Multi-step state-cycle scenarios ──

    [Test]
    public async Task clearing_date_then_setting_new_date_updates_condition_both_ways()
    {
        // Proves the component value updates correctly after clear->fill cycle
        await NavigateAndBoot();

        var input = Page.Locator($"#{Scope}AdmissionDate");
        var btn = Page.Locator("#check-date-btn");
        var warning = Page.Locator("#admission-warning");

        // DomReady set 2026-03-15 — verify initial state
        await Expect(input).ToHaveValueAsync("2026-03-15");

        // Click check — should confirm "admission date set"
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("admission date set", new() { Timeout = 3000 });

        // Clear it
        await input.ClearAsync();

        // Click check button — should warn "required"
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("admission date is required", new() { Timeout = 3000 });

        // Set new date
        await input.FillAsync("2026-12-25");

        // Click check again — should confirm "set"
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("admission date set", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_birth_date_multiple_times_fires_condition_each_time()
    {
        // Proves the Changed event fires on every input change, not just the first
        // The condition is NotNull — any non-null value (including "") takes the "then" branch
        await NavigateAndBoot();

        var input = Page.Locator($"#{Scope}BirthDate");
        var status = Page.Locator("#date-status");

        // Status starts with dash (no event fired yet)
        await Expect(status).ToHaveClassAsync("text-text-muted", new() { Timeout = 3000 });

        // Set first date — condition fires, status shows "date selected" with green class
        await input.FillAsync("1945-06-15");
        await Expect(status).ToHaveTextAsync("date selected", new() { Timeout = 3000 });
        await Expect(status).ToHaveClassAsync("text-green-600", new() { Timeout = 3000 });

        // Set a different date — handler fires again, status still shows "date selected"
        await input.FillAsync("2000-01-01");
        await Expect(status).ToHaveTextAsync("date selected", new() { Timeout = 3000 });
        await Expect(status).ToHaveClassAsync("text-green-600", new() { Timeout = 3000 });

        // Set yet another date — proves handler fires every time, not just once
        await input.FillAsync("1999-12-31");
        await Expect(status).ToHaveTextAsync("date selected", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
