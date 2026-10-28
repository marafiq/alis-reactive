namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

/// <summary>
/// A rule written under a condition applies only while the condition holds, in the browser as on the server,
/// for every form a condition the browser can read takes: the six comparisons with the member on either
/// side, a value the validator fixes, null, empty text, the three text tests and a pattern, a fixed array and
/// list, a nullable's value, a flag, a negation, a nested member, and the conditions joined by and and or.
/// Each field starts filled; a test empties it, sets what the condition reads so that it holds, and the
/// browser refuses with nothing sent; then it sets it so that it does not, and the request is saved.
/// </summary>
[TestFixture]
public sealed class WhenARuleIsWrittenUnderACondition : AppPageTest
{
    private readonly List<string> _saves = new();

    private ILocator Saved => Page.Locator("#saved");

    [SetUp]
    public void WatchTheEndpoint()
    {
        _saves.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Validation/Escort", StringComparison.Ordinal))
                lock (_saves) _saves.Add(request.Method);
        };
    }

    private ILocator Field(string label) =>
        Page.Locator("#escort-form > div").Filter(new() { Has = Page.GetByLabel(label, new() { Exact = true }) });

    private Task Write(string label, string text) => Page.GetByLabel(label, new() { Exact = true }).FillAsync(text);

    private Task Save() => Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();

    private async Task ExpectHeld(string field, string message)
    {
        await Expect(Field(field).GetByText(message)).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");
    }

    private async Task ExpectSaved(string field, string message)
    {
        await Expect(Saved).ToHaveTextAsync("saved");
        await Expect(Field(field).GetByText(message)).ToBeHiddenAsync();
    }

    [TestCase("ForTwo", "Guests", "2", "3", "Needed for exactly two guests.", TestName = "equal_holds_at_the_value")]
    [TestCase("NotForTwo", "Guests", "3", "2", "Needed for other than two guests.", TestName = "not_equal_holds_away_from_the_value")]
    [TestCase("ForMore", "Guests", "3", "2", "Needed for more than two guests.", TestName = "greater_than_holds_above_the_value")]
    [TestCase("ForTwoOrMore", "Guests", "2", "1", "Needed for two guests or more.", TestName = "at_least_holds_at_the_value")]
    [TestCase("ForFewer", "Guests", "1", "2", "Needed for fewer than two guests.", TestName = "less_than_holds_below_the_value")]
    [TestCase("ForTwoOrFewer", "Guests", "2", "3", "Needed for two guests or fewer.", TestName = "at_most_holds_at_the_value")]
    [TestCase("TwoBelow", "Guests", "3", "2", "Needed when two is below the guests.", TestName = "a_value_on_the_left_below_the_member_mirrors_to_greater_than")]
    [TestCase("TwoAtMost", "Guests", "2", "1", "Needed when two is at most the guests.", TestName = "a_value_on_the_left_at_most_the_member_mirrors_to_at_least")]
    [TestCase("TwoAbove", "Guests", "1", "2", "Needed when two is above the guests.", TestName = "a_value_on_the_left_above_the_member_mirrors_to_less_than")]
    [TestCase("TwoAtLeast", "Guests", "2", "3", "Needed when two is at least the guests.", TestName = "a_value_on_the_left_at_least_the_member_mirrors_to_at_most")]
    [TestCase("OverLimit", "Guests", "5", "4", "Needed for more guests than the limit.", TestName = "a_value_read_from_a_field_of_the_validator_is_fixed_when_the_page_renders")]
    [TestCase("NoWing", "Wing", "", "East", "Needed when no wing is named.", TestName = "equal_to_null_holds_for_nothing")]
    [TestCase("AnyWing", "Wing", "East", "", "Needed when a wing is named.", TestName = "not_equal_to_null_holds_for_something")]
    [TestCase("BlankWing", "Wing", "", "East", "Needed when the wing is left blank.", TestName = "is_null_or_empty_holds_for_nothing")]
    [TestCase("GivenWing", "Wing", "East", "", "Needed when the wing is given.", TestName = "not_is_null_or_empty_holds_for_text")]
    [TestCase("AnnexWing", "Wing", "East Annex", "East", "Needed when the wing is an annex.", TestName = "contains_holds_for_text_holding_the_value")]
    [TestCase("EastFirst", "Wing", "East Annex", "North East", "Needed when the wing starts with East.", TestName = "starts_with_holds_for_text_starting_with_the_value_and_not_for_text_holding_it_later")]
    [TestCase("AnnexLast", "Wing", "East Annex", "Annex North", "Needed when the wing ends with Annex.", TestName = "ends_with_holds_for_text_ending_with_the_value_and_not_for_text_holding_it_earlier")]
    [TestCase("PoleWing", "Wing", "North", "East North", "Needed when the wing is north or south.", TestName = "a_pattern_holds_for_text_it_matches_and_not_for_text_holding_its_letters")]
    [TestCase("KnownWing", "Wing", "East", "North", "Needed when the wing is a known one.", TestName = "a_fixed_array_holds_a_member_of_it")]
    [TestCase("OtherWing", "Wing", "North", "East", "Needed when the wing is another one.", TestName = "not_a_fixed_array_holds_outside_it")]
    [TestCase("ListedWing", "Wing", "West", "South", "Needed when the wing is a listed one.", TestName = "a_fixed_list_holds_a_member_of_it")]
    [TestCase("KnownFloor", "Floor", "3", "", "Needed when the floor is known.", TestName = "has_value_holds_for_a_number_and_not_for_nothing")]
    [TestCase("HighFloor", "Floor", "3", "2", "Needed when the floor is above two.", TestName = "a_nullables_value_compares_as_the_member")]
    [TestCase("ByNight", "Overnight", "true", "false", "Needed for an overnight visit.", TestName = "a_flag_holds_while_it_is_true")]
    [TestCase("ByDay", "Overnight", "false", "true", "Needed for a day visit.", TestName = "a_negated_flag_holds_while_it_is_false")]
    [TestCase("NotMany", "Guests", "2", "3", "Needed unless there are more than two guests.", TestName = "a_negated_comparison_holds_where_the_comparison_does_not")]
    [TestCase("TourPurpose", "Purpose", "Tour", "Lunch", "Needed for a tour.", TestName = "a_nested_members_comparison_holds_at_the_value")]
    public async Task a_rule_applies_only_while_its_condition_holds(string field, string driver, string holds, string other, string message)
    {
        await Open("/Validation/Escort");
        await Write(field, "");

        await Write(driver, holds);
        await Save();
        await ExpectHeld(field, message);

        await Write(driver, other);
        await Save();
        await ExpectSaved(field, message);
    }

    [Test]
    public async Task a_condition_the_browser_cannot_read_is_refused_where_the_page_renders()
    {
        await Page.GotoAsync($"{BaseUrl}/Validation/EscortRefused");

        await Expect(Page.GetByText("has no browser form").First).ToBeVisibleAsync();
        await Expect(Page.GetByText("declare that the server alone decides it: WhenOnServer or UnlessOnServer").First).ToBeVisibleAsync();
    }

    [Test]
    public async Task conditions_joined_by_and_hold_only_while_both_do()
    {
        await Open("/Validation/Escort");
        await Write("ManyByNight", "");

        await Write("Guests", "3");
        await Write("Overnight", "true");
        await Save();
        await ExpectHeld("ManyByNight", "Needed for many guests overnight.");

        await Write("Overnight", "false");
        await Save();
        await ExpectSaved("ManyByNight", "Needed for many guests overnight.");
    }

    [Test]
    public async Task conditions_joined_by_or_hold_while_either_does()
    {
        await Open("/Validation/Escort");
        await Write("ManyOrByNight", "");

        await Write("Guests", "1");
        await Write("Overnight", "true");
        await Save();
        await ExpectHeld("ManyOrByNight", "Needed for many guests or overnight.");

        await Write("Overnight", "false");
        await Save();
        await ExpectSaved("ManyOrByNight", "Needed for many guests or overnight.");
    }
}
