using System.Collections.Concurrent;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.HttpPipeline.VisitEscort;

/// <summary>
/// As the memory-care coordinator, I want every visit time I book for a family to have an escort booked
/// at the wing's desk, and every time I release to let its escort go, so that no family arrives without
/// someone to walk them in.
/// </summary>
/// <remarks>
/// The page opens from a link that names the resident, the wing and the desk: the week of times comes from
/// the desk by the resident in the link. The visit planner is a widget the sandbox wrote itself. Each time
/// booked asks the desk for an escort: the time from the planner's event, the wing from the link, which
/// visit of the week from the planner's count, and the desk from the link's X-Desk header, which a link
/// may leave out. Each time released releases its escort with a reason. Sharing the week waits for the
/// summary the planner prepares and sends it with the visits left; naming the party at the earliest visit
/// sends the visitors and the note inside the party and the companion as the first companion, as JSON on
/// the way to the desk and as query keys when asking about the seating.
/// </remarks>
[TestFixture]
public class WhenCoordinatorBooksVisitEscorts : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/VisitEscort";
    private const string HelensLink = Path + "?resident=helen&wing=Maple&desk=east";
    private const string EscortsUrl = "/Sandbox/HttpPipeline/VisitEscort/Escorts/";
    private const string PartyUrl = "/Sandbox/HttpPipeline/VisitEscort/Party/";

    private PagePlan<VisitEscortModel> _plan = null!;
    private ConcurrentQueue<IRequest> _releases = null!;
    private ConcurrentQueue<IRequest> _parties = null!;

    private PlannerField Planner => new(Page.Locator($"#{IdGenerator.For<VisitEscortModel, object?>(m => m.VisitSlots)}"));
    private ILocator Resident => Page.Locator("#escort-resident");
    private ILocator EscortStatus => Page.Locator("#escort-status");
    private ILocator ShareWeek => Page.Locator("#share-week-btn");
    private ILocator ShareStatus => Page.Locator("#share-status");
    private ILocator NameTheParty => Page.Locator("#update-party-btn");
    private ILocator PartyStatus => Page.Locator("#party-status");
    private ILocator CheckSeating => Page.Locator("#check-seating-btn");
    private ILocator SeatingStatus => Page.Locator("#seating-status");

    private async Task OpenLink(string link = HelensLink)
    {
        _releases = new ConcurrentQueue<IRequest>();
        _parties = new ConcurrentQueue<IRequest>();
        Page.Request += (_, request) =>
        {
            if (request.Method == "DELETE" && request.Url.Contains(EscortsUrl)) _releases.Enqueue(request);
            if (request.Method == "PUT" && request.Url.Contains(PartyUrl)) _parties.Enqueue(request);
        };
        await NavigateToAndWaitForBoot(link);
        _plan = await PagePlan<VisitEscortModel>.FromPage(Page);
    }

    private async Task OpenHelensWeek()
    {
        await OpenLink();
        await Expect(Planner.Time("Mon 10:00 AM")).ToBeVisibleAsync();
    }

    [Test]
    public async Task the_link_opens_the_residents_week_from_the_wings_desk()
    {
        await OpenLink();

        await Expect(Resident).ToHaveTextAsync("Helen Park");
        await Expect(Planner.Wing).ToHaveTextAsync("Maple");
        await Expect(Planner.Times).ToHaveTextAsync(new[] { "Mon 10:00 AM", "Wed 2:00 PM", "Sat 11:00 AM" });
        await Expect(Planner.Left).ToHaveTextAsync("2 visits left");
        await Expect(EscortStatus).ToHaveTextAsync("No escort asked for yet.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_link_naming_a_resident_the_desk_does_not_know_says_so()
    {
        await OpenLink(Path + "?resident=nobody&wing=Maple&desk=east");

        await Expect(Planner.EmptyMessage).ToHaveTextAsync("No resident matches this link.");
        await Expect(Resident).ToHaveTextAsync("Unknown resident");
        await Expect(Planner.Times).ToHaveCountAsync(0);
        AssertNoConsoleErrorsExcept("the server responded with a status of 404");
    }

    [Test]
    public async Task booking_a_time_asks_the_desk_in_the_link_for_an_escort_at_that_time()
    {
        await OpenHelensWeek();

        await Planner.Time("Mon 10:00 AM").ClickAsync();

        await Expect(EscortStatus).ToHaveTextAsync("Dana will escort Mon 10:00 AM: visit 1 of the week, Maple wing, east desk.");
        await Expect(Planner.Time("Mon 10:00 AM")).ToHaveAttributeAsync("aria-pressed", "true");
        AssertNoConsoleErrors();
    }

    // The visit count is read when each request is sent, so the second booking asks for the week's second visit.
    [Test]
    public async Task a_second_booking_asks_for_the_weeks_second_visit()
    {
        await OpenHelensWeek();
        await Planner.Time("Mon 10:00 AM").ClickAsync();
        await Expect(EscortStatus).ToHaveTextAsync("Dana will escort Mon 10:00 AM: visit 1 of the week, Maple wing, east desk.");

        await Planner.Time("Sat 11:00 AM").ClickAsync();

        await Expect(EscortStatus).ToHaveTextAsync("Dana will escort Sat 11:00 AM: visit 2 of the week, Maple wing, east desk.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_link_naming_the_west_desk_asks_that_desk()
    {
        await OpenLink(Path + "?resident=helen&wing=Maple&desk=west");
        await Expect(Planner.Time("Mon 10:00 AM")).ToBeVisibleAsync();

        await Planner.Time("Mon 10:00 AM").ClickAsync();

        await Expect(EscortStatus).ToHaveTextAsync("Malik will escort Mon 10:00 AM: visit 1 of the week, Maple wing, west desk.");
        AssertNoConsoleErrors();
    }

    // A link with no desk sends no X-Desk header at all, and the desk system refuses a request that names none.
    [Test]
    public async Task a_link_naming_no_desk_asks_me_to_book_the_escort_by_phone()
    {
        await OpenLink(Path + "?resident=helen&wing=Maple");
        await Expect(Planner.Time("Mon 10:00 AM")).ToBeVisibleAsync();

        await Planner.Time("Mon 10:00 AM").ClickAsync();

        await Expect(EscortStatus).ToHaveTextAsync("No desk is named in the link: book the escort by phone.");
        await Expect(Planner.Time("Mon 10:00 AM")).ToHaveAttributeAsync("aria-pressed", "true");
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task releasing_a_booked_time_releases_its_escort()
    {
        await OpenHelensWeek();
        await Planner.Time("Mon 10:00 AM").ClickAsync();
        await Expect(EscortStatus).ToHaveTextAsync("Dana will escort Mon 10:00 AM: visit 1 of the week, Maple wing, east desk.");

        await Planner.Time("Mon 10:00 AM").ClickAsync();

        await Expect(EscortStatus).ToHaveTextAsync("Mon 10:00 AM: escort released (the family released the time).");
        await Expect(Planner.Time("Mon 10:00 AM")).ToHaveAttributeAsync("aria-pressed", "false");
        Assert.That(_releases, Has.Count.EqualTo(1), "A released time sends one DELETE for its escort.");
        Assert.That(_releases.Single().PostData, Does.Contain("the family released the time"));
        AssertNoConsoleErrors();
    }

    // The summary is prepared by the planner over a moment and read back as a promise; the request is
    // sent only once both have settled, with the visits left counted at that moment.
    [Test]
    public async Task sharing_the_week_sends_the_family_the_planners_summary_and_the_visits_left()
    {
        await OpenHelensWeek();
        await Planner.Time("Mon 10:00 AM").ClickAsync();
        await Planner.Time("Sat 11:00 AM").ClickAsync();
        await Expect(Planner.Left).ToHaveTextAsync("0 visits left");

        await ClickWhenStable(ShareWeek);

        await Expect(ShareStatus).ToHaveTextAsync("Sent to the family: Maple: Mon 10:00 AM, Sat 11:00 AM. 0 visits left this week.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sharing_before_a_visit_is_booked_tells_me_there_is_nothing_to_share()
    {
        await OpenHelensWeek();

        await ClickWhenStable(ShareWeek);

        await Expect(ShareStatus).ToHaveTextAsync("Nothing to share yet: no visit is booked.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task naming_the_party_updates_the_earliest_booked_visit()
    {
        await OpenHelensWeek();
        await Planner.Time("Sat 11:00 AM").ClickAsync();
        await Planner.Time("Mon 10:00 AM").ClickAsync();
        await Expect(Planner.Booked).ToHaveCountAsync(2);
        await _plan.TextBox(m => m.Visitors).Fill("3");
        await _plan.TextBox(m => m.Companion).Fill("Ruth Park");
        await _plan.TextBox(m => m.Note).Fill("Bring the album");

        await ClickWhenStable(NameTheParty);

        await Expect(PartyStatus).ToHaveTextAsync("Mon 10:00 AM: 3 visitors, companion Ruth Park; note: Bring the album.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task naming_the_party_before_a_visit_is_booked_asks_for_a_booking_first()
    {
        await OpenHelensWeek();
        await _plan.TextBox(m => m.Companion).Fill("Ruth Park");

        await ClickWhenStable(NameTheParty);

        await Expect(PartyStatus).ToHaveTextAsync("Book a time before naming the party.");
        Assert.That(_parties, Is.Empty, "Nothing is sent to the desk about a visit that is not booked.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task checking_the_seating_asks_about_the_party_as_entered()
    {
        await OpenHelensWeek();
        await _plan.TextBox(m => m.Visitors).Fill("4");
        await _plan.TextBox(m => m.Companion).Fill("Ruth Park");

        await ClickWhenStable(CheckSeating);

        await Expect(SeatingStatus).ToHaveTextAsync("The family room seats 4 visitors and Ruth Park.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task checking_the_seating_with_no_companion_named_says_so()
    {
        await OpenHelensWeek();

        await ClickWhenStable(CheckSeating);

        await Expect(SeatingStatus).ToHaveTextAsync("The family room seats 2 visitors and no companion.");
        AssertNoConsoleErrors();
    }

    // The planner's own markup, as the coordinator sees it.
    private sealed class PlannerField
    {
        private readonly ILocator _root;

        internal PlannerField(ILocator root) => _root = root;

        internal ILocator Wing => _root.Locator(".vp-wing");
        internal ILocator Left => _root.Locator(".vp-left");
        internal ILocator EmptyMessage => _root.Locator(".vp-empty");
        internal ILocator Times => _root.Locator("button.vp-slot");
        internal ILocator Booked => _root.Locator("button.vp-slot[aria-pressed='true']");

        internal ILocator Time(string label) =>
            _root.GetByRole(AriaRole.Button, new() { Name = label, Exact = true });
    }
}
