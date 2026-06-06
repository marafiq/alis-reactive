using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Switch;

/// <summary>
/// Proves FusionSwitch property writes, value reads, changed events,
/// event-args conditions, and component-read conditions through page-visible behavior.
/// </summary>
[TestFixture]
public class WhenSwitchToggles : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Switch";

    // Generated component IDs are the DOM/Reactive Plan join keys under test.
    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_SwitchModel";
    private const string ReceiveNotificationsId = GeneratedTypeScope + "__ReceiveNotifications";
    private const string EmailAlertsId = GeneratedTypeScope + "__EmailAlerts";
    private const string SmsAlertsId = GeneratedTypeScope + "__SmsAlerts";

    private SwitchLocator ReceiveNotifications => new(Page, ReceiveNotificationsId);
    private SwitchLocator SmsAlerts => new(Page, SmsAlertsId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#value-echo");
    }

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
        Assert.That(planJson, Does.Contain("\"set\""),
            "Plan must contain set reactions");
        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""),
            "Plan must contain fusion vendor");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_unchecks_notifications_switch()
    {
        await NavigateAndBoot();

        await Expect(ReceiveNotifications.Input).Not.ToBeCheckedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_reads_value_into_echo()
    {
        await NavigateAndBoot();
        var valueEcho = Page.Locator("#value-echo");
        await Expect(valueEcho).Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_displays_checked_state()
    {
        await NavigateAndBoot();

        await ReceiveNotifications.Toggle();

        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_matches_when_checked()
    {
        await NavigateAndBoot();

        await ReceiveNotifications.Toggle();

        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("notifications enabled", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_matches_when_unchecked()
    {
        await NavigateAndBoot();

        await ReceiveNotifications.Toggle();
        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("notifications enabled", new() { Timeout = 5000 });

        await ReceiveNotifications.Toggle();

        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("notifications disabled", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_condition_shows_indicator_when_checked()
    {
        await NavigateAndBoot();

        await ReceiveNotifications.Toggle();

        await Expect(Page.Locator("#selected-indicator"))
            .ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#selected-indicator"))
            .ToHaveTextAsync("notifications active", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_warns_when_unchecked()
    {
        await NavigateAndBoot();

        await Page.Locator("#check-sms-btn").ClickAsync();

        var warning = Page.Locator("#sms-warning");
        await Expect(warning).ToHaveTextAsync("SMS alerts are disabled", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_confirms_when_checked()
    {
        await NavigateAndBoot();

        await SmsAlerts.Toggle();

        await Page.Locator("#check-sms-btn").ClickAsync();

        var warning = Page.Locator("#sms-warning");
        await Expect(warning).ToHaveTextAsync("SMS alerts are enabled", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task toggling_switch_multiple_times_fires_condition_each_time()
    {
        await NavigateAndBoot();

        var argsCondition = Page.Locator("#args-condition");
        var selectedIndicator = Page.Locator("#selected-indicator");

        await ReceiveNotifications.Toggle();
        await Expect(argsCondition).ToHaveTextAsync("notifications enabled", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(selectedIndicator).ToHaveTextAsync("notifications active", new() { Timeout = 3000 });

        await ReceiveNotifications.Toggle();
        await Expect(argsCondition).ToHaveTextAsync("notifications disabled", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeHiddenAsync(new() { Timeout = 3000 });

        await ReceiveNotifications.Toggle();
        await Expect(argsCondition).ToHaveTextAsync("notifications enabled", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task toggling_sms_switch_updates_condition_result_each_time()
    {
        await NavigateAndBoot();

        var checkSmsButton = Page.Locator("#check-sms-btn");
        var warning = Page.Locator("#sms-warning");

        await checkSmsButton.ClickAsync();
        await Expect(warning).ToHaveTextAsync("SMS alerts are disabled", new() { Timeout = 3000 });

        await SmsAlerts.Toggle();
        await checkSmsButton.ClickAsync();
        await Expect(warning).ToHaveTextAsync("SMS alerts are enabled", new() { Timeout = 3000 });

        await SmsAlerts.Toggle();
        await checkSmsButton.ClickAsync();
        await Expect(warning).ToHaveTextAsync("SMS alerts are disabled", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

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
    public async Task plan_carries_checked_value_member_for_component_source()
    {
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"member\": \"checked\""),
            "Plan must carry property member 'checked' for FusionSwitch component sources — " +
            "runtime reads this property to get the switch state");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_carries_boolean_coerce_for_setchecked()
    {
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"kind\": \"boolean\""),
            "Plan must carry shape boolean for SetChecked — " +
            "without it, string 'false' is truthy and switch stays checked");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_carries_prop_checked_for_setchecked_mutation()
    {
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"property\": \"checked\""),
            "Plan must carry property .checked. for SetChecked mutation — " +
            "runtime uses bracket notation root[prop] = val");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task boot_trace_is_emitted_on_page_load()
    {
        await NavigateAndBoot();

        var hasBootTrace = _consoleMessages.Any(m => m.Contains("booted"));
        Assert.That(hasBootTrace, Is.True,
            "Boot trace must be emitted — confirms runtime boot discovered and executed the plan");
        AssertNoConsoleErrors();
    }
}
