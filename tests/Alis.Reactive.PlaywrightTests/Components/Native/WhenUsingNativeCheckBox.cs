namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// Exercises NativeCheckBox API end-to-end in the browser:
/// property writes (SetChecked), property reads (Value as source),
/// reactive events (Changed with typed condition), and component-read conditions.
///
/// Page under test: /Sandbox/CheckBox
/// </summary>
[TestFixture]
public class WhenUsingNativeCheckBox : PlaywrightTestBase
{
    private const string Path = "/Sandbox/CheckBox";
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_CheckBoxModel__";

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
        await Expect(Page).ToHaveTitleAsync("NativeCheckBox — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Plan_json_is_rendered()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("mutate-element"),
            "Plan must contain mutate-element commands");
        Assert.That(planJson, Does.Contain("\"prop\""),
            "Plan must contain structured prop field");
        AssertNoConsoleErrors();
    }

    // ── Section 1: Property Write — DomReady unchecks medication checkbox ──

    [Test]
    public async Task DomReady_unchecks_medication_checkbox()
    {
        // ReceivesMedication starts checked in the model, but DomReady calls SetChecked(false).
        // Expected: checkbox is unchecked after boot.
        await NavigateAndBoot();

        var cb = Page.Locator($"#{Scope}ReceivesMedication");
        await Expect(cb).Not.ToBeCheckedAsync(new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 2: Property Read — DomReady reads AllowsVisitors value into echo ──

    [Test]
    public async Task Value_echoed_from_component_read()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#value-echo");
        await Expect(echo).ToHaveTextAsync("false", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 3: Changed event with typed condition ──

    [Test]
    public async Task Changed_event_shows_restrictions_when_checked()
    {
        await NavigateAndBoot();

        // Restrictions panel starts hidden
        await Expect(Page.Locator("#restrictions-panel")).ToBeHiddenAsync();

        // Check the dietary restrictions checkbox
        await Page.Locator($"#{Scope}HasDietaryRestrictions").CheckAsync();

        // Restrictions panel should appear and status should say "checked"
        await Expect(Page.Locator("#restrictions-panel"))
            .ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(Page.Locator("#restrictions-status"))
            .ToHaveTextAsync("checked", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Changed_event_hides_restrictions_when_unchecked()
    {
        await NavigateAndBoot();

        // Check then uncheck the dietary restrictions checkbox
        await Page.Locator($"#{Scope}HasDietaryRestrictions").CheckAsync();
        await Expect(Page.Locator("#restrictions-panel"))
            .ToBeVisibleAsync(new() { Timeout = 3000 });

        await Page.Locator($"#{Scope}HasDietaryRestrictions").UncheckAsync();

        // Restrictions panel should hide and status should say "unchecked"
        await Expect(Page.Locator("#restrictions-panel"))
            .ToBeHiddenAsync(new() { Timeout = 3000 });
        await Expect(Page.Locator("#restrictions-status"))
            .ToHaveTextAsync("unchecked", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 4: Component value condition ──

    [Test]
    public async Task Component_value_condition_confirms_when_checked()
    {
        await NavigateAndBoot();

        // DomReady unchecked the medication checkbox — re-check it first
        await Page.Locator($"#{Scope}ReceivesMedication").CheckAsync();

        // Click the button that checks medication status
        await Page.Locator("#check-medication-btn").ClickAsync();

        var warning = Page.Locator("#medication-warning");
        await Expect(warning).ToHaveTextAsync("resident receives medication", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Component_value_condition_warns_when_unchecked()
    {
        await NavigateAndBoot();

        // DomReady already unchecked the medication checkbox — just click check
        await Page.Locator("#check-medication-btn").ClickAsync();

        var warning = Page.Locator("#medication-warning");
        await Expect(warning).ToHaveTextAsync("no medication on record", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }
}
