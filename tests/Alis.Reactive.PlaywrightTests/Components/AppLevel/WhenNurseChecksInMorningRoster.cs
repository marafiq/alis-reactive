namespace Alis.Reactive.PlaywrightTests.Components.AppLevel;

[TestFixture]
public class WhenNurseChecksInMorningRoster : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Loader/Roster";

    private SemaphoreSlim _checkInReleased = null!;
    private SemaphoreSlim _rowsReleased = null!;
    private SemaphoreSlim _careSummaryReleased = null!;

    private ILocator CheckInMargaret => Page.Locator("#check-in-margaret-btn");
    private ILocator RefreshRoster => Page.Locator("#refresh-roster-btn");
    private ILocator RosterStatus => Page.Locator("#roster-status");
    private ILocator MargaretsRow => Page.Locator("#roster-resident-1");
    private ILocator CareSummary => Page.Locator("#roster-care-summary");
    private ILocator CareSummaryText => Page.Locator("#roster-care-summary-text");
    private ILocator Loader => Page.Locator("#alis-loader");

    private async Task NavigateAndBoot()
    {
        _checkInReleased = await HoldRequests("**/Sandbox/Components/Loader/Roster/CheckIn/**");
        _rowsReleased = await HoldRequests("**/Sandbox/Components/Loader/Roster/Rows");
        _careSummaryReleased = await HoldRequests("**/Sandbox/Components/Loader/Roster/CareSummary");
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    private async Task<SemaphoreSlim> HoldRequests(string urlGlob)
    {
        var released = new SemaphoreSlim(0);
        await Page.RouteAsync(urlGlob, async route =>
        {
            await released.WaitAsync();
            await route.ContinueAsync();
        });
        return released;
    }

    private async Task ExpectTheLoaderOnThePageAndHidden()
    {
        await Expect(Loader).ToBeAttachedAsync();
        await Expect(Loader).ToBeHiddenAsync();
    }

    [Test]
    public async Task checking_in_redraws_the_roster_over_the_covered_row_and_uncovers_the_page()
    {
        await NavigateAndBoot();

        await ClickWhenStable(CheckInMargaret);
        await Expect(MargaretsRow.Locator("#alis-loader")).ToBeVisibleAsync();
        _checkInReleased.Release();

        await Expect(RosterStatus).ToHaveTextAsync("Margaret Hill checked in.");
        await Expect(MargaretsRow).ToContainTextAsync("Checked in");
        await ExpectTheLoaderOnThePageAndHidden();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task refreshing_redraws_the_roster_then_covers_the_care_summary_while_it_loads()
    {
        await NavigateAndBoot();

        await ClickWhenStable(RefreshRoster);
        await Expect(MargaretsRow.Locator("#alis-loader")).ToBeVisibleAsync();
        _rowsReleased.Release();

        await Expect(CareSummary.Locator("#alis-loader")).ToBeVisibleAsync();
        await Expect(CareSummaryText).ToHaveTextAsync("Refresh the roster to load Margaret Hill's care summary.");
        _careSummaryReleased.Release();

        await Expect(CareSummaryText).ToHaveTextAsync("Margaret Hill: two medications due at noon.");
        await ExpectTheLoaderOnThePageAndHidden();
        AssertNoConsoleErrors();
    }
}
