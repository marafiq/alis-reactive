namespace Alis.Reactive.Poc.PlaywrightTests.Requests;

[TestFixture]
public sealed class WhenARegisterRequestIsRefused : AppPageTest
{
    [Test]
    public async Task a_placeholder_no_member_of_the_request_fills_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/PlaceholderNoMemberFills");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: The URL '/Residents/Summary/{resident}' has the placeholder resident, and the request has no member named resident; add the member, or take the placeholder out of the URL. (Parameter 'url')");
    }

    [Test]
    public async Task a_brace_without_its_pair_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/UnpairedBrace");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: The URL '/Residents/Summary/{name' holds a brace without its pair; a placeholder is a member's name in braces, as {name}. (Parameter 'url')");
    }

    [Test]
    public async Task a_url_with_a_fragment_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/Fragment");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: The URL '/Residents/Summary/{name}#medicines' holds a fragment, which a request does not send. (Parameter 'url')");
    }

    [Test]
    public async Task a_reply_read_after_its_request_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/ReplyAfterItsRequest");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A request's reply is read outside its routes. Read the reply in the route that receives it: pipeline.Get<TRequest>(url, request => ..).OnSuccess<TBody>((reply, pipeline) => ..).");
    }

    [Test]
    public async Task a_member_set_twice_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/MemberSetTwice");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: The request sets 'Name' twice; set each member once. (Parameter 'member')");
    }

    [Test]
    public async Task a_path_value_naming_no_placeholder_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/PathWithNoPlaceholder");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: The URL '/Residents/Summary/{name}' has no placeholder Ward; add {Ward} to the URL, or send Ward with Query. (Parameter 'member')");
    }

    [Test]
    public async Task a_member_of_an_object_in_the_path_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/PathOfANestedMember");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'r => r.NextOfKin.Name' fills a placeholder, which only a member of the request itself fills, as r => r.Name; send NextOfKin.Name with Query. (Parameter 'member')");
    }

    [Test]
    public async Task a_placeholder_sent_in_the_query_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/QueryOfAPlaceholder");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: The URL '/Residents/Summary/{name}' has the placeholder name, which Query does not fill; fill it with Path(r => r.Name, value). (Parameter 'url')");
    }

    [Test]
    public async Task a_member_lambda_that_reads_past_the_members_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/MemberNotAPath");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'r => r.Ward.Trim()' is not a member path of the request; a request sends its members, as r => r.Name or r => r.NextOfKin.Name. (Parameter 'member')");
    }

    [Test]
    public async Task a_route_written_after_another_step_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/RouteAfterAStep");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A route belongs to the request it follows: write it in the same sentence, before any other step, as pipeline.Get<TRequest>(url, request => ..).OnSuccess<TBody>((reply, pipeline) => ..) or pipeline.Get(url).Into(id).");
    }

    [Test]
    public async Task a_request_held_in_a_variable_and_routed_twice_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/RequestWrittenTwice");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A request is written as one chain, each door on the stage the door before it returned, and this stage took a door already: write pipeline.Get<TRequest>(url, request => ..).OnSuccess<TBody>((reply, pipeline) => ..).OnError((failed, pipeline) => ..).");
    }

    [Test]
    public async Task a_routed_request_held_in_a_variable_and_routed_twice_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/RoutedRequestWrittenTwice");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A request is written as one chain, each door on the stage the door before it returned, and this stage took a door already: write pipeline.Get<TRequest>(url, request => ..).OnSuccess<TBody>((reply, pipeline) => ..).OnError((failed, pipeline) => ..).");
    }

    [Test]
    public async Task a_second_route_for_one_status_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/StatusTwice");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A request has one route for the status 404.");
    }

    [Test]
    public async Task an_error_route_for_a_success_status_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/StatusNotAnError");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentOutOfRangeException: An error route is for a status from 300 to 599, not 204. (Parameter 'status')");
    }

    [Test]
    public async Task a_second_route_for_every_other_outcome_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/CatchAllTwice");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A request has one route for every other outcome.");
    }
}
