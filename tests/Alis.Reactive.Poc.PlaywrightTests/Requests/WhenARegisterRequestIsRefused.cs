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
            "InvalidOperationException: A request's reply is read outside its routes. Read the reply in the route that receives it: Response(response => response.OnSuccess<TBody>((body, pipeline) => ..)).");
    }

    [Test]
    public async Task a_member_set_twice_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/MemberSetTwice");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: The request sets 'Name' twice; set each member once. (Parameter 'member')");
    }

    [Test]
    public async Task a_member_of_an_object_the_request_holds_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/NestedMember");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'r => r.NextOfKin.Name' is not a member of the request itself; a request sets its own members, as r => r.Name. (Parameter 'member')");
    }

    [Test]
    public async Task a_response_written_after_another_step_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/ResponseAfterAStep");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: Response routes the request it follows: write it in the same sentence, before any other step, as pipeline.Get<TRequest>(url, request => ..).Response(response => ..).");
    }

    [Test]
    public async Task a_second_response_of_one_request_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/ResponseTwice");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A request takes one Response: write each route inside it.");
    }

    [Test]
    public async Task a_second_success_route_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/SuccessTwice");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A request has one success route.");
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
