using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.DateRangePicker;

[TestFixture]
public class WhenDateRangeSelected : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/DateRangePicker";

    private const string FamilysWeek =
        "Thursday, December 10 to Thursday, December 17: 7 nights at $245 a night, $1,715.";

    private PagePlan<DateRangePickerModel> _plan = null!;

    private DateRangePickerLocator Stay => _plan.DateRangePicker(m => m.StayPeriod);
    private ILocator StayMessage => _plan.ErrorFor(m => m.StayPeriod);
    private ILocator StaySource => Page.Locator("#stay-source");
    private ILocator StayLength => Page.Locator("#stay-length");
    private ILocator StayQuote => Page.Locator("#stay-quote");
    private ILocator LongStayNote => Page.Locator("#long-stay-note");
    private ILocator UseFamilysDates => Page.Locator("#use-family-dates-btn");
    private ILocator ArriveWhenFree => Page.Locator("#arrive-when-free-btn");
    private ILocator StayUntilBooked => Page.Locator("#stay-until-booked-btn");
    private ILocator ReserveStay => Page.Locator("#reserve-stay-btn");
    private ILocator ReservationStatus => Page.Locator("#stay-reservation-status");

    public override BrowserNewContextOptions ContextOptions()
    {
        var options = base.ContextOptions() ?? new BrowserNewContextOptions();
        options.TimezoneId = "Asia/Karachi";
        return options;
    }

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<DateRangePickerModel>.FromPage(Page);
        await Expect(Stay.RangeIcon).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    private async Task PickTheFamilysWeek()
    {
        await Stay.SelectRange(2026, 12, 10, 2026, 12, 17);
        await Expect(StayQuote).ToHaveTextAsync(FamilysWeek);
    }

    [Test]
    public async Task picking_the_stay_dates_shows_its_length_and_price()
    {
        await NavigateAndBoot();

        await Stay.SelectRange(2026, 12, 10, 2026, 12, 17);

        await Expect(StayQuote).ToHaveTextAsync(FamilysWeek);
        await Expect(StayLength).ToHaveTextAsync("8");
        await Expect(StaySource).ToHaveTextAsync("Dates you picked.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task taking_the_familys_dates_fills_them_in_to_read_back_and_prices_them()
    {
        await NavigateAndBoot();

        await ClickWhenStable(UseFamilysDates);

        await Expect(Stay.Input).ToHaveValueAsync("12/10/2026 - 12/17/2026");
        await Expect(StaySource).ToHaveTextAsync("Dates filled in for you. Read them back to the family.");
        await Expect(StayQuote).ToHaveTextAsync(FamilysWeek);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task arriving_when_the_suite_frees_up_moves_only_the_arrival()
    {
        await NavigateAndBoot();
        await PickTheFamilysWeek();

        await ClickWhenStable(ArriveWhenFree);

        await Expect(Stay.Input).ToHaveValueAsync("12/7/2026 - 12/17/2026");
        await Expect(StayQuote).ToHaveTextAsync("Monday, December 7 to Thursday, December 17: 10 nights at $245 a night, $2,450.");
        await Expect(StayLength).ToHaveTextAsync("11");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task staying_until_the_suite_is_next_booked_moves_only_the_departure()
    {
        await NavigateAndBoot();
        await PickTheFamilysWeek();

        await ClickWhenStable(StayUntilBooked);

        await Expect(Stay.Input).ToHaveValueAsync("12/10/2026 - 12/20/2026");
        await Expect(StayQuote).ToHaveTextAsync("Thursday, December 10 to Sunday, December 20: 10 nights at $245 a night, $2,450.");
        await Expect(StayLength).ToHaveTextAsync("11");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_the_stay_dates_takes_the_price_away()
    {
        await NavigateAndBoot();
        await PickTheFamilysWeek();

        await Stay.Clear();
        await Stay.Blur();

        await Expect(StayQuote).ToHaveTextAsync("Pick the stay dates to see the price.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reserving_the_stay_books_the_days_picked()
    {
        await NavigateAndBoot();
        await PickTheFamilysWeek();

        await ClickWhenStable(ReserveStay);

        await Expect(ReservationStatus).ToHaveTextAsync(
            "Eleanor Voss is booked into Garden Suite 3 from Thursday, December 10 to Thursday, December 17, 2026: 7 nights at $245 a night, $1,715.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reserving_without_dates_asks_for_them_until_they_are_picked()
    {
        await NavigateAndBoot();
        await ClickWhenStable(ReserveStay);
        await Expect(StayMessage).ToHaveTextAsync("Choose the stay dates.");

        await Stay.SelectRange(2026, 12, 10, 2026, 12, 17);

        await Expect(StayMessage).ToBeHiddenAsync();
        await Expect(Stay.Input).ToBeFocusedAsync();
        await Expect(ReservationStatus).ToHaveTextAsync("No stay reserved yet.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_stay_over_thirty_days_is_flagged_as_an_admission_and_not_reserved()
    {
        await NavigateAndBoot();
        await Stay.SelectRange(2026, 12, 1, 2026, 12, 31);
        await Expect(StayLength).ToHaveTextAsync("31");
        await Expect(LongStayNote).ToBeVisibleAsync();

        await ClickWhenStable(ReserveStay);

        await Expect(StayMessage).ToHaveTextAsync("A respite stay covers at most 30 days. Book a longer stay as an admission.");
        await Expect(ReservationStatus).ToHaveTextAsync("No stay reserved yet.");
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task shortening_a_long_stay_takes_the_admission_note_away()
    {
        await NavigateAndBoot();
        await Stay.SelectRange(2026, 12, 1, 2026, 12, 31);
        await Expect(LongStayNote).ToBeVisibleAsync();

        await Stay.SelectRange(2026, 12, 10, 2026, 12, 17);

        await Expect(LongStayNote).ToBeHiddenAsync();
        await Expect(StayLength).ToHaveTextAsync("8");
        AssertNoConsoleErrors();
    }
}
