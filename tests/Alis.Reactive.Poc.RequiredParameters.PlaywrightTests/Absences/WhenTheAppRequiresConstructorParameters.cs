namespace Alis.Reactive.Poc.RequiredParameters.PlaywrightTests.Absences;

[TestFixture]
public sealed class WhenTheAppRequiresConstructorParameters : AppPageTest
{
    [Test]
    public async Task a_dose_that_lacks_a_constructor_parameter_the_app_requires_stops_the_read_and_the_steps_after_it()
    {
        await Open("/Doses/RequiredParameters");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses with no refills left", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census-status")).ToHaveTextAsync("Counting refills");
        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("unknown");
    }

    [Test]
    public async Task a_dose_that_holds_every_parameter_the_app_requires_reads_the_start_of_a_member_it_lacks()
    {
        await Open("/Doses/RequiredParameters");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Show Digoxin's refills left", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("0");
    }
}
