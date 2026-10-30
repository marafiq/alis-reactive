namespace Alis.Reactive.Framework.PlaywrightTests.Rows;

/// <summary>
/// The page's model governs every row (ADR-0003, grammar.md F1): the sheet's rule for each contact is written once,
/// in the page's plan, by the contact's binding name, and checks every contact on the page when the sheet is saved,
/// whoever rendered the row: the list partial rendered with the page, or rendered again into its slot when the desk
/// adds a contact. A row's rule reads the members of its own row; a row that left the page is not checked; and the
/// server's message for a row lands beside that row.
/// </summary>
[TestFixture]
public sealed class WhenAPagesRulesCheckItsRows : AppPageTest
{
    private readonly List<string> _saves = new();

    [SetUp]
    public void WatchTheEndpoint()
    {
        _saves.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Rows/SaveContacts", StringComparison.Ordinal))
                lock (_saves) _saves.Add(request.Url);
        };
    }

    private ILocator Saved => Page.Locator("#saved");

    private ILocator Summary => Page.Locator("[data-reactive-validation-summary]");

    private ILocator Input(string label) => Page.GetByLabel(label, new() { Exact = true });

    private ILocator Field(string label) => Page.Locator(".alis-field").Filter(new() { Has = Input(label) });

    private Task Click(string button) => Page.GetByRole(AriaRole.Button, new() { Name = button, Exact = true }).ClickAsync();

    private async Task ExpectNothingSent()
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the sheet");
    }

    [Test]
    public async Task the_pages_rule_checks_each_row_the_list_partial_rendered()
    {
        await Open("/Rows/Contacts");

        await Input("Contact 2").FillAsync("");
        await Click("Save the contacts");
        await Expect(Field("Contact 2").GetByText("Name each contact.")).ToBeVisibleAsync();
        await ExpectNothingSent();

        await Input("Contact 2").FillAsync("Di");
        await Click("Save the contacts");
        await Expect(Saved).ToHaveTextAsync("Ada's contacts: Bo, Di");
    }

    [Test]
    public async Task a_row_added_later_is_checked_by_the_pages_rule()
    {
        await Open("/Rows/Contacts");
        await Click("Add a contact");
        await Expect(Input("Contact 3")).ToBeVisibleAsync();

        await Click("Save the contacts");
        await Expect(Field("Contact 3").GetByText("Name each contact.")).ToBeVisibleAsync();
        await ExpectNothingSent();

        await Input("Contact 3").FillAsync("Di");
        await Click("Save the contacts");
        await Expect(Saved).ToHaveTextAsync("Ada's contacts: Bo, Cy, Di");
    }

    [Test]
    public async Task leaving_a_row_added_later_checks_it_by_the_pages_rule()
    {
        await Open("/Rows/Contacts");
        await Click("Add a contact");
        await Input("Contact 3").FocusAsync();

        await Input("Contact 3").PressAsync("Tab");
        await Expect(Field("Contact 3").GetByText("Name each contact.")).ToBeVisibleAsync();

        await Input("Contact 3").FillAsync("Di");
        await Input("Contact 3").PressAsync("Tab");
        await Expect(Field("Contact 3").GetByText("Name each contact.")).ToBeHiddenAsync();
    }

    [Test]
    public async Task a_row_that_left_the_page_is_not_checked()
    {
        await Open("/Rows/Contacts?contacts=0");
        await Click("Add a contact");
        await Expect(Input("Contact 1")).ToBeVisibleAsync();
        await Click("Remove contact 1");
        await Expect(Input("Contact 1")).ToHaveCountAsync(0);

        await Click("Save the contacts");

        await Expect(Saved).ToHaveTextAsync("Ada's contacts: no contacts");
    }

    [Test]
    public async Task a_rows_rule_reads_the_members_of_its_own_row()
    {
        await Open("/Rows/Contacts");

        await Input("Contact 1's phone").FillAsync("");
        await Input("Contact 2's phone").FillAsync("");
        await Click("Save the contacts");

        await Expect(Field("Contact 2's phone").GetByText("Give the guardian a phone the desk can call.")).ToBeVisibleAsync();
        await Expect(Field("Contact 1's phone").GetByText("Give the guardian a phone the desk can call.")).ToBeHiddenAsync();
        await ExpectNothingSent();
    }

    [Test]
    public async Task the_servers_message_for_a_row_lands_beside_that_row()
    {
        await Open("/Rows/Contacts");

        await Input("Contact 2").FillAsync("Front desk");
        await Click("Save the contacts");

        await Expect(Field("Contact 2").GetByText("Give the contact's own name, not the front desk.")).ToBeVisibleAsync();
        await Expect(Field("Contact 1").GetByText("Give the contact's own name, not the front desk.")).ToBeHiddenAsync();
        await Expect(Summary).ToBeHiddenAsync();
    }
}
