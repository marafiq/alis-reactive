namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridCareOps : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/CareOps";
    private const string GridId = "careops-grid";

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

    [Test]
    public async Task per_row_flag_critical_action_link_updates_the_row_and_reinjects_the_list()
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
        await editor.FillAsync("99");

        // Leaving the cell runs EJ2's native cell validation. The rule was generated
        // from ResidentCareItemValidator.ClientRule(r => r.OpenTasks).Range(0, 7, ...) —
        // the same FluentValidation metadata that powers form validation. No second
        // ruleset is authored: the message proves the single source reached the cell.
        await openTasksCells.Nth(1).DblClickAsync();

        await Expect(Page.GetByText("Open tasks must be between 0 and 7."))
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
