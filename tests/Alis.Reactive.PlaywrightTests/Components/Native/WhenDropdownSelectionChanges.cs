using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// As the front-desk coordinator, I want to book the community van for a resident's trip, have a nurse
/// aide sent with every medical appointment, and be offered the next free time when the van is already
/// out, so that no resident waits at the door for a van that is not coming.
/// </summary>
/// <remarks>
/// The van is out at 10:00 AM (the dialysis run); 11:00 AM is the next free time. A drop-down raises change
/// as soon as the coordinator picks an option, without leaving it (HTML, the select element's input and
/// change events); a value the page writes raises none. Every member of the drop-down is exercised: its
/// value written (the nurse aide, the next free time) and read (the time the van is out, the booking the
/// server answers), Focus, Change with its value, and its value committed for the trip check the moment
/// the coordinator picks an option.
/// </remarks>
[TestFixture]
public class WhenDropdownSelectionChanges : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/NativeDropDown";
    private const string VanOut = "the server responded with a status of 409";

    private PagePlan<NativeDropDownModel> _plan = null!;

    private NativeDropDownLocator Resident => _plan.NativeDropDown(m => m.ResidentId);
    private NativeDropDownLocator Trip => _plan.NativeDropDown(m => m.Trip);
    private NativeDropDownLocator Departure => _plan.NativeDropDown(m => m.Departure);
    private NativeDropDownLocator Escort => _plan.NativeDropDown(m => m.Escort);
    private ILocator ResidentError => _plan.ErrorFor(m => m.ResidentId);
    private ILocator TripError => _plan.ErrorFor(m => m.Trip);
    private ILocator DepartureError => _plan.ErrorFor(m => m.Departure);
    private ILocator MedicalNote => Page.Locator("#medical-note");
    private ILocator VanBusy => Page.Locator("#van-busy");
    private ILocator BookTheVan => Page.Locator("#book-van-btn");
    private ILocator Status => Page.Locator("#booking-status");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<NativeDropDownModel>.FromPage(Page);
    }

    private async Task Choose(string resident, string trip, string departure)
    {
        await Resident.Select(resident);
        await Trip.Select(trip);
        await Departure.Select(departure);
    }

    // Margaret Hale's medical appointment at 10:00 AM, when the van is out.
    private async Task BookWhileTheVanIsOut()
    {
        await Choose("Margaret Hale", "Medical appointment", "10:00 AM");
        await ClickWhenStable(BookTheVan);
        await Expect(VanBusy).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_medical_appointment_sends_a_nurse_aide_and_asks_for_the_medication_list()
    {
        await NavigateAndBoot();
        await Expect(Escort.Input).ToHaveValueAsync("No escort");

        await Trip.Select("Medical appointment");

        await Expect(Escort.Input).ToHaveValueAsync("Nurse aide");
        await Expect(MedicalNote).ToBeVisibleAsync();
        await Expect(MedicalNote).ToHaveTextAsync("A nurse aide goes with every medical appointment. Bring the resident's medication list and insurance card.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task other_trips_do_not_ask_for_the_medication_list()
    {
        await NavigateAndBoot();
        await Trip.Select("Medical appointment");
        await Expect(MedicalNote).ToBeVisibleAsync();

        await Trip.Select("Shopping");

        await Expect(MedicalNote).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task booking_the_van_confirms_the_resident_the_trip_the_time_and_the_escort()
    {
        await NavigateAndBoot();
        await Choose("Harold Benson", "Shopping", "9:00 AM");

        await ClickWhenStable(BookTheVan);

        await Expect(Status).ToHaveTextAsync("Van booked for Harold Benson: Shopping at 9:00 AM, no escort.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task when_the_van_is_out_the_next_free_time_is_chosen_instead()
    {
        await NavigateAndBoot();
        await Choose("Margaret Hale", "Medical appointment", "10:00 AM");

        await ClickWhenStable(BookTheVan);

        await Expect(VanBusy).ToBeVisibleAsync();
        await Expect(VanBusy).ToHaveTextAsync("The van is out at 10:00 AM. 11:00 AM is free and now chosen; book again to confirm.");
        await Expect(Departure.Input).ToHaveValueAsync("11:00 AM");
        await Expect(Status).ToHaveTextAsync("");
        AssertNoConsoleErrorsExcept(VanOut);
    }

    // The click leaves the cursor on the button; the server's answer moves it to the time to change.
    [Test]
    public async Task when_the_van_is_out_the_cursor_waits_on_the_departure_time()
    {
        await NavigateAndBoot();

        await BookWhileTheVanIsOut();

        await Expect(Departure.Input).ToBeFocusedAsync();
        AssertNoConsoleErrorsExcept(VanOut);
    }

    [Test]
    public async Task booking_again_at_the_free_time_books_the_van()
    {
        await NavigateAndBoot();
        await BookWhileTheVanIsOut();

        await ClickWhenStable(BookTheVan);

        await Expect(Status).ToHaveTextAsync("Van booked for Margaret Hale: Medical appointment at 11:00 AM, with a nurse aide.");
        await Expect(VanBusy).ToBeHiddenAsync();
        AssertNoConsoleErrorsExcept(VanOut);
    }

    // Refused in the browser: a booking sent to the server would answer 400, a console error.
    [Test]
    public async Task booking_with_nothing_chosen_says_what_is_missing()
    {
        await NavigateAndBoot();

        await ClickWhenStable(BookTheVan);

        await Expect(ResidentError).ToHaveTextAsync("Choose the resident.");
        await Expect(TripError).ToHaveTextAsync("Choose what the trip is for.");
        await Expect(DepartureError).ToHaveTextAsync("Choose when the van leaves.");
        await Expect(Status).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_the_resident_clears_its_refusal_without_booking_again()
    {
        await NavigateAndBoot();
        await ClickWhenStable(BookTheVan);
        await Expect(ResidentError).ToHaveTextAsync("Choose the resident.");

        await Resident.Select("Ruth Coleman");

        await Expect(ResidentError).ToBeHiddenAsync();
        await Expect(TripError).ToHaveTextAsync("Choose what the trip is for.");
        AssertNoConsoleErrors();
    }

    // Picking an option commits it without leaving the drop-down, so the trip is checked on the pick.
    [Test]
    public async Task putting_the_trip_back_to_choose_is_refused_at_once()
    {
        await NavigateAndBoot();
        await Trip.Select("Shopping");
        await Expect(TripError).ToBeHiddenAsync();

        await Trip.Select("-- Choose the trip --");

        await Expect(TripError).ToHaveTextAsync("Choose what the trip is for.");
        AssertNoConsoleErrors();
    }
}
