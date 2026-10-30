namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

/// <summary>
/// The messages a rule says. A check the browser makes says the validator's own words for it, naming the
/// member in words and the values the page knows, and leaving out the sentence that names what was entered;
/// a message of its own fills its placeholders with their formats. The server says the whole of its default
/// message, a message built from the model, and the messages of predicates, a predicate that waits, and a
/// rule whose condition only the server decides.
/// </summary>
[TestFixture]
public sealed class WhenARuleSaysItsMessage : AppPageTest
{
    private readonly List<string> _saves = new();

    private ILocator Saved => Page.Locator("#saved");

    private ILocator Summary => Page.Locator("[data-reactive-validation-summary]");

    [SetUp]
    public void WatchTheEndpoint()
    {
        _saves.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Validation/Notice", StringComparison.Ordinal))
                lock (_saves) _saves.Add(request.Method);
        };
    }

    private ILocator Field(string label) =>
        Page.Locator("#notice-form > div").Filter(new() { Has = Page.GetByLabel(label, new() { Exact = true }) });

    private Task Write(string label, string text) => Page.GetByLabel(label, new() { Exact = true }).FillAsync(text);

    private Task Save() => Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();

    [TestCase("Rooms", "", "'Rooms' must not be empty.", TestName = "not_null_says_the_member_must_not_be_empty")]
    [TestCase("First name", "", "'First Name' must not be empty.", TestName = "not_empty_names_the_member_in_words")]
    [TestCase("Spare", "x", "'Spare' must be empty.", TestName = "empty_says_the_member_must_be_empty")]
    [TestCase("Nickname", "A", "'Nickname' must be between 2 and 5 characters.", TestName = "a_length_range_names_its_bounds_and_leaves_out_the_length_entered")]
    [TestCase("Pin", "123", "'Pin' must be 4 characters in length.", TestName = "an_exact_length_names_its_length")]
    [TestCase("Code", "ab", "The length of 'Code' must be at least 3 characters.", TestName = "a_minimum_length_names_its_minimum")]
    [TestCase("Short", "abcdef", "The length of 'Short' must be 5 characters or fewer.", TestName = "a_maximum_length_names_its_maximum")]
    [TestCase("Email", "ada", "'Email' is not a valid email address.", TestName = "an_email_check_says_the_address_is_not_valid")]
    [TestCase("Letters", "a1", "'Letters' is not in the correct format.", TestName = "a_pattern_says_the_text_is_not_in_the_format")]
    [TestCase("Card", "4111111111111112", "'Card' is not a valid credit card number.", TestName = "a_card_check_says_the_number_is_not_valid")]
    [TestCase("Website", "visits", "'Website' must be a valid URL.", TestName = "a_url_check_says_the_address_is_not_valid")]
    [TestCase("Answer", "no", "'Answer' must be equal to 'yes'.", TestName = "equal_names_the_value")]
    [TestCase("Other", "no", "'Other' must not be equal to 'no'.", TestName = "not_equal_names_the_value")]
    [TestCase("Wing", "0", "'Wing' must be greater than '0'.", TestName = "greater_than_names_the_value")]
    [TestCase("Floor", "0", "'Floor' must be greater than or equal to '1'.", TestName = "at_least_names_the_value")]
    [TestCase("Level", "10", "'Level' must be less than '10'.", TestName = "less_than_names_the_value")]
    [TestCase("Bay", "10", "'Bay' must be less than or equal to '9'.", TestName = "at_most_names_the_value")]
    [TestCase("Stars", "6", "'Stars' must be between 1 and 5.", TestName = "an_inclusive_range_names_its_bounds_and_leaves_out_the_value_entered")]
    [TestCase("Score", "10", "'Score' must be between 0 and 10 (exclusive).", TestName = "an_exclusive_range_names_its_bounds")]
    [TestCase("Repeat", "other", "'Repeat' must be equal to 'Original'.", TestName = "equal_to_a_member_names_the_member")]
    [TestCase("City", "", "'Address City' must not be empty.", TestName = "a_nested_member_is_named_by_its_path_in_words")]
    [TestCase("Motto", "far too long", "Keep 'Motto' under 008 characters.", TestName = "a_message_of_its_own_fills_its_placeholders_with_their_formats")]
    [TestCase("Contact phone", "", "'Phone' must not be empty.", TestName = "a_rule_of_inline_child_rules_names_the_childs_member")]
    public async Task a_check_the_browser_makes_says_the_validators_words(string label, string refused, string message)
    {
        await Open("/Validation/Notice");

        await Write(label, refused);
        await Save();

        await Expect(Field(label).GetByText(message, new() { Exact = true })).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");
    }

    [TestCase("Tagline", "abcdefg", "The length of 'Tagline' must be 5 characters or fewer. You entered 7 characters.", TestName = "the_servers_default_message_says_the_length_entered")]
    [TestCase("Rating", "7", "'Rating' must be between 1 and 5. You entered 7.", TestName = "the_servers_default_message_says_the_value_entered")]
    [TestCase("Host", "", "Ada needs a host.", TestName = "a_message_built_from_the_model_is_the_servers")]
    [TestCase("Room", "Z-0", "The specified condition was not met for 'Room'.", TestName = "a_predicate_the_server_runs_says_its_condition_was_not_met")]
    [TestCase("Bed", "A-1", "The bed cannot share the room's name.", TestName = "a_predicate_that_reads_the_model_is_the_servers")]
    [TestCase("Pass", "P-9", "Pass P-9 is withdrawn.", TestName = "a_message_that_only_names_the_value_entered_is_the_servers")]
    [TestCase("Locker", "L-0", "That locker is taken.", TestName = "a_predicate_that_waits_is_the_servers")]
    public async Task a_message_only_the_server_can_say_comes_back_beside_its_field(string label, string refused, string message)
    {
        await Open("/Validation/Notice");

        await Write(label, refused);
        await Save();

        await Expect(Field(label).GetByText(message, new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Summary).ToBeHiddenAsync();
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), "only the server can say it, so the request goes");
    }

    [Test]
    public async Task a_rule_the_server_applies_when_its_condition_holds_is_asked_only_then()
    {
        await Open("/Validation/Notice");
        var message = Field("Escort").GetByText("The far wings need an escort.", new() { Exact = true });
        await Write("Escort", "");

        await Write("Wing", "6");
        await Save();
        await Expect(message).ToBeVisibleAsync();
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), "the server decides the condition");

        await Write("Wing", "2");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
    }

    [Test]
    public async Task a_rule_the_server_applies_unless_its_condition_holds_is_asked_only_then()
    {
        await Open("/Validation/Notice");
        var message = Field("Parking").GetByText("The near wings need parking.", new() { Exact = true });
        await Write("Parking", "");

        await Write("Wing", "2");
        await Save();
        await Expect(message).ToBeVisibleAsync();
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), "the server decides the condition");

        await Write("Wing", "6");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
    }

    [Test]
    public async Task the_notice_as_it_starts_is_saved()
    {
        await Open("/Validation/Notice");

        await Save();

        await Expect(Saved).ToHaveTextAsync("saved");
    }
}
