namespace Alis.Reactive.Framework.PlaywrightTests.Arrays;

/// <summary>
/// <c>Where</c> with each predicate the lambda compiler accepts: the six comparisons, a literal on the left,
/// a boolean member, <c>!</c>, <c>&amp;&amp;</c> and <c>||</c>, the three text tests (and a text test on a
/// member that is missing), an enum member, and a date member compared for equality and for order. Each keeps
/// the residents it holds for, in their order, and drops the others. The residents endpoint answers Cyd Marsh
/// (81, present, North), Ada Hartley (102, away, East, no nickname), Ben Hart (79, present, North) and Dot
/// Ashton (80, away, South), in that order.
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
}
