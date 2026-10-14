namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As a developer looking for a grid behavior, I want the components page to lead to the grid page and the
/// grid page to list each grid journey as a card that opens it, so that every journey can be reached.
/// </summary>
/// <remarks>
/// The grid page carries no grid of its own: each journey is its own page with its own fixture, so no test
/// here opens a journey.
/// </remarks>
[TestFixture]
public class WhenUsingFusionGrid : PlaywrightTestBase
{
    private const string ComponentsPath = "/Sandbox/Components";
    private const string GridPath = "/Sandbox/Components/Grid";

    private ILocator JourneyCard(string label) => Page.GetByRole(AriaRole.Link).Filter(new() { HasText = label });

    [Test]
    public async Task the_components_page_leads_to_the_grid_journeys()
    {
        await NavigateTo(ComponentsPath);

        await ClickWhenStable(Page.Locator("a:has(h3:text-is('Grid'))"));

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "FusionGrid", Level = 1 })).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(JourneyCard("Clinical updates")).ToBeVisibleAsync();
        await Expect(JourneyCard("Wing care reviews")).ToBeVisibleAsync();
        await Expect(JourneyCard("Huddle sheet")).ToBeVisibleAsync();
        await Expect(JourneyCard("Census by age")).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    // The card leads to the census journey's own page; what that page shows is its own fixture's to prove.
    [Test]
    public async Task the_census_by_age_card_leads_to_the_census()
    {
        await NavigateTo(GridPath);

        await Expect(JourneyCard("Census by age")).ToHaveAttributeAsync("href", "/Sandbox/Components/Grid/Census", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }
}
