using System.Text.RegularExpressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.CoreBehaviors.RoomWhiteboard;

namespace Alis.Reactive.PlaywrightTests.CoreBehaviors.RoomWhiteboard;

/// <summary>
/// As the evening nurse, I want room 12's whiteboard to show what I put on it, the way I put it, so that
/// whoever walks in reads the board I meant to leave.
/// </summary>
/// <remarks>
/// The whiteboard is a widget the sandbox wrote itself (Scripts/whiteboard.ts, BroughtComponents/Whiteboard).
/// Each test is one kind of reaction: a property set from a literal, from an input, and to nothing; a
/// method called with a literal, with an input, with a field and a fixed argument, with a result drawn once
/// and used twice, and with a promise waited for; an element's text, HTML, visibility and classes; a payload
/// composed from a widget, an input, a nullable property and a literal; and a chain run line by line.
/// </remarks>
[TestFixture]
public class WhenTheNurseKeepsTheWhiteboard : PlaywrightTestBase
{
    private const string Path = "/Sandbox/CoreBehaviors/RoomWhiteboard";

    private WhiteboardField Board => new(Page.Locator("#room-board"));

    private ILocator Goal => Page.Locator($"#{IdGenerator.For<RoomWhiteboardModel>(m => m.Goal)}");
    private ILocator PageWho => Page.Locator($"#{IdGenerator.For<RoomWhiteboardModel>(m => m.PageWho)}");
    private ILocator Note => Page.Locator($"#{IdGenerator.For<RoomWhiteboardModel>(m => m.Note)}");

    private ILocator OnDuty => Page.Locator("#on-duty");
    private ILocator LastNumber => Page.Locator("#last-number");
    private ILocator NoteCount => Page.Locator("#note-count");
    private ILocator HeardNote => Page.Locator("#heard-note");
    private ILocator HeardNumber => Page.Locator("#heard-number");
    private ILocator HeardText => Page.Locator("#heard-text");
    private ILocator WipedLine => Page.Locator("#wiped-line");
    private ILocator RefreshLine => Page.Locator("#refresh-line");
    private ILocator Motto => Page.Locator("#motto");
    private ILocator Banner => Page.Locator("#banner");
    private ILocator RoomCard => Page.Locator("#room-card");
    private ILocator Posted => Page.Locator("#posted");
    private ILocator PostedNurse => Page.Locator("#posted-nurse");
    private ILocator PostedRoom => Page.Locator("#posted-room");
    private ILocator PostedMinutes => Page.Locator("#posted-minutes");
    private ILocator PostedGoal => Page.Locator("#posted-goal");
    private ILocator HandoverLine => Page.Locator("#handover-line");
    private ILocator HandoverFrom => Page.Locator("#handover-from");
    private ILocator HandoverTo => Page.Locator("#handover-to");

    private ILocator TakeShift => Page.Locator("#take-shift");
    private ILocator PostGoal => Page.Locator("#post-goal");
    private ILocator ScheduleCheck => Page.Locator("#schedule-check");
    private ILocator ClearCheck => Page.Locator("#clear-check");
    private ILocator PageCharge => Page.Locator("#page-charge");
    private ILocator PageSomeone => Page.Locator("#page-who");
    private ILocator HighlightGoal => Page.Locator("#highlight-goal");
    private ILocator AddNote => Page.Locator("#add-note");
    private ILocator RefreshBoard => Page.Locator("#refresh-board");
    private ILocator QuietHours => Page.Locator("#quiet-hours");
    private ILocator EndQuietHours => Page.Locator("#end-quiet-hours");
    private ILocator FlagRoom => Page.Locator("#flag-room");
    private ILocator UnflagRoom => Page.Locator("#unflag-room");
    private ILocator NightLight => Page.Locator("#night-light");
    private ILocator PostBoard => Page.Locator("#post-board");
    private ILocator HandOver => Page.Locator("#hand-over");
    private ILocator WipeBoard => Page.Locator("#wipe-board");

    private static readonly Regex Flagged = new(@"\broom-flagged\b");
    private static readonly Regex NightLit = new(@"\bnight-light\b");
    private static readonly Regex Pulsing = new(@"\bwb-effect-pulse\b");

    private async Task OpenTheRoom()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Board.Row("nurse")).ToHaveTextAsync("Nurse Okafor");
    }

    [Test]
    public async Task taking_the_shift_puts_nurse_patel_on_the_board_and_on_duty()
    {
        await OpenTheRoom();
        await Expect(OnDuty).ToHaveTextAsync("Nurse Okafor");

        await TakeShift.ClickAsync();

        await Expect(Board.Row("nurse")).ToHaveTextAsync("Nurse Patel");
        await Expect(OnDuty).ToHaveTextAsync("Nurse Patel");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task posting_the_goal_writes_what_was_typed_on_the_board()
    {
        await OpenTheRoom();
        await Expect(Board.Row("goal")).ToHaveTextAsync("");
        await Goal.FillAsync("Walk to the dining room");

        await PostGoal.ClickAsync();

        await Expect(Board.Row("goal")).ToHaveTextAsync("Walk to the dining room");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task scheduling_a_check_puts_it_on_the_board_and_clearing_it_takes_it_off()
    {
        await OpenTheRoom();
        await Expect(Board.Row("nextCheck")).ToHaveTextAsync("No check scheduled");

        await ScheduleCheck.ClickAsync();
        await Expect(Board.Row("nextCheck")).ToHaveTextAsync("In 30 min");

        await ClearCheck.ClickAsync();
        await Expect(Board.Row("nextCheck")).ToHaveTextAsync("No check scheduled");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task paging_names_who_was_paged_from_the_page_or_from_the_box()
    {
        await OpenTheRoom();
        await Expect(Board.Status).ToHaveTextAsync("");

        await PageCharge.ClickAsync();
        await Expect(Board.Status).ToHaveTextAsync("Paged the charge nurse");

        await PageWho.FillAsync("Aide Reyes");
        await PageSomeone.ClickAsync();
        await Expect(Board.Status).ToHaveTextAsync("Paged Aide Reyes");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task highlighting_the_goal_makes_that_row_alone_pulse()
    {
        await OpenTheRoom();
        await Expect(Board.RowItem("goal")).Not.ToHaveClassAsync(Pulsing);

        await HighlightGoal.ClickAsync();

        await Expect(Board.RowItem("goal")).ToHaveClassAsync(Pulsing);
        await Expect(Board.RowItem("nurse")).Not.ToHaveClassAsync(Pulsing);
        await Expect(Board.RowItem("nextCheck")).Not.ToHaveClassAsync(Pulsing);
        AssertNoConsoleErrors();
    }

    // The number is drawn once per note: the board, the line beside it and the desk all show the same
    // number; a second draw for the line would read 2 beside a board that says 1.
    [Test]
    public async Task adding_a_note_numbers_it_once_for_the_board_the_line_beside_it_and_the_desk()
    {
        await OpenTheRoom();
        await Note.FillAsync("Likes the window open");

        await AddNote.ClickAsync();

        await Expect(Board.Notes).ToHaveTextAsync(new[] { "1. Likes the window open" });
        await Expect(LastNumber).ToHaveTextAsync("1");
        await Expect(NoteCount).ToHaveTextAsync("1");
        await Expect(HeardNote).ToBeVisibleAsync();
        await Expect(HeardNumber).ToHaveTextAsync("1");
        await Expect(HeardText).ToHaveTextAsync("Likes the window open");

        await Note.FillAsync("Tea at nine");
        await AddNote.ClickAsync();
        await Expect(Board.Notes).ToHaveTextAsync(new[] { "1. Likes the window open", "2. Tea at nine" });
        await Expect(LastNumber).ToHaveTextAsync("2");
        await Expect(NoteCount).ToHaveTextAsync("2");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task refreshing_the_board_waits_for_the_refresh_and_for_the_motto()
    {
        await OpenTheRoom();
        await Expect(RefreshLine).ToHaveTextAsync("Not refreshed yet");
        await Expect(Motto).ToHaveTextAsync("");

        await RefreshBoard.ClickAsync();

        await Expect(RefreshLine).ToHaveTextAsync("Refreshed 1");
        await Expect(Board.Refresh).ToHaveTextAsync("Refreshed 1");
        await Expect(Motto).ToHaveTextAsync("Kindness first");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task quiet_hours_show_a_bold_banner_until_they_end()
    {
        await OpenTheRoom();
        await Expect(Banner).ToBeHiddenAsync();

        await QuietHours.ClickAsync();

        await Expect(Banner).ToBeVisibleAsync();
        await Expect(Banner).ToHaveTextAsync("Quiet hours from 9 PM");
        await Expect(Banner.Locator("strong")).ToHaveTextAsync("Quiet hours");

        await EndQuietHours.ClickAsync();
        await Expect(Banner).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task flagging_the_room_marks_the_card_until_the_flag_is_cleared()
    {
        await OpenTheRoom();
        await Expect(RoomCard).Not.ToHaveClassAsync(Flagged);

        await FlagRoom.ClickAsync();
        await Expect(RoomCard).ToHaveClassAsync(Flagged);

        await UnflagRoom.ClickAsync();
        await Expect(RoomCard).Not.ToHaveClassAsync(Flagged);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_night_light_goes_on_with_one_press_and_off_with_the_next()
    {
        await OpenTheRoom();
        await Expect(RoomCard).Not.ToHaveClassAsync(NightLit);

        await NightLight.ClickAsync();
        await Expect(RoomCard).ToHaveClassAsync(NightLit);

        await NightLight.ClickAsync();
        await Expect(RoomCard).Not.ToHaveClassAsync(NightLit);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task posting_the_board_sends_the_nurse_the_room_the_check_and_the_goal_together()
    {
        await OpenTheRoom();
        await TakeShift.ClickAsync();
        await Goal.FillAsync("Walk to the dining room");
        await ScheduleCheck.ClickAsync();
        await Expect(Board.Row("nextCheck")).ToHaveTextAsync("In 30 min");
        await Expect(Posted).ToBeHiddenAsync();

        await PostBoard.ClickAsync();

        await Expect(Posted).ToBeVisibleAsync();
        await Expect(PostedNurse).ToHaveTextAsync("Nurse Patel");
        await Expect(PostedRoom).ToHaveTextAsync("12");
        await Expect(PostedMinutes).ToHaveTextAsync("30");
        await Expect(PostedGoal).ToHaveTextAsync("Walk to the dining room");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_board_posted_with_no_check_scheduled_tells_the_desk_so()
    {
        await OpenTheRoom();
        await Goal.FillAsync("Rest after lunch");

        await PostBoard.ClickAsync();

        // Read as the nurse sees it: the hidden "next check in" part of the sentence is not shown.
        await Expect(Posted).ToHaveTextAsync(
            "Desk has the board: Nurse Okafor in room 12, no check scheduled, goal \"Rest after lunch\".",
            new() { UseInnerText = true });
        AssertNoConsoleErrors();
    }

    // Each line runs after the one before it: the board names Nurse Patel while she holds it, then
    // Nurse Okafor once it is set, so a hand-over records both. Lines run together would name one nurse twice.
    [Test]
    public async Task handing_over_records_who_held_the_board_and_who_takes_it()
    {
        await OpenTheRoom();
        await TakeShift.ClickAsync();
        await Expect(Board.Row("nurse")).ToHaveTextAsync("Nurse Patel");

        await HandOver.ClickAsync();

        await Expect(HandoverLine).ToBeVisibleAsync();
        await Expect(HandoverFrom).ToHaveTextAsync("Nurse Patel");
        await Expect(HandoverTo).ToHaveTextAsync("Nurse Okafor");
        await Expect(Board.Row("nurse")).ToHaveTextAsync("Nurse Okafor");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task wiping_the_notes_clears_the_board_and_says_so()
    {
        await OpenTheRoom();
        await Note.FillAsync("Likes the window open");
        await AddNote.ClickAsync();
        await Expect(Board.Notes).ToHaveCountAsync(1);
        await Expect(WipedLine).ToBeHiddenAsync();

        await WipeBoard.ClickAsync();

        await Expect(Board.Notes).ToHaveCountAsync(0);
        await Expect(WipedLine).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    private sealed class WhiteboardField
    {
        private readonly ILocator _root;

        internal WhiteboardField(ILocator root) => _root = root;

        internal ILocator Row(string row) => RowItem(row).Locator(".wb-row-value");

        internal ILocator RowItem(string row) => _root.Locator($"li.wb-row[data-row='{row}']");

        internal ILocator Notes => _root.Locator(".wb-note");

        internal ILocator Refresh => _root.Locator(".wb-refresh");

        internal ILocator Status => _root.Locator(".wb-status");
    }
}
