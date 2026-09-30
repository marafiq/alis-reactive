namespace Alis.Reactive.PlaywrightTests.Components.AppLevel;

/// <summary>
/// As a coordinator, I want the loader to stay up until my care plan is saved, even right after
/// a quick census refresh, so that I never act on a page that is still saving.
/// </summary>
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
        // Page time is paused and moved only by the test, so the census loader's 3 s safety
        // timeout elapses while the save is still in flight, and never on its own.
        await Page.Clock.PauseAtAsync(DateTime.UtcNow.AddSeconds(1));

        await ClickWhenStable(RefreshCensus);
        await Expect(CensusStatus).ToHaveTextAsync("42 residents in today's census.");
        await Expect(Loader).ToBeHiddenAsync();

        await ClickWhenStable(SaveCarePlan);
        await Expect(Loader).ToBeVisibleAsync();
        await Page.Clock.RunForAsync(3500);

        // Hiding sets aria-hidden at once, while the fade keeps the overlay painted for 0.2 s,
        // so the loader's hidden state is read from aria-hidden rather than from paint.
        await Expect(Loader).Not.ToHaveAttributeAsync("aria-hidden", "true");
        await Expect(CarePlanStatus).ToHaveTextAsync("Care plan saved.");
        await Expect(Loader).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }
}
