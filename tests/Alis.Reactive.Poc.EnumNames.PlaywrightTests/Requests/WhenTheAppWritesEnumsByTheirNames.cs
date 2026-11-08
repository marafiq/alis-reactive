namespace Alis.Reactive.Poc.EnumNames.PlaywrightTests.Requests;

[TestFixture]
public sealed class WhenTheAppWritesEnumsByTheirNames : AppPageTest
{
    [Test]
    public async Task the_route_for_every_other_outcome_reads_a_network_failure_whatever_the_app_names_its_enums()
    {
        await Open("/Residents/Lookup");
        await Page.GetByLabel("Ward").FillAsync("Basement");

        await Page.GetByLabel("Resident name").FillAsync("Helen Park");
        await Page.GetByLabel("Resident name").PressAsync("Tab");

        await Expect(Page.Locator("#failure")).ToHaveTextAsync("The network failed");
    }
}
