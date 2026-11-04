namespace Alis.Reactive.Framework.PlaywrightTests.Boundaries;

[TestFixture]
public sealed class WhenAPropertyHoldsAPromise : AppPageTest
{
    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    [Test]
    public async Task a_value_read_from_it_stops_the_rest_of_its_pipeline_and_the_page_goes_on()
    {
        await Open("/Boundaries/Desk");

        await Button("Ask whether the slate is ready").ClickAsync();
        await Expect(Page.Locator("#ready-asked")).ToHaveTextAsync("asked");

        await Button("Read the title").ClickAsync();
        await Expect(Page.Locator("#title")).ToHaveTextAsync("Night");
        await Expect(Page.Locator("#ready")).ToHaveTextAsync("");
        await Expect(Page.Locator("#ready-went-on")).ToHaveTextAsync("");
    }
}
