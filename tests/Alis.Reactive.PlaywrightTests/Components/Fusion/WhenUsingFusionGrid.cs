namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

[TestFixture]
public class WhenUsingFusionGrid : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    [Test]
    public async Task page_loads_with_initial_data()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#load-status"))
            .ToHaveTextAsync("initial data loaded", new() { Timeout = 10000 });

        var gridRows = Page.Locator("#residents-grid .e-row");
        await Expect(gridRows.First).ToBeVisibleAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sorting_a_column_fetches_sorted_data()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#load-status"))
            .ToHaveTextAsync("initial data loaded", new() { Timeout = 10000 });

        // Click Name column header to sort
        var nameHeader = Page.Locator("#residents-grid .e-headercell").Filter(
            new() { HasText = "Name" });
        await Expect(nameHeader).ToBeVisibleAsync(new() { Timeout = 5000 });
        await nameHeader.ClickAsync();

        // dataStateChange fires → POST → data refreshed
        await Expect(Page.Locator("#action-status"))
            .ToHaveTextAsync("data refreshed", new() { Timeout = 10000 });

        // Echo shows sorting action
        await Expect(Page.Locator("#evt-action-type"))
            .ToHaveTextAsync("sorting", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task paging_fetches_next_page_from_server()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#load-status"))
            .ToHaveTextAsync("initial data loaded", new() { Timeout = 10000 });

        // Pager should show (200 items / 10 per page = 20 pages)
        var pager = Page.Locator("#residents-grid .e-pagercontainer");
        await Expect(pager).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Click page 2
        var page2 = Page.Locator("#residents-grid .e-numericitem:has-text('2')");
        await Expect(page2).ToBeVisibleAsync(new() { Timeout = 5000 });
        await page2.ClickAsync();

        // dataStateChange fires → POST with skip=10 → data refreshed
        await Expect(Page.Locator("#action-status"))
            .ToHaveTextAsync("data refreshed", new() { Timeout = 10000 });

        // Echo shows paging action with skip=10
        await Expect(Page.Locator("#evt-skip"))
            .ToHaveTextAsync("10", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
