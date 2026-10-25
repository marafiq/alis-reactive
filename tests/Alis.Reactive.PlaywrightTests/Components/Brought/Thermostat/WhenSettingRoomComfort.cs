using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought;

namespace Alis.Reactive.PlaywrightTests.Components.Brought.Thermostat;

/// <summary>
/// As the overnight nurse, I want to set a resident's room comfort before rounds, within the wing's range
/// and with its night setting, so that the resident sleeps well and the thermostat stays as I left it.
/// </summary>
/// <remarks>
/// The thermostat is a widget the sandbox wrote itself (Scripts/thermostat.ts) and declares in its own
/// assembly (BroughtComponents/Thermostat). Each test drives one kind of member through the page:
/// properties read, written and written only; methods with no argument, one, several and an object;
/// functions, one returning an object; a nested object with members of its own; and events with and
/// without a payload.
/// </remarks>
[TestFixture]
public class WhenSettingRoomComfort : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Thermostat";

    private PagePlan<RoomComfortModel> _plan = null!;

    private ThermostatField Thermostat => new(Page.Locator($"#{_plan.FindComponent(m => m.Target)!.ElementId}"));

    private ILocator Target => Page.Locator("#target");
    private ILocator Previous => Page.Locator("#previous");
    private ILocator Distance => Page.Locator("#distance");
    private ILocator Minutes => Page.Locator("#minutes");
    private ILocator Comfortable => Page.Locator("#comfortable");
    private ILocator Away => Page.Locator("#away");
    private ILocator Capped => Page.Locator("#capped");
    private ILocator RangeMin => Page.Locator("#range-min");
    private ILocator RangeMax => Page.Locator("#range-max");
    private ILocator NextChange => Page.Locator("#next-change");
    private ILocator NightActive => Page.Locator("#night-active");
    private ILocator Mode => Page.Locator("#mode");
    private ILocator Reading => Page.Locator("#reading");
    private ILocator LockStatus => Page.Locator("#lock-status");
    private ILocator PlanSummary => Page.Locator("#plan-summary");
    private ILocator Confirmation => Page.Locator("#confirmation");

    private ILocator Warmer => Page.Locator("#warmer");
    private ILocator Cooler => Page.Locator("#cooler");
    private ILocator Hold => Page.Locator("#hold");
    private ILocator Unlock => Page.Locator("#unlock");
    private ILocator NightMode => Page.Locator("#night-mode");
    private ILocator Cool => Page.Locator("#cool");
    private ILocator Off => Page.Locator("#off");
    private ILocator WingPlan => Page.Locator("#wing-plan");
    private ILocator SummerPlan => Page.Locator("#summer-plan");
    private ILocator Save => Page.Locator("#save-comfort");

    private async Task OpenRoom()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<RoomComfortModel>.FromPage(Page);
        // The wing's policy has arrived when the range shows.
        await Expect(RangeMax).ToHaveTextAsync("72", new() { Timeout = 5000 });
    }

    [Test]
    public async Task the_room_opens_with_the_wings_range_and_night_setting()
    {
        await OpenRoom();

        await Expect(RangeMin).ToHaveTextAsync("64");
        await Expect(Thermostat.Range).ToHaveTextAsync("64–72°F");
        await Expect(NextChange).ToHaveTextAsync("66°F at 9:00 PM");
        await Expect(Thermostat.Night).ToHaveTextAsync("Night 66°F");
        await Expect(Away).ToBeVisibleAsync();
        await Expect(Distance).ToHaveTextAsync("2");
        await Expect(Minutes).ToHaveTextAsync("8");
        await Expect(Comfortable).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task warmer_raises_the_target_a_degree_and_says_how_long_it_takes()
    {
        await OpenRoom();

        await Warmer.ClickAsync();

        await Expect(Target).ToHaveTextAsync("71");
        await Expect(Previous).ToHaveTextAsync("70");
        await Expect(Thermostat.TargetText).ToHaveTextAsync("71°F");
        await Expect(Thermostat.Status).ToHaveTextAsync("Accepted 71°F");
        await Expect(Distance).ToHaveTextAsync("3");
        await Expect(Minutes).ToHaveTextAsync("12");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task cooler_brings_the_target_within_a_degree_and_the_room_is_comfortable()
    {
        await OpenRoom();

        await Cooler.ClickAsync();

        await Expect(Target).ToHaveTextAsync("69");
        await Expect(Comfortable).ToBeVisibleAsync();
        await Expect(Away).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_target_above_the_wings_limit_is_capped_at_the_limit()
    {
        await OpenRoom();
        await Warmer.ClickAsync();
        await Warmer.ClickAsync();
        await Expect(Target).ToHaveTextAsync("72");

        await Warmer.ClickAsync();

        await Expect(Capped).ToBeVisibleAsync();
        await Expect(Target).ToHaveTextAsync("72");
        await Expect(Thermostat.TargetText).ToHaveTextAsync("72°F");
        await Expect(Thermostat.Status).ToHaveTextAsync("Capped at the wing's limit");
        await Expect(Previous).ToHaveTextAsync("71");

        await Cooler.ClickAsync();
        await Expect(Target).ToHaveTextAsync("71");
        await Expect(Capped).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task holding_the_target_shows_how_long_it_is_held()
    {
        await OpenRoom();
        await _plan.NumericTextBox(m => m.HoldMinutes).FillAndBlur("45");

        await Hold.ClickAsync();

        await Expect(Thermostat.Status).ToHaveTextAsync("Held for 45 min");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_right_pin_unlocks_the_thermostats_own_buttons()
    {
        await OpenRoom();
        await Expect(Thermostat.Up).ToBeDisabledAsync();
        await _plan.TextBox(m => m.Pin).Fill("1234");

        await Unlock.ClickAsync();

        await Expect(LockStatus).ToHaveTextAsync("Thermostat unlocked for Margaret's room.");
        await Expect(Thermostat.Lock).ToHaveTextAsync("Unlocked");
        await Expect(Thermostat.Up).ToBeEnabledAsync();

        await Thermostat.Up.ClickAsync();
        await Expect(Target).ToHaveTextAsync("71");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_wrong_pin_leaves_the_thermostat_locked()
    {
        await OpenRoom();
        await _plan.TextBox(m => m.Pin).Fill("0000");

        await Unlock.ClickAsync();

        await Expect(LockStatus).ToHaveTextAsync("Still locked: check the PIN.");
        await Expect(Thermostat.Status).ToHaveTextAsync("Wrong PIN");
        await Expect(Thermostat.Lock).ToHaveTextAsync("Locked");
        await Expect(Thermostat.Up).ToBeDisabledAsync();
        await Expect(Target).ToHaveTextAsync("70");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task night_mode_applies_the_wings_night_setting()
    {
        await OpenRoom();

        await NightMode.ClickAsync();

        await Expect(Target).ToHaveTextAsync("66");
        await Expect(Previous).ToHaveTextAsync("70");
        await Expect(NightActive).ToBeVisibleAsync();
        await Expect(NextChange).ToHaveTextAsync("66°F at 9:00 PM");
        await Expect(Thermostat.TargetText).ToHaveTextAsync("66°F");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task switching_to_cool_shows_the_mode_and_the_rooms_reading()
    {
        await OpenRoom();

        await Cool.ClickAsync();

        await Expect(Mode).ToHaveTextAsync("cool");
        await Expect(Thermostat.ModeText).ToHaveTextAsync("cool");
        await Expect(Reading).ToHaveTextAsync("68°F");
        await Expect(Minutes).ToHaveTextAsync("6");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task switching_off_makes_a_room_near_its_target_not_comfortable()
    {
        await OpenRoom();
        await Cooler.ClickAsync();
        await Expect(Comfortable).ToBeVisibleAsync();

        await Off.ClickAsync();

        await Expect(Mode).ToHaveTextAsync("off");
        await Expect(Comfortable).ToBeHiddenAsync();
        await Expect(Away).ToBeVisibleAsync();
        await Expect(Minutes).ToHaveTextAsync("0");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_wings_night_plan_from_the_server_is_applied()
    {
        await OpenRoom();

        await WingPlan.ClickAsync();

        await Expect(Target).ToHaveTextAsync("66");
        await Expect(Previous).ToHaveTextAsync("70");
        await Expect(PlanSummary).ToHaveTextAsync("heat to 66°F, 2 night settings");
        await Expect(Thermostat.Status).ToHaveTextAsync("Plan applied");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_summer_plan_written_in_the_page_is_applied()
    {
        await OpenRoom();

        await SummerPlan.ClickAsync();

        await Expect(Target).ToHaveTextAsync("72");
        await Expect(Mode).ToHaveTextAsync("cool");
        await Expect(Thermostat.ModeText).ToHaveTextAsync("cool");
        await Expect(PlanSummary).ToHaveTextAsync("cool to 72°F, 1 night setting");
        await Expect(NextChange).ToHaveTextAsync("72°F at 9:00 PM");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_posts_the_target_and_confirms_it()
    {
        await OpenRoom();
        await Warmer.ClickAsync();
        await Expect(Target).ToHaveTextAsync("71");

        await Save.ClickAsync();

        await Expect(Confirmation).ToHaveTextAsync("Margaret's room set to 71°F.");
        AssertNoConsoleErrors();
    }

    private sealed class ThermostatField
    {
        private readonly ILocator _root;

        internal ThermostatField(ILocator root) => _root = root;

        internal ILocator TargetText => _root.Locator(".th-target");
        internal ILocator ModeText => _root.Locator(".th-mode");
        internal ILocator Range => _root.Locator(".th-range");
        internal ILocator Night => _root.Locator(".th-night");
        internal ILocator Lock => _root.Locator(".th-lock");
        internal ILocator Status => _root.Locator(".th-status");
        internal ILocator Up => _root.GetByRole(AriaRole.Button, new() { Name = "Warmer", Exact = true });
    }
}
