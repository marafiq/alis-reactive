namespace Alis.Reactive.Framework.PlaywrightTests.Boundaries;

[TestFixture]
public sealed class WhenAPageLoadsTheRuntimeItsOwnWay : AppPageTest
{
    [Test]
    public async Task a_runtime_that_runs_after_the_document_is_parsed_boots_the_page()
    {
        await Open("/Boundaries/Late");

        await Expect(Page.Locator("#desk")).ToHaveTextAsync("open");
    }

    [Test]
    public async Task a_page_with_no_components_or_plugins_of_its_own_boots()
    {
        await Open("/Boundaries/Bare");

        await Expect(Page.Locator("#desk")).ToHaveTextAsync("open");
    }
}
