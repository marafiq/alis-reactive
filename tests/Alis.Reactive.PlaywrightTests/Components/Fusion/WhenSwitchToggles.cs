using Alis.Reactive.PlaywrightTests.Support.Controls;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

[TestFixture]
public class WhenSwitchToggles : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Switch";

    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_SwitchModel";
    private const string ReceiveNotificationsId = Scope + "__ReceiveNotifications";
    private const string EmailAlertsId = Scope + "__EmailAlerts";
    private const string SmsAlertsId = Scope + "__SmsAlerts";

    private SwitchLocator ReceiveNotifications => new(Page, ReceiveNotificationsId);
    private SwitchLocator SmsAlerts => new(Page, SmsAlertsId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#value-echo");
    }

    // ── Page loads ──

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionSwitch — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    // ── Section 1: Property Write ──

    [Test]
    public async Task domready_unchecks_notifications_switch()
    {
        // ReceiveNotifications starts checked in model, but DomReady calls SetChecked(false).
        // Expected: switch is unchecked after boot.
        await NavigateAndBoot();

        await Expect(ReceiveNotifications.Input).Not.ToBeCheckedAsync();
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

        // Toggle the switch on by clicking the wrapper
        await ReceiveNotifications.Toggle();

        // SF change event payload contains the new checked state
        await Expect(Page.Locator("#change-value"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task event_args_condition_matches_when_checked()
    {
        await NavigateAndBoot();

        // Toggle the switch on by clicking the wrapper
        await ReceiveNotifications.Toggle();

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
        await ReceiveNotifications.Toggle();
        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("notifications enabled", new() { Timeout = 5000 });

        await ReceiveNotifications.Toggle();

        // When(args, x => x.Checked).Truthy() => Else branch
        await Expect(Page.Locator("#args-condition"))
            .ToHaveTextAsync("notifications disabled", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_condition_shows_indicator_when_checked()
    {
        await NavigateAndBoot();

        // Toggle the switch on by clicking the wrapper
        await ReceiveNotifications.Toggle();

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

        // Toggle SmsAlerts on by clicking the wrapper
        await SmsAlerts.Toggle();

        // Click check button
        await Page.Locator("#check-sms-btn").ClickAsync();

        var warning = Page.Locator("#sms-warning");
        await Expect(warning).ToHaveTextAsync("SMS alerts are enabled", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // ── State-cycle scenarios ──

    [Test]
    public async Task toggling_switch_multiple_times_fires_condition_each_time()
    {
        await NavigateAndBoot();

        var argsCondition = Page.Locator("#args-condition");
        var selectedIndicator = Page.Locator("#selected-indicator");

        // Cycle 1: toggle on — condition evaluates "notifications enabled", indicator shows
        await ReceiveNotifications.Toggle();
        await Expect(argsCondition).ToHaveTextAsync("notifications enabled", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(selectedIndicator).ToHaveTextAsync("notifications active", new() { Timeout = 3000 });

        // Cycle 2: toggle off — condition evaluates "notifications disabled", indicator hides
        await ReceiveNotifications.Toggle();
        await Expect(argsCondition).ToHaveTextAsync("notifications disabled", new() { Timeout = 5000 });
        await Expect(selectedIndicator).ToBeHiddenAsync(new() { Timeout = 3000 });

        // Cycle 3: toggle on again — proves state is not stuck
        await ReceiveNotifications.Toggle();
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
        await SmsAlerts.Toggle();
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("SMS alerts are enabled", new() { Timeout = 3000 });

        // Toggle off again -> button should revert to "SMS alerts are disabled"
        await SmsAlerts.Toggle();
        await btn.ClickAsync();
        await Expect(warning).ToHaveTextAsync("SMS alerts are disabled", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
