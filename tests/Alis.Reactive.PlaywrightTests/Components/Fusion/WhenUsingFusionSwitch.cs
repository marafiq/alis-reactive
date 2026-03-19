namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

/// <summary>
/// Exercises all FusionSwitch API end-to-end in the browser:
/// property writes (SetChecked), property reads (Value as source),
/// reactive events (Changed with typed condition), and component-read conditions.
///
/// Page under test: /Sandbox/Switch
///
/// Syncfusion Switch renders an input element inside a wrapper.
/// The wrapper element gets the IdGenerator-based ID; the ej2 instance
/// is attached to this element. Playwright uses the ej2 instance API to
/// toggle state and trigger change events reliably.
///
/// Senior living domain: notification preferences, email/SMS alerts.
/// </summary>
[TestFixture]
public class WhenUsingFusionSwitch : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Switch";

    // IdGenerator produces: {TypeScope}__{PropertyName}
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_SwitchModel";
    private const string ReceiveNotificationsId = Scope + "__ReceiveNotifications";
    private const string EmailAlertsId = Scope + "__EmailAlerts";
    private const string SmsAlertsId = Scope + "__SmsAlerts";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    /// <summary>
    /// Sets the checked state on a Syncfusion Switch ej2 instance via JS.
    /// Setting .checked + .dataBind() updates internal state but does NOT fire the
    /// SF "change" event. We must explicitly trigger the event via the ej2 instance's
    /// trigger() method — this is the SF event system, not DOM dispatchEvent.
    /// The event args must include "checked" and "isInteracted" to match the SF contract.
    /// </summary>
    private async Task SetSwitchChecked(string elementId, bool isChecked)
    {
        var jsChecked = isChecked ? "true" : "false";
        await Page.EvaluateAsync(
            $@"() => {{
                const el = document.getElementById('{elementId}');
                const ej2 = el.ej2_instances[0];
                ej2.checked = {jsChecked};
                ej2.dataBind();
                ej2.trigger('change', {{ checked: {jsChecked}, isInteracted: true }});
            }}");
    }

    // ── Page loads ──

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionSwitch — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_is_rendered()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("mutate-element"),
            "Plan must contain mutate-element commands");
        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""),
            "Plan must contain fusion vendor");
        AssertNoConsoleErrors();
    }

    // ── Section 1: Property Write ──

    [Test]
    public async Task domready_unchecks_notifications_switch()
    {
        // ReceiveNotifications starts checked in model, but DomReady calls SetChecked(false).
        // Expected: switch is unchecked after boot.
        await NavigateAndBoot();

        var isChecked = await Page.EvaluateAsync<bool>(
            $"() => {{ const el = document.getElementById('{ReceiveNotificationsId}'); return el.ej2_instances[0].checked; }}");
        Assert.That(isChecked, Is.False, "DomReady SetChecked(false) should uncheck the switch");
        AssertNoConsoleErrors();
    }

    // ── Section 2: Property Read ──

    [Test]
    public async Task domready_reads_value_into_echo()
    {
        await NavigateAndBoot();
        var echo = Page.Locator("#value-echo");
        await Expect(echo).Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Section 3: Events — Changed with typed condition ──

    [Test]
    public async Task changed_event_displays_checked_state()
    {
        await NavigateAndBoot();

        // Toggle the switch on via ej2 instance
        await SetSwitchChecked(ReceiveNotificationsId, true);

        // SF change event payload contains the new checked state
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_matches_when_checked()
    {
        await NavigateAndBoot();

        // Toggle the switch on via ej2 instance
        await SetSwitchChecked(ReceiveNotificationsId, true);

        // When(args, x => x.Checked).Truthy() => Then branch
        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("notifications enabled", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_matches_when_unchecked()
    {
        await NavigateAndBoot();

        // DomReady already set it to false, toggle on then off to trigger event
        await SetSwitchChecked(ReceiveNotificationsId, true);
        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("notifications enabled", new() { Timeout = 5000 });

        await SetSwitchChecked(ReceiveNotificationsId, false);

        // When(args, x => x.Checked).Truthy() => Else branch
        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("notifications disabled", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_condition_shows_indicator_when_checked()
    {
        await NavigateAndBoot();

        // Toggle the switch on via ej2 instance
        await SetSwitchChecked(ReceiveNotificationsId, true);

        // Indicator should appear with text "notifications active"
        await Expect(Page.Locator("#selected-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator"))
            .ToHaveTextAsync("notifications active", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Section 4: Component-Read Condition ──

    [Test]
    public async Task component_value_condition_warns_when_unchecked()
    {
        await NavigateAndBoot();

        // SmsAlerts starts false — click check
        await Page.Locator("#check-sms-btn").ClickAsync();

        var warning = Page.Locator("#sms-warning");
        await Expect(warning).ToHaveTextAsync("SMS alerts are disabled", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_confirms_when_checked()
    {
        await NavigateAndBoot();

        // Toggle SmsAlerts on
        await SetSwitchChecked(SmsAlertsId, true);

        // Click check button
        await Page.Locator("#check-sms-btn").ClickAsync();

        var warning = Page.Locator("#sms-warning");
        await Expect(warning).ToHaveTextAsync("SMS alerts are enabled", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── Deep BDD: state-cycle scenarios ──

    [Test]
    public async Task toggling_switch_multiple_times_fires_condition_each_time()
    {
        await NavigateAndBoot();

        var argsCondition = Page.Locator("#args-condition");
        var selectedIndicator = Page.Locator("#selected-indicator");

        // Cycle 1: toggle on — condition evaluates "notifications enabled", indicator shows
        await SetSwitchChecked(ReceiveNotificationsId, true);
        await Expect(argsCondition).ToHaveTextAsync("notifications enabled", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(selectedIndicator).ToHaveTextAsync("notifications active", new() { Timeout = 3000 });

        // Cycle 2: toggle off — condition evaluates "notifications disabled", indicator hides
        await SetSwitchChecked(ReceiveNotificationsId, false);
        await Expect(argsCondition).ToHaveTextAsync("notifications disabled", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeHiddenAsync(new() { Timeout = 3000 });

        // Cycle 3: toggle on again — proves state is not stuck
        await SetSwitchChecked(ReceiveNotificationsId, true);
        await Expect(argsCondition).ToHaveTextAsync("notifications enabled", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task toggling_sms_switch_updates_condition_result_each_time()
    {
        await NavigateAndBoot();

        var btn = Page.Locator("#check-sms-btn");
        var warning = Page.Locator("#sms-warning");

        // SMS starts off -> button should say "SMS alerts are disabled"
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("SMS alerts are disabled", new() { Timeout = 3000 });

        // Toggle on -> button should now say "SMS alerts are enabled"
        await SetSwitchChecked(SmsAlertsId, true);
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("SMS alerts are enabled", new() { Timeout = 3000 });

        // Toggle off again -> button should revert to "SMS alerts are disabled"
        await SetSwitchChecked(SmsAlertsId, false);
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("SMS alerts are disabled", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Plan JSON structure — refactoring safety ──

    [Test]
    public async Task plan_carries_fusion_vendor_for_switch_mutations()
    {
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""),
            "Plan must carry vendor 'fusion' for switch mutations — " +
            "runtime uses this to choose resolveRoot strategy (ej2_instances)");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_carries_checked_readexpr_for_component_source()
    {
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"readExpr\": \"checked\""),
            "Plan must carry readExpr 'checked' for FusionSwitch component sources — " +
            "runtime walks this path to read the switch state");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_carries_boolean_coerce_for_setchecked()
    {
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"coerce\": \"boolean\""),
            "Plan must carry coerce 'boolean' for SetChecked — " +
            "without it, string 'false' is truthy and switch stays checked");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_carries_prop_checked_for_setchecked_mutation()
    {
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"prop\": \"checked\""),
            "Plan must carry prop 'checked' for SetChecked mutation — " +
            "runtime uses bracket notation root[prop] = val");
        AssertNoConsoleErrors();
    }

    // ── Boot trace ──

    [Test]
    public async Task boot_trace_is_emitted_on_page_load()
    {
        await NavigateAndBoot();

        var hasBootTrace = _consoleMessages.Any(m => m.Contains("booted"));
        Assert.That(hasBootTrace, Is.True,
            "Boot trace must be emitted — confirms auto-boot discovered and executed the plan");
        AssertNoConsoleErrors();
    }
}
