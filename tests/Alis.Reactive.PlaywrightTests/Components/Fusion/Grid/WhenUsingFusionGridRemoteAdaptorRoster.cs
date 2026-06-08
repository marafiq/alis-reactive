namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridRemoteAdaptorRoster : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/RemoteAdaptorRoster";

    [Test]
    public async Task data_manager_adaptor_fetches_the_remote_roster_on_load()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);

        // The grid fetches the {result, count} page itself through the DataManager URL adaptor.
        await Expect(Page.Locator("#remote-adaptor-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.Locator("#remote-adaptor-grid")).ToContainTextAsync("Memory Care", new() { Timeout = 10000 });
        // The pager confirms server paging from the remote count.
        await Expect(Page.Locator("#remote-adaptor-grid .e-pager")).ToBeVisibleAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
