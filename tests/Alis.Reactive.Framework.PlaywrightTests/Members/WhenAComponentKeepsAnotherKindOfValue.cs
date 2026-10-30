namespace Alis.Reactive.Framework.PlaywrightTests.Members;

/// <summary>
/// A component's member read as the type its declaration names when the component keeps another kind of value
/// (P9, grammar.md 2.1): a number read as a flag is false only at zero, a list as a flag is true once it holds
/// an item, a day as a flag is true and nothing is not, a flag read as a number counts one or none, and a
/// timestamp read as a day arrives as a day where a method takes one.
/// </summary>
[TestFixture]
public sealed class WhenAComponentKeepsAnotherKindOfValue : AppPageTest
{
    private ILocator Line(string id) => Page.Locator($"#{id}");

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();

    [Test]
    public async Task a_number_read_as_a_flag_is_false_only_at_zero()
    {
        await Open("/Members/Kinds");

        await Click("Limit the slate to 3");
        await Click("Is the slate limited?");
        await Expect(Line("limited")).ToHaveTextAsync("limited");

        await Click("Limit the slate to 0");
        await Click("Is the slate limited?");
        await Expect(Line("limited")).ToHaveTextAsync("not limited");
    }

    [Test]
    public async Task a_list_read_as_a_flag_is_true_once_it_holds_an_item()
    {
        await Open("/Members/Kinds");

        await Click("Does the slate hold a card?");
        await Expect(Line("holds-cards")).ToHaveTextAsync("holds no card");

        await Click("Fill a card");
        await Click("Does the slate hold a card?");
        await Expect(Line("holds-cards")).ToHaveTextAsync("holds a card");
    }

    [Test]
    public async Task a_day_read_as_a_flag_is_true_and_no_day_is_not()
    {
        await Open("/Members/Kinds");

        await Click("Has a line been placed?");
        await Expect(Line("placed")).ToHaveTextAsync("nothing is placed");

        await Click("Place a line with no flag");
        await Click("Has a line been placed?");
        await Expect(Line("placed")).ToHaveTextAsync("a line is placed");
    }

    [Test]
    public async Task a_flag_read_as_a_number_counts_one_for_true_and_none_for_false()
    {
        await Open("/Members/Kinds");

        await Click("Place a flagged line on the day the slate started");
        await Click("Count the placed flag");
        await Expect(Line("flags")).ToHaveTextAsync("1");

        await Click("Place a line with no flag");
        await Click("Count the placed flag");
        await Expect(Line("flags")).ToHaveTextAsync("0");
    }

    [Test]
    public async Task a_timestamp_read_as_a_day_arrives_as_a_day_where_a_method_takes_one()
    {
        await Open("/Members/Kinds");

        await Click("Place a flagged line on the day the slate started");

        await Expect(Page.Locator("#slate").GetByText("Ada · 1.0 · yes · 2026-10-27")).ToBeVisibleAsync();
    }
}
