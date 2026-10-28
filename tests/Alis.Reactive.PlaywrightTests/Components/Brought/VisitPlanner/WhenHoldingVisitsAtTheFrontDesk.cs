using System.Text.RegularExpressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought;

namespace Alis.Reactive.PlaywrightTests.Components.Brought.VisitPlanner;

/// <summary>
/// As the front desk coordinator, I want every visit time I book for a family to be held in the desk
/// system, and every time I unbook to be released, so that the desk and the planner never disagree.
/// </summary>
/// <remarks>
/// The page is opened from a link that names the resident and the family, and may name how many
/// visitors are coming and how many the family's visitor pass allows. Everything the desk is told
/// comes from the visit planner (a widget the sandbox wrote itself), from that link, or from the
/// desk's own earlier answer: the slot from the planner's event, the wing from its property, the
/// visit count from its count, the earliest visit from its function, and the wing whose desk holds
/// the family's times from the week the desk answered first. The desk answers by the time asked for:
/// Monday 10:00 is held, Tuesday 3:00 is held until the family confirms, Saturday 11:00 was taken by
/// another family, and on Sunday the desk does not answer. The Hale family already holds Wednesday
/// 2:00 at the Garden wing desk.
/// </remarks>
[TestFixture]
public class WhenHoldingVisitsAtTheFrontDesk : PlaywrightTestBase
{
    private const string FamilyLink = "/Sandbox/Components/VisitPlanner/FrontDesk?resident=margaret&family=hale";

    private PagePlan<FrontDeskHoldsModel> _plan = null!;

    private PlannerField Planner => new(Page.Locator($"#{_plan.FindComponent(m => m.VisitSlots)!.ElementId}"));
    private ILocator Resident => Page.Locator("#desk-resident");
    private ILocator Family => Page.Locator("#desk-family");
    private ILocator DeskName => Page.Locator("#desk-name");
    private ILocator Held => Page.Locator("#desk-held");
    private ILocator Loading => Page.Locator("#desk-loading");
    private ILocator Status => Page.Locator("#desk-status");
    private ILocator AwaitingFamily => Page.Locator("#desk-awaiting");
    private ILocator Toast => Page.Locator(".e-toast").First;
    private ILocator ToastTitle => Toast.Locator(".e-toast-title");
    private ILocator VisitorLimit => Page.Locator("#desk-visitor-limit");
    private ILocator ReleaseEveryHold => Page.Locator("#release-every-hold");
    private ILocator ConfirmDialog => Page.Locator("#alisConfirmDialog");
    private ILocator PrepareArrival => Page.Locator("#prepare-arrival");
    private ILocator Drawer => Page.Locator("#alis-drawer");
    private ILocator DrawerTitle => Page.Locator("#alis-drawer-title");
    private ILocator ArrivalTime => Page.Locator("#arrival-time");
    private ILocator ArrivalRoom => Page.Locator("#arrival-room");
    private ILocator LoaderOverDesk => Page.Locator("#desk-card #alis-loader");
    private NumericTextBoxLocator Visitors => _plan.NumericTextBox(m => m.Visitors);

    // The drawer drops this class the moment it starts closing, so whether it is open is read from it.
    private static readonly Regex DrawerOpen = new("alis-drawer--visible");
    private static readonly Regex SuccessTone = new(@"\be-toast-success\b");
    private static readonly Regex DangerTone = new(@"\be-toast-danger\b");
    private const string ConnectionRefused = "net::ERR_CONNECTION_REFUSED";
    private const string NetworkError = "[alis:http] fetch.network-error";

    private async Task OpenDesk(string link = FamilyLink)
    {
        await NavigateToAndWaitForBoot(link);
        _plan = await PagePlan<FrontDeskHoldsModel>.FromPage(Page);
    }

    private async Task OpenDeskForTheHales()
    {
        await OpenDesk();
        await Expect(Planner.Time("Wed 2:00 PM")).ToHaveAttributeAsync("aria-pressed", "true");
    }

    [Test]
    public async Task the_family_link_opens_the_residents_week_with_the_times_the_family_already_holds()
    {
        await OpenDesk();

        await Expect(Resident).ToHaveTextAsync("Margaret Hale");
        await Expect(Family).ToHaveTextAsync("Hale family");
        await Expect(Planner.Wing).ToHaveTextAsync("Garden wing");
        await Expect(Planner.Times).ToHaveCountAsync(6);
        await Expect(Planner.Booked).ToHaveCountAsync(1);
        await Expect(Planner.Time("Wed 2:00 PM")).ToHaveAttributeAsync("aria-pressed", "true");
        await Expect(Held).ToHaveTextAsync("1");
        await Expect(DeskName).ToHaveTextAsync("Garden wing desk");
        await Expect(Status).ToHaveTextAsync("The desk has not been asked yet.");
        await Expect(Visitors.Input).ToHaveValueAsync("2");
        AssertNoConsoleErrors();
    }

    // The holds are asked of the desk of the wing the week answered with, read from that answer.
    [Test]
    public async Task the_familys_holds_are_asked_of_the_desk_of_the_wing_the_week_names()
    {
        await OpenDesk("/Sandbox/Components/VisitPlanner/FrontDesk?resident=harold&family=benson");

        await Expect(Resident).ToHaveTextAsync("Harold Benson");
        await Expect(Planner.Wing).ToHaveTextAsync("Maple wing");
        await Expect(DeskName).ToHaveTextAsync("Maple wing desk");
        await Expect(Family).ToHaveTextAsync("Benson family");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_link_that_names_the_visitors_starts_the_visitors_field_at_that_number()
    {
        await OpenDesk(FamilyLink + "&visitors=4");

        await Expect(Visitors.Input).ToHaveValueAsync("4");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_desk_says_it_is_asking_until_the_week_arrives()
    {
        var weekReleased = new SemaphoreSlim(0);
        await Page.RouteAsync("**/Sandbox/Components/VisitPlanner/FrontDesk/Week/*", async route =>
        {
            await weekReleased.WaitAsync(TimeSpan.FromSeconds(30));
            await route.ContinueAsync();
        });
        await NavigateToAndWaitForBoot(FamilyLink);

        await Expect(Loading).ToBeVisibleAsync();
        weekReleased.Release();
        await Expect(Resident).ToHaveTextAsync("Margaret Hale");
        await Expect(Loading).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // The family's holds are asked for only after the week has arrived; the desk stops saying it is
    // asking as soon as the week is there, without waiting for the holds.
    [Test]
    public async Task the_desk_stops_saying_it_is_asking_once_the_week_arrives_while_the_holds_still_load()
    {
        var weekReleased = new SemaphoreSlim(0);
        await Page.RouteAsync("**/Sandbox/Components/VisitPlanner/FrontDesk/Week/*", async route =>
        {
            await weekReleased.WaitAsync(TimeSpan.FromSeconds(30));
            await route.ContinueAsync();
        });
        var holdsReleased = new SemaphoreSlim(0);
        await Page.RouteAsync("**/Sandbox/Components/VisitPlanner/FrontDesk/Holds/*", async route =>
        {
            await holdsReleased.WaitAsync(TimeSpan.FromSeconds(30));
            await route.ContinueAsync();
        });
        await NavigateToAndWaitForBoot(FamilyLink);
        await Expect(Loading).ToBeVisibleAsync();

        weekReleased.Release();

        await Expect(Resident).ToHaveTextAsync("Margaret Hale");
        await Expect(Loading).ToBeHiddenAsync();
        await Expect(Family).ToHaveTextAsync("Family");
        holdsReleased.Release();
        await Expect(Family).ToHaveTextAsync("Hale family");
        await Expect(Held).ToHaveTextAsync("1");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_link_to_an_unknown_resident_is_answered_in_the_planner()
    {
        await OpenDesk("/Sandbox/Components/VisitPlanner/FrontDesk?resident=nobody&family=hale");

        await Expect(Planner.EmptyMessage).ToHaveTextAsync("No resident matches this link.");
        await Expect(Resident).ToHaveTextAsync("Unknown resident");
        await Expect(Family).ToHaveTextAsync("Family");
        await Expect(Loading).ToBeHiddenAsync();
        AssertNoConsoleErrorsExcept("the server responded with a status of 404");
    }

    [Test]
    public async Task booking_a_time_holds_it_for_the_family_at_the_wings_desk()
    {
        await OpenDeskForTheHales();

        await Planner.Time("Mon 10:00 AM").ClickAsync();

        const string Message = "Held Mon 10:00 AM for the Hale family at the Garden wing desk; visit 2 this week.";
        await Expect(Status).ToHaveTextAsync(Message);
        await Expect(Toast).ToContainTextAsync(Message);
        await Expect(ToastTitle).ToHaveTextAsync("Front desk");
        await Expect(Toast).ToHaveClassAsync(SuccessTone);
        await Expect(Held).ToHaveTextAsync("2");
        await Expect(AwaitingFamily).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_time_held_until_the_family_confirms_is_flagged_as_waiting()
    {
        await OpenDeskForTheHales();

        await Planner.Time("Tue 3:00 PM").ClickAsync();

        await Expect(Status).ToHaveTextAsync("Tue 3:00 PM is held until the Hale family confirms.");
        await Expect(AwaitingFamily).ToBeVisibleAsync();
        await Expect(Planner.Time("Tue 3:00 PM")).ToHaveAttributeAsync("aria-pressed", "true");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_time_another_family_took_is_unbooked_with_the_desks_reason()
    {
        await OpenDeskForTheHales();

        await Planner.Time("Sat 11:00 AM").ClickAsync();

        await Expect(Status).ToHaveTextAsync("Sat 11:00 AM was just taken by the Okafor family.");
        await Expect(Toast).ToContainTextAsync("Sat 11:00 AM was just taken by the Okafor family.");
        await Expect(ToastTitle).ToHaveTextAsync("Time not held");
        await Expect(Toast).ToHaveClassAsync(DangerTone);
        await Expect(Planner.Time("Sat 11:00 AM")).ToHaveAttributeAsync("aria-pressed", "false");
        await Expect(Planner.Booked).ToHaveCountAsync(1);
        AssertNoConsoleErrorsExcept("the server responded with a status of 409");
    }

    [Test]
    public async Task when_the_desk_does_not_answer_the_time_stays_booked_but_is_not_held()
    {
        await OpenDeskForTheHales();

        await Planner.Time("Sun 11:00 AM").ClickAsync();

        await Expect(Status).ToHaveTextAsync("The front desk is not answering; the time is booked here but not held.");
        await Expect(Planner.Time("Sun 11:00 AM")).ToHaveAttributeAsync("aria-pressed", "true");
        AssertNoConsoleErrorsExcept("the server responded with a status of 503");
    }

    [Test]
    public async Task when_the_desk_cannot_be_reached_the_time_stays_booked_but_is_not_held()
    {
        await Page.RouteAsync("**/Sandbox/Components/VisitPlanner/FrontDesk/Hold/*", route => route.AbortAsync("connectionrefused"));
        await OpenDeskForTheHales();

        await Planner.Time("Mon 10:00 AM").ClickAsync();

        await Expect(Status).ToHaveTextAsync("The front desk is not answering; the time is booked here but not held.");
        await Expect(Planner.Time("Mon 10:00 AM")).ToHaveAttributeAsync("aria-pressed", "true");
        AssertNoConsoleErrorsExcept(ConnectionRefused, NetworkError);
    }

    [Test]
    public async Task unbooking_a_held_time_releases_it_at_the_desk()
    {
        await OpenDeskForTheHales();

        await Planner.Time("Wed 2:00 PM").ClickAsync();

        await Expect(Status).ToHaveTextAsync("Released Wed 2:00 PM.");
        await Expect(Held).ToHaveTextAsync("0");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_the_visitors_updates_the_earliest_held_visit()
    {
        await OpenDeskForTheHales();
        await Planner.Time("Mon 10:00 AM").ClickAsync();
        await Expect(Held).ToHaveTextAsync("2");

        await _plan.NumericTextBox(m => m.Visitors).FillAndBlur("3");

        await Expect(Status).ToHaveTextAsync("Mon 10:00 AM now expects 3 visitors.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task more_visitors_than_the_familys_pass_allows_are_flagged()
    {
        await OpenDesk(FamilyLink + "&maxVisitors=4");

        await Visitors.FillAndBlur("5");
        await Expect(VisitorLimit).ToBeVisibleAsync();

        await Visitors.FillAndBlur("4");
        await Expect(VisitorLimit).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_link_without_a_visitor_pass_flags_no_number_of_visitors()
    {
        await OpenDeskForTheHales();

        await Visitors.FillAndBlur("6");

        await Expect(Status).ToHaveTextAsync("Wed 2:00 PM now expects 6 visitors.");
        await Expect(VisitorLimit).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task declining_to_release_every_hold_keeps_them()
    {
        await OpenDeskForTheHales();

        await ReleaseEveryHold.ClickAsync();
        await Expect(ConfirmDialog).ToBeVisibleAsync(new() { Timeout = 10000 });
        await ConfirmDialog.Locator("button:not(.e-primary)").Last.ClickAsync();

        await Expect(Status).ToHaveTextAsync("No hold was released.");
        await Expect(Planner.Booked).ToHaveCountAsync(1);
        await Expect(Held).ToHaveTextAsync("1");
        AssertNoConsoleErrors();
    }

    // The release names the family in its body, beside the resident in its path.
    [Test]
    public async Task releasing_every_hold_tells_the_desk_which_family_let_them_go()
    {
        await OpenDeskForTheHales();

        await ReleaseEveryHold.ClickAsync();
        await Expect(ConfirmDialog).ToBeVisibleAsync(new() { Timeout = 10000 });
        await ConfirmDialog.Locator("button.e-primary").ClickAsync();

        await Expect(Status).ToHaveTextAsync("Released every hold the Hale family had for Margaret Hale.");
        await Expect(Planner.Booked).ToHaveCountAsync(0);
        await Expect(Held).ToHaveTextAsync("0");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task with_nothing_held_releasing_every_hold_asks_nothing()
    {
        await OpenDeskForTheHales();
        await Planner.Time("Wed 2:00 PM").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Released Wed 2:00 PM.");

        await ReleaseEveryHold.ClickAsync();

        await Expect(Status).ToHaveTextAsync("No hold was released.");
        await Expect(ConfirmDialog).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task preparing_an_arrival_prints_the_badge_and_opens_the_arrival_card()
    {
        await OpenDeskForTheHales();

        await ClickWhenStable(PrepareArrival);

        await Expect(Drawer).ToHaveClassAsync(DrawerOpen);
        await Expect(DrawerTitle).ToHaveTextAsync("Arrival");
        await Expect(ArrivalTime).ToHaveTextAsync("Wed 2:00 PM");
        await Expect(ArrivalRoom).ToHaveTextAsync("Meet in the garden lounge.");
        await Expect(Status).ToHaveTextAsync("Badge printed for the Hale family, Wed 2:00 PM.");
        AssertNoConsoleErrors();
    }

    // The badge and the card are asked for side by side; the drawer waits for both, with the desk covered.
    [Test]
    public async Task the_arrival_card_waits_for_the_badge_with_the_desk_covered()
    {
        var badgeReleased = new SemaphoreSlim(0);
        await Page.RouteAsync("**/Sandbox/Components/VisitPlanner/FrontDesk/Badge/*", async route =>
        {
            await badgeReleased.WaitAsync(TimeSpan.FromSeconds(30));
            await route.ContinueAsync();
        });
        await OpenDeskForTheHales();

        await ClickWhenStable(PrepareArrival);

        await Expect(LoaderOverDesk).ToBeVisibleAsync();
        await Expect(ArrivalRoom).ToHaveTextAsync("Meet in the garden lounge.");
        await Expect(Drawer).Not.ToHaveClassAsync(DrawerOpen);
        badgeReleased.Release();
        await Expect(Drawer).ToHaveClassAsync(DrawerOpen);
        await Expect(LoaderOverDesk).ToBeHiddenAsync();
        await Expect(Status).ToHaveTextAsync("Badge printed for the Hale family, Wed 2:00 PM.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_the_badge_printer_does_not_answer_the_arrival_card_still_opens()
    {
        await Page.RouteAsync("**/Sandbox/Components/VisitPlanner/FrontDesk/Badge/*", route => route.AbortAsync("connectionrefused"));
        await OpenDeskForTheHales();

        await ClickWhenStable(PrepareArrival);

        await Expect(Status).ToHaveTextAsync("The badge printer did not answer; write the family's badge by hand.");
        await Expect(Drawer).ToHaveClassAsync(DrawerOpen);
        await Expect(DrawerTitle).ToHaveTextAsync("Arrival");
        await Expect(ArrivalRoom).ToHaveTextAsync("Meet in the garden lounge.");
        await Expect(LoaderOverDesk).ToBeHiddenAsync();
        AssertNoConsoleErrorsExcept(ConnectionRefused, NetworkError);
    }

    [Test]
    public async Task with_no_time_held_the_desk_asks_for_a_hold_before_an_arrival()
    {
        await OpenDeskForTheHales();
        await Planner.Time("Wed 2:00 PM").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Released Wed 2:00 PM.");

        await ClickWhenStable(PrepareArrival);

        await Expect(Status).ToHaveTextAsync("Hold a time first; an arrival is prepared from a hold.");
        await Expect(Drawer).Not.ToHaveClassAsync(DrawerOpen);
        AssertNoConsoleErrors();
    }

    private sealed class PlannerField
    {
        private readonly ILocator _root;

        internal PlannerField(ILocator root) => _root = root;

        internal ILocator Wing => _root.Locator(".vp-wing");
        internal ILocator EmptyMessage => _root.Locator(".vp-empty");
        internal ILocator Times => _root.Locator("button.vp-slot");
        internal ILocator Booked => _root.Locator("button.vp-slot[aria-pressed='true']");

        internal ILocator Time(string label) =>
            _root.GetByRole(AriaRole.Button, new() { Name = label, Exact = true });
    }
}
