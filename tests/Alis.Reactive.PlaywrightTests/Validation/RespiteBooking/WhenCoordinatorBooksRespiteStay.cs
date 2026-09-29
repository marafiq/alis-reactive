using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Validation.RespiteBooking;

/// <summary>
/// As an admissions coordinator, I want a blank community, rate, length of stay, or transport
/// answer to mean "not entered" so that a respite stay is never booked at $0 a day, for zero
/// nights, or without transport nobody decided against.
/// </summary>
[TestFixture]
public class WhenCoordinatorBooksRespiteStay : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/RespiteBooking";

    private PagePlan<RespiteBookingModel> _plan = null!;

    private ILocator BookingStatus => Page.Locator("#respite-booking-status");
    private ILocator BookStay => Page.Locator("#book-respite-btn");
    private NativeDropDownLocator Community => _plan.NativeDropDown(m => m.CommunityId);
    private NativeRadioGroupLocator Transport => _plan.NativeRadioGroup(m => m.NeedsTransport);

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<RespiteBookingModel>.FromPage(Page);
    }

    [Test]
    public async Task leaving_the_daily_rate_blank_asks_for_the_rate()
    {
        await NavigateAndBoot();
        await _plan.TextBox(m => m.ResidentName).Fill("Helen Park");
        await Community.Select("Maple Grove");

        await ClickWhenStable(BookStay);

        await Expect(_plan.ErrorFor(m => m.DailyRate)).ToHaveTextAsync("Daily rate is required.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task leaving_the_community_on_its_placeholder_asks_for_a_community()
    {
        await NavigateAndBoot();
        await _plan.TextBox(m => m.ResidentName).Fill("Helen Park");
        await _plan.TextBox(m => m.DailyRate).Fill("185");

        await ClickWhenStable(BookStay);

        await Expect(_plan.ErrorFor(m => m.CommunityId)).ToHaveTextAsync("Choose a community.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_stay_booked_without_expected_nights_is_booked_as_open_ended()
    {
        await NavigateAndBoot();
        await _plan.TextBox(m => m.ResidentName).Fill("Helen Park");
        await Community.Select("Maple Grove");
        await _plan.TextBox(m => m.DailyRate).Fill("185");
        await Transport.Choose("No");

        await ClickWhenStable(BookStay);

        await Expect(BookingStatus).ToHaveTextAsync(
            "Respite stay booked for Helen Park at Maple Grove: $185.00 per day, open-ended, no transport.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_stay_booked_without_a_transport_answer_leaves_transport_to_be_decided()
    {
        await NavigateAndBoot();
        await _plan.TextBox(m => m.ResidentName).Fill("Helen Park");
        await Community.Select("Maple Grove");
        await _plan.TextBox(m => m.DailyRate).Fill("185");
        await _plan.TextBox(m => m.ExpectedNights).Fill("7");

        await ClickWhenStable(BookStay);

        await Expect(BookingStatus).ToHaveTextAsync(
            "Respite stay booked for Helen Park at Maple Grove: $185.00 per day, 7 nights, transport to be decided.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task answering_no_to_transport_books_the_stay_without_transport()
    {
        await NavigateAndBoot();
        await _plan.TextBox(m => m.ResidentName).Fill("Helen Park");
        await Community.Select("Maple Grove");
        await _plan.TextBox(m => m.DailyRate).Fill("185");
        await _plan.TextBox(m => m.ExpectedNights).Fill("7");
        await Transport.Choose("No");

        await ClickWhenStable(BookStay);

        await Expect(BookingStatus).ToHaveTextAsync(
            "Respite stay booked for Helen Park at Maple Grove: $185.00 per day, 7 nights, no transport.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_complete_booking_confirms_the_daily_rate_nights_and_transport()
    {
        await NavigateAndBoot();
        await _plan.TextBox(m => m.ResidentName).Fill("Helen Park");
        await Community.Select("Cedar Ridge");
        await _plan.TextBox(m => m.DailyRate).Fill("212.50");
        await _plan.TextBox(m => m.ExpectedNights).Fill("14");
        await Transport.Choose("Yes");

        await ClickWhenStable(BookStay);

        await Expect(BookingStatus).ToHaveTextAsync(
            "Respite stay booked for Helen Park at Cedar Ridge: $212.50 per day, 14 nights, transport arranged.");
        AssertNoConsoleErrors();
    }
}
