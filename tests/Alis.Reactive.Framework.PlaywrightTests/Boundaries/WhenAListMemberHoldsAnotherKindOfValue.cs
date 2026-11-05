namespace Alis.Reactive.Framework.PlaywrightTests.Boundaries;

[TestFixture]
public sealed class WhenAListMemberHoldsAnotherKindOfValue : AppPageTest
{
    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    [Test]
    public async Task an_object_where_a_list_is_declared_stops_the_rest_of_its_pipeline_and_the_page_goes_on()
    {
        await Open("/Boundaries/Desk");

        await Button("Count the headers").ClickAsync();
        await Expect(Page.Locator("#header-asked")).ToHaveTextAsync("asked");

        await Button("Read the title").ClickAsync();
        await Expect(Page.Locator("#title")).ToHaveTextAsync("Night");
        await Expect(Page.Locator("#header-count")).ToHaveTextAsync("");
        await Expect(Page.Locator("#header-went-on")).ToHaveTextAsync("");
    }
}
