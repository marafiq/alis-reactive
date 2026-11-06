namespace Alis.Reactive.Poc.PlaywrightTests.Properties;

[TestFixture]
public sealed class WhenAShiftRosterIsChanged : AppPageTest
{
    [Test]
    public async Task a_lead_written_to_a_roster_its_vendor_finds_at_a_nested_path_shows_on_the_roster()
    {
        await Open("/Rosters/Shift");

        await Page.GetByLabel("Shift lead").FillAsync("Amara Osei");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Set lead" }).ClickAsync();

        await Expect(Page.Locator("#night-roster .roster-lead")).ToHaveTextAsync("Amara Osei");
    }

    [Test]
    public async Task a_number_written_to_a_number_property_reaches_the_roster_as_a_number()
    {
        await Open("/Rosters/Shift");
        await Expect(Page.Locator("#night-roster .roster-staffing")).ToHaveTextAsync("Short staffed");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Staff four nurses" }).ClickAsync();

        await Expect(Page.Locator("#night-roster .roster-staffing")).ToHaveTextAsync("Fully staffed");
    }
}
