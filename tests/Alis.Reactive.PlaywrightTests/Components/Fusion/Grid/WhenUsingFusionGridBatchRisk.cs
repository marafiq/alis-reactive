namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridBatchRisk : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/BatchRiskReview";

    private async Task NavigateBatchRisk()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#batch-risk-status"))
            .ToHaveTextAsync("loaded", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-risk-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task flagging_a_risk_cell_and_gathering_batch_changes_reports_the_pending_change()
    {
        await NavigateBatchRisk();

        // UpdateCell(string overload) flags the risk in batch mode.
        await ClickWhenStable(Page.Locator("#batch-risk-flag"));
        await Expect(Page.Locator("#batch-risk-status"))
            .ToHaveTextAsync("updateCell string called", new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-risk-grid .e-updatedtd").First)
            .ToHaveTextAsync("Critical", new() { Timeout = 10000 });

        // BatchChanges source gathered to the server reports the pending change.
        await ClickWhenStable(Page.Locator("#batch-risk-gather"));
        await Expect(Page.Locator("#batch-summary"))
            .ToContainTextAsync("changed 1", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
