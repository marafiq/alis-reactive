using System.Collections.Concurrent;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.RoomEvening;

namespace Alis.Reactive.PlaywrightTests.Patterns.RoomEvening;

[TestFixture]
public class WhenTheAideHostsAFamilyVisit : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Patterns/RoomEvening";
    private const string MissingEscortCard = "#escort-status is not on the page";

    private ILocator VisitSlot => Page.Locator("#visit-slot");
    private ILocator VisitStatus => Page.Locator("#visit-status");
    private ILocator VisitBellLine => Page.Locator("#visit-bell-line");
    private ILocator VisitBellCount => Page.Locator("#visit-bell-count");
    private ILocator EscortStatus => VisitSlot.Locator("#escort-slot #escort-status");
    private ILocator ShiftStatus => Page.Locator("#shift-status");
    private ILocator AnsweredLine => Page.Locator("#answered-line");
    private ILocator LogStatus => Page.Locator("#log-status");
    private ILocator Summary => Page.Locator("[data-reactive-validation-summary]");
    private ILocator VisitorError => Page.Locator("span[data-valmsg-for='Visitor']");

    private ILocator Visitor => Page.Locator($"#{IdGenerator.For<RoomEveningModel>(m => m.Visitor)}");
    private ILocator Relationship => Page.Locator($"#{IdGenerator.For<RoomEveningModel>(m => m.Relationship)}");
    private ILocator Bell => Page.Locator("#room-bell");
    private ILocator BellPress => Bell.GetByRole(AriaRole.Button, new() { Name = "Call the desk", Exact = true });

    private ILocator OpenVisit => Page.Locator("#open-visit");
    private ILocator EndVisit => Page.Locator("#end-visit");
    private ILocator SaveLog => Page.Locator("#save-log");
    private ILocator SignIn => Page.Locator("#sign-in");
    private ILocator RequestEscort => Page.Locator("#request-escort");
    private ILocator AnswerBell => Page.Locator("#answer-bell");
    private ILocator ShiftChange => Page.Locator("#shift-change");
    private ILocator PageEscort => Page.Locator("#page-escort");

    private async Task OpenTheRoom()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Bell.Locator(".cb-state")).ToHaveTextAsync("Quiet");
    }

    private async Task OpenTheFamilyVisit()
    {
        await OpenVisit.ClickAsync();
        await Expect(Visitor).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    private async Task RequestAnEscort()
    {
        await RequestEscort.ClickAsync();
        await Expect(EscortStatus).ToHaveTextAsync("Aide Reyes is on the way.", new() { Timeout = 5000 });
    }

    private async Task EndTheVisit()
    {
        await EndVisit.ClickAsync();
        await Expect(VisitSlot).ToHaveTextAsync("Quiet night; no visitors expected.", new() { Timeout = 5000 });
    }

    private ConcurrentQueue<string> CapturePosts()
    {
        var posts = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST") posts.Enqueue(request.Url);
        };
        return posts;
    }

    [Test]
    public async Task opening_the_visit_signs_a_visitor_in_from_the_card()
    {
        await OpenTheRoom();
        await Expect(VisitSlot).ToHaveTextAsync("No visit this evening.");

        await OpenTheFamilyVisit();
        await Visitor.FillAsync("Ruth Hale");
        await Relationship.SelectOptionAsync("Daughter");
        await SignIn.ClickAsync();

        await Expect(VisitStatus).ToHaveTextAsync("Ruth Hale signed in for room 12 as daughter.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task signing_in_without_a_name_is_refused_on_the_card_and_sends_nothing()
    {
        await OpenTheRoom();
        await OpenTheFamilyVisit();
        var posts = CapturePosts();

        await SignIn.ClickAsync();

        await Expect(VisitorError).ToHaveTextAsync("The visitor's name is required.");
        await Expect(VisitorError).ToBeVisibleAsync();
        await Expect(VisitStatus).ToHaveTextAsync("No visitor signed in.");
        Assert.That(posts, Is.Empty, "A refused sign-in sends nothing.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_visit_card_hears_the_rooms_bell()
    {
        await OpenTheRoom();
        await OpenTheFamilyVisit();
        await Expect(VisitBellLine).ToBeHiddenAsync();

        await BellPress.ClickAsync();

        await Expect(VisitBellLine).ToBeVisibleAsync();
        await Expect(VisitBellCount).ToHaveTextAsync("1");
        await Expect(Bell.Locator(".cb-status")).ToHaveTextAsync("Call placed");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_evening_log_saves_the_visitor_while_the_visit_is_open()
    {
        await OpenTheRoom();
        await OpenTheFamilyVisit();
        await Visitor.FillAsync("Ruth Hale");

        await SaveLog.ClickAsync();

        await Expect(LogStatus).ToHaveTextAsync("Evening log saved: Quiet evening; Margaret in good spirits. Visitor: Ruth Hale.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_evening_log_waits_for_the_visitors_name_while_the_visit_is_open()
    {
        await OpenTheRoom();
        await OpenTheFamilyVisit();
        var posts = CapturePosts();

        await SaveLog.ClickAsync();

        await Expect(VisitorError).ToHaveTextAsync("The visitor's name is required.");
        await Expect(VisitorError).ToBeVisibleAsync();
        await Expect(LogStatus).ToHaveTextAsync("The evening log is not saved yet.");
        Assert.That(posts, Is.Empty, "A log held back by the visitor's rule sends nothing.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task ending_the_visit_takes_the_card_and_its_rule_away_and_the_log_saves_without_a_visitor()
    {
        await OpenTheRoom();
        await OpenTheFamilyVisit();

        await EndTheVisit();

        await Expect(Visitor).ToHaveCountAsync(0);
        await SaveLog.ClickAsync();
        await Expect(LogStatus).ToHaveTextAsync("Evening log saved: Quiet evening; Margaret in good spirits. Visitor: none.");
        await Expect(Summary).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task ending_the_visit_leaves_the_bell_unheard_by_the_card_and_the_rooms_own_buttons_working()
    {
        await OpenTheRoom();
        await OpenTheFamilyVisit();
        await BellPress.ClickAsync();
        await Expect(VisitBellCount).ToHaveTextAsync("1");

        await EndTheVisit();

        await BellPress.ClickAsync();
        await Expect(Bell.Locator(".cb-status")).ToHaveTextAsync("No one is listening");
        await Expect(Bell.Locator(".cb-count")).ToHaveTextAsync("2 calls tonight");

        await AnswerBell.ClickAsync();
        await Expect(AnsweredLine).ToHaveTextAsync("Bell answered after 2 calls.");
        await Expect(Bell.Locator(".cb-state")).ToHaveTextAsync("Quiet");

        await ShiftChange.ClickAsync();
        await Expect(ShiftStatus).ToHaveTextAsync("Night shift on duty.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_escort_requested_from_the_visit_opens_inside_it_and_hears_the_shift_change()
    {
        await OpenTheRoom();
        await OpenTheFamilyVisit();

        await RequestAnEscort();
        await ShiftChange.ClickAsync();

        await Expect(EscortStatus).ToHaveTextAsync("Escort handed to the night staff.");
        await Expect(ShiftStatus).ToHaveTextAsync("Night shift on duty.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task ending_the_visit_takes_the_escort_card_and_its_listeners_with_it()
    {
        await OpenTheRoom();
        await OpenTheFamilyVisit();
        await RequestAnEscort();

        await EndTheVisit();
        await Expect(EscortStatus).ToHaveCountAsync(0);
        await ShiftChange.ClickAsync();

        await Expect(ShiftStatus).ToHaveTextAsync("Night shift on duty.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task paging_the_escort_reaches_the_open_escort_card()
    {
        await OpenTheRoom();
        await OpenTheFamilyVisit();
        await RequestAnEscort();

        await PageEscort.ClickAsync();

        await Expect(EscortStatus).ToHaveTextAsync("Escort paged from the room.");
        await Expect(ShiftStatus).ToHaveTextAsync("Escort paged.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task paging_the_escort_before_a_card_is_open_changes_nothing_and_reports_the_missing_card()
    {
        await OpenTheRoom();

        await PageEscort.ClickAsync();

        await WaitForTraceMessage(MissingEscortCard);
        await Expect(ShiftStatus).ToHaveTextAsync("Evening shift on duty.");
        AssertNoConsoleErrorsExcept(MissingEscortCard);
    }

    [Test]
    public async Task reopening_the_visit_after_ending_it_brings_a_fresh_card_that_hears_the_bell_again()
    {
        await OpenTheRoom();
        await OpenTheFamilyVisit();
        await Visitor.FillAsync("Ruth Hale");
        await EndTheVisit();
        await BellPress.ClickAsync();
        await Expect(Bell.Locator(".cb-status")).ToHaveTextAsync("No one is listening");

        await OpenTheFamilyVisit();

        await Expect(Visitor).ToHaveValueAsync("");
        await Expect(VisitBellLine).ToBeHiddenAsync();
        await BellPress.ClickAsync();
        await Expect(VisitBellLine).ToBeVisibleAsync();
        await Expect(VisitBellCount).ToHaveTextAsync("2");
        await Expect(Bell.Locator(".cb-status")).ToHaveTextAsync("Call placed");
        AssertNoConsoleErrors();
    }
}
