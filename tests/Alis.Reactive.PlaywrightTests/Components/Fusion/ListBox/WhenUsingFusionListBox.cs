using System.Text.RegularExpressions;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.ListBox;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.ListBox;

/// <summary>
/// As the activities coordinator, I want to choose who rides the morning bus to Thursday's garden outing, with
/// everyone else on the afternoon bus, so that the morning bus is not overfilled, the residents in wheelchairs
/// get the lift, and nobody misses a morning appointment for the trip.
/// </summary>
/// <remarks>
/// Eight residents signed up; the list's selection is the morning bus, which seats six. Walter Dean and Margaret
/// Hale ride in wheelchairs and the lift is on the morning bus only. Ruth Coleman and Dorothy Kim have
/// appointments that morning. Last Thursday Arthur Bell, Grace Liu and Samuel Ortiz rode in the morning.
/// Last week's groups are applied with DataBind before the lift riders are selected: without it the list still
/// shows the old selection when SelectItems reads the selection back from the list (list-box.js:891-911,
/// 2225-2246), that read-back cancels the pending write (ej2-base base.js:60-75), and only the lift riders end up
/// on the morning bus.
/// Not exercised: SelectItems and EnableItems by text (their third argument false); every call here names the
/// residents by ID, which is what the page holds.
/// </remarks>
[TestFixture]
public class WhenUsingFusionListBox : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionListBox";
    private static readonly Regex Selected = new(@"\be-selected\b");
    private static readonly Regex Disabled = new(@"\be-disabled\b");

    private PagePlan<FusionListBoxModel> _plan = null!;

    private FusionListBoxLocator MorningBus => new(Page, "morning-bus");
    private ILocator SeatsTaken => Page.Locator("#morning-seats-taken");
    private ILocator BusFull => Page.Locator("#morning-bus-full");
    private ILocator LastWeek => Page.Locator("#last-week-btn");
    private ILocator SelectEveryone => Page.Locator("#select-everyone-btn");
    private ILocator ClearMorningBus => Page.Locator("#clear-morning-bus-btn");
    private ILocator AppointmentsNote => Page.Locator("#appointments-note");
    private ILocator SaveGroups => Page.Locator("#save-groups-btn");
    private ILocator GroupsStatus => Page.Locator("#groups-status");
    private NativeCheckBoxLocator AppointmentsRideLater => _plan.NativeCheckBox(m => m.MorningAppointmentsRideLater);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<FusionListBoxModel>.FromPage(Page);
        await Expect(MorningBus.Item("Arthur Bell")).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    [Test]
    public async Task choosing_residents_counts_the_seats_taken_on_the_morning_bus()
    {
        await NavigateAndBoot();

        await MorningBus.ClickItem("Arthur Bell");
        await MorningBus.CtrlClickItem("Walter Dean");

        await Expect(SeatsTaken).ToHaveTextAsync("2", new() { Timeout = 5000 });
        await Expect(MorningBus.Item("Arthur Bell")).ToHaveClassAsync(Selected);
        await Expect(MorningBus.Item("Walter Dean")).ToHaveClassAsync(Selected);
        await Expect(BusFull).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selecting_everyone_warns_that_the_morning_bus_seats_six()
    {
        await NavigateAndBoot();

        await ClickWhenStable(SelectEveryone);

        await Expect(MorningBus.SelectedItems).ToHaveCountAsync(OutingDay.SignedUp.Count, new() { Timeout = 5000 });
        await Expect(SeatsTaken).ToHaveTextAsync("8");
        await Expect(BusFull).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_the_morning_bus_sends_everyone_to_the_afternoon()
    {
        await NavigateAndBoot();
        await MorningBus.ClickItem("Arthur Bell");
        await Expect(SeatsTaken).ToHaveTextAsync("1", new() { Timeout = 5000 });

        await ClickWhenStable(ClearMorningBus);

        await Expect(MorningBus.SelectedItems).ToHaveCountAsync(0, new() { Timeout = 5000 });
        await Expect(SeatsTaken).ToHaveTextAsync("0");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task last_weeks_groups_come_back_with_the_lift_riders_on_the_morning_bus()
    {
        await NavigateAndBoot();

        await ClickWhenStable(LastWeek);

        await Expect(SeatsTaken).ToHaveTextAsync("5", new() { Timeout = 5000 });
        foreach (var rider in new[] { "Arthur Bell", "Grace Liu", "Samuel Ortiz", "Walter Dean", "Margaret Hale" })
            await Expect(MorningBus.Item(rider)).ToHaveClassAsync(Selected);
        await Expect(MorningBus.SelectedItems).ToHaveCountAsync(5);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_resident_with_a_morning_appointment_leaves_the_morning_bus_and_cannot_rejoin_it()
    {
        await NavigateAndBoot();
        await MorningBus.ClickItem("Ruth Coleman");
        await Expect(SeatsTaken).ToHaveTextAsync("1", new() { Timeout = 5000 });

        await AppointmentsRideLater.Check();

        await Expect(MorningBus.Item("Ruth Coleman")).Not.ToHaveClassAsync(Selected, new() { Timeout = 5000 });
        await Expect(MorningBus.Item("Ruth Coleman")).ToHaveClassAsync(Disabled);
        await Expect(MorningBus.Item("Dorothy Kim")).ToHaveClassAsync(Disabled);
        await Expect(MorningBus.Item("Arthur Bell")).Not.ToHaveClassAsync(Disabled);
        await Expect(SeatsTaken).ToHaveTextAsync("0");
        await Expect(AppointmentsNote).ToHaveTextAsync("Ruth Coleman and Dorothy Kim ride in the afternoon.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task lifting_the_appointment_rule_lets_those_residents_ride_in_the_morning_again()
    {
        await NavigateAndBoot();
        await AppointmentsRideLater.Check();
        await Expect(MorningBus.Item("Ruth Coleman")).ToHaveClassAsync(Disabled, new() { Timeout = 5000 });

        await AppointmentsRideLater.Input.UncheckAsync();

        await Expect(MorningBus.Item("Ruth Coleman")).Not.ToHaveClassAsync(Disabled, new() { Timeout = 5000 });
        await MorningBus.ClickItem("Ruth Coleman");
        await Expect(MorningBus.Item("Ruth Coleman")).ToHaveClassAsync(Selected);
        await Expect(SeatsTaken).ToHaveTextAsync("1");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_the_groups_tells_the_office_who_rides_in_the_morning()
    {
        await NavigateAndBoot();
        await MorningBus.ClickItem("Arthur Bell");
        await MorningBus.CtrlClickItem("Walter Dean");
        await Expect(SeatsTaken).ToHaveTextAsync("2", new() { Timeout = 5000 });

        await ClickWhenStable(SaveGroups);

        await Expect(GroupsStatus).ToHaveTextAsync(
            "Saved. Morning bus: Arthur Bell, Walter Dean. The other 6 ride in the afternoon.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_before_anyone_is_chosen_asks_who_rides_in_the_morning()
    {
        await NavigateAndBoot();

        await ClickWhenStable(SaveGroups);

        await Expect(GroupsStatus).ToHaveTextAsync("Choose who rides the morning bus before saving.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
