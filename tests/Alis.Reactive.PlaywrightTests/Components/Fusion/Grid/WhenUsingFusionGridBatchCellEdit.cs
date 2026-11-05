using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridBatchCellEdit : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/BatchCellEdit";
    private const string GridId = "task-count-grid";

    private const int ResidentCell = 1;
    private const int OpenTasksCell = 2;
    private const int MissedDosesCell = 3;
    private const int Amina = 0;
    private const int Henry = 1;

    private ILocator Rows => Page.Locator($"#{GridId} .e-row");
    private ILocator Cell(int row, int cell) => Rows.Nth(row).Locator(".e-rowcell").Nth(cell);
    private ILocator Editor(string field) => Page.Locator($"#{GridId}{field}");
    private ILocator Status => Page.Locator("#count-status");
    private ILocator Unsaved => Page.Locator("#count-unsaved");
    private ILocator Refused => Page.Locator("#count-refused");
    private ILocator Held => Page.Locator("#count-held");
    private static readonly Regex Changed = new("e-updatedtd");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Cell(Amina, ResidentCell)).ToHaveTextAsync("Amina Patel", new() { Timeout = 10000 });
    }

    private async Task TypeCount(int row, int cell, string field, string current, string value)
    {
        await Cell(row, cell).DblClickAsync();
        await Expect(Editor(field)).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Editor(field)).ToHaveValueAsync(current, new() { Timeout = 10000 });
        await Editor(field).FillAsync(value);
    }

    private async Task MoveOn(int row) => await ClickWhenStable(Cell(row, ResidentCell));

    [Test]
    public async Task starting_the_count_opens_the_first_residents_open_tasks()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Page.Locator("#start-count"));

        var editor = Cell(Amina, OpenTasksCell).Locator($"#{GridId}openTasks");
        await Expect(editor).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(editor).ToHaveValueAsync("2", new() { Timeout = 10000 });
        await editor.FillAsync("5");
        await MoveOn(Amina);
        await Expect(Unsaved).ToHaveTextAsync("Not saved yet: Amina Patel, open tasks 2 → 5.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_changed_missed_dose_count_is_noted_until_the_count_is_saved()
    {
        await NavigateAndBoot();

        await TypeCount(Henry, MissedDosesCell, "missedDoses", "1", "2");
        await MoveOn(Henry);

        await Expect(Unsaved).ToHaveTextAsync("Not saved yet: Henry Liu, missed doses 1 → 2.", new() { Timeout = 10000 });
        await Expect(Cell(Henry, MissedDosesCell)).ToHaveTextAsync("2");
        await Expect(Cell(Henry, MissedDosesCell)).ToHaveClassAsync(Changed);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_open_task_count_over_twenty_is_refused_until_it_is_corrected()
    {
        await NavigateAndBoot();
        await TypeCount(Amina, OpenTasksCell, "openTasks", "2", "99");

        await MoveOn(Amina);

        await Expect(Refused).ToHaveTextAsync(
            "Amina Patel: 99 open tasks is more than a shift can hold. The count stays at 2.", new() { Timeout = 10000 });
        await Expect(Editor("openTasks")).ToBeVisibleAsync();
        await Expect(Unsaved).ToBeHiddenAsync();

        await ClickWhenStable(Editor("openTasks"));
        await Editor("openTasks").FillAsync("9");
        await MoveOn(Amina);

        await Expect(Refused).ToBeHiddenAsync(new() { Timeout = 10000 });
        await Expect(Unsaved).ToHaveTextAsync("Not saved yet: Amina Patel, open tasks 2 → 9.", new() { Timeout = 10000 });
        await Expect(Cell(Amina, OpenTasksCell)).ToHaveTextAsync("9");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task checking_the_task_board_brings_in_henrys_newer_count()
    {
        await NavigateAndBoot();
        await Expect(Cell(Henry, OpenTasksCell)).ToHaveTextAsync("3");

        await ClickWhenStable(Page.Locator("#check-task-board"));

        await Expect(Status).ToHaveTextAsync("The task board shows 6 open tasks for Henry Liu.", new() { Timeout = 10000 });
        await Expect(Cell(Henry, OpenTasksCell)).ToHaveTextAsync("6", new() { Timeout = 10000 });
        await Expect(Cell(Henry, OpenTasksCell)).ToHaveClassAsync(Changed);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reviewing_the_changes_counts_the_number_still_being_typed()
    {
        await NavigateAndBoot();
        await TypeCount(Amina, OpenTasksCell, "openTasks", "2", "4");

        await ClickWhenStable(Page.Locator("#review-count"));

        await Expect(Status).ToHaveTextAsync("Changed so far: Amina Patel, open tasks 2 → 4.", new() { Timeout = 10000 });
        await Expect(Cell(Amina, OpenTasksCell)).ToHaveTextAsync("4");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_the_count_sends_it_to_the_server_and_clears_the_unsaved_note()
    {
        await NavigateAndBoot();
        await TypeCount(Amina, OpenTasksCell, "openTasks", "2", "4");
        await MoveOn(Amina);
        await Expect(Unsaved).ToBeVisibleAsync(new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#save-count"));

        await Expect(Status).ToHaveTextAsync("Count saved: Amina Patel, open tasks 2 → 4.", new() { Timeout = 10000 });
        await Expect(Unsaved).ToBeHiddenAsync(new() { Timeout = 10000 });
        await Expect(Cell(Amina, OpenTasksCell)).ToHaveTextAsync("4");
        await Expect(Cell(Amina, OpenTasksCell)).Not.ToHaveClassAsync(Changed, new() { Timeout = 10000 });
        await Expect(Held).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task eight_open_tasks_for_any_resident_hold_the_save_until_the_supervisor_is_paged()
    {
        await NavigateAndBoot();
        await TypeCount(Amina, OpenTasksCell, "openTasks", "2", "4");
        await MoveOn(Amina);
        await TypeCount(Henry, OpenTasksCell, "openTasks", "3", "8");
        await MoveOn(Henry);
        await Expect(Unsaved).ToHaveTextAsync("Not saved yet: Henry Liu, open tasks 3 → 8.", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#save-count"));

        await Expect(Held).ToHaveTextAsync(
            "Henry Liu would have 8 open tasks. Page the night supervisor before saving the count.", new() { Timeout = 10000 });
        await Expect(Status).ToHaveTextAsync("Tonight's count has not been saved.");
        await Expect(Unsaved).ToBeVisibleAsync();
        await Expect(Cell(Henry, OpenTasksCell)).ToHaveClassAsync(Changed);
        await Expect(Cell(Amina, OpenTasksCell)).ToHaveClassAsync(Changed);
        AssertNoConsoleErrors();
    }
}
