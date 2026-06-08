namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridBatchEdit : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/BatchTaskUpdate";

    private async Task NavigateBatch()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#batch-status"))
            .ToHaveTextAsync("loaded", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-task-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task editing_updating_and_saving_a_task_cell_commits_the_new_value()
    {
        await NavigateBatch();

        // EditCell opens the cell editor (an input appears in the grid body).
        await ClickWhenStable(Page.Locator("#batch-edit-cell"));
        await Expect(Page.Locator("#batch-status"))
            .ToHaveTextAsync("editCell called", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-task-grid .e-gridcontent input").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        // SaveCell commits the open editor and closes it.
        await ClickWhenStable(Page.Locator("#batch-save-cell"));
        await Expect(Page.Locator("#batch-status"))
            .ToHaveTextAsync("saveCell called", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-task-grid .e-gridcontent input"))
            .ToHaveCountAsync(0, new() { Timeout = 10000 });

        // UpdateCell sets the cell value directly; EJ2 marks the changed cell .e-updatedtd.
        await ClickWhenStable(Page.Locator("#batch-update-cell"));
        await Expect(Page.Locator("#batch-status"))
            .ToHaveTextAsync("updateCell called", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-task-grid .e-updatedtd").First)
            .ToHaveTextAsync("6", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
