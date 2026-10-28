using System.Text.RegularExpressions;

namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

/// <summary>
/// A rule written under a condition applies only while the condition holds, in the browser as on the server,
/// for every form a condition the browser can read takes: the six comparisons with the member on either
/// side, a value the validator fixes, null, empty text, the three text tests and a pattern, a fixed array and
/// list, a nullable's value, a flag, a negation, a nested member, a list member holding an item, and the
/// conditions joined by and and or. A child validator's rules apply only while the condition written on the
/// child holds, and each condition of its own reads the child's inputs.
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
    [TestCase("NoAnnex", "Wing", "East", "East Annex", "Needed when the wing is no annex.", TestName = "a_negated_text_test_holds_for_text_without_the_value")]
    [TestCase("EastNamedFirst", "Wing", "East", "West", "Needed when East is the wing.", TestName = "a_value_on_the_left_equal_to_the_member_holds_at_the_value")]
    [TestCase("GuestOfNote", "Tags", "vip", "garden", "Needed for a guest of note.", TestName = "a_list_member_holding_a_fixed_item_holds_while_the_list_holds_it")]
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
    public async Task an_ordering_against_nothing_is_refused_where_the_page_renders()
    {
        await Page.GotoAsync($"{BaseUrl}/Validation/EscortRefused?unreadable=OrderAgainstNothing");

        await Expect(Page.GetByText(new Regex("The condition '.*NoGuestLimit.*' has no browser form")).First).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_text_method_the_browser_has_no_form_of_is_refused_where_the_page_renders()
    {
        await Page.GotoAsync($"{BaseUrl}/Validation/EscortRefused?unreadable=TextEquals");

        await Expect(Page.GetByText(new Regex("The condition '.*x\\.Wing\\.Equals\\(\"East\"\\).*' has no browser form")).First).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_childs_rule_applies_only_while_the_condition_written_on_the_child_holds()
    {
        await Open("/Validation/Escort");
        await Write("Companion name", "");

        await Write("Overnight", "true");
        await Save();
        await ExpectHeld("Companion name", "Name the companion.");

        await Write("Overnight", "false");
        await Save();
        await ExpectSaved("Companion name", "Name the companion.");
    }

    [Test]
    public async Task a_childs_rule_under_its_own_condition_applies_only_while_that_condition_and_the_one_on_the_child_both_hold()
    {
        await Open("/Validation/Escort");
        await Write("Companion room", "");

        await Write("Overnight", "true");
        await Write("Companion stay", "overnight");
        await Save();
        await ExpectHeld("Companion room", "Give the companion's room for the night.");

        await Write("Companion stay", "day");
        await Save();
        await ExpectSaved("Companion room", "Give the companion's room for the night.");

        await Write("Companion stay", "overnight");
        await Write("Overnight", "false");
        await Save();
        await ExpectSaved("Companion room", "Give the companion's room for the night.");
    }

    [Test]
    public async Task a_childs_conditions_joined_by_and_read_the_childs_own_inputs()
    {
        await Open("/Validation/Escort");
        await Write("Companion phone", "");
        await Write("Overnight", "true");
        await Write("Companion stay", "overnight");

        await Write("Companion kind", "carer");
        await Save();
        await ExpectHeld("Companion phone", "Give the night carer's phone.");

        await Write("Companion kind", "family");
        await Save();
        await ExpectSaved("Companion phone", "Give the night carer's phone.");
    }

    [Test]
    public async Task a_childs_conditions_joined_by_or_read_the_childs_own_inputs()
    {
        await Open("/Validation/Escort");
        await Write("Companion badge", "");
        await Write("Overnight", "true");

        await Write("Companion kind", "nurse");
        await Save();
        await ExpectHeld("Companion badge", "Give the companion's badge.");

        await Write("Companion kind", "family");
        await Save();
        await ExpectSaved("Companion badge", "Give the companion's badge.");
    }

    [Test]
    public async Task a_childs_negated_condition_reads_the_childs_own_input()
    {
        await Open("/Validation/Escort");
        await Write("Companion pass", "");
        await Write("Overnight", "true");

        await Write("Companion kind", "carer");
        await Save();
        await ExpectHeld("Companion pass", "Give a pass to a companion who is not family.");

        await Write("Companion kind", "family");
        await Save();
        await ExpectSaved("Companion pass", "Give a pass to a companion who is not family.");
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
