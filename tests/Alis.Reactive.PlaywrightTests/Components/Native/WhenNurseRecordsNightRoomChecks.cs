using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.HiddenField;

namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// As a night nurse, I want each room check I record to carry the wing, the start of the round and the
/// resident in the room, without my typing them, and to see the night care of the resident I am about to
/// check, so that the morning report shows who was checked on which round and nobody's care is missed.
/// </summary>
/// <remarks>
/// The page keeps three hidden fields: the wing the server writes when the page opens, the start time the
/// server answers when the round starts, and the resident the server names for the room chosen (each a
/// <c>Value</c> write from a response). Starting the round again reads the start time back (a <c>Value</c>
/// read). Every check is sent with all three, and the server's answer is built from them. Writing the
/// resident raises the hidden field's <c>Change</c>: its reaction asks for the night care of the resident
/// its <c>Value</c> names, and, as the field's commit, it checks the field's rule again, so the summary's
/// "no resident" message clears as soon as the server names one. Not exercised: a write of the value the
/// field already holds, which raises nothing; no step on the page writes the same resident twice.
/// </remarks>
[TestFixture]
public class WhenNurseRecordsNightRoomChecks : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/NativeHiddenField";
    private const string NoResident = "The check names no resident yet: the room you choose names who sleeps there.";

    private PagePlan<NightRoomCheck> _plan = null!;

    private NativeDropDownLocator Room => _plan.NativeDropDown(m => m.Room);
    private NativeDropDownLocator Finding => _plan.NativeDropDown(m => m.Finding);
    private ILocator StartRound => Page.Locator("#start-round-btn");
    private ILocator RoundStatus => Page.Locator("#round-status");
    private ILocator RoundUnderway => Page.Locator("#round-underway");
    private ILocator RoomChecks => Page.Locator("#room-check-panel");
    private ILocator RoomResident => Page.Locator("#room-resident");
    private ILocator NightCare => Page.Locator("#resident-night-care");
    private ILocator ValidationSummary => Page.Locator("[data-reactive-validation-summary]");
    private ILocator RecordCheck => Page.Locator("#record-room-check-btn");
    private ILocator CheckStatus => Page.Locator("#room-check-status");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<NightRoomCheck>.FromPage(Page);
    }

    private async Task StartTheRound()
    {
        await ClickWhenStable(StartRound);
        await Expect(RoomChecks).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    private async Task ChooseRoom(string room, string resident)
    {
        await Room.Select(room);
        await Expect(RoomResident).ToHaveTextAsync(resident, new() { Timeout = 10000 });
    }

    [Test]
    public async Task starting_the_round_announces_it_for_the_wing_and_opens_the_room_checks()
    {
        await NavigateAndBoot();
        await Expect(RoomChecks).ToBeHiddenAsync();

        await ClickWhenStable(StartRound);

        await Expect(RoundStatus).ToHaveTextAsync("Maple wing round started at 02:00.", new() { Timeout = 10000 });
        await Expect(RoomChecks).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task starting_the_round_again_says_when_it_started()
    {
        await NavigateAndBoot();
        await StartTheRound();
        await Expect(RoundUnderway).ToBeHiddenAsync();

        await ClickWhenStable(StartRound);

        await Expect(RoundUnderway).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(RoundUnderway).ToHaveTextAsync("This round started at 02:00. Record each room before starting another.");
        await Expect(RoundStatus).ToHaveTextAsync("Maple wing round started at 02:00.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_a_room_names_the_resident_who_sleeps_there()
    {
        await NavigateAndBoot();
        await StartTheRound();

        await Room.Select("Maple 112");

        await Expect(RoomResident).ToHaveTextAsync("Lillian Ford", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    // The resident the server names for the room brings their night care; another room, another resident's.
    [Test]
    public async Task the_night_care_shown_is_that_of_the_resident_in_the_room_chosen()
    {
        await NavigateAndBoot();
        await StartTheRound();
        await Expect(NightCare).ToBeHiddenAsync();

        await ChooseRoom("Maple 112", "Lillian Ford");
        await Expect(NightCare).ToHaveTextAsync("Night care: Bed alarm on; walk her to the bathroom if she is up.", new() { Timeout = 10000 });

        await ChooseRoom("Maple 114", "Samuel Ortiz");
        await Expect(NightCare).ToHaveTextAsync("Night care: Turn him at 02:00 and 04:00.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    // The resident travels in a hidden field with no message slot, so its message is in the summary. The
    // server naming the resident clears it at once, before the check is recorded again.
    [Test]
    public async Task choosing_the_room_clears_the_missing_resident_from_the_summary_at_once()
    {
        await NavigateAndBoot();
        await StartTheRound();
        await ClickWhenStable(RecordCheck);
        await Expect(ValidationSummary).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(ValidationSummary).ToHaveTextAsync(NoResident);

        await ChooseRoom("Maple 112", "Lillian Ford");

        await Expect(ValidationSummary).ToBeHiddenAsync();
        await Expect(CheckStatus).ToHaveTextAsync("No rooms checked yet.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task recording_a_check_sends_the_wing_the_round_and_the_resident_with_it()
    {
        await NavigateAndBoot();
        await StartTheRound();
        await ChooseRoom("Maple 112", "Lillian Ford");
        await Finding.Select("Asleep");

        await ClickWhenStable(RecordCheck);

        await Expect(CheckStatus).ToHaveTextAsync("Maple wing, 02:00 round: Lillian Ford (resident 3107), asleep.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task moving_on_to_another_room_sends_the_resident_of_that_room()
    {
        await NavigateAndBoot();
        await StartTheRound();
        await ChooseRoom("Maple 112", "Lillian Ford");
        await ChooseRoom("Maple 114", "Samuel Ortiz");
        await Finding.Select("Up and needs help");

        await ClickWhenStable(RecordCheck);

        await Expect(CheckStatus).ToHaveTextAsync("Maple wing, 02:00 round: Samuel Ortiz (resident 3111), up and needs help.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_check_is_not_recorded_without_the_room_and_how_the_resident_was_found()
    {
        await NavigateAndBoot();
        await StartTheRound();

        await ClickWhenStable(RecordCheck);

        await Expect(_plan.ErrorFor(m => m.Room)).ToHaveTextAsync("Choose the room you checked.", new() { Timeout = 5000 });
        await Expect(_plan.ErrorFor(m => m.Finding)).ToHaveTextAsync("Say how you found the resident.");
        await Expect(CheckStatus).ToHaveTextAsync("No rooms checked yet.");
        AssertNoConsoleErrors();
    }
}
