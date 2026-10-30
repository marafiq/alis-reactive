namespace Alis.Reactive.Framework.PlaywrightTests.Arrays;

/// <summary>
/// <c>Where</c> with each predicate the lambda compiler accepts: the six comparisons, a literal on the left,
/// a boolean member, <c>!</c>, <c>&amp;&amp;</c> and <c>||</c>, the three text tests (and a text test on a
/// member that is missing), an enum member, a date member compared for equality and for order; the members of
/// grammar.md 2.3 an item's values have: a date's year, month, day, hour, minute, second and day of the week, a
/// nullable's value and whether it has one, a text's length, and a widening cast; and the table's tests:
/// <c>string.IsNullOrEmpty</c>, <c>Enumerable.Contains</c> of fixed values (and a fixed array's own
/// <c>Contains</c>, which C# 14 binds to a span, and a fixed list's) and <c>Regex.IsMatch</c>; and a length of
/// time compared for equality, which its text decides as C# does. Each keeps the
/// residents it holds for, in their order, and drops the others. The residents endpoint answers Cyd Marsh (81,
/// present, North, nickname blank, keyworker Fay, admitted Monday 5 January 2026 at 09:45:30, score 7), Ada
/// Hartley (102, away, East, no nickname, Gus, admitted Saturday 14 March 2026 at midnight, no score), Ben Hart
/// (79, present, North, Benny, Fay, admitted Friday 19 June 2026 at 14:09:45, score 0) and Dot Ashton (80, away,
/// South, Dottie, Hal, admitted Sunday 2 November 2025 at 21:30:09, score 9), in that order, given 23, 26, 4 and 53
/// hours of care this week. Each date part keeps residents no other part keeps.
/// </summary>
[TestFixture]
public sealed class WhenAPageFiltersAnArray : AppPageTest
{
    private ILocator Kept => Page.Locator("#kept");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    [Test]
    public async Task greater_than_keeps_the_items_above_the_value_and_drops_the_one_at_it()
    {
        await Open("/Arrays/Filters");

        await Button("Older than 80").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Cyd Marsh,Ada Hartley");
    }

    [Test]
    public async Task at_least_keeps_the_item_at_the_value_too()
    {
        await Open("/Arrays/Filters");

        await Button("80 or older").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Cyd Marsh,Ada Hartley,Dot Ashton");
    }

    [Test]
    public async Task less_than_keeps_the_items_below_the_value_and_drops_the_one_at_it()
    {
        await Open("/Arrays/Filters");

        await Button("Younger than 80").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Ben Hart");
    }

    [Test]
    public async Task at_most_keeps_the_item_at_the_value_too()
    {
        await Open("/Arrays/Filters");

        await Button("80 or younger").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Ben Hart,Dot Ashton");
    }

    [Test]
    public async Task equal_keeps_only_the_item_with_the_value()
    {
        await Open("/Arrays/Filters");

        await Button("Exactly 80").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Dot Ashton");
    }

    [Test]
    public async Task not_equal_drops_only_the_item_with_the_value()
    {
        await Open("/Arrays/Filters");

        await Button("Not 80").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Cyd Marsh,Ada Hartley,Ben Hart");
    }

    [Test]
    public async Task a_literal_on_the_left_compares_in_the_order_it_is_written()
    {
        await Open("/Arrays/Filters");

        await Button("80 is younger than them").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Cyd Marsh,Ada Hartley");
    }

    [Test]
    public async Task a_boolean_member_keeps_the_items_it_is_true_for()
    {
        await Open("/Arrays/Filters");

        await Button("Present").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Cyd Marsh,Ben Hart");
    }

    [Test]
    public async Task not_keeps_the_items_its_test_is_false_for()
    {
        await Open("/Arrays/Filters");

        await Button("Away").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Ada Hartley,Dot Ashton");
    }

    [Test]
    public async Task and_keeps_only_the_items_both_tests_hold_for()
    {
        await Open("/Arrays/Filters");

        await Button("Present and older than 80").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Cyd Marsh");
    }

    [Test]
    public async Task or_keeps_the_items_either_test_holds_for()
    {
        await Open("/Arrays/Filters");

        await Button("Over 100 or under 80").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Ada Hartley,Ben Hart");
    }

    [Test]
    public async Task contains_keeps_the_items_whose_text_holds_the_text_anywhere()
    {
        await Open("/Arrays/Filters");

        await Button("Named Hart").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Ada Hartley,Ben Hart");
    }

    [Test]
    public async Task starts_with_keeps_the_items_whose_text_begins_with_the_captured_text()
    {
        await Open("/Arrays/Filters");

        await Button("Starting with A").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Ada Hartley");
    }

    [Test]
    public async Task ends_with_keeps_the_items_whose_text_ends_with_the_text()
    {
        await Open("/Arrays/Filters");

        await Button("Ending in Hart").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Ben Hart");
    }

    [Test]
    public async Task a_text_test_on_a_missing_member_drops_the_item_and_keeps_the_rest()
    {
        await Open("/Arrays/Filters");

        await Button("Nickname starting with D").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Dot Ashton");
    }

    [Test]
    public async Task an_enum_member_equals_the_member_it_is_compared_with()
    {
        await Open("/Arrays/Filters");

        await Button("North wing").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Cyd Marsh,Ben Hart");
    }

    [Test]
    public async Task a_date_member_equals_the_day_it_is_compared_with()
    {
        await Open("/Arrays/Filters");

        await Button("Admitted on 14 March 2026").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Ada Hartley");
    }

    [Test]
    public async Task a_date_member_compares_before_a_day_by_time()
    {
        await Open("/Arrays/Filters");

        await Button("Admitted before March 2026").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Cyd Marsh,Dot Ashton");
    }

    [Test]
    public async Task a_dates_year_keeps_the_items_admitted_in_that_year()
    {
        await Open("/Arrays/Filters");

        await Button("Admitted in 2025").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Dot Ashton");
    }

    [Test]
    public async Task a_dates_month_counts_from_1_for_january()
    {
        await Open("/Arrays/Filters");

        await Button("Admitted in January or February").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Cyd Marsh");
    }

    [Test]
    public async Task a_dates_day_is_its_day_of_the_month_not_of_the_week()
    {
        await Open("/Arrays/Filters");

        await Button("Admitted on the 5th of a month").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Cyd Marsh");
    }

    [Test]
    public async Task a_dates_hour_is_its_local_hour()
    {
        await Open("/Arrays/Filters");

        await Button("Admitted in the 9 o'clock hour").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Cyd Marsh");
    }

    [Test]
    public async Task a_dates_minute_is_its_minute()
    {
        await Open("/Arrays/Filters");

        await Button("Admitted at 9 minutes past the hour").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Ben Hart");
    }

    [Test]
    public async Task a_dates_second_is_its_second()
    {
        await Open("/Arrays/Filters");

        await Button("Admitted at 9 seconds past the minute").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Dot Ashton");
    }

    [Test]
    public async Task a_dates_day_of_the_week_equals_the_day_it_is_compared_with()
    {
        await Open("/Arrays/Filters");

        await Button("Admitted on a Friday").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Ben Hart");
    }

    [Test]
    public async Task a_nullables_value_is_the_member_itself()
    {
        await Open("/Arrays/Filters");

        await Button("Scored above 6").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Cyd Marsh,Dot Ashton");
    }

    [Test]
    public async Task a_nullables_has_value_keeps_every_item_with_a_value_and_a_zero_too()
    {
        await Open("/Arrays/Filters");

        await Button("Scored today").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Cyd Marsh,Ben Hart,Dot Ashton");
    }

    [Test]
    public async Task a_texts_length_counts_its_characters()
    {
        await Open("/Arrays/Filters");

        await Button("Named in more than 9 characters").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Ada Hartley,Dot Ashton");
    }

    [Test]
    public async Task a_widening_cast_is_read_as_the_value_itself()
    {
        await Open("/Arrays/Filters");

        await Button("Older than 100, read as a long").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Ada Hartley");
    }

    [Test]
    public async Task is_null_or_empty_keeps_the_items_whose_text_is_missing_and_those_whose_text_is_blank()
    {
        await Open("/Arrays/Filters");

        await Button("Called by no nickname").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Cyd Marsh,Ada Hartley");
    }

    [Test]
    public async Task enumerable_contains_keeps_the_items_whose_value_is_among_the_fixed_values()
    {
        await Open("/Arrays/Filters");

        await Button("In the east or south wing").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Ada Hartley,Dot Ashton");
    }

    [Test]
    public async Task an_arrays_own_contains_keeps_the_items_whose_enum_member_it_holds()
    {
        await Open("/Arrays/Filters");

        await Button("In the north or south wing").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Cyd Marsh,Ben Hart,Dot Ashton");
    }

    [Test]
    public async Task an_arrays_own_contains_keeps_the_items_whose_text_it_holds()
    {
        await Open("/Arrays/Filters");

        await Button("With a keyworker on leave").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Ada Hartley,Dot Ashton");
    }

    [Test]
    public async Task a_lists_own_contains_keeps_the_items_whose_text_it_holds()
    {
        await Open("/Arrays/Filters");

        await Button("With a keyworker on shift").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Cyd Marsh,Ben Hart");
    }

    [Test]
    public async Task regex_is_match_keeps_the_items_whose_text_matches_the_pattern()
    {
        await Open("/Arrays/Filters");

        await Button("In a room numbered in two digits").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Cyd Marsh");
    }

    [Test]
    public async Task a_length_of_time_compared_for_equality_keeps_the_item_that_holds_it()
    {
        await Open("/Arrays/Filters");

        await Button("Given 26 hours of care this week").ClickAsync();

        await Expect(Kept).ToHaveTextAsync("Ada Hartley");
    }
}
