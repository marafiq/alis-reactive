using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Switch;

// Journey: a resident manages their Care Alert Preferences. A master "receive care alerts"
// switch decides whether any alerts go out; two channel switches (email, text message) decide
// how. The resident toggles the master, pauses everything with a button, reviews each channel,
// and saves — the server confirms which channels will be used.
[TestFixture]
public class WhenSwitchToggles : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Switch";

    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_SwitchModel";
    private const string ReceiveCareAlertsId = GeneratedTypeScope + "__ReceiveCareAlerts";
    private const string EmailRemindersId = GeneratedTypeScope + "__EmailReminders";
    private const string TextMessageAlertsId = GeneratedTypeScope + "__TextMessageAlerts";

    private SwitchLocator ReceiveCareAlerts => new(Page, ReceiveCareAlertsId);
    private SwitchLocator EmailReminders => new(Page, EmailRemindersId);
    private SwitchLocator TextMessageAlerts => new(Page, TextMessageAlertsId);

    private ILocator AlertsStatus => Page.Locator("#alerts-status");
    private ILocator EmailReview => Page.Locator("#email-review");
    private ILocator TextReview => Page.Locator("#text-review");
    private ILocator SaveConfirmation => Page.Locator("#save-confirmation");
    private ILocator PauseButton => Page.Locator("#pause-alerts");
    private ILocator ReviewEmailButton => Page.Locator("#review-email");
    private ILocator ReviewTextButton => Page.Locator("#review-text");
    private ILocator SaveButton => Page.Locator("#save-preferences");

    private async Task OpenPreferences()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(ReceiveCareAlerts.Wrapper).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    // RENDERS — the FusionSwitch builder renders each toggle bound to the resident's saved
    // preference: care alerts on, email reminders on, text-message alerts off.
    [Test]
    public async Task preferences_open_showing_the_resident_saved_toggles()
    {
        await OpenPreferences();

        await Expect(ReceiveCareAlerts.Input).ToBeCheckedAsync();
        await Expect(EmailReminders.Input).ToBeCheckedAsync();
        await Expect(TextMessageAlerts.Input).Not.ToBeCheckedAsync();

        AssertNoConsoleErrors();
    }

    // INTERACTS + Checked — toggling the master switch off fires Changed through the .Reactive
    // wiring; the FusionSwitchChangeArgs.Checked payload (false) routes the "paused" message.
    [Test]
    public async Task turning_care_alerts_off_tells_the_resident_alerts_are_paused()
    {
        await OpenPreferences();

        await ReceiveCareAlerts.Toggle();

        await Expect(AlertsStatus)
            .ToHaveTextAsync("Care alerts are paused. We will not send reminders until you turn them back on.");

        AssertNoConsoleErrors();
    }

    // Checked carrying true — toggling the master back on routes the other branch over
    // FusionSwitchChangeArgs.Checked, proving the payload carries the state in both directions.
    [Test]
    public async Task turning_care_alerts_back_on_tells_the_resident_to_pick_channels()
    {
        await OpenPreferences();

        await ReceiveCareAlerts.Toggle();
        await Expect(AlertsStatus)
            .ToHaveTextAsync("Care alerts are paused. We will not send reminders until you turn them back on.");

        await ReceiveCareAlerts.Toggle();

        await Expect(AlertsStatus)
            .ToHaveTextAsync("Care alerts are on. Choose how we should reach you below.");

        AssertNoConsoleErrors();
    }

    // SetChecked — the "Pause all alerts" button writes checked=false onto the master switch;
    // the switch visibly turns off without the resident touching it.
    [Test]
    public async Task pausing_all_alerts_turns_the_master_switch_off()
    {
        await OpenPreferences();

        await Expect(ReceiveCareAlerts.Input).ToBeCheckedAsync();

        await PauseButton.ClickAsync();

        await Expect(ReceiveCareAlerts.Input).Not.ToBeCheckedAsync();
        await Expect(AlertsStatus)
            .ToHaveTextAsync("Care alerts are paused. We will not send reminders until you turn them back on.");

        AssertNoConsoleErrors();
    }

    // Value() as a component-read condition — the review button reads the email switch's live
    // checked state. Email reminders start on, so the resident sees "on".
    [Test]
    public async Task reviewing_email_reminders_reports_them_on_when_the_switch_is_on()
    {
        await OpenPreferences();

        await ReviewEmailButton.ClickAsync();

        await Expect(EmailReview).ToHaveTextAsync("Email reminders are on.");

        AssertNoConsoleErrors();
    }

    // Value() reads the toggled-off state — after the resident turns email reminders off,
    // the review button reads checked=false and reports "off", proving Value() reads live state.
    [Test]
    public async Task reviewing_email_reminders_reports_them_off_after_the_resident_turns_them_off()
    {
        await OpenPreferences();

        await EmailReminders.Toggle();
        await ReviewEmailButton.ClickAsync();

        await Expect(EmailReview).ToHaveTextAsync("Email reminders are off.");

        AssertNoConsoleErrors();
    }

    // Value() reads a second switch independently — the text-message switch starts off, so its
    // review reports "off"; after the resident turns it on, the review reports "on".
    [Test]
    public async Task reviewing_text_alerts_follows_the_text_switch_the_resident_sets()
    {
        await OpenPreferences();

        await ReviewTextButton.ClickAsync();
        await Expect(TextReview).ToHaveTextAsync("Text-message alerts are off.");

        await TextMessageAlerts.Toggle();
        await ReviewTextButton.ClickAsync();

        await Expect(TextReview).ToHaveTextAsync("Text-message alerts are on.");

        AssertNoConsoleErrors();
    }

    // SUBMITS — the three Value() sources feed the gather body; the server confirmation the
    // resident sees names the channels that were saved.
    [Test]
    public async Task saving_confirms_the_channels_the_resident_chose()
    {
        await OpenPreferences();

        await TextMessageAlerts.Toggle();
        await SaveButton.ClickAsync();

        await Expect(SaveConfirmation)
            .ToHaveTextAsync("Saved. We will send your care alerts by email and text message.",
                new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    // GATHERS — the framework gather pipeline carries all three switch Value() sources into the
    // POST body under their declared keys. (Framework gather test: asserts request.PostData.)
    [Test]
    public async Task saving_posts_each_switch_value_to_the_server()
    {
        await OpenPreferences();

        await EmailReminders.Toggle();

        var requestTask = Page.WaitForRequestAsync(request =>
            request.Url.Contains("/Sandbox/Components/Switch/Save") && request.Method == "POST",
            new() { Timeout = 10000 });

        await SaveButton.ClickAsync();

        var request = await requestTask;
        var body = request.PostData ?? "";

        Assert.That(body, Does.Contain("\"receiveCareAlerts\":true"),
            "the gather pipeline must carry the master switch Value() under its declared key");
        Assert.That(body, Does.Contain("\"emailReminders\":false"),
            "the gather pipeline must carry the email switch Value() (toggled off) under its declared key");
        Assert.That(body, Does.Contain("\"textMessageAlerts\":false"),
            "the gather pipeline must carry the text switch Value() under its declared key");

        AssertNoConsoleErrors();
    }
}
