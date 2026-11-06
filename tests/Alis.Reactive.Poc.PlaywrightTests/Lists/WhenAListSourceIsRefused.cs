namespace Alis.Reactive.Poc.PlaywrightTests.Lists;

[TestFixture]
public sealed class WhenAListSourceIsRefused : AppPageTest
{
    [Test]
    public async Task a_source_the_framework_did_not_make_is_refused_with_what_a_source_is()
    {
        await Page.GotoAsync(BaseUrl + "/Stations/HandWrittenSource");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: A source is one the framework makes: a member's read, a list's operation or a value read from data. (Parameter 'source')");
    }
}
