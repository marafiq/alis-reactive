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
    public async Task a_url_with_a_fragment_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Cards/FragmentUrl");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: The URL '/Cards/Card?resident=Ada%20Byron#notes' holds a fragment, which a request does not send. (Parameter 'url')");
    }

    [Test]
    public async Task a_placeholder_in_the_url_of_a_request_that_sets_no_members_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Cards/PlaceholderUrl");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: The URL '/Cards/Card/{resident}' has the placeholder resident, which a request that sets no members cannot fill; take the placeholder out of the URL. (Parameter 'url')");
    }

    [Test]
    public async Task a_brace_without_its_pair_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Cards/UnpairedBraceUrl");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: The URL '/Cards/Card/{resident' holds a brace without its pair; a placeholder is a member's name in braces, as {name}. (Parameter 'url')");
    }

    [Test]
    public async Task an_into_written_after_another_step_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Cards/IntoAfterAStep");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A route belongs to the request it follows: write it in the same sentence, before any other step, as pipeline.Get<TRequest>(url, request => ..).OnSuccess<TBody>((reply, pipeline) => ..) or pipeline.Get(url).Into(id).");
    }

    [Test]
    public async Task a_second_into_of_one_request_held_in_a_variable_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Cards/IntoTwice");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A request is written as one chain, each door on the stage the door before it returned, and this stage took a door already: write pipeline.Get<TRequest>(url, request => ..).OnSuccess<TBody>((reply, pipeline) => ..).OnError((failed, pipeline) => ..).");
    }
}
