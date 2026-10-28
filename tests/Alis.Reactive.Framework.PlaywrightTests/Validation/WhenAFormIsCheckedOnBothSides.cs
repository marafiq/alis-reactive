namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

/// <summary>
/// A form whose rules are written once: the browser checks the rules it can, each message beside its own
/// field; a rule under a condition applies only while the condition holds; a rule comparing two members
/// checks them against each other; a rule only the server decides comes back from the server to its field,
/// and one for a member the page has no input for comes back to the summary.
/// </summary>
[TestFixture]
public sealed class WhenAFormIsCheckedOnBothSides : AppPageTest
{
    private readonly List<string> _bookings = new();

    private ILocator Booked => Page.Locator("#booked");

    // Every request that reached the booking endpoint, as the network carried it.
    [SetUp]
    public void WatchTheBookingEndpoint()
    {
        _bookings.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Url.EndsWith("/Validation/Book", StringComparison.Ordinal))
                lock (_bookings) _bookings.Add(request.Method);
        };
    }

    // The field's own wrapper: its label, its input and its message slot, and nothing of another field. The
    // label is matched whole, with the mark a required field carries, so "Email" is not "Email again".
    private ILocator Field(string label) =>
        Page.Locator("#visit-form > div").Filter(new()
        {
            Has = Page.GetByLabel(new System.Text.RegularExpressions.Regex($"^{System.Text.RegularExpressions.Regex.Escape(label)}( \\*)?$")),
        });

    [Test]
    public async Task a_failure_the_server_finds_reads_as_its_field_and_its_message()
    {
        await Open("/Validation/Visit");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Ask the desk what is wrong", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#failures")).ToHaveTextAsync("Visitor: Name the visitor.");
    }

    private async Task Fill(string visitor, string relation, string company, string phone, string email, string again, string guests, string notes)
    {
        await Page.GetByLabel("Visitor").FillAsync(visitor);
        await Page.GetByLabel("Relation").FillAsync(relation);
        await Page.GetByLabel("Company").FillAsync(company);
        await Page.GetByLabel("Phone").FillAsync(phone);
        await Page.GetByLabel("Email", new() { Exact = true }).FillAsync(email);
        await Page.GetByLabel("Email again").FillAsync(again);
        await Page.GetByLabel("Guests").FillAsync(guests);
        await Page.GetByLabel("Notes").FillAsync(notes);
    }

    private Task Book() => Page.GetByRole(AriaRole.Button, new() { Name = "Book the visit", Exact = true }).ClickAsync();

    // The browser held the request: once the network settles, nothing reached the endpoint. An empty result
    // alone would pass when the server refused what the browser should have held.
    private async Task ExpectTheBrowserHeldIt()
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_bookings) Assert.That(_bookings, Is.Empty, "a request reached the booking endpoint");
        await Expect(Booked).ToHaveTextAsync("");
    }

    [Test]
    public async Task each_rule_the_browser_checks_shows_its_own_message_beside_its_own_field()
    {
        await Open("/Validation/Visit");
        await Fill("", "Family", "", "5550100", "ada-at-home", "ada-at-home", "6", "A note far longer than twenty");

        await Book();

        await Expect(Field("Visitor").GetByText("Name the visitor.")).ToBeVisibleAsync();
        await Expect(Field("Phone").GetByText("Write the phone as 555-0100.")).ToBeVisibleAsync();
        await Expect(Field("Email").GetByText("Give an email we can write to.")).ToBeVisibleAsync();
        await Expect(Field("Guests").GetByText("Bring one to four guests.")).ToBeVisibleAsync();
        await Expect(Field("Notes").GetByText("Keep the note to 20 characters.")).ToBeVisibleAsync();
        await ExpectTheBrowserHeldIt();
    }

    [Test]
    public async Task values_that_keep_every_rule_are_booked_with_no_message()
    {
        await Open("/Validation/Visit");
        await Fill("Ada", "Family", "", "555-0100", "ada@home.test", "ada@home.test", "2", "Bring flowers");

        await Book();

        await Expect(Booked).ToHaveTextAsync("booked Ada for 2 guest(s)");
        await Expect(Page.Locator("#visit-form").GetByText("Name the visitor.")).ToBeHiddenAsync();
    }

    [Test]
    public async Task a_rule_under_a_condition_does_not_apply_while_its_condition_does_not_hold()
    {
        await Open("/Validation/Visit");
        await Fill("Ada", "Family", "", "555-0100", "ada@home.test", "ada@home.test", "2", "");

        await Book();

        await Expect(Booked).ToHaveTextAsync("booked Ada for 2 guest(s)");
    }

    [Test]
    public async Task a_rule_under_a_condition_applies_in_the_browser_while_its_condition_holds()
    {
        await Open("/Validation/Visit");
        await Fill("Ada", "Contractor", "", "555-0100", "ada@home.test", "ada@home.test", "2", "");

        await Book();

        await Expect(Field("Company").GetByText("Name the contractor's company.")).ToBeVisibleAsync();
        await ExpectTheBrowserHeldIt();
    }

    [Test]
    public async Task a_rule_comparing_two_members_checks_one_against_the_other()
    {
        await Open("/Validation/Visit");
        await Fill("Ada", "Family", "", "555-0100", "ada@home.test", "ada@work.test", "2", "");

        await Book();
        await Expect(Field("Email again").GetByText("Write the same email twice.")).ToBeVisibleAsync();
        await ExpectTheBrowserHeldIt();

        await Page.GetByLabel("Email again").FillAsync("ada@home.test");
        await Book();
        await Expect(Booked).ToHaveTextAsync("booked Ada for 2 guest(s)");
    }

    [Test]
    public async Task a_rule_only_the_server_decides_comes_back_from_the_server_to_its_own_field()
    {
        await Open("/Validation/Visit");
        await Fill("Mallory", "Family", "", "555-0100", "m@home.test", "m@home.test", "2", "");

        await Book();

        await Expect(Field("Visitor").GetByText("Mallory may not visit.")).ToBeVisibleAsync();
        lock (_bookings) Assert.That(_bookings, Is.EqualTo(new[] { "POST" }), "the browser cannot decide this rule, so the request goes to the server");
        await Expect(Booked).ToHaveTextAsync("");
    }

    [Test]
    public async Task a_server_message_for_a_member_the_page_has_no_input_for_comes_back_to_the_summary()
    {
        await Open("/Validation/Visit");
        await Fill("Ada", "Family", "", "555-0100", "ada@home.test", "ada@home.test", "2", "");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Book the midnight slot", Exact = true }).ClickAsync();

        await Expect(Page.GetByText("The desk is closed at midnight.")).ToBeVisibleAsync();
        await Expect(Page.Locator("#visit-form").GetByText("The desk is closed at midnight.")).ToHaveCountAsync(0);
        await Expect(Booked).ToHaveTextAsync("");
    }
}
