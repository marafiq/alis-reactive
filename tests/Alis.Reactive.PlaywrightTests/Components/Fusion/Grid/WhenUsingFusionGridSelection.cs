namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As the activities director, I want to seat residents on Thursday's garden outing from the list of those
/// who asked to go, start over when the plan changes, and book the bus with the page naming who goes, so
/// that the bus leaves with the residents who were promised a seat and no more than it holds.
/// </summary>
/// <remarks>
/// Six residents asked to go, in this order: Helen Park, Arthur Bell, Walter Kim, Irene Morgan, Rosa Diaz and
/// Grace Bennett. The bus seats four. Rosa Diaz was turned away in September, so the director may start the
/// bus with her. Booking sends the seated residents to the server, which names them or refuses the booking.
/// Selection is the grid's own: a seated resident's row is highlighted (aria-selected).
/// Not exercised: the RowSelected event and GetSelectedRowIndexes, which this page does not read
/// (WhenUsingFusionGridDirectory.picking_a_resident_opens_their_card_without_filtering_the_directory and
/// next_resident_opens_the_card_of_the_resident_below prove them).
/// </remarks>
[TestFixture]
public class WhenUsingFusionGridSelection : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/RosterSelection";
    private const string SignUpsId = "outing-sign-ups";

    // The grid numbers its columns from 1; the hidden ID is the first.
    private const int ResidentColumn = 2;

    private ILocator Rows => Page.Locator($"#{SignUpsId} .e-row");
    private ILocator Row(string resident) => Rows.Filter(new() { HasText = resident });
    private ILocator SeatedResidents => Page.Locator($"#{SignUpsId} .e-row[aria-selected='true'] td[aria-colindex='{ResidentColumn}']");
    private ILocator SeatFirstFour => Page.Locator("#seat-first-signed-up");
    private ILocator StartWithRosa => Page.Locator("#seat-turned-away-first");
    private ILocator ClearTheBus => Page.Locator("#clear-the-bus");
    private ILocator BookTheBus => Page.Locator("#book-the-bus");
    private ILocator Status => Page.Locator("#outing-status");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Rows).ToHaveCountAsync(6, new() { Timeout = 10000 });
    }

    private async Task SeatTheFirstFour()
    {
        await ClickWhenStable(SeatFirstFour);
        await Expect(SeatedResidents).ToHaveCountAsync(4, new() { Timeout = 10000 });
    }

    // Ctrl-click adds a resident to the residents already seated, or takes a seated one off.
    private async Task CtrlClick(string resident) =>
        await Row(resident).Locator($"td[aria-colindex='{ResidentColumn}']")
            .ClickAsync(new() { Modifiers = new[] { KeyboardModifier.Control } });

    [Test]
    public async Task the_sign_up_sheet_lists_the_residents_in_the_order_they_signed_up()
    {
        await NavigateAndBoot();

        await Expect(Rows.First).ToContainTextAsync("Helen Park");
        await Expect(Rows.Nth(4)).ToContainTextAsync("Rosa Diaz");
        await Expect(Rows.Last).ToContainTextAsync("Grace Bennett");
        await Expect(SeatedResidents).ToHaveCountAsync(0);
        await Expect(Status).ToHaveTextAsync("The bus is not booked yet.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task seating_the_first_four_to_sign_up_highlights_them_on_the_list()
    {
        await NavigateAndBoot();

        await ClickWhenStable(SeatFirstFour);

        await Expect(SeatedResidents).ToHaveTextAsync(
            new[] { "Helen Park", "Arthur Bell", "Walter Kim", "Irene Morgan" }, new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task booking_the_bus_names_the_seated_residents_and_how_many_go()
    {
        await NavigateAndBoot();
        await SeatTheFirstFour();

        await ClickWhenStable(BookTheBus);

        await Expect(Status).ToHaveTextAsync(
            "Booked 4 residents for Thursday's garden outing: Helen Park, Arthur Bell, Walter Kim and Irene Morgan.",
            new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    // Starting with Rosa Diaz starts the bus over: the four seated before her are off it.
    [Test]
    public async Task starting_with_the_resident_turned_away_last_time_seats_only_her()
    {
        await NavigateAndBoot();
        await SeatTheFirstFour();

        await ClickWhenStable(StartWithRosa);

        await Expect(SeatedResidents).ToHaveTextAsync(new[] { "Rosa Diaz" }, new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_the_bus_leaves_nobody_to_book()
    {
        await NavigateAndBoot();
        await SeatTheFirstFour();

        await ClickWhenStable(ClearTheBus);

        await Expect(SeatedResidents).ToHaveCountAsync(0, new() { Timeout = 10000 });
        await ClickWhenStable(BookTheBus);
        await Expect(Status).ToHaveTextAsync("Seat at least one resident before booking the bus.", new() { Timeout = 10000 });
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task residents_seated_by_hand_after_clearing_are_the_ones_booked()
    {
        await NavigateAndBoot();
        await SeatTheFirstFour();
        await ClickWhenStable(ClearTheBus);
        await Expect(SeatedResidents).ToHaveCountAsync(0, new() { Timeout = 10000 });

        await CtrlClick("Rosa Diaz");
        await CtrlClick("Grace Bennett");
        await Expect(SeatedResidents).ToHaveTextAsync(new[] { "Rosa Diaz", "Grace Bennett" }, new() { Timeout = 10000 });
        await ClickWhenStable(BookTheBus);

        await Expect(Status).ToHaveTextAsync(
            "Booked 2 residents for Thursday's garden outing: Rosa Diaz and Grace Bennett.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task more_residents_than_the_bus_seats_are_not_booked()
    {
        await NavigateAndBoot();
        await SeatTheFirstFour();
        await CtrlClick("Rosa Diaz");
        await Expect(SeatedResidents).ToHaveCountAsync(5, new() { Timeout = 10000 });

        await ClickWhenStable(BookTheBus);

        await Expect(Status).ToHaveTextAsync("The bus seats 4; take 1 resident off before booking.", new() { Timeout = 10000 });
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }
}
