using System.Text.RegularExpressions;

namespace Alis.Reactive.Poc.PlaywrightTests.Requests;

[TestFixture]
public sealed class WhenARequestHeaderIsRefused : AppPageTest
{
    [Test]
    public async Task a_header_name_and_value_the_model_holds_are_sent_without_the_space_around_the_value()
    {
        await Open("/Residents/HeaderNamed?name=X-Asked-By&value=%20%20the%20night%20nurse%20%20");

        var asked = await Page.RunAndWaitForRequestAsync(() => Page.GetByRole(AriaRole.Button, new() { Name = "Look up Helen Park" }).ClickAsync(), new Regex("/Residents/Summary/"));

        Assert.That(await asked.HeaderValueAsync("X-Asked-By"), Is.EqualTo("the night nurse"));
    }

    [Test]
    public async Task a_header_name_that_is_no_token_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/HeaderNamed?name=X%20Desk&value=north");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'X Desk' is no header name: a header name is letters, digits and !#$%&'*+-.^_`|~, as X-Requested-With. (Parameter 'name')");
    }

    [Test]
    public async Task a_header_sent_twice_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/HeaderNamed?name=x-desk&value=north");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: The request sends the header x-desk twice; send each header once. (Parameter 'name')");
    }

    [Test]
    public async Task content_type_as_a_header_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/HeaderNamed?name=Content-Type&value=text%2Fplain");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: Content-Type is set by the body door, and a request with no body sends none. (Parameter 'name')");
    }

    [Test]
    public async Task accept_as_a_header_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/HeaderNamed?name=accept&value=text%2Fhtml");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: Accept is set by the kind the success route reads, never by a header the view writes. (Parameter 'name')");
    }

    [Test]
    public async Task the_antiforgery_header_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/HeaderNamed?name=RequestVerificationToken&value=token");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: RequestVerificationToken carries the antiforgery token, which the framework sends, never a header the view writes. (Parameter 'name')");
    }

    [Test]
    public async Task a_trace_context_header_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/HeaderNamed?name=traceparent&value=00-ward");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: traceparent carries the trace context the framework sends, never a header the view writes. (Parameter 'name')");
    }

    [Test]
    public async Task a_header_the_browser_sets_itself_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/HeaderNamed?name=Cookie&value=ward%3Dnorth");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: Cookie is a header the browser sets itself and never sends from a page (the Fetch Standard's forbidden request headers). (Parameter 'name')");
    }

    [Test]
    public async Task a_header_under_a_prefix_the_browser_keeps_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/HeaderNamed?name=Sec-Purpose&value=prefetch");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: Sec-Purpose starts with Sec-, which the browser keeps for the headers it sets itself (the Fetch Standard's forbidden request headers). (Parameter 'name')");
    }

    [Test]
    public async Task a_header_that_overrides_the_method_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/HeaderNamed?name=X-HTTP-Method-Override&value=DELETE");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: X-HTTP-Method-Override overrides the request's method, which its verb states; write the verb. (Parameter 'name')");
    }

    [Test]
    public async Task a_header_value_holding_a_line_break_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/HeaderNamed?name=X-Ward&value=North%0AWing");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: The header X-Ward holds a line break, which HTTP does not carry in a field value; send it in the query. (Parameter 'value')");
    }

    [Test]
    public async Task a_header_value_past_us_ascii_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Residents/HeaderNamed?name=X-Ward&value=Zo%C3%AB");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: The header X-Ward holds 'ë', which a header value does not carry: a header carries visible US-ASCII, spaces and tabs; send it in the query. (Parameter 'value')");
    }
}
