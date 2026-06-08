namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridBatchRosterChange : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/BatchRosterChange";

    private async Task NavigateRoster()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#roster-load-status"))
            .ToHaveTextAsync("loaded roster rows", new() { Timeout = 10000 });
        await Expect(Page.Locator("#resident-roster-batch-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task before_batch_save_reads_added_and_deleted_records_of_a_roster_change()
    {
        await NavigateRoster();

        // Discharge the first resident (Amina Patel) and admit a new one in one batch.
        await ClickWhenStable(Page.Locator("#roster-discharge"));
        await Expect(Page.Locator("#roster-command-status"))
            .ToHaveTextAsync("discharge staged", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#roster-admit"));
        await Expect(Page.Locator("#roster-command-status"))
            .ToHaveTextAsync("admit staged", new() { Timeout = 10000 });

        // Review the batch: beforeBatchSave reads both AddedRecords and DeletedRecords.
        await ClickWhenStable(Page.Locator("#roster-review"));

        await Expect(Page.Locator("#roster-added-resident"))
            .ToHaveTextAsync("Zara Admitted", new() { Timeout = 10000 });
        await Expect(Page.Locator("#roster-deleted-resident"))
            .ToHaveTextAsync("Amina Patel", new() { Timeout = 10000 });
        await Expect(Page.Locator("#roster-review-status"))
            .ToHaveTextAsync("batch reviewed", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
