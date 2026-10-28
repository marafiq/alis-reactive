namespace Alis.Reactive.Framework.PlaywrightTests.Requests;

/// <summary>
/// What a request carries, as the endpoint that receives it says: a list in a query as one key per item, a
/// value the address leaves out as an empty key and a list it leaves out as no key (H2), the same in form data
/// (H4), blank text in a JSON body as null while a list keeps its blank items (H3), and a query member whose
/// items have no text form stopping its own request.
/// </summary>
[TestFixture]
public sealed class WhenAValueIsSentInAQueryOrAForm : AppPageTest
{
    private ILocator Line(string id) => Page.Locator($"#{id}");

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();

    [Test]
    public async Task a_list_in_a_query_is_sent_as_one_key_per_item()
    {
        await Open("/Requests/Census");

        await Click("Ask about two wings");

        await Expect(Line("listed")).ToHaveTextAsync("query ?Wings=East&Wings=West, bound wing none and wings East and West");
    }

    [Test]
    public async Task a_value_left_out_is_sent_as_an_empty_key_in_a_query_and_a_list_left_out_as_no_key()
    {
        await Open("/Requests/Census");

        await Click("Ask about the address's wings");

        await Expect(Line("queried")).ToHaveTextAsync("query ?Wing=, bound wing none and wings none");
    }

    [Test]
    public async Task a_value_left_out_is_sent_as_an_empty_field_in_form_data_and_a_list_left_out_as_no_field()
    {
        await Open("/Requests/Census");

        await Click("Post the address's wings");

        await Expect(Line("posted")).ToHaveTextAsync("fields Wing=''");
    }

    [Test]
    public async Task blank_text_in_a_json_body_is_sent_as_null()
    {
        await Open("/Requests/Census");

        await Click("Send the untitled slate's title");

        await Expect(Line("blank-title")).ToHaveTextAsync("ledger titled nothing");
    }

    [Test]
    public async Task a_blank_item_of_a_list_in_a_json_body_keeps_its_text()
    {
        await Open("/Requests/Census");

        await Click("Send a blank label and Ada");

        await Expect(Line("labels")).ToHaveTextAsync("labels '' and 'Ada'");
    }

    [Test]
    public async Task a_query_member_whose_items_have_no_text_form_stops_its_request_and_the_steps_after_it()
    {
        await Open("/Requests/Census");
        await Click("Ask about the cards");
        await Expect(Line("cards-asked")).ToHaveTextAsync("asked with query nothing");
        await Expect(Line("cards-after")).ToHaveTextAsync("1");
        await Click("Fill a card");

        await Click("Ask about the cards");

        await Expect(Line("cards-tried")).ToHaveTextAsync("2");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Line("cards-asked")).ToHaveTextAsync("asked with query nothing");
        await Expect(Line("cards-after")).ToHaveTextAsync("1");
        await Click("Ask about two wings");
        await Expect(Line("listed")).ToHaveTextAsync("query ?Wings=East&Wings=West, bound wing none and wings East and West");
    }
}
