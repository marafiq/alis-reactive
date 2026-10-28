namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

/// <summary>
/// Every check the validator offers, made in the browser the way the server makes it: a value the rule
/// refuses is held with the rule's message beside its own field and nothing sent, and a value it takes is
/// saved by the endpoint running the same rules. A rule only the server makes comes back from the server.
/// </summary>
[TestFixture]
public sealed class WhenEachCheckIsMade : AppPageTest
{
    private readonly List<string> _saves = new();

    private ILocator Saved => Page.Locator("#saved");

    [SetUp]
    public void WatchTheEndpoint()
    {
        _saves.Clear();
        Page.Request += (_, request) =>
        {
            // The page itself is a GET of the same path; a save is the POST.
            if (request.Method == "POST" && request.Url.EndsWith("/Validation/Checks", StringComparison.Ordinal))
                lock (_saves) _saves.Add(request.Method);
        };
    }

    private ILocator Field(string label) =>
        Page.Locator("#checks-form > div").Filter(new() { Has = Page.GetByLabel(label, new() { Exact = true }) });

    private Task Save() => Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();

    [TestCase("Count", "", "2", "Give a count.", TestName = "not_null_refuses_a_number_left_blank_and_takes_one")]
    [TestCase("Spare", "something", "", "Leave the spare field empty.", TestName = "empty_refuses_any_text_and_takes_none")]
    [TestCase("Code", "ab", "abc", "Use at least 3 characters.", TestName = "a_minimum_length_refuses_shorter_text_and_takes_text_of_its_length")]
    [TestCase("Pin", "12345", "9876", "Use exactly 4 characters.", TestName = "an_exact_length_refuses_longer_text_and_takes_its_own_length")]
    [TestCase("Pin", "123", "9876", "Use exactly 4 characters.", TestName = "an_exact_length_refuses_shorter_text_and_takes_its_own_length")]
    [TestCase("Nickname", "A", "Be", "Use 2 to 5 characters.", TestName = "a_length_range_refuses_text_below_it_and_takes_its_lower_bound")]
    [TestCase("Nickname", "Beatrix", "Beata", "Use 2 to 5 characters.", TestName = "a_length_range_refuses_text_above_it_and_takes_its_upper_bound")]
    [TestCase("Website", "visits", "http://ada.test/visits", "Give a web address.", TestName = "a_url_check_refuses_a_word_and_takes_an_address")]
    [TestCase("Card", "4111111111111112", "4111111111111111", "Give a card number.", TestName = "a_card_check_refuses_a_number_that_fails_luhn_and_takes_one_that_passes")]
    [TestCase("Card", "4111 1111 1111 1112", "4111-1111-1111-1111", "Give a card number.", TestName = "a_card_check_reads_a_number_written_in_groups")]
    [TestCase("Letters", "abc1", "ABC", "Use letters only.", TestName = "a_pattern_with_options_refuses_a_digit_and_takes_capitals_it_ignores_case_of")]
    [TestCase("Answer", "no", "yes", "Write yes.", TestName = "equal_to_a_value_refuses_another_and_takes_it")]
    [TestCase("Other", "no", "perhaps", "Anything but no.", TestName = "not_equal_to_a_value_refuses_it_and_takes_another")]
    [TestCase("Rooms", "0", "2", "More rooms than none.", TestName = "greater_than_refuses_the_bound_and_takes_above_it")]
    [TestCase("Wing", "0", "1", "Wing one at least.", TestName = "greater_than_or_equal_refuses_below_and_takes_the_bound")]
    [TestCase("Floor", "10", "9", "A floor below ten.", TestName = "less_than_refuses_the_bound_and_takes_below_it")]
    [TestCase("Level", "10", "9", "Level nine at most.", TestName = "less_than_or_equal_refuses_above_and_takes_the_bound")]
    [TestCase("Score", "10", "9", "Between 0 and 10, not either.", TestName = "an_exclusive_range_refuses_its_upper_bound_and_takes_below_it")]
    [TestCase("Score", "0", "1", "Between 0 and 10, not either.", TestName = "an_exclusive_range_refuses_its_lower_bound_and_takes_above_it")]
    [TestCase("Budget", "0", "", "A budget above zero.", TestName = "a_nullable_bound_refuses_a_value_below_it_and_takes_none")]
    [TestCase("Later", "2026-10-01", "2026-10-03", "Later comes after earlier.", TestName = "a_member_bound_refuses_its_peers_own_date_and_takes_a_later_one")]
    [TestCase("Email", "ada.visits.test", "bea@visits.test", "Give an email address.", TestName = "an_email_check_refuses_text_without_an_at_and_takes_an_address")]
    [TestCase("Short", "abcdef", "abcde", "Five characters at most.", TestName = "a_maximum_length_refuses_longer_text_and_takes_text_of_its_length")]
    [TestCase("Lower", "ABC", "xyz", "Lower case only.", TestName = "a_pattern_without_options_refuses_capitals_and_takes_lower_case")]
    [TestCase("Stars", "6", "5", "One to five stars.", TestName = "an_inclusive_range_refuses_above_it_and_takes_its_upper_bound")]
    [TestCase("Stars", "0", "1", "One to five stars.", TestName = "an_inclusive_range_refuses_below_it_and_takes_its_lower_bound")]
    [TestCase("Repeat", "secreT", "secret", "Repeat the original.", TestName = "equal_to_a_member_refuses_other_text_and_takes_the_members")]
    [TestCase("Fresh", "old", "newer", "Not the former one.", TestName = "not_equal_to_a_member_refuses_the_members_text_and_takes_other")]
    [TestCase("Purpose", "   ", "Tour", "Give a purpose.", TestName = "not_empty_refuses_only_spaces_and_takes_text")]
    [TestCase("Guests", "0", "1", "At least one guest.", TestName = "not_empty_refuses_a_numbers_default_and_takes_another")]
    [TestCase("Leaving", "2026-09-30", "2026-10-01", "Leave on or after arriving.", TestName = "a_nullable_member_bound_refuses_a_date_before_its_peer_and_takes_the_peers_own")]
    public async Task a_check_refuses_what_its_rule_refuses_and_takes_what_it_takes(string field, string refused, string taken, string message)
    {
        await Open("/Validation/Checks");

        await Page.GetByLabel(field, new() { Exact = true }).FillAsync(refused);
        await Save();
        await Expect(Field(field).GetByText(message)).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Page.GetByLabel(field, new() { Exact = true }).FillAsync(taken);
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
        await Expect(Field(field).GetByText(message)).ToBeHiddenAsync();
    }

    [Test]
    public async Task a_rule_under_unless_applies_only_while_its_condition_does_not_hold()
    {
        await Open("/Validation/Checks");
        var reasonWanted = Field("Reason").GetByText("Give a reason unless the answer is yes.");
        await Page.GetByLabel("Reason", new() { Exact = true }).FillAsync("");

        await Page.GetByLabel("Answer", new() { Exact = true }).FillAsync("no");
        await Save();
        await Expect(reasonWanted).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Page.GetByLabel("Answer", new() { Exact = true }).FillAsync("yes");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
        await Expect(reasonWanted).ToBeHiddenAsync();
    }

    [Test]
    public async Task a_rule_marked_for_the_server_alone_is_sent_and_comes_back_from_the_server()
    {
        await Open("/Validation/Checks");

        await Page.GetByLabel("Badge", new() { Exact = true }).FillAsync("B-17");
        await Save();

        await Expect(Field("Badge").GetByText("Badges start with A-.")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-reactive-validation-summary]")).ToBeHiddenAsync();
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), "the browser does not make this check, so the request goes");
        await Expect(Saved).ToHaveTextAsync("");
    }

    [Test]
    public async Task a_child_validators_rule_the_server_alone_makes_comes_back_beside_its_field()
    {
        await Open("/Validation/Checks");

        await Page.GetByLabel("Phone", new() { Exact = true }).FillAsync("556-0101");
        await Save();

        await Expect(Field("Phone").GetByText("Use a 555 number.")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-reactive-validation-summary]")).ToBeHiddenAsync();
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), "the browser does not make this check, so the request goes");
    }
}
