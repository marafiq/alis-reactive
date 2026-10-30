namespace Alis.Reactive.Framework.PlaywrightTests.Requests;

/// <summary>
/// Text holding every character a path, a query or a form treats as its own (a slash, an ampersand, an equals
/// sign, a plus, a space, quotes, a percent sign, a hash, a question mark) and text beyond ASCII, each reaching
/// the endpoint exactly as the page wrote it: in a query, in form data, and in a route value (H1, H2, H4). A
/// route value is one path segment, so a slash in it is sent inside its segment and the request reaches the
/// endpoint its template names; ASP.NET Core's routing leaves that slash encoded (<c>%2F</c>) in the value it
/// binds. A member named beyond ASCII is sent by its own name in a query and in form data.
/// </summary>
[TestFixture]
public sealed class WhenATextHoldsCharactersAUrlOrAFormReserves : AppPageTest
{
    private ILocator Line(string id) => Page.Locator($"#{id}");

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();

    [Test]
    public async Task a_query_value_reaches_the_endpoint_exactly_as_the_page_wrote_it()
    {
        await Open("/Requests/Spellings");

        await Click("Look up the name");

        await Expect(Line("looked-up")).ToHaveTextAsync("query name [Zoë & \"Rose\" O'Neil / 1+1=2, 50% #3?]");
    }

    [Test]
    public async Task a_form_data_value_reaches_the_endpoint_exactly_as_the_page_wrote_it()
    {
        await Open("/Requests/Spellings");

        await Click("Post the name");

        await Expect(Line("posted")).ToHaveTextAsync("form name [Zoë & \"Rose\" O'Neil / 1+1=2, 50% #3?]");
    }

    [Test]
    public async Task a_route_value_reaches_the_endpoint_exactly_as_the_page_wrote_it()
    {
        await Open("/Requests/Spellings");

        await Click("Open the name's page");

        await Expect(Line("routed")).ToHaveTextAsync("route name [Zoë & \"Rose\" O'Neil 1+1=2, 50% #3?]");
    }

    [Test]
    public async Task a_slash_in_a_route_value_stays_inside_its_segment_and_aspnet_core_binds_it_encoded()
    {
        await Open("/Requests/Spellings");

        await Click("Open the ward's page");

        await Expect(Line("routed")).ToHaveTextAsync("route name [Ward 2%2FB]");
    }

    [Test]
    public async Task a_member_named_beyond_ascii_is_sent_by_its_own_name_in_a_query()
    {
        await Open("/Requests/Spellings");

        await Click("Look up the street");

        await Expect(Line("street-looked-up")).ToHaveTextAsync("query Straße [Hauptstraße 1]");
    }

    [Test]
    public async Task a_member_named_beyond_ascii_is_sent_by_its_own_name_in_form_data()
    {
        await Open("/Requests/Spellings");

        await Click("Post the street");

        await Expect(Line("street-posted")).ToHaveTextAsync("form Straße [Hauptstraße 1]");
    }
}
