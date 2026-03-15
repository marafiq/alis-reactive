namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// Exercises NativeDropDown API end-to-end in the browser:
/// property writes (SetValue), property reads (Value as source),
/// reactive events (Changed with typed condition), and component-read conditions.
///
/// Page under test: /Sandbox/NativeDropDown
/// </summary>
[TestFixture]
public class WhenUsingNativeDropDown : PlaywrightTestBase
{
    private const string Path = "/Sandbox/NativeDropDown";
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_NativeDropDownModel__";

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
        await Expect(Page).ToHaveTitleAsync("NativeDropDown — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    // ── Section 1: Property Write — DomReady sets care level ──

    [Test]
    public async Task domready_sets_initial_care_level()
    {
        await NavigateAndBoot();

        var select = Page.Locator($"#{Scope}CareLevel");
        await Expect(select).ToHaveValueAsync("Memory Care");
        AssertNoConsoleErrors();
    }

    // ── Section 2: Property Read — DomReady reads care level into echo ──

    [Test]
    public async Task value_echoed_from_component_read()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#value-echo");
        await Expect(echo).ToHaveTextAsync("Memory Care", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 3: Changed event with typed condition ──

    [Test]
    public async Task changed_event_with_condition_shows_medical_notice()
    {
        await NavigateAndBoot();

        var select = Page.Locator($"#{Scope}FacilityType");
        await select.SelectOptionAsync("Medical");

        var notice = Page.Locator("#medical-notice");
        await Expect(notice).ToHaveTextAsync("medical facility selected", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_with_condition_shows_else_for_non_medical()
    {
        await NavigateAndBoot();

        var select = Page.Locator($"#{Scope}FacilityType");
        await select.SelectOptionAsync("Residential");

        var notice = Page.Locator("#medical-notice");
        await Expect(notice).ToHaveTextAsync("not a medical facility", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 4: Component value condition ──

    [Test]
    public async Task component_value_condition_confirms_when_selected()
    {
        await NavigateAndBoot();

        // DomReady already set care level to "Memory Care" — just click check
        await Page.Locator("#check-care-btn").ClickAsync();

        var status = Page.Locator("#care-confirmation");
        await Expect(status).ToHaveTextAsync("care level confirmed", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_warns_when_empty()
    {
        await NavigateAndBoot();

        // Reset care level to placeholder (empty value)
        var select = Page.Locator($"#{Scope}CareLevel");
        await select.SelectOptionAsync("");

        // Click the button that checks care level
        await Page.Locator("#check-care-btn").ClickAsync();

        var status = Page.Locator("#care-confirmation");
        await Expect(status).ToHaveTextAsync("care level is required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Multi-step state-cycle scenarios ──

    [Test]
    public async Task changing_selection_multiple_times_updates_status_each_time()
    {
        // Proves the reactive handler fires on EVERY change, not just the first
        await NavigateAndBoot();

        var select = Page.Locator($"#{Scope}FacilityType");
        var notice = Page.Locator("#medical-notice");

        // Select Medical — should show "medical facility selected"
        await select.SelectOptionAsync("Medical");
        await Expect(notice).ToHaveTextAsync("medical facility selected", new() { Timeout = 3000 });

        // Select Residential — should show "not a medical facility"
        await select.SelectOptionAsync("Residential");
        await Expect(notice).ToHaveTextAsync("not a medical facility", new() { Timeout = 3000 });

        // Select Medical again — should show "medical facility selected" again
        await select.SelectOptionAsync("Medical");
        await Expect(notice).ToHaveTextAsync("medical facility selected", new() { Timeout = 3000 });

        // Select Rehabilitation — should show "not a medical facility"
        await select.SelectOptionAsync("Rehabilitation");
        await Expect(notice).ToHaveTextAsync("not a medical facility", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_care_level_then_reselecting_updates_condition_both_ways()
    {
        // Proves the component-read condition re-evaluates after clear->reselect cycle
        await NavigateAndBoot();

        var select = Page.Locator($"#{Scope}CareLevel");
        var btn = Page.Locator("#check-care-btn");
        var status = Page.Locator("#care-confirmation");

        // DomReady set "Memory Care" — button should confirm
        await btn.ClickAsync();
        await Expect(status).ToHaveTextAsync("care level confirmed", new() { Timeout = 3000 });

        // Clear to placeholder
        await select.SelectOptionAsync("");
        await btn.ClickAsync();
        await Expect(status).ToHaveTextAsync("care level is required", new() { Timeout = 3000 });

        // Select a different value
        await select.SelectOptionAsync("Skilled Nursing");
        await btn.ClickAsync();
        await Expect(status).ToHaveTextAsync("care level confirmed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
