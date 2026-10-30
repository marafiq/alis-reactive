namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As the front desk, I want the resident roster listed by name and narrowed to the active residents for the
/// day's sign-in sheet without waiting on the server again, so that the sheet is ready while the family is
/// still at the desk.
/// </summary>
/// <remarks>
/// The server sends Bo, Di, Ada, Ed and Cy in that order; the page sorts them by name before the grid shows
/// them. Narrowing reads the rows the grid holds (its <c>dataSource</c>) and keeps the active ones, with no
/// request. Not exercised here: the sort after narrowing, because the rows the grid holds are already in name
/// order; Refresh, because writing the grid's data source redraws it anyway (WhenUsingFusionGridBilling's
/// raising_a_selected_residents_rate_five_percent_shows_the_new_rate fails when Refresh breaks).
/// </remarks>
[TestFixture]
public class WhenBindingArrayToGrid : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/ArrayGrid";

    private ILocator Rows => Page.Locator("#roster-grid .e-row");
    private ILocator NameIn(int row) => Rows.Nth(row).Locator("td").First;
    private ILocator Showing => Page.Locator("#roster-showing");
    private ILocator ShowActiveOnly => Page.Locator("#show-active-btn");

    private async Task OpenTheRoster()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Rows).ToHaveCountAsync(5, new() { Timeout = 10000 });
    }

    [Test]
    public async Task the_roster_lists_every_resident_by_name()
    {
        await OpenTheRoster();

        await Expect(NameIn(0)).ToHaveTextAsync("Ada");
        await Expect(NameIn(1)).ToHaveTextAsync("Bo");
        await Expect(NameIn(2)).ToHaveTextAsync("Cy");
        await Expect(NameIn(3)).ToHaveTextAsync("Di");
        await Expect(NameIn(4)).ToHaveTextAsync("Ed");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task showing_active_only_narrows_the_roster_without_asking_the_server_again()
    {
        await OpenTheRoster();

        var rosterRequests = 0;
        void CountRosterRequests(object? _, Microsoft.Playwright.IRequest request)
        {
            if (request.Url.Contains("/Sandbox/Components/ArrayGrid/Residents"))
                rosterRequests++;
        }

        Page.Request += CountRosterRequests;
        try
        {
            await ClickWhenStable(ShowActiveOnly);

            await Expect(Rows).ToHaveCountAsync(3, new() { Timeout = 10000 });
            await Expect(NameIn(0)).ToHaveTextAsync("Ada");
            await Expect(NameIn(1)).ToHaveTextAsync("Cy");
            await Expect(NameIn(2)).ToHaveTextAsync("Ed");
            await Expect(Page.Locator("#roster-grid")).Not.ToContainTextAsync("discharged");
            await Expect(Page.Locator("#roster-grid")).Not.ToContainTextAsync("critical");
            await Expect(Showing).ToHaveTextAsync("Active residents, by name.");
        }
        finally
        {
            Page.Request -= CountRosterRequests;
        }

        Assert.That(rosterRequests, Is.EqualTo(0), "Narrowing reads the rows on screen; it does not fetch the roster again.");
        AssertNoConsoleErrors();
    }
}
