using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Slider;

// Journey: a resident sets their Comfort & Care Preferences. The page carries over
// the room temperature and afternoon rest window they saved last month. The resident
// adjusts the temperature with the slider, can apply the care team's recommendation,
// sets the rest window, then saves.
[TestFixture]
public class WhenUsingFusionSlider : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Slider";
    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_SliderModel";
    private const string RoomTemperatureId = GeneratedTypeScope + "__RoomTemperature";
    private const string QuietHoursId = GeneratedTypeScope + "__QuietHours";

    private FusionSliderLocator RoomTemperature => new(Page, RoomTemperatureId);
    private FusionSliderLocator QuietHours => new(Page, QuietHoursId);

    private ILocator ComfortReading => Page.Locator("#comfort-reading");
    private ILocator ComfortZone => Page.Locator("#comfort-zone");
    private ILocator SettleFrom => Page.Locator("#settle-from");
    private ILocator SettleAction => Page.Locator("#settle-action");
    private ILocator ComfortSource => Page.Locator("#comfort-source");
    private ILocator TempGuidance => Page.Locator("#temp-guidance");
    private ILocator RestSummary => Page.Locator("#rest-summary");
    private ILocator SaveConfirmation => Page.Locator("#save-confirmation");

    private ILocator ApplyRecommendedTemp => Page.Locator("#apply-recommended-temp");
    private ILocator CheckTempGuidance => Page.Locator("#check-temp-guidance");
    private ILocator ApplyRecommendedRest => Page.Locator("#apply-recommended-rest");
    private ILocator SavePreferences => Page.Locator("#save-preferences");

    private async Task OpenPreferences()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(RoomTemperature.Handle()).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    // RENDERS — the FusionSlider builder renders the slider bound to the carried-over model value.
    [Test]
    public async Task preferences_open_showing_the_temperature_carried_over_from_last_month()
    {
        await OpenPreferences();

        await Expect(RoomTemperature.Handle()).ToHaveAttributeAsync("aria-valuenow", "68");
        await Expect(ComfortReading).ToHaveTextAsync("68");

        AssertNoConsoleErrors();
    }

    // INTERACTS / Change — nudging the handle fires the Change event through the .Reactive wiring;
    // the FusionSliderChangeArgs payload carries Text into the live reading and Value into the
    // comfort-zone branch as the resident moves the slider.
    [Test]
    public async Task warming_the_room_updates_the_live_reading_and_the_comfort_note()
    {
        await OpenPreferences();

        await RoomTemperature.NudgeUp();

        await Expect(RoomTemperature.Handle()).ToHaveAttributeAsync("aria-valuenow", "70");
        await Expect(ComfortReading).ToHaveTextAsync("70");
        await Expect(ComfortZone)
            .ToHaveTextAsync("That's a comfortable mid-range temperature for most residents.");

        AssertNoConsoleErrors();
    }

    // Changed — when the handle settles, the Changed event's PreviousValue records the value before
    // the change and Action names how it changed.
    [Test]
    public async Task adjusting_the_temperature_records_what_it_changed_from()
    {
        await OpenPreferences();

        await RoomTemperature.NudgeUp();

        await Expect(SettleFrom).ToHaveTextAsync("68");
        await Expect(SettleAction).ToHaveTextAsync("changed");

        AssertNoConsoleErrors();
    }

    // IsInteracted — a temperature the resident chose reads as their own; a temperature applied for
    // them reads as a recommendation. FusionSliderChangeArgs.IsInteracted distinguishes the two.
    [Test]
    public async Task choosing_a_temperature_reads_differently_from_applying_a_recommendation()
    {
        await OpenPreferences();

        await RoomTemperature.NudgeUp();
        await Expect(ComfortSource).ToHaveTextAsync("You set this temperature yourself.");

        await ApplyRecommendedTemp.ClickAsync();

        await Expect(RoomTemperature.Handle()).ToHaveAttributeAsync("aria-valuenow", "72");
        await Expect(ComfortSource)
            .ToHaveTextAsync("We applied the temperature recommended by your care team.");

        AssertNoConsoleErrors();
    }

    // SetValue — applying the recommendation writes 72 onto the slider and repaints the handle there.
    [Test]
    public async Task applying_the_recommended_temperature_moves_the_slider_to_72()
    {
        await OpenPreferences();

        await ApplyRecommendedTemp.ClickAsync();

        await Expect(RoomTemperature.Handle()).ToHaveAttributeAsync("aria-valuenow", "72");

        AssertNoConsoleErrors();
    }

    // Value() — the guidance button reads the slider's current value through a condition; a warm
    // setting routes the overnight-check message.
    [Test]
    public async Task checking_a_warm_temperature_warns_about_an_overnight_check()
    {
        await OpenPreferences();

        await RoomTemperature.NudgeUp();
        await RoomTemperature.NudgeUp();
        await RoomTemperature.NudgeUp();
        await Expect(RoomTemperature.Handle()).ToHaveAttributeAsync("aria-valuenow", "74");

        await CheckTempGuidance.ClickAsync();

        await Expect(TempGuidance)
            .ToHaveTextAsync("A care manager will check in to make sure that's not too warm overnight.");

        AssertNoConsoleErrors();
    }

    // SetRangeValue + RangeValue — applying the recommended window writes both handles, and the
    // RangeValue source reads the written window back into the saved summary.
    [Test]
    public async Task applying_the_recommended_rest_window_moves_both_handles_and_updates_the_summary()
    {
        await OpenPreferences();

        await ApplyRecommendedRest.ClickAsync();

        await Expect(QuietHours.Handle(0)).ToHaveAttributeAsync("aria-valuenow", "14");
        await Expect(QuietHours.Handle(1)).ToHaveAttributeAsync("aria-valuenow", "16");
        await Expect(RestSummary).ToHaveTextAsync("14,16");

        AssertNoConsoleErrors();
    }

    // SUBMITS — the Value() and RangeValue() sources feed the gather body; the server confirmation
    // the resident sees reflects the submitted temperature and rest window.
    [Test]
    public async Task saving_confirms_the_temperature_and_rest_window()
    {
        await OpenPreferences();

        await SavePreferences.ClickAsync();

        await Expect(SaveConfirmation)
            .ToHaveTextAsync("Saved. We'll keep your room at 68°F and hold non-urgent visits from 13:00 to 15:00.",
                new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    // GATHERS — the framework gather pipeline carries the Value() and RangeValue() sources into the
    // POST body under their declared keys. (Framework gather test: asserts request.PostData.)
    [Test]
    public async Task saving_posts_the_temperature_and_rest_window_to_the_server()
    {
        await OpenPreferences();

        var requestTask = Page.WaitForRequestAsync(request =>
            request.Url.Contains("/Sandbox/Components/Slider/Save") && request.Method == "POST",
            new() { Timeout = 10000 });

        await SavePreferences.ClickAsync();

        var request = await requestTask;
        var body = request.PostData ?? "";

        Assert.That(body, Does.Contain("\"roomTemperature\":68"),
            "the gather pipeline must carry the Value() source under its declared key");
        Assert.That(body, Does.Contain("\"quietHours\":[13,15]"),
            "the gather pipeline must carry the RangeValue() source under its declared key");

        AssertNoConsoleErrors();
    }
}
