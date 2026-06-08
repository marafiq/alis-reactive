namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridPrintableRoster : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/PrintableRoster";

    [Test]
    public async Task printing_the_roster_opens_the_print_view_with_the_rows()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#printable-roster-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        // grid.Print() opens the browser print view in a popup window populated with the rows.
        var printView = await Page.RunAndWaitForPopupAsync(async () =>
        {
            await ClickWhenStable(Page.Locator("#roster-print"));
        });

        await printView.WaitForLoadStateAsync();
        await Expect(printView.Locator("body")).ToContainTextAsync("Amina Patel", new() { Timeout = 10000 });
        await Expect(printView.Locator("body")).ToContainTextAsync("Memory Care", new() { Timeout = 10000 });

        await Expect(Page.Locator("#print-status")).ToHaveTextAsync("print issued", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
