namespace Alis.Reactive.Poc.PlaywrightTests.Slots;

[TestFixture]
public sealed class WhenACardRequestIsRefused : AppPageTest
{
    [Test]
    public async Task a_request_to_a_url_that_is_no_page_path_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Cards/RelativeUrl");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: A request's URL is a page path, such as /Cards/Card, or an absolute http or https URI. (Parameter 'url')");
    }

    [Test]
    public async Task an_into_written_after_another_step_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Cards/IntoAfterAStep");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: Into is the route of the request it follows: write it in the same sentence, before any other step, as pipeline.Get(url).Into(id).");
    }

    [Test]
    public async Task a_second_into_of_one_request_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Cards/IntoTwice");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A request loads one slot: write one Into.");
    }
}
