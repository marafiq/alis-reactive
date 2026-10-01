using System.Text.RegularExpressions;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.HttpPipeline.OutingRoster;

/// <summary>
/// As an activities coordinator, I want to find the residents who use the mobility aids I choose and
/// book the bus, so that everyone is found when I choose none and boarding help is booked only for the
/// aids I choose.
/// </summary>
/// <remarks>
/// The search sends the chosen aids in a GET query; the booking posts them as form data. Helen Park
/// uses a walker, Arthur Bell a wheelchair, Frank Moore a cane; Rosa Diaz uses none, and nobody uses a
/// mobility scooter. The bus has two boarding-help spaces.
/// </remarks>
[TestFixture]
public class WhenCoordinatorPlansGardenOuting : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/OutingRoster";

    private PagePlan<OutingRosterModel> _plan = null!;

    private MultiSelectLocator MobilityAids => _plan.MultiSelect(m => m.MobilityAids);
    private ILocator FindResidents => Page.Locator("#find-residents-btn");
    private ILocator BookBus => Page.Locator("#book-bus-btn");
    private ILocator RosterResidents => Page.Locator("#roster-residents");
    private ILocator BusBooking => Page.Locator("#bus-booking");

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<OutingRosterModel>.FromPage(Page);
    }

    [Test]
    public async Task finding_residents_with_no_mobility_aid_chosen_lists_every_resident()
    {
        await NavigateAndBoot();

        await ClickWhenStable(FindResidents);

        await Expect(RosterResidents).ToHaveTextAsync("Helen Park, Arthur Bell, Rosa Diaz, Frank Moore");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task booking_the_bus_with_no_mobility_aid_chosen_books_no_boarding_help()
    {
        await NavigateAndBoot();

        await ClickWhenStable(BookBus);

        await Expect(BusBooking).ToHaveTextAsync("Bus booked. No boarding help needed.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task finding_residents_who_use_a_wheelchair_lists_only_them()
    {
        await NavigateAndBoot();
        await MobilityAids.SelectItem("Wheelchair");

        await ClickWhenStable(FindResidents);

        await Expect(RosterResidents).ToHaveTextAsync("Arthur Bell");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task finding_residents_who_use_walkers_or_wheelchairs_lists_both()
    {
        await NavigateAndBoot();
        await MobilityAids.SelectItems("Walker", "Wheelchair");

        await ClickWhenStable(FindResidents);

        await Expect(RosterResidents).ToHaveTextAsync("Helen Park, Arthur Bell");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_the_chosen_aid_and_searching_again_lists_every_resident()
    {
        await NavigateAndBoot();
        await MobilityAids.SelectItem("Wheelchair");
        await ClickWhenStable(FindResidents);
        await Expect(RosterResidents).ToHaveTextAsync("Arthur Bell");

        await MobilityAids.RemoveItem("Wheelchair");
        await ClickWhenStable(FindResidents);

        await Expect(RosterResidents).ToHaveTextAsync("Helen Park, Arthur Bell, Rosa Diaz, Frank Moore");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task finding_residents_who_use_a_mobility_scooter_tells_me_no_resident_uses_it()
    {
        await NavigateAndBoot();
        await MobilityAids.SelectItem("Mobility scooter");

        await ClickWhenStable(FindResidents);

        await Expect(RosterResidents).ToHaveTextAsync("No resident uses the chosen aids.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task booking_the_bus_after_clearing_the_chosen_aid_books_no_boarding_help()
    {
        await NavigateAndBoot();
        await MobilityAids.SelectItem("Wheelchair");
        await MobilityAids.RemoveItem("Wheelchair");

        await ClickWhenStable(BookBus);

        await Expect(BusBooking).ToHaveTextAsync("Bus booked. No boarding help needed.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task booking_the_bus_for_walkers_and_wheelchairs_books_boarding_help_for_both()
    {
        await NavigateAndBoot();
        await MobilityAids.SelectItems("Walker", "Wheelchair");

        await ClickWhenStable(BookBus);

        await Expect(BusBooking).ToHaveTextAsync("Bus booked with boarding help for: Walker, Wheelchair.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task booking_boarding_help_for_more_aids_than_the_bus_holds_tells_me_it_cannot_be_booked()
    {
        await NavigateAndBoot();
        await MobilityAids.SelectItems("Walker", "Wheelchair", "Cane");

        await ClickWhenStable(BookBus);

        await Expect(BusBooking).ToHaveTextAsync("The bus has two boarding-help spaces: choose at most two aids.");
        await Expect(BusBooking).ToHaveClassAsync(new Regex("text-red-600"));
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task dropping_an_aid_after_the_bus_refuses_books_the_bus()
    {
        await NavigateAndBoot();
        await MobilityAids.SelectItems("Walker", "Wheelchair", "Cane");
        await ClickWhenStable(BookBus);
        await Expect(BusBooking).ToHaveTextAsync("The bus has two boarding-help spaces: choose at most two aids.");

        await MobilityAids.RemoveItem("Cane");
        await ClickWhenStable(BookBus);

        await Expect(BusBooking).ToHaveTextAsync("Bus booked with boarding help for: Walker, Wheelchair.");
        await Expect(BusBooking).Not.ToHaveClassAsync(new Regex("text-red-600"));
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }
}
