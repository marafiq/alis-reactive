namespace Alis.Reactive.Framework.PlaywrightTests.Boundaries;

/// <summary>
/// A member declared sync whose JavaScript returns a promise, read where the plan reads a value (grammar.md
/// 3.3, 3.4): the read fails at the object, so the step before it ran, the steps after it do not, and the
/// page goes on. A promise is never written as a value. The slate's look-up answers a moment later; the
/// page's declaration of it says it answers at once.
/// </summary>
[TestFixture]
public sealed class WhenAMethodDeclaredPlainReturnsAPromise : AppPageTest
{
    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    [Test]
    public async Task a_value_read_from_it_stops_the_rest_of_its_pipeline_and_the_page_goes_on()
    {
        await Open("/Boundaries/Desk");

        await Button("Look Ben up at once").ClickAsync();
        await Expect(Page.Locator("#asked")).ToHaveTextAsync("asked");

        await Button("Read the title").ClickAsync();
        await Expect(Page.Locator("#title")).ToHaveTextAsync("Night");
        await Expect(Page.Locator("#found")).ToHaveTextAsync("");
        await Expect(Page.Locator("#went-on")).ToHaveTextAsync("");
    }
}
