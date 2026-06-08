namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridExport : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/RosterExport";

    private async Task NavigateExport()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#export-status"))
            .ToHaveTextAsync("loaded", new() { Timeout = 10000 });
        await Expect(Page.Locator("#roster-export-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task exporting_the_roster_downloads_csv_excel_and_pdf_files()
    {
        await NavigateExport();

        var csv = await Page.RunAndWaitForDownloadAsync(async () =>
            await ClickWhenStable(Page.Locator("#export-csv")));
        Assert.That(csv.SuggestedFilename, Does.EndWith(".csv"));

        var excel = await Page.RunAndWaitForDownloadAsync(async () =>
            await ClickWhenStable(Page.Locator("#export-excel")));
        Assert.That(excel.SuggestedFilename, Does.EndWith(".xlsx"));

        var pdf = await Page.RunAndWaitForDownloadAsync(async () =>
            await ClickWhenStable(Page.Locator("#export-pdf")));
        Assert.That(pdf.SuggestedFilename, Does.EndWith(".pdf"));

        AssertNoConsoleErrors();
    }
}
