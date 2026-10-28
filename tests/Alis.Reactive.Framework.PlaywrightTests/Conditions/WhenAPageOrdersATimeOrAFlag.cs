namespace Alis.Reactive.Framework.PlaywrightTests.Conditions;

/// <summary>
/// An ordering of values the grammar orders by their kind (grammar.md 2.2): a time of day the address gives,
/// which the plan reads as its text, ordered as C# orders the times; and a method's flag, ordered false before
/// true as C# orders a bool.
/// </summary>
[TestFixture]
public sealed class WhenAPageOrdersATimeOrAFlag : AppPageTest
{
    private ILocator Line(string id) => Page.Locator($"#{id}");

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();

    [Test]
    public async Task a_time_of_day_orders_before_a_later_one()
    {
        await Open("/Conditions/Shifts?starts=07:00:00");

        await Expect(Line("shift")).ToHaveTextAsync("a morning shift");
    }

    [Test]
    public async Task a_time_of_day_does_not_order_before_an_earlier_one()
    {
        await Open("/Conditions/Shifts?starts=14:30:00");

        await Expect(Line("shift")).ToHaveTextAsync("not a morning shift");
    }

    [Test]
    public async Task a_flag_orders_true_after_false_and_false_not_after_false()
    {
        await Open("/Conditions/Shifts");

        await Click("Is Ada on the rota?");
        await Expect(Line("on-rota")).ToHaveTextAsync("Ada is on the rota");

        await Click("Is Zed on the rota?");
        await Expect(Line("on-rota")).ToHaveTextAsync("Zed is not on the rota");
    }
}
