using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.CoreBehaviors.NightRound;

namespace Alis.Reactive.PlaywrightTests.CoreBehaviors.NightRound;

/// <summary>
/// As the night nurse, I want the round to open as the page does, each room I check to be logged, room
/// 12's bell and thermostat to be heard, and a note I file or a resident I flag to reach the desk, so that
/// nothing on the Garden wing waits for me to say it twice.
/// </summary>
/// <remarks>
/// Each test is one trigger: the page opening; a room chosen in a native dropdown; the bell pressed (a
/// brought widget's callback, with its payload); the thermostat's schedule applied (a nested object's own
/// event); a document event raised with a payload composed from two inputs, raised with a payload fixed
/// in the view, and raised without one, so that its members read as missing; and a flag raised by the
/// chart's own stamped button.
/// </remarks>
[TestFixture]
public class WhenTheNightNurseStartsTheRound : PlaywrightTestBase
{
    private const string Path = "/Sandbox/CoreBehaviors/NightRound";

    private ILocator RoundStatus => Page.Locator("#round-status");
    private ILocator DeskStatus => Page.Locator("#desk-status");
    private ILocator RoomLine => Page.Locator("#room-line");
    private ILocator RoomNow => Page.Locator("#room-now");
    private ILocator Checked => Page.Locator("#checked");
    private ILocator CheckedRoom => Page.Locator("#checked-room");
    private ILocator CheckedNote => Page.Locator("#checked-note");
    private ILocator NoteUnsigned => Page.Locator("#note-unsigned");
    private ILocator NoteSigned => Page.Locator("#note-signed");
    private ILocator NoteAuthor => Page.Locator("#note-author");
    private ILocator Flagged => Page.Locator("#flagged");
    private ILocator FlagRoom => Page.Locator("#flag-room");
    private ILocator FlagReason => Page.Locator("#flag-reason");
    private ILocator BellHeard => Page.Locator("#bell-heard");
    private ILocator BellRoom => Page.Locator("#bell-room");
    private ILocator BellCount => Page.Locator("#bell-count");
    private ILocator NightApplied => Page.Locator("#night-applied");
    private ILocator NightTarget => Page.Locator("#night-target");

    private ILocator Room => Page.Locator($"#{IdGenerator.For<NightRoundModel>(m => m.Room)}");
    private ILocator Note => Page.Locator($"#{IdGenerator.For<NightRoundModel>(m => m.Note)}");
    private ILocator Thermostat => Page.Locator($"#{IdGenerator.For<NightRoundModel>(m => m.Target)}");
    private ILocator BellPress => Page.Locator("#room-bell").GetByRole(AriaRole.Button, new() { Name = "Call the desk", Exact = true });

    private ILocator LogCheck => Page.Locator("#log-check");
    private ILocator FileUnsigned => Page.Locator("#file-unsigned");
    private ILocator FileSigned => Page.Locator("#file-signed");
    private ILocator FlagFallRisk => Page.Locator("#flag-fall-risk");
    private ILocator NightMode => Page.Locator("#night-mode");

    private async Task StartTheRound()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(RoundStatus).ToHaveTextAsync("Round open on the Garden wing");
    }

    [Test]
    public async Task the_round_opens_as_the_page_does_and_the_desk_is_told()
    {
        await StartTheRound();

        await Expect(DeskStatus).ToHaveTextAsync("Desk told: the round is open");
        await Expect(Thermostat.Locator(".th-night")).ToHaveTextAsync("Night 66°F");
        await Expect(RoomLine).ToBeHiddenAsync();
        await Expect(Checked).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_a_room_says_which_room_is_being_checked()
    {
        await StartTheRound();

        await Room.SelectOptionAsync("14");

        await Expect(RoomLine).ToBeVisibleAsync();
        await Expect(RoomNow).ToHaveTextAsync("14");

        await Room.SelectOptionAsync("15");
        await Expect(RoomNow).ToHaveTextAsync("15");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task logging_a_check_sends_the_room_and_the_note_to_the_desk_together()
    {
        await StartTheRound();
        await Room.SelectOptionAsync("12");
        await Note.FillAsync("Asleep, call light within reach");

        await LogCheck.ClickAsync();

        await Expect(Checked).ToBeVisibleAsync();
        await Expect(CheckedRoom).ToHaveTextAsync("12");
        await Expect(CheckedNote).ToHaveTextAsync("Asleep, call light within reach");

        await Room.SelectOptionAsync("15");
        await Note.FillAsync("Reading; declined the night snack");
        await LogCheck.ClickAsync();
        await Expect(CheckedRoom).ToHaveTextAsync("15");
        await Expect(CheckedNote).ToHaveTextAsync("Reading; declined the night snack");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_note_filed_unsigned_reaches_the_desk_without_an_author()
    {
        await StartTheRound();
        await Expect(NoteUnsigned).ToBeHiddenAsync();

        await FileUnsigned.ClickAsync();

        await Expect(NoteUnsigned).ToBeVisibleAsync();
        await Expect(NoteSigned).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_note_filed_by_nurse_patel_names_her_and_an_unsigned_one_after_it_does_not()
    {
        await StartTheRound();

        await FileSigned.ClickAsync();

        await Expect(NoteSigned).ToBeVisibleAsync();
        await Expect(NoteAuthor).ToHaveTextAsync("Nurse Patel");
        await Expect(NoteUnsigned).ToBeHiddenAsync();

        await FileUnsigned.ClickAsync();
        await Expect(NoteUnsigned).ToBeVisibleAsync();
        await Expect(NoteSigned).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task flagging_arthur_lee_from_the_chart_reaches_the_desk_with_the_reason()
    {
        await StartTheRound();
        await Expect(Flagged).ToBeHiddenAsync();

        await FlagFallRisk.ClickAsync();

        await Expect(Flagged).ToBeVisibleAsync();
        await Expect(FlagRoom).ToHaveTextAsync("14");
        await Expect(FlagReason).ToHaveTextAsync("Fall risk");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task room_12s_bell_is_heard_on_the_round_with_its_call_count()
    {
        await StartTheRound();
        await Expect(BellHeard).ToBeHiddenAsync();

        await BellPress.ClickAsync();
        await BellPress.ClickAsync();

        await Expect(BellHeard).ToBeVisibleAsync();
        await Expect(BellRoom).ToHaveTextAsync("12");
        await Expect(BellCount).ToHaveTextAsync("2");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task night_mode_applies_the_wings_night_setting_through_the_thermostats_own_schedule()
    {
        await StartTheRound();
        await Expect(NightApplied).ToBeHiddenAsync();
        await Expect(Thermostat.Locator(".th-target")).ToHaveTextAsync("70°F");

        await NightMode.ClickAsync();

        await Expect(NightApplied).ToBeVisibleAsync();
        await Expect(NightTarget).ToHaveTextAsync("66");
        await Expect(Thermostat.Locator(".th-target")).ToHaveTextAsync("66°F");
        AssertNoConsoleErrors();
    }
}
