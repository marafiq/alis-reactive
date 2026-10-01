namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridBatchChangeReview : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/BatchChangeReview";

    [Test]
    public async Task committing_a_batch_binds_the_review_grid_from_the_event_payload()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#batch-review-edit-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        // Edit Amina Patel's open-tasks cell and save it into the batch.
        await ClickWhenStable(Page.Locator("#review-edit-cell"));
        var tasksEditor = Page.Locator("#batch-review-edit-grid input.e-field").First;
        await Expect(tasksEditor).ToBeVisibleAsync(new() { Timeout = 10000 });
        // The grid focuses its numeric editor a moment after showing it, and that focus rewrites
        // "0.00" to "0". A fill before then focuses the box itself, the rewrite drops the fill's
        // selection, and the digits land after the number. Wait for the plain number first.
        await Expect(tasksEditor).ToHaveValueAsync("0", new() { Timeout = 10000 });
        await tasksEditor.FillAsync("5");
        await ClickWhenStable(Page.Locator("#review-save-cell"));

        // Commit: beforeBatchSave binds the review grid from its payload's ChangedRecords array.
        await ClickWhenStable(Page.Locator("#review-commit"));

        await Expect(Page.Locator("#review-bound-status")).ToHaveTextAsync("review bound from event", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-review-changed-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-review-changed-grid")).ToContainTextAsync("Amina Patel", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
