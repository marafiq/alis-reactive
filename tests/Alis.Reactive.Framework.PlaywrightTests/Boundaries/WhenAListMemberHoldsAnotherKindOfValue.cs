namespace Alis.Reactive.Framework.PlaywrightTests.Boundaries;

/// <summary>
/// An array operation over a member declared a list whose component keeps an object there, which is no
/// list (grammar.md 2.2, toArray): the step fails at the component, so the steps after it do not run and
/// the page goes on. The slate keeps its header as an object; the page's declaration says it is a list of
/// text.
/// </summary>
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
