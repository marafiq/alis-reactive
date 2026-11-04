namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

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

    [Test]
    public async Task the_census_by_age_card_leads_to_the_census()
    {
        await NavigateTo(GridPath);

        await Expect(JourneyCard("Census by age")).ToHaveAttributeAsync("href", "/Sandbox/Components/Grid/Census", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }
}
