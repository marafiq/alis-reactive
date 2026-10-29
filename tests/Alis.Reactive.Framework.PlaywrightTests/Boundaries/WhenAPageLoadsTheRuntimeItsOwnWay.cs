namespace Alis.Reactive.Framework.PlaywrightTests.Boundaries;

/// <summary>
/// The runtime boots a page however the page loads it (runtime/root.ts): from a deferred script, which runs
/// once the document is parsed, and with no bundle of components or plugins beside it. Each page writes its
/// own HTML; the layout's way, a plain script at the end of the body after the components' bundle, is every
/// other page of the app.
/// </summary>
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
