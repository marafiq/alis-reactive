using System.Collections.Concurrent;
using System.Linq.Expressions;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Validation.RoomTransfer;

/// <summary>
/// As a coordinator requesting a room transfer, I want the page to refuse what it can see is wrong and
/// the server to tell me, in the page, what only it knows, so that a request that goes through is one
/// housekeeping can act on.
/// </summary>
/// <remarks>
/// The rules that stay on the server stay by what they are: C# that asks the room book, a lookup that
/// waits, a message written from the model, a rule kept there on purpose, a condition only C# can
/// evaluate. Each is proven by the request being sent once and the server's answer appearing: beside
/// the field when the field has a rule the page runs, in the summary when it has none.
/// </remarks>
[TestFixture]
public class WhenCoordinatorRequestsRoomTransfer : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/RoomTransfer";
    private const string Requested = "Transfer of Helen Park to MA-216 requested.";
    private const string ServerRefusal = "the server responded with a status of 400";
    private const string Tuesday = "2026-03-03";
    private const string Saturday = "2026-03-07";
    private const string Sunday = "2026-03-08";

    private PagePlan<RoomTransferModel> _plan = null!;
    private ConcurrentQueue<string> _requestsSent = null!;
    private ConcurrentQueue<string> _urgentRequestsSent = null!;
    private ConcurrentQueue<string> _housekeepingAsked = null!;

    private ILocator RequestTransfer => Page.Locator("#request-transfer-btn");
    private ILocator RequestUrgentTransfer => Page.Locator("#request-urgent-btn");
    private ILocator AskHousekeeping => Page.Locator("#ask-housekeeping-btn");
    private ILocator TransferStatus => Page.Locator("#transfer-status");
    private ILocator HousekeepingStatus => Page.Locator("#housekeeping-status");
    private ILocator ValidationSummary => Page.Locator("[data-reactive-validation-summary]");

    private async Task NavigateAndBoot()
    {
        _requestsSent = new ConcurrentQueue<string>();
        _urgentRequestsSent = new ConcurrentQueue<string>();
        _housekeepingAsked = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.EndsWith("/Sandbox/Validation/RoomTransfer/Request")) _requestsSent.Enqueue(request.Url);
            if (request.Url.EndsWith("/Sandbox/Validation/RoomTransfer/RequestUrgent")) _urgentRequestsSent.Enqueue(request.Url);
            if (request.Url.EndsWith("/Sandbox/Validation/RoomTransfer/Housekeeping")) _housekeepingAsked.Enqueue(request.Url);
        };
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<RoomTransferModel>.FromPage(Page);
    }

    private async Task Write(Expression<Func<RoomTransferModel, object?>> field, string text)
    {
        await _plan.TextBox(field).Fill(text);
        await Page.Keyboard.PressAsync("Tab");
    }

    // Helen Park, from MA-214 to the free room next door, on a Tuesday.
    private async Task DescribeTheTransfer(string toRoom = "MA-216", string day = Tuesday)
    {
        await Write(m => m.ResidentName, "Helen Park");
        await Write(m => m.FromRoom, "MA-214");
        await Write(m => m.ToRoom, toRoom);
        await Write(m => m.TransferDate, day);
    }

    private async Task AssertRequested(int requestsSoFar, string confirmation = Requested)
    {
        await ClickWhenStable(RequestTransfer);
        await Expect(TransferStatus).ToHaveTextAsync(confirmation);
        await Expect(ValidationSummary).ToBeHiddenAsync();
        Assert.That(_requestsSent, Has.Count.EqualTo(requestsSoFar));
    }

    [Test]
    public async Task a_transfer_to_the_same_room_is_refused_before_it_is_sent()
    {
        await NavigateAndBoot();
        await DescribeTheTransfer(toRoom: "MA-214");

        await ClickWhenStable(RequestTransfer);

        await Expect(_plan.ErrorFor(m => m.ToRoom)).ToHaveTextAsync("The resident is already in that room.");
        await Expect(TransferStatus).ToHaveTextAsync("No transfer requested yet.");
        Assert.That(_requestsSent, Is.Empty, "The page compared the two rooms, so nothing was sent.");

        await Write(m => m.ToRoom, "MA-216");
        await AssertRequested(1);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_transfer_to_an_occupied_room_is_sent_and_refused_beside_the_room()
    {
        await NavigateAndBoot();
        await DescribeTheTransfer(toRoom: "MA-220");

        await ClickWhenStable(RequestTransfer);

        await Expect(_plan.ErrorFor(m => m.ToRoom)).ToHaveTextAsync("MA-220 is occupied.");
        await Expect(TransferStatus).ToHaveTextAsync("No transfer requested yet.");
        Assert.That(_requestsSent, Has.Count.EqualTo(1), "Only the server has the room book, so the request is sent.");

        await Write(m => m.ToRoom, "MA-216");
        await AssertRequested(2);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_transfer_to_an_occupied_room_on_another_wing_is_refused_for_both_reasons()
    {
        await NavigateAndBoot();
        await DescribeTheTransfer(toRoom: "MC-220");

        await ClickWhenStable(RequestTransfer);

        await Expect(_plan.ErrorFor(m => m.ToRoom)).ToHaveTextAsync("MC-220 is occupied. Transfers stay on the resident's wing.");
        await Expect(TransferStatus).ToHaveTextAsync("No transfer requested yet.");
        Assert.That(_requestsSent, Has.Count.EqualTo(1));
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_transfer_on_a_sunday_is_refused_by_housekeepings_calendar()
    {
        await NavigateAndBoot();
        await DescribeTheTransfer(day: Sunday);
        await Write(m => m.WeekendApprover, "Dana Ortiz");

        await ClickWhenStable(RequestTransfer);

        await Expect(_plan.ErrorFor(m => m.TransferDate)).ToHaveTextAsync("Housekeeping does not turn rooms on a Sunday.");
        await Expect(TransferStatus).ToHaveTextAsync("No transfer requested yet.");
        Assert.That(_requestsSent, Has.Count.EqualTo(1), "Housekeeping's calendar is asked on the server, and the answer waits.");

        await Write(m => m.TransferDate, Tuesday);
        await AssertRequested(2);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    // The reason's length is a rule the page could run, kept on the server on purpose.
    [Test]
    public async Task a_reason_over_a_hundred_characters_is_sent_and_the_server_asks_to_shorten_it()
    {
        var aHundredCharacters = new string('r', 100);
        await NavigateAndBoot();
        await DescribeTheTransfer();
        await Write(m => m.Reason, aHundredCharacters + "r");

        await ClickWhenStable(RequestTransfer);

        await Expect(ValidationSummary).ToHaveTextAsync("Keep the reason under 100 characters.");
        await Expect(_plan.ErrorFor(m => m.Reason)).ToHaveTextAsync("");
        await Expect(TransferStatus).ToHaveTextAsync("No transfer requested yet.");
        Assert.That(_requestsSent, Has.Count.EqualTo(1), "The page does not count the reason's characters: the server does.");

        await Write(m => m.Reason, aHundredCharacters);
        await AssertRequested(2);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task long_notes_are_refused_in_the_residents_own_name()
    {
        await NavigateAndBoot();
        await DescribeTheTransfer();
        await Write(m => m.Notes, "Needs the bed by the window");

        await ClickWhenStable(RequestTransfer);

        await Expect(ValidationSummary).ToHaveTextAsync("Helen Park's notes run over 20 characters.");
        await Expect(TransferStatus).ToHaveTextAsync("No transfer requested yet.");
        Assert.That(_requestsSent, Has.Count.EqualTo(1), "A message written from the model is the server's to say.");

        await Write(m => m.Notes, "Bed by the window");
        await AssertRequested(2);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_saturday_transfer_needs_the_administrator_on_call_which_only_the_server_asks_for()
    {
        await NavigateAndBoot();
        await DescribeTheTransfer(day: Saturday);

        await ClickWhenStable(RequestTransfer);

        await Expect(ValidationSummary).ToHaveTextAsync("A weekend transfer needs the administrator on call to approve it.");
        await Expect(_plan.ErrorFor(m => m.WeekendApprover)).ToHaveTextAsync("");
        await Expect(TransferStatus).ToHaveTextAsync("No transfer requested yet.");
        Assert.That(_requestsSent, Has.Count.EqualTo(1), "Which days are weekends is decided in C#, so the server is asked.");

        await Write(m => m.WeekendApprover, "Dana Ortiz");
        await AssertRequested(2);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task the_urgent_request_checks_the_same_form_before_it_is_sent()
    {
        await NavigateAndBoot();
        await DescribeTheTransfer(toRoom: "");

        await ClickWhenStable(RequestUrgentTransfer);

        await Expect(_plan.ErrorFor(m => m.ToRoom)).ToHaveTextAsync("Enter the room the resident moves to.");
        await Expect(TransferStatus).ToHaveTextAsync("No transfer requested yet.");
        Assert.That(_urgentRequestsSent, Is.Empty, "The urgent request checks the same form, so nothing was sent.");

        await Write(m => m.ToRoom, "MA-216");
        await ClickWhenStable(RequestUrgentTransfer);
        await Expect(TransferStatus).ToHaveTextAsync("Urgent transfer of Helen Park to MA-216 requested.");
        Assert.That(_urgentRequestsSent, Has.Count.EqualTo(1));
        Assert.That(_requestsSent, Is.Empty);
        AssertNoConsoleErrors();
    }

    // Housekeeping's desk answers with a note, not a record of field errors: nothing is placed on the form.
    [Test]
    public async Task a_refusal_from_housekeeping_shows_no_field_messages()
    {
        await NavigateAndBoot();
        await DescribeTheTransfer(day: Sunday);
        await Write(m => m.WeekendApprover, "Dana Ortiz");

        await ClickWhenStable(AskHousekeeping);

        await Expect(HousekeepingStatus).ToHaveTextAsync("Housekeeping did not take the request.");
        await Expect(ValidationSummary).ToBeHiddenAsync();
        await Expect(_plan.ErrorFor(m => m.TransferDate)).ToHaveTextAsync("");
        await Expect(_plan.ErrorFor(m => m.ToRoom)).ToHaveTextAsync("");
        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("");
        Assert.That(_housekeepingAsked, Has.Count.EqualTo(1));
        AssertNoConsoleErrorsExcept(ServerRefusal, "server-errors.not-a-record");
    }

    [Test]
    public async Task housekeeping_confirms_a_weekday_transfer()
    {
        await NavigateAndBoot();
        await DescribeTheTransfer();

        await ClickWhenStable(AskHousekeeping);

        await Expect(HousekeepingStatus).ToHaveTextAsync("Housekeeping will prepare MA-216 on Tuesday, March 3.");
        Assert.That(_housekeepingAsked, Has.Count.EqualTo(1));
        AssertNoConsoleErrors();
    }
}
