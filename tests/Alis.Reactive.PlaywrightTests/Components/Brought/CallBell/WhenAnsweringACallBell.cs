namespace Alis.Reactive.PlaywrightTests.Components.Brought.CallBell;

/// <summary>
/// As the night nurse, I want a resident's call bell heard at the desk while someone is on duty and
/// answered from the page, so that no call goes unnoticed and the bell itself says when no one is listening.
/// </summary>
/// <remarks>
/// The call bell is a widget the sandbox wrote itself (Scripts/call-bell.ts) that takes each listener as a
/// property, the way a chart takes options.onClick (BroughtComponents/CallBell). The desk's card is an
/// injected partial whose plan listens to the page's bell with two handlers of one callback; ending
/// coverage unloads the card, and the bell must be left as it was before the desk came on duty.
/// </remarks>
[TestFixture]
public class WhenAnsweringACallBell : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/CallBell";

    private CallBellField Bell => new(Page.Locator("#room-bell"));

    private ILocator Answered => Page.Locator("#answered");
    private ILocator NoCall => Page.Locator("#no-call");
    private ILocator Room => Page.Locator("#room");
    private ILocator DeskSlot => Page.Locator("#desk-slot");
    private ILocator DeskNotified => Page.Locator("#desk-notified");
    private ILocator DeskCall => Page.Locator("#desk-call");
    private ILocator DeskRoom => Page.Locator("#desk-room");
    private ILocator Escalated => Page.Locator("#escalated");
    private ILocator DeskLog => Page.Locator("#desk-log");
    private ILocator DeskAnswered => Page.Locator("#desk-answered");

    private ILocator Answer => Page.Locator("#answer");
    private ILocator Move => Page.Locator("#move");
    private ILocator StartCoverage => Page.Locator("#desk-on");
    private ILocator EndCoverage => Page.Locator("#desk-off");

    private async Task OpenRoom()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Bell.State).ToHaveTextAsync("Quiet");
    }

    private async Task OpenRoomWithTheDeskOnDuty()
    {
        await OpenRoom();
        await StartCoverage.ClickAsync();
        await Expect(DeskSlot).ToHaveTextAsync("Nurse Patel is on duty at the desk.", new() { Timeout = 5000 });
    }

    private async Task EndTheDesksCoverage()
    {
        await EndCoverage.ClickAsync();
        await Expect(DeskSlot).ToHaveTextAsync("No one is at the desk tonight.", new() { Timeout = 5000 });
    }

    [Test]
    public async Task a_call_with_no_one_at_the_desk_rings_unanswered()
    {
        await OpenRoom();

        await Bell.Press.ClickAsync();

        await Expect(Bell.State).ToHaveTextAsync("Ringing");
        await Expect(Bell.Status).ToHaveTextAsync("No one is listening");
        await Expect(Bell.Count).ToHaveTextAsync("1 call tonight");
        await Expect(DeskNotified).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_desk_on_duty_hears_the_call()
    {
        await OpenRoomWithTheDeskOnDuty();

        await Bell.Press.ClickAsync();

        await Expect(Bell.Status).ToHaveTextAsync("Call placed");
        await Expect(DeskNotified).ToBeVisibleAsync();
        await Expect(DeskCall).ToHaveTextAsync("1");
        await Expect(DeskRoom).ToHaveTextAsync("12");
        await Expect(Escalated).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_third_call_is_escalated_by_a_second_handler_of_the_same_callback()
    {
        await OpenRoomWithTheDeskOnDuty();
        await Bell.Press.ClickAsync();
        await Bell.Press.ClickAsync();
        await Expect(DeskCall).ToHaveTextAsync("2");
        await Expect(Escalated).ToBeHiddenAsync();

        await Bell.Press.ClickAsync();

        await Expect(DeskCall).ToHaveTextAsync("3");
        await Expect(Escalated).ToBeVisibleAsync();
        await Expect(Bell.Count).ToHaveTextAsync("3 calls tonight");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task answering_the_call_quiets_the_bell_and_logs_it_on_the_page_and_at_the_desk()
    {
        await OpenRoomWithTheDeskOnDuty();
        await Bell.Press.ClickAsync();
        await Bell.Press.ClickAsync();
        await Expect(DeskCall).ToHaveTextAsync("2");

        await Answer.ClickAsync();

        await Expect(Bell.State).ToHaveTextAsync("Quiet");
        await Expect(Bell.Status).ToHaveTextAsync("");
        await Expect(Answered).ToHaveTextAsync("Room 12 answered after 2 calls.");
        await Expect(DeskLog).ToHaveTextAsync("Desk log: answered after 2 calls.");
        await Expect(NoCall).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task answering_with_nothing_ringing_says_there_is_no_call()
    {
        await OpenRoom();

        await Answer.ClickAsync();

        await Expect(NoCall).ToBeVisibleAsync();
        await Expect(Answered).ToBeHiddenAsync();
        await Expect(Bell.State).ToHaveTextAsync("Quiet");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task moving_the_bell_names_the_new_room_in_the_next_call()
    {
        await OpenRoomWithTheDeskOnDuty();

        await Move.ClickAsync();

        await Expect(Room).ToHaveTextAsync("Respite 3");
        await Expect(Bell.Room).ToHaveTextAsync("Room Respite 3");

        await Bell.Press.ClickAsync();
        await Expect(DeskRoom).ToHaveTextAsync("Respite 3");
        await Expect(DeskCall).ToHaveTextAsync("1");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task ending_desk_coverage_leaves_the_bell_unanswered_again_while_the_page_still_logs_answers()
    {
        await OpenRoomWithTheDeskOnDuty();
        await Bell.Press.ClickAsync();
        await Expect(DeskCall).ToHaveTextAsync("1");
        await Answer.ClickAsync();
        await Expect(DeskAnswered).ToHaveTextAsync("1");

        await EndTheDesksCoverage();

        await Bell.Press.ClickAsync();
        await Expect(Bell.Status).ToHaveTextAsync("No one is listening");
        await Expect(Bell.Count).ToHaveTextAsync("2 calls tonight");
        await Expect(DeskCall).ToHaveTextAsync("1");

        await Answer.ClickAsync();
        await Expect(Answered).ToHaveTextAsync("Room 12 answered after 2 calls.");
        await Expect(DeskAnswered).ToHaveTextAsync("1");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_desk_coming_back_on_duty_hears_the_bell_again()
    {
        await OpenRoomWithTheDeskOnDuty();
        await EndTheDesksCoverage();
        await StartCoverage.ClickAsync();
        await Expect(DeskSlot).ToHaveTextAsync("Nurse Patel is on duty at the desk.", new() { Timeout = 5000 });

        await Bell.Press.ClickAsync();

        await Expect(Bell.Status).ToHaveTextAsync("Call placed");
        await Expect(DeskCall).ToHaveTextAsync("1");
        AssertNoConsoleErrors();
    }

    private sealed class CallBellField
    {
        private readonly ILocator _root;

        internal CallBellField(ILocator root) => _root = root;

        internal ILocator Room => _root.Locator(".cb-room");
        internal ILocator State => _root.Locator(".cb-state");
        internal ILocator Status => _root.Locator(".cb-status");
        internal ILocator Count => _root.Locator(".cb-count");
        internal ILocator Press => _root.GetByRole(AriaRole.Button, new() { Name = "Call the desk", Exact = true });
    }
}
