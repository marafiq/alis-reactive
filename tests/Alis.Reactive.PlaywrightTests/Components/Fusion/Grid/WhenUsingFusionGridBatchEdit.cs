using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridBatchEdit : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/BatchTaskUpdate";
    private const string BoardId = "day-task-board";

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

    private async Task TypeAminasOpenTasks(string count)
    {
        await Cell("Amina Patel", OpenTasksColumn).DblClickAsync();
        await Expect(OpenTasksEditor).ToHaveValueAsync("2", new() { Timeout = 10000 });
        await OpenTasksEditor.FillAsync(count);
    }

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
    public async Task choosing_a_resident_after_being_asked_marks_their_tasks_done_and_takes_the_request_away()
    {
        await NavigateAndBoot();
        await ClickWhenStable(AllTasksDone);
        await Expect(ChooseFirst).ToBeVisibleAsync(new() { Timeout = 10000 });
        await ChooseResident("Henry Liu");

        await ClickWhenStable(AllTasksDone);

        await Expect(Cell("Henry Liu", OpenTasksColumn)).ToHaveTextAsync("0", new() { Timeout = 10000 });
        await Expect(ChooseFirst).ToBeHiddenAsync();
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
