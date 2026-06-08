using Alis.Reactive.Fusion.Components;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridBatchCellEdit : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/BatchCellEdit";

    private async Task NavigateBatch()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#batch-load-status"))
            .ToHaveTextAsync("loaded batch rows", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-batch-edit-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task batch_cell_save_reads_typed_cell_fields_and_blocks_an_impossible_value()
    {
        // cellSaved has no Cancel; the typed surface excludes raw EJ2 cell metadata.
        Assert.That(
            typeof(FusionGridCellSaveArgs<ResidentDirectoryGridItem, int>).GetProperty("Cell"),
            Is.Null);
        Assert.That(
            typeof(FusionGridCellSaveArgs<ResidentDirectoryGridItem, int>).GetProperty("ColumnObject"),
            Is.Null);
        Assert.That(
            typeof(FusionGridCellSavedArgs<ResidentDirectoryGridItem, int>).GetProperty("Cancel"),
            Is.Null);

        await NavigateBatch();

        // Edit Amina Patel's open-tasks cell to 4 and save it.
        await ClickWhenStable(Page.Locator("#batch-edit-cell"));
        await Expect(Page.Locator("#resident-batch-edit-grid input.e-field").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        await Page.Locator("#resident-batch-edit-grid input.e-field").First.FillAsync("4");
        await ClickWhenStable(Page.Locator("#batch-save-cell"));

        // cellSave reads the typed cell fields before the value commits.
        await Expect(Page.Locator("#batch-cell-save-column")).ToHaveTextAsync("openTasks", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-cell-save-value")).ToHaveTextAsync("4", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-cell-save-previous")).ToHaveTextAsync("0", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-cell-save-resident")).ToHaveTextAsync("Amina Patel", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-cell-save-cancel")).ToHaveTextAsync("false", new() { Timeout = 10000 });
        // cellSaved reads the same typed fields after the value commits.
        await Expect(Page.Locator("#batch-cell-saved-column")).ToHaveTextAsync("openTasks", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-cell-saved-value")).ToHaveTextAsync("4", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-cell-saved-previous")).ToHaveTextAsync("0", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-cell-saved-resident")).ToHaveTextAsync("Amina Patel", new() { Timeout = 10000 });

        // updateCell sets the same cell to 6; the batch grid shows the staged value.
        await ClickWhenStable(Page.Locator("#batch-update-cell"));
        await Expect(Page.Locator("#resident-batch-edit-grid")).ToContainTextAsync("6", new() { Timeout = 10000 });

        // An impossible per-cell value (99) is blocked by the cellSave Cancel mutation:
        // the value never commits and the grid keeps 6.
        await ClickWhenStable(Page.Locator("#batch-edit-cell"));
        await Expect(Page.Locator("#resident-batch-edit-grid input.e-field").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        await Page.Locator("#resident-batch-edit-grid input.e-field").First.FillAsync("99");
        await ClickWhenStable(Page.Locator("#batch-save-cell"));
        await Expect(Page.Locator("#batch-cell-save-value")).ToHaveTextAsync("99", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-cell-save-previous")).ToHaveTextAsync("6", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-cell-save-cancelled")).ToHaveTextAsync("blocked 99", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-batch-edit-grid")).ToContainTextAsync("6", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-batch-edit-grid")).Not.ToContainTextAsync("99", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task before_batch_save_reads_batch_changes_and_blocks_an_oversized_batch()
    {
        Assert.That(
            typeof(FusionGridBeforeBatchSaveArgs<ResidentDirectoryGridItem>).GetProperty("Name"),
            Is.Null);

        await NavigateBatch();

        // Stage a valid change (6 tasks) and commit it: beforeBatchSave reads the batch
        // changes and allows the commit, so actionComplete fires.
        await ClickWhenStable(Page.Locator("#batch-edit-cell"));
        await Expect(Page.Locator("#resident-batch-edit-grid input.e-field").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        await Page.Locator("#resident-batch-edit-grid input.e-field").First.FillAsync("6");
        await ClickWhenStable(Page.Locator("#batch-save-cell"));

        await ClickWhenStable(Page.Locator("#batch-gather-changes"));
        await Expect(Page.Locator("#batch-summary")).ToHaveTextAsync(
            "batch added 0, changed 1, deleted 0",
            new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#batch-end-edit"));
        await Expect(Page.Locator("#batch-before-save-resident")).ToHaveTextAsync("Amina Patel", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-before-save-tasks")).ToHaveTextAsync("6", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-before-save-cancel")).ToHaveTextAsync("false", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-action-complete")).Not.ToHaveTextAsync("waiting", new() { Timeout = 10000 });

        // Stage an oversized change (8 tasks): beforeBatchSave Cancel blocks the whole
        // commit, so actionComplete never reports a fresh requestType.
        await ClickWhenStable(Page.Locator("#batch-edit-cell"));
        await Expect(Page.Locator("#resident-batch-edit-grid input.e-field").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        await Page.Locator("#resident-batch-edit-grid input.e-field").First.FillAsync("8");
        await ClickWhenStable(Page.Locator("#batch-save-cell"));

        await ClickWhenStable(Page.Locator("#batch-end-edit"));
        await Expect(Page.Locator("#batch-before-save-tasks")).ToHaveTextAsync("8", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-before-save-cancelled")).ToHaveTextAsync("blocked batch 8", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-action-complete")).ToHaveTextAsync("waiting after cancelled batch", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
