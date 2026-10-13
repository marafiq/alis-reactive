using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridCareOps : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/CareOps";
    private const string GridId = "careops-grid";
    private const string OpenTasksColumnIndex = "8";

    private ILocator OpenTasksCells => Page.Locator($"#{GridId} td[aria-colindex='{OpenTasksColumnIndex}']");
    private ILocator OpenTasksEditor => Page.Locator("#careops-gridopenTasks");
    private ILocator SaveAllCareEdits => Page.Locator("#careops-save-all");
    private ILocator CareSummary => Page.Locator("#careops-summary");

    private async Task NavigateCareOps()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#careops-status"))
            .ToHaveTextAsync("loaded care census", new() { Timeout = 10000 });
        await Expect(Page.Locator($"#{GridId} .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task care_board_loads_with_chip_filters_and_census()
    {
        await NavigateCareOps();

        await Expect(Page.Locator("#careops-risk-chips")).ToContainTextAsync("Critical", new() { Timeout = 10000 });
        await Expect(Page.Locator($"#{GridId}")).ToContainTextAsync("Amina Patel", new() { Timeout = 10000 });
        await Expect(Page.Locator("#careops-summary")).ToContainTextAsync("whole census", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task critical_chip_fast_filter_shows_only_critical_residents()
    {
        await NavigateCareOps();

        await ClickWhenStable(Page.Locator("#careops-risk-chips").GetByText("Critical", new() { Exact = true }));

        await Expect(Page.Locator("#careops-summary"))
            .ToContainTextAsync("Critical risk", new() { Timeout = 10000 });
        await Expect(Page.Locator($"#{GridId}")).Not.ToContainTextAsync("Moderate", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task editing_nurse_with_the_typed_select_editor_saves_to_the_server()
    {
        await NavigateCareOps();

        var nurseCell = Page.Locator($"#{GridId}")
            .GetByRole(AriaRole.Gridcell).Filter(new() { HasText = "Nora Ellis" }).First;
        await nurseCell.DblClickAsync();

        var editor = Page.Locator($"#{GridId} select[name='primaryNurse']");
        await Expect(editor).ToBeVisibleAsync(new() { Timeout = 10000 });
        await editor.SelectOptionAsync("Night Float Team");

        // Save directly with the editor still OPEN (no intermediate cell click).
        // Save-All must call grid.SaveCell() to commit the open cell
        // into the batch before getBatchChanges — regression guard for the bug
        // where changing a select then clicking Save did not persist.
        await ClickWhenStable(Page.Locator("#careops-save-all"));

        await Expect(Page.Locator("#careops-summary"))
            .ToContainTextAsync("saved care-plan updates", new() { Timeout = 10000 });
        await Expect(Page.Locator($"#{GridId}")).ToContainTextAsync("Night Float Team", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task discharging_selected_resident_through_native_action_link_removes_them()
    {
        await NavigateCareOps();

        await ClickWhenStable(Page.Locator($"#{GridId} .e-row").First.Locator(".e-checkselect").First);
        await ClickWhenStable(Page.GetByRole(AriaRole.Link, new() { Name = "Discharge Selected" }));

        var dialog = Page.Locator("#alisConfirmDialog");
        await Expect(dialog).ToBeVisibleAsync(new() { Timeout = 10000 });
        await dialog.Locator("button.e-primary").ClickAsync();

        await Expect(Page.Locator("#careops-status"))
            .ToHaveTextAsync("resident discharged", new() { Timeout = 10000 });
        await Expect(Page.Locator("#careops-summary"))
            .ToContainTextAsync("discharged", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    // The flag raises one refresh event, and the page has two listeners for it: one reloads the
    // action list, the other the census grid.
    [Test]
    public async Task flagging_a_resident_critical_refreshes_both_the_action_list_and_the_census()
    {
        await NavigateCareOps();
        await Expect(Page.Locator("#careops-action-rows a").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#careops-action-rows")
            .GetByRole(AriaRole.Link, new() { Name = "Flag Critical" }).First);

        await Expect(Page.Locator("#careops-rows-status"))
            .ToContainTextAsync("flagged critical", new() { Timeout = 10000 });
        await Expect(Page.Locator("[data-testid^='careops-action-row-']").First)
            .ToContainTextAsync("Critical risk", new() { Timeout = 10000 });
        await Expect(Page.Locator("#careops-status"))
            .ToHaveTextAsync("census updated", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task per_row_discharge_action_link_confirms_and_removes_that_resident()
    {
        await NavigateCareOps();
        await Expect(Page.Locator("#careops-action-rows a").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        const string firstSeedResident = "Amina Patel";

        await ClickWhenStable(Page.Locator("#careops-action-rows")
            .GetByRole(AriaRole.Link, new() { Name = "Discharge" }).First);

        var dialog = Page.Locator("#alisConfirmDialog");
        await Expect(dialog).ToBeVisibleAsync(new() { Timeout = 10000 });
        await dialog.Locator("button.e-primary").ClickAsync();

        await Expect(Page.Locator("#careops-rows-status"))
            .ToContainTextAsync("discharged", new() { Timeout = 10000 });
        await Expect(Page.Locator("#careops-action-rows"))
            .Not.ToContainTextAsync(firstSeedResident, new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_out_of_range_open_tasks_edit_is_blocked_by_the_generated_care_rule()
    {
        await NavigateCareOps();

        const string openTasksColumnIndex = "8";
        var openTasksCells = Page.Locator($"#{GridId} td[aria-colindex='{openTasksColumnIndex}']");
        await openTasksCells.First.DblClickAsync();

        var editor = Page.Locator("#careops-gridopenTasks");
        await Expect(editor).ToBeVisibleAsync(new() { Timeout = 10000 });
        // The grid focuses its numeric editor a moment after showing it, and that focus rewrites
        // "0.00" to "0". A fill before then focuses the box itself, the rewrite drops the fill's
        // selection, and the digits land after the number. Wait for the plain number first.
        await Expect(editor).ToHaveValueAsync("0", new() { Timeout = 10000 });
        await editor.FillAsync("99");

        // Leaving the cell runs EJ2's native cell validation. The rule was generated
        // from ResidentCareItemValidator's RuleFor(r => r.OpenTasks).InclusiveBetween(0, 7) —
        // the same validator rules that power form validation. No second
        // ruleset is authored: the message proves the single source reached the cell.
        await openTasksCells.Nth(1).DblClickAsync();

        await Expect(Page.GetByText("Open tasks must be between 0 and 7."))
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    // Starts an edit of a resident's open tasks: the grid focuses its numeric editor a moment after
    // showing it and rewrites "6.00" to "6"; a fill before then lands after the number.
    private async Task EditOpenTasks(int row, string current, string value)
    {
        await OpenTasksCells.Nth(row).DblClickAsync();
        await Expect(OpenTasksEditor).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(OpenTasksEditor).ToHaveValueAsync(current, new() { Timeout = 10000 });
        await OpenTasksEditor.FillAsync(value);
    }

    // Only a critical-risk resident must keep an open task. A grid column's rule holds for every row,
    // so that rule is not the cell's: a high-risk resident's tasks can drop to 0 and be saved.
    [Test]
    public async Task a_resident_who_is_not_critical_can_be_left_with_no_open_tasks()
    {
        await NavigateCareOps();
        // The second row is Amina Patel at high risk, with 6 open tasks.
        await EditOpenTasks(1, "6", "0");

        await ClickWhenStable(SaveAllCareEdits);

        await Expect(CareSummary).ToHaveTextAsync("saved care-plan updates for 1 resident(s)", new() { Timeout = 10000 });
        await Expect(OpenTasksCells.Nth(1)).ToHaveTextAsync("0");
        await Expect(Page.GetByText(ResidentCareItemValidator.CriticalNeedsAnOpenTask)).ToHaveCountAsync(0);

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_critical_resident_left_with_no_open_tasks_is_refused_when_the_edits_are_saved()
    {
        await NavigateCareOps();
        // The fourth row is Clara Hoffman at critical risk, with 6 open tasks.
        await EditOpenTasks(3, "6", "0");

        await ClickWhenStable(SaveAllCareEdits);

        // The cell let the edit through; the server checks the critical-risk rule and keeps the census.
        await Expect(CareSummary).ToHaveTextAsync(
            $"Clara Hoffman: {ResidentCareItemValidator.CriticalNeedsAnOpenTask}", new() { Timeout = 10000 });
        await Expect(Page.Locator("#careops-status")).ToHaveTextAsync("care edits refused");

        AssertNoConsoleErrorsExcept("400");
    }
}
