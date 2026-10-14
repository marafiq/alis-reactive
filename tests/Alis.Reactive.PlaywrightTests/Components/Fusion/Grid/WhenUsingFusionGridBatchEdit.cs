using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As the day nurse on Birch wing, I want to keep each resident's open tasks current on the task board as I work
/// through the wing, mark a resident's tasks all done in one step, and hand the board over to the evening shift, so
/// that the evening nurse starts from the counts the day actually left.
/// </summary>
/// <remarks>
/// The board loads from the record: Amina Patel (room 101, 2 open tasks) first, then Henry Liu (104, 3), Grace
/// Bennett (108, 1), Walter Grant (112, 0), Edith Hale (115, 4) and Jonah Reed (118, 2). A count is typed into its
/// cell; All tasks done sets the chosen resident's count to none; the handover sends the board's changes to the
/// server, which says what changed against the record and answers with the board as saved.
/// A button outside the grid takes the focus without saving the cell being typed (the grid saves on focus loss only
/// when the focus lands on nothing or on another input, grid.js:5907-5919), so the handover keeps that count only
/// through its own SaveCell.
/// Not exercised:
/// The pending changes' AddedRecords and DeletedRecords: the board adds and removes no residents, so both arrive
/// empty (batch-edit.js:347-372).
/// </remarks>
[TestFixture]
public class WhenUsingFusionGridBatchEdit : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/BatchTaskUpdate";
    private const string BoardId = "day-task-board";

    // The board numbers its columns from 1.
    private const int ResidentColumn = 2;
    private const int OpenTasksColumn = 4;

    private static readonly Regex Changed = new("e-updatedtd");

    private ILocator Rows => Page.Locator($"#{BoardId} .e-row");
    private ILocator Cell(string resident, int column) =>
        Rows.Filter(new() { HasText = resident }).Locator($"td[aria-colindex='{column}']");
    private ILocator OpenTasksEditor => Page.Locator($"#{BoardId}openTasks");
    private ILocator ChangedCells => Page.Locator($"#{BoardId} .e-updatedtd");
    private ILocator StartWithTheFirstResident => Page.Locator("#board-start");
    private ILocator AllTasksDone => Page.Locator("#board-all-done");
    private ILocator HandOver => Page.Locator("#board-hand-over");
    private ILocator ChooseFirst => Page.Locator("#board-choose");
    private ILocator Status => Page.Locator("#board-status");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Rows).ToHaveCountAsync(6, new() { Timeout = 10000 });
    }

    // Opens Amina Patel's open tasks and types over them, leaving the cell open. The grid shows the editor holding
    // the count a moment after the double-click; a fill before then is lost.
    private async Task TypeAminasOpenTasks(string count)
    {
        await Cell("Amina Patel", OpenTasksColumn).DblClickAsync();
        await Expect(OpenTasksEditor).ToHaveValueAsync("2", new() { Timeout = 10000 });
        await OpenTasksEditor.FillAsync(count);
    }

    // Choosing a resident is clicking their name; the board marks the row as chosen.
    private async Task ChooseResident(string resident)
    {
        await Cell(resident, ResidentColumn).ClickAsync();
        await Expect(Rows.Filter(new() { HasText = resident })).ToHaveAttributeAsync("aria-selected", "true", new() { Timeout = 10000 });
    }

    [Test]
    public async Task the_board_opens_with_each_residents_open_tasks_on_record()
    {
        await NavigateAndBoot();

        await Expect(Rows.First).ToContainTextAsync("Amina Patel");
        await Expect(Cell("Amina Patel", OpenTasksColumn)).ToHaveTextAsync("2");
        await Expect(Cell("Edith Hale", OpenTasksColumn)).ToHaveTextAsync("4");
        await Expect(Rows.Last).ToContainTextAsync("Jonah Reed");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task starting_with_the_first_resident_opens_aminas_open_tasks()
    {
        await NavigateAndBoot();

        await ClickWhenStable(StartWithTheFirstResident);

        await Expect(Cell("Amina Patel", OpenTasksColumn).Locator($"#{BoardId}openTasks"))
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(OpenTasksEditor).ToHaveValueAsync("2", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task handing_over_keeps_the_count_still_being_typed()
    {
        await NavigateAndBoot();
        await TypeAminasOpenTasks("4");

        await ClickWhenStable(HandOver);

        await Expect(Status).ToHaveTextAsync(
            "Handed over to the evening shift: Amina Patel, 2 → 4 open tasks.", new() { Timeout = 10000 });
        await Expect(OpenTasksEditor).ToHaveCountAsync(0);
        await Expect(Cell("Amina Patel", OpenTasksColumn)).ToHaveTextAsync("4");
        AssertNoConsoleErrors();
    }

    // The board the server answers replaces the one on screen, so nothing is left marked as changed.
    [Test]
    public async Task a_handed_over_board_shows_the_saved_counts_with_nothing_left_unsaved()
    {
        await NavigateAndBoot();
        await TypeAminasOpenTasks("5");
        await Cell("Henry Liu", ResidentColumn).ClickAsync();
        await Expect(Cell("Amina Patel", OpenTasksColumn)).ToHaveClassAsync(Changed, new() { Timeout = 10000 });

        await ClickWhenStable(HandOver);

        await Expect(Status).ToHaveTextAsync(
            "Handed over to the evening shift: Amina Patel, 2 → 5 open tasks.", new() { Timeout = 10000 });
        await Expect(ChangedCells).ToHaveCountAsync(0, new() { Timeout = 10000 });
        await Expect(Cell("Amina Patel", OpenTasksColumn)).ToHaveTextAsync("5");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task all_tasks_done_sets_the_chosen_residents_open_tasks_to_none()
    {
        await NavigateAndBoot();
        await ChooseResident("Henry Liu");

        await ClickWhenStable(AllTasksDone);

        await Expect(Cell("Henry Liu", OpenTasksColumn)).ToHaveTextAsync("0", new() { Timeout = 10000 });
        await Expect(Cell("Henry Liu", OpenTasksColumn)).ToHaveClassAsync(Changed);
        await Expect(Cell("Amina Patel", OpenTasksColumn)).ToHaveTextAsync("2");
        await Expect(ChangedCells).ToHaveCountAsync(1);
        await Expect(ChooseFirst).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task tasks_marked_done_are_part_of_the_handover()
    {
        await NavigateAndBoot();
        await ChooseResident("Edith Hale");
        await ClickWhenStable(AllTasksDone);
        await Expect(Cell("Edith Hale", OpenTasksColumn)).ToHaveTextAsync("0", new() { Timeout = 10000 });

        await ClickWhenStable(HandOver);

        await Expect(Status).ToHaveTextAsync(
            "Handed over to the evening shift: Edith Hale, 4 → 0 open tasks.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task all_tasks_done_without_a_chosen_resident_asks_for_one_and_changes_nothing()
    {
        await NavigateAndBoot();

        await ClickWhenStable(AllTasksDone);

        await Expect(ChooseFirst).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(ChooseFirst).ToHaveTextAsync("Choose a resident on the board, then mark their tasks done.");
        await Expect(ChangedCells).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task handing_over_an_unchanged_board_says_the_evening_starts_from_the_record()
    {
        await NavigateAndBoot();

        await ClickWhenStable(HandOver);

        await Expect(Status).ToHaveTextAsync(
            "No open tasks changed, so the evening shift starts from the counts on record.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }
}
