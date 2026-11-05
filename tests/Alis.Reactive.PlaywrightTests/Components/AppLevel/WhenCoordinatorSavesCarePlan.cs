namespace Alis.Reactive.PlaywrightTests.Components.AppLevel;

[TestFixture]
public class WhenCoordinatorSavesCarePlan : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Loader";

    private ILocator Loader => Page.Locator("#alis-loader");
    private ILocator RefreshCensus => Page.Locator("#refresh-census-btn");
    private ILocator SaveCarePlan => Page.Locator("#save-care-plan-btn");
    private ILocator CensusStatus => Page.Locator("#census-status");
    private ILocator CarePlanStatus => Page.Locator("#care-plan-status");

    [Test]
    public async Task the_loader_stays_up_until_the_care_plan_saves_after_a_quick_census_refresh()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Page.Clock.PauseAtAsync(DateTime.UtcNow.AddSeconds(1));

        await ClickWhenStable(RefreshCensus);
        await Expect(CensusStatus).ToHaveTextAsync("42 residents in today's census.");
        await Expect(Loader).ToBeHiddenAsync();

        await ClickWhenStable(SaveCarePlan);
        await Expect(Loader).ToBeVisibleAsync();
        await Page.Clock.RunForAsync(3500);

        await Expect(Loader).Not.ToHaveAttributeAsync("aria-hidden", "true");
        await Expect(CarePlanStatus).ToHaveTextAsync("Care plan saved.");
        await Expect(Loader).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }
}
