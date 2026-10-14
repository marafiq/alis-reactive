namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As the evening nurse on East wing, I want what I record for a resident (a task done, a fall) and what
/// their chart now says to show on that resident's row of the rounds board as soon as the server answers,
/// so that the board I walk the wing with is right without reloading it.
/// </summary>
/// <remarks>
/// The board: Martha Collins E-201 (Low, 2 tasks), Joseph Rivera E-203 (Moderate, 3), Ruth Adler E-205
/// (Moderate, 1), Victor Nguyen E-207 (Low, 0) and Lillian Brooks E-209 (High, 4). The nurse picks a resident
/// on the board; the server answers with that resident's key, and the page changes that row by key: one cell
/// (SetCellValue, a number for open tasks and a text for fall risk) or the whole row (SetRowData). Ruth
/// Adler's chart changed this afternoon (moved to E-214, High fall risk, 3 open tasks); nobody else's did.
/// </remarks>
[TestFixture]
public class WhenUsingFusionGridKeyedUpdate : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/KeyedUpdate";
    private const string BoardId = "evening-rounds";

    // The grid numbers its columns from 1; the hidden ID is the first.
    private const int ResidentColumn = 2;
    private const int RoomColumn = 3;
    private const int FallRiskColumn = 4;
    private const int OpenTasksColumn = 5;

    private ILocator Rows => Page.Locator($"#{BoardId} .e-row");
    private ILocator Row(string resident) => Rows.Filter(new() { HasText = resident });
    private ILocator Cell(string resident, int column) => Row(resident).Locator($"td[aria-colindex='{column}']");
    private ILocator MarkTaskDone => Page.Locator("#mark-task-done");
    private ILocator RecordFall => Page.Locator("#record-fall");
    private ILocator RefreshFromChart => Page.Locator("#refresh-from-chart");
    private ILocator Status => Page.Locator("#rounds-status");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Rows).ToHaveCountAsync(5, new() { Timeout = 10000 });
    }

    private async Task Pick(string resident)
    {
        await Cell(resident, ResidentColumn).ClickAsync();
        await Expect(Row(resident)).ToHaveAttributeAsync("aria-selected", "true", new() { Timeout = 10000 });
    }

    [Test]
    public async Task the_board_lists_the_wings_residents_for_the_evening()
    {
        await NavigateAndBoot();

        await Expect(Rows.First).ToContainTextAsync("Martha Collins");
        await Expect(Cell("Joseph Rivera", OpenTasksColumn)).ToHaveTextAsync("3");
        await Expect(Cell("Ruth Adler", RoomColumn)).ToHaveTextAsync("E-205");
        await Expect(Status).ToHaveTextAsync("Pick a resident, then record what you did.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task marking_a_task_done_takes_one_off_that_residents_open_tasks()
    {
        await NavigateAndBoot();
        await Pick("Joseph Rivera");

        await ClickWhenStable(MarkTaskDone);

        await Expect(Status).ToHaveTextAsync("Joseph Rivera has 2 open tasks left.", new() { Timeout = 10000 });
        await Expect(Cell("Joseph Rivera", OpenTasksColumn)).ToHaveTextAsync("2", new() { Timeout = 10000 });
        await Expect(Cell("Martha Collins", OpenTasksColumn)).ToHaveTextAsync("2");
        await Expect(Cell("Lillian Brooks", OpenTasksColumn)).ToHaveTextAsync("4");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_resident_with_no_open_tasks_has_none_to_mark_done()
    {
        await NavigateAndBoot();
        await Pick("Victor Nguyen");

        await ClickWhenStable(MarkTaskDone);

        await Expect(Status).ToHaveTextAsync("Victor Nguyen has no open tasks left.", new() { Timeout = 10000 });
        await Expect(Cell("Victor Nguyen", OpenTasksColumn)).ToHaveTextAsync("0");
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task recording_a_fall_shows_high_fall_risk_on_that_residents_row()
    {
        await NavigateAndBoot();
        await Pick("Martha Collins");

        await ClickWhenStable(RecordFall);

        await Expect(Status).ToHaveTextAsync("Fall recorded for Martha Collins; fall risk is now High.", new() { Timeout = 10000 });
        await Expect(Cell("Martha Collins", FallRiskColumn)).ToHaveTextAsync("High", new() { Timeout = 10000 });
        await Expect(Cell("Victor Nguyen", FallRiskColumn)).ToHaveTextAsync("Low");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task refreshing_from_the_chart_brings_in_the_afternoons_room_move_and_orders()
    {
        await NavigateAndBoot();
        await Pick("Ruth Adler");

        await ClickWhenStable(RefreshFromChart);

        await Expect(Status).ToHaveTextAsync(
            "Ruth Adler's chart: moved to E-214 this afternoon, High fall risk, 3 open tasks.", new() { Timeout = 10000 });
        await Expect(Cell("Ruth Adler", RoomColumn)).ToHaveTextAsync("E-214", new() { Timeout = 10000 });
        await Expect(Cell("Ruth Adler", FallRiskColumn)).ToHaveTextAsync("High");
        await Expect(Cell("Ruth Adler", OpenTasksColumn)).ToHaveTextAsync("3");
        await Expect(Cell("Joseph Rivera", RoomColumn)).ToHaveTextAsync("E-203");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task recording_with_nobody_picked_asks_the_nurse_to_pick_a_resident()
    {
        await NavigateAndBoot();

        await ClickWhenStable(RecordFall);

        await Expect(Status).ToHaveTextAsync("Pick a resident on the board first.", new() { Timeout = 10000 });
        await Expect(Cell("Martha Collins", FallRiskColumn)).ToHaveTextAsync("Low");
        await Expect(Cell("Victor Nguyen", FallRiskColumn)).ToHaveTextAsync("Low");
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }
}
