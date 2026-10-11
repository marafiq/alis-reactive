using System.Collections.Concurrent;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought;

namespace Alis.Reactive.PlaywrightTests.Components.Brought.VisitPlanner;

/// <summary>
/// As the life enrichment coordinator, I want to plan a family's visits with a resident for the week,
/// within the wing's times, the week's allowance and the therapy schedule, so that the front desk
/// expects every family that comes.
/// </summary>
/// <remarks>
/// The visit planner is a widget the sandbox wrote itself (Scripts/visit-planner.ts) and declares in
/// its own assembly (BroughtComponents/VisitPlanner). Every behavior here runs through the
/// framework's generic grammar: properties of every shape, functions, methods with mixed arguments,
/// events with nested and cancelable payloads, validation, gather and partials.
/// </remarks>
[TestFixture]
public class WhenPlanningFamilyVisits : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/VisitPlanner";

    // The companion panel renders CompanionVisitPlanModel.VisitSlots; its ID is the framework's
    // deterministic ID for that member.
    private const string CompanionPlannerId =
        "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_Components_Brought_CompanionVisitPlanModel__VisitSlots";

    private PagePlan<FamilyVisitPlanModel> _plan = null!;
    private ConcurrentQueue<string> _saves = null!;
    private ConcurrentQueue<string> _holds = null!;

    private PlannerField Planner => new(Page.Locator($"#{_plan.FindComponent(m => m.VisitSlots)!.ElementId}"));
    private PlannerField CompanionPlanner => new(Page.Locator($"#{CompanionPlannerId}"));
    private ILocator PlannerError => _plan.ErrorFor(m => m.VisitSlots);

    private ILocator ResidentName => Page.Locator("#resident-name");
    private ILocator OfferedCount => Page.Locator("#offered-count");
    private ILocator PlannedCount => Page.Locator("#planned-count");
    private ILocator VisitsLeft => Page.Locator("#visits-left");
    private ILocator PlanEmpty => Page.Locator("#plan-empty");
    private ILocator QuietHours => Page.Locator("#quiet-hours");
    private ILocator WeekFull => Page.Locator("#week-full");
    private ILocator BrunchStatus => Page.Locator("#brunch-status");
    private ILocator RequestedStatus => Page.Locator("#requested-status");
    private ILocator BrunchPlanned => Page.Locator("#brunch-planned");
    private ILocator SundayOffered => Page.Locator("#sunday-offered");
    private ILocator CheckRequested => Page.Locator("#check-requested");
    private ILocator ShareWeek => Page.Locator("#share-week");
    private ILocator ShareStatus => Page.Locator("#share-status");
    private ILocator FamilySummary => Page.Locator("#family-summary");
    private ILocator Confirmation => Page.Locator("#visit-confirmation");
    private ILocator CompanionPlanned => Page.Locator("#companion-planned");
    private ILocator CompanionHeld => Page.Locator("#companion-held");

    private ILocator BookFirstOpen => Page.Locator("#book-first-open");
    private ILocator CheckBrunch => Page.Locator("#check-brunch");
    private ILocator ClearWeek => Page.Locator("#clear-week");
    private ILocator SaveVisits => Page.Locator("#save-visits");
    private ILocator AddCompanion => Page.Locator("#companion-add");
    private ILocator RemoveCompanion => Page.Locator("#companion-remove");

    private async Task OpenPlanner()
    {
        _saves = new ConcurrentQueue<string>();
        _holds = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains("/Sandbox/Components/VisitPlanner/Save")) _saves.Enqueue(request.Url);
            if (request.Url.Contains("/Sandbox/Components/VisitPlanner/CompanionHold")) _holds.Enqueue(request.Url);
        };
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<FamilyVisitPlanModel>.FromPage(Page);
    }

    private async Task ChooseMargaret()
    {
        await _plan.NativeDropDown(m => m.ResidentId).Select("Margaret Hale");
        await Expect(Planner.Wing).ToHaveTextAsync("Garden wing", new() { Timeout = 5000 });
    }

    [Test]
    public async Task before_a_resident_is_chosen_the_planner_says_what_to_do()
    {
        await OpenPlanner();

        await Expect(Planner.EmptyMessage).ToHaveTextAsync("Choose a resident to see this week's visit times.");
        await Expect(Planner.Times).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_a_resident_offers_the_times_of_their_wing()
    {
        await OpenPlanner();

        await ChooseMargaret();

        await Expect(Planner.Left).ToHaveTextAsync("3 visits left");
        await Expect(Planner.Times).ToHaveCountAsync(6);
        await Expect(ResidentName).ToHaveTextAsync("Margaret Hale");
        await Expect(OfferedCount).ToHaveTextAsync("6");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_page_says_when_the_week_offers_a_sunday_visit()
    {
        await OpenPlanner();

        await ChooseMargaret();
        await Expect(SundayOffered).ToBeVisibleAsync();

        await _plan.NativeDropDown(m => m.ResidentId).Select("Harold Benson");
        await Expect(Planner.Wing).ToHaveTextAsync("Maple wing", new() { Timeout = 5000 });
        await Expect(SundayOffered).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task booking_a_time_counts_it_and_notes_who_is_coming()
    {
        await OpenPlanner();
        await ChooseMargaret();
        await _plan.TextBox(m => m.Note).Fill("Bring the photo album");

        await Planner.Time("Mon 10:00 AM").ClickAsync();

        await Expect(Planner.Time("Mon 10:00 AM")).ToHaveAttributeAsync("aria-pressed", "true");
        await Expect(Planner.NoteUnder("Mon 10:00 AM")).ToHaveTextAsync("2 visitors · Bring the photo album");
        await Expect(PlannedCount).ToHaveTextAsync("1");
        await Expect(VisitsLeft).ToHaveTextAsync("2");
        await Expect(Planner.Left).ToHaveTextAsync("2 visits left");
        await Expect(PlanEmpty).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_time_in_quiet_hours_is_refused_before_it_is_booked()
    {
        await OpenPlanner();
        await ChooseMargaret();

        await Planner.Time("Thu 1:00 PM").ClickAsync();

        await Expect(QuietHours).ToHaveTextAsync("Thu 1:00 PM is during quiet hours; choose another time.");
        await Expect(Planner.Time("Thu 1:00 PM")).ToHaveAttributeAsync("aria-pressed", "false");
        await Expect(PlannedCount).ToHaveTextAsync("0");

        await Planner.Time("Sat 11:00 AM").ClickAsync();
        await Expect(Planner.Time("Sat 11:00 AM")).ToHaveAttributeAsync("aria-pressed", "true");
        await Expect(QuietHours).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_full_week_refuses_another_visit_until_one_is_released()
    {
        await OpenPlanner();
        await ChooseMargaret();
        await Planner.Time("Mon 10:00 AM").ClickAsync();
        await Planner.Time("Tue 3:00 PM").ClickAsync();
        await Planner.Time("Sat 11:00 AM").ClickAsync();
        await Expect(Planner.Left).ToHaveTextAsync("0 visits left");

        await Planner.Time("Sun 11:00 AM").ClickAsync();

        await Expect(WeekFull).ToHaveTextAsync("This week is full: 3 visits.");
        await Expect(Planner.Time("Sun 11:00 AM")).ToHaveAttributeAsync("aria-pressed", "false");
        await Expect(Planner.WaitlistUnder("Sun 11:00 AM")).ToHaveTextAsync("On the waitlist");

        await Planner.Time("Tue 3:00 PM").ClickAsync();
        await Expect(WeekFull).ToBeHiddenAsync();
        await Planner.Time("Sun 11:00 AM").ClickAsync();
        await Expect(Planner.Time("Sun 11:00 AM")).ToHaveAttributeAsync("aria-pressed", "true");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_full_week_can_replace_its_earliest_visit_instead()
    {
        await OpenPlanner();
        await ChooseMargaret();
        await _plan.NativeCheckBox(m => m.SwapWhenFull).Check();
        await Planner.Time("Mon 10:00 AM").ClickAsync();
        await Planner.Time("Tue 3:00 PM").ClickAsync();
        await Planner.Time("Sat 11:00 AM").ClickAsync();

        await Planner.Time("Sun 11:00 AM").ClickAsync();

        await Expect(Planner.Time("Sun 11:00 AM")).ToHaveAttributeAsync("aria-pressed", "true");
        await Expect(Planner.Time("Mon 10:00 AM")).ToHaveAttributeAsync("aria-pressed", "false");
        await Expect(Planner.Booked).ToHaveCountAsync(3);
        await Expect(PlannedCount).ToHaveTextAsync("3");
        await Expect(WeekFull).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task booking_the_first_open_time_takes_the_planners_suggestion_each_time()
    {
        await OpenPlanner();
        await ChooseMargaret();

        await BookFirstOpen.ClickAsync();
        await Expect(Planner.Time("Mon 10:00 AM")).ToHaveAttributeAsync("aria-pressed", "true");

        await BookFirstOpen.ClickAsync();
        await Expect(Planner.Time("Tue 3:00 PM")).ToHaveAttributeAsync("aria-pressed", "true");
        await Expect(PlannedCount).ToHaveTextAsync("2");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_brunch_check_asks_the_planner_whether_sunday_is_booked()
    {
        await OpenPlanner();
        await ChooseMargaret();

        await CheckBrunch.ClickAsync();
        await Expect(BrunchStatus).ToHaveTextAsync("Sunday brunch is still open.");

        await Planner.Time("Sun 11:00 AM").ClickAsync();
        await CheckBrunch.ClickAsync();
        await Expect(BrunchStatus).ToHaveTextAsync("Sunday brunch is booked.");
        await Expect(BrunchPlanned).ToBeVisibleAsync();

        await Planner.Time("Sun 11:00 AM").ClickAsync();
        await Expect(BrunchPlanned).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task checking_the_time_the_family_asked_for_asks_the_planner_about_that_time()
    {
        await OpenPlanner();
        await ChooseMargaret();
        await Planner.Time("Sat 11:00 AM").ClickAsync();

        await _plan.NativeDropDown(m => m.RequestedTime).Select("Sat 11:00 AM");
        await CheckRequested.ClickAsync();
        await Expect(RequestedStatus).ToHaveTextAsync("That time is already booked for the family.");

        await _plan.NativeDropDown(m => m.RequestedTime).Select("Mon 10:00 AM");
        await CheckRequested.ClickAsync();
        await Expect(RequestedStatus).ToHaveTextAsync("That time is not booked yet.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sharing_the_week_waits_for_the_planner_to_prepare_its_summary()
    {
        await OpenPlanner();
        await ChooseMargaret();
        await Planner.Time("Mon 10:00 AM").ClickAsync();
        await Planner.Time("Sun 11:00 AM").ClickAsync();

        await ShareWeek.ClickAsync();

        await Expect(ShareStatus).ToHaveTextAsync("Summary ready to send.", new() { Timeout = 5000 });
        await Expect(FamilySummary).ToHaveTextAsync("Garden wing: Mon 10:00 AM, Sun 11:00 AM");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sharing_an_empty_week_asks_for_a_visit_first()
    {
        await OpenPlanner();
        await ChooseMargaret();

        await ShareWeek.ClickAsync();

        await Expect(ShareStatus).ToHaveTextAsync("Plan a visit before sharing the week.", new() { Timeout = 5000 });
        await Expect(FamilySummary).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_without_a_visit_asks_for_one_beside_the_planner()
    {
        await OpenPlanner();
        await ChooseMargaret();

        await SaveVisits.ClickAsync();

        await Expect(PlannerError).ToHaveTextAsync("Plan at least one visit.");
        Assert.That(_saves, Is.Empty, "A plan the page can see is incomplete is not sent.");

        await Planner.Time("Mon 10:00 AM").ClickAsync();
        await Expect(PlannerError).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_server_refuses_a_therapy_time_beside_the_planner_and_saves_the_rest()
    {
        await OpenPlanner();
        await ChooseMargaret();
        await Planner.Time("Wed 2:00 PM").ClickAsync();

        await SaveVisits.ClickAsync();

        await Expect(PlannerError).ToHaveTextAsync("Wed 2:00 PM is Margaret's physical therapy; choose another time.");
        await Expect(Confirmation).ToHaveTextAsync("");

        await Planner.Time("Wed 2:00 PM").ClickAsync();
        await Planner.Time("Mon 10:00 AM").ClickAsync();
        await Planner.Time("Sun 11:00 AM").ClickAsync();
        await SaveVisits.ClickAsync();

        await Expect(Confirmation).ToHaveTextAsync("Visits saved for Margaret Hale: Mon 10:00 AM, Sun 11:00 AM.");
        Assert.That(_saves, Has.Count.EqualTo(2));
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task locking_the_week_freezes_its_times()
    {
        await OpenPlanner();
        await ChooseMargaret();
        await Planner.Time("Mon 10:00 AM").ClickAsync();

        await _plan.NativeCheckBox(m => m.LockWeek).Check();

        await Expect(Planner.Time("Mon 10:00 AM")).ToBeDisabledAsync();
        await Expect(Planner.Time("Tue 3:00 PM")).ToBeDisabledAsync();

        await _plan.NativeCheckBox(m => m.LockWeek).Input.UncheckAsync();
        await Expect(Planner.Time("Tue 3:00 PM")).ToBeEnabledAsync();
        await Planner.Time("Tue 3:00 PM").ClickAsync();
        await Expect(PlannedCount).ToHaveTextAsync("2");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_the_week_releases_every_visit()
    {
        await OpenPlanner();
        await ChooseMargaret();
        await Planner.Time("Mon 10:00 AM").ClickAsync();
        await Planner.Time("Sat 11:00 AM").ClickAsync();
        await Expect(PlannedCount).ToHaveTextAsync("2");

        await ClearWeek.ClickAsync();

        await Expect(Planner.Booked).ToHaveCountAsync(0);
        await Expect(PlannedCount).ToHaveTextAsync("0");
        await Expect(VisitsLeft).ToHaveTextAsync("3");
        await Expect(PlanEmpty).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task planning_the_companions_visits_holds_each_time_at_the_front_desk()
    {
        await OpenPlanner();

        await AddCompanion.ClickAsync();
        await Expect(CompanionPlanner.Wing).ToHaveTextAsync("Garden wing", new() { Timeout = 5000 });
        await CompanionPlanner.Time("Tue 10:00 AM").ClickAsync();

        await Expect(CompanionHeld).ToHaveTextAsync("tue-10");
        await Expect(CompanionPlanned).ToHaveTextAsync("1");
        await Expect(PlannedCount).ToHaveTextAsync("0");
        Assert.That(_holds, Has.Count.EqualTo(1));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task removing_and_adding_the_companion_again_holds_each_time_once()
    {
        await OpenPlanner();
        await AddCompanion.ClickAsync();
        await Expect(CompanionPlanner.Wing).ToHaveTextAsync("Garden wing", new() { Timeout = 5000 });
        await RemoveCompanion.ClickAsync();
        await Expect(Page.Locator("#companion-slot")).ToHaveTextAsync("No companion on this plan.");

        await AddCompanion.ClickAsync();
        await Expect(CompanionPlanner.Wing).ToHaveTextAsync("Garden wing", new() { Timeout = 5000 });
        await CompanionPlanner.Time("Thu 3:00 PM").ClickAsync();

        await Expect(CompanionHeld).ToHaveTextAsync("thu-15");
        Assert.That(_holds, Has.Count.EqualTo(1), "A panel loaded twice answers one booking once.");
        AssertNoConsoleErrors();
    }

    // The planner's own markup, as a family coordinator sees it.
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

        internal ILocator NoteUnder(string label) => ItemOf(label).Locator(".vp-note");

        internal ILocator WaitlistUnder(string label) => ItemOf(label).Locator(".vp-waitlist");

        // A filter's inner locator is matched inside each item, so it starts from the page, not the root.
        private ILocator ItemOf(string label) =>
            _root.Locator("li.vp-item")
                .Filter(new() { Has = _root.Page.GetByRole(AriaRole.Button, new() { Name = label, Exact = true }) });
    }
}
