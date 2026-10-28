namespace Alis.Reactive.Framework.PlaywrightTests.Conditions;

/// <summary>
/// A number a component holds, compared with fixed values: each of the six comparisons, a range with both
/// bounds included, and truthiness. The meter starts at 3; each comparison is asked below, at and above it.
/// </summary>
[TestFixture]
public sealed class WhenAPageComparesANumber : AppPageTest
{
    private ILocator Line(string id) => Page.Locator($"#{id}");

    private Task Assess() => Page.GetByRole(AriaRole.Button, new() { Name = "Assess", Exact = true }).ClickAsync();

    private Task Raise() => Page.Locator("#pain").GetByRole(AriaRole.Button, new() { Name = "Raise", Exact = true }).ClickAsync();

    private Task Lower() => Page.Locator("#pain").GetByRole(AriaRole.Button, new() { Name = "Lower", Exact = true }).ClickAsync();

    [Test]
    public async Task greater_than_holds_only_above_the_value()
    {
        await Open("/Conditions/Levels");

        await Assess();
        await Expect(Line("above")).ToHaveTextAsync("not above 3");

        await Raise();
        await Assess();
        await Expect(Line("above")).ToHaveTextAsync("above 3");

        await Lower();
        await Lower();
        await Assess();
        await Expect(Line("above")).ToHaveTextAsync("not above 3");
    }

    [Test]
    public async Task greater_or_equal_holds_at_the_value_and_above_it()
    {
        await Open("/Conditions/Levels");

        await Assess();
        await Expect(Line("at-least")).ToHaveTextAsync("3 or more");

        await Lower();
        await Assess();
        await Expect(Line("at-least")).ToHaveTextAsync("less than 3");

        await Raise();
        await Raise();
        await Assess();
        await Expect(Line("at-least")).ToHaveTextAsync("3 or more");
    }

    [Test]
    public async Task less_than_holds_only_below_the_value()
    {
        await Open("/Conditions/Levels");

        await Assess();
        await Expect(Line("below")).ToHaveTextAsync("not below 3");

        await Lower();
        await Assess();
        await Expect(Line("below")).ToHaveTextAsync("below 3");

        await Raise();
        await Raise();
        await Assess();
        await Expect(Line("below")).ToHaveTextAsync("not below 3");
    }

    [Test]
    public async Task less_or_equal_holds_at_the_value_and_below_it()
    {
        await Open("/Conditions/Levels");

        await Assess();
        await Expect(Line("at-most")).ToHaveTextAsync("3 or less");

        await Raise();
        await Assess();
        await Expect(Line("at-most")).ToHaveTextAsync("more than 3");

        await Lower();
        await Lower();
        await Assess();
        await Expect(Line("at-most")).ToHaveTextAsync("3 or less");
    }

    [Test]
    public async Task equal_holds_only_at_the_value()
    {
        await Open("/Conditions/Levels");

        await Lower();
        await Assess();
        await Expect(Line("exactly")).ToHaveTextAsync("not 3");

        await Raise();
        await Assess();
        await Expect(Line("exactly")).ToHaveTextAsync("exactly 3");

        await Raise();
        await Assess();
        await Expect(Line("exactly")).ToHaveTextAsync("not 3");
    }

    [Test]
    public async Task not_equal_holds_everywhere_but_at_the_value()
    {
        await Open("/Conditions/Levels");

        await Lower();
        await Assess();
        await Expect(Line("other-than")).ToHaveTextAsync("other than 3");

        await Raise();
        await Assess();
        await Expect(Line("other-than")).ToHaveTextAsync("3 itself");

        await Raise();
        await Assess();
        await Expect(Line("other-than")).ToHaveTextAsync("other than 3");
    }

    [Test]
    public async Task between_holds_from_the_low_bound_to_the_high_bound_both_included()
    {
        await Open("/Conditions/Levels");

        await Lower();
        await Lower();
        await Assess();
        await Expect(Line("within")).ToHaveTextAsync("outside 2 to 4");

        await Raise();
        await Assess();
        await Expect(Line("within")).ToHaveTextAsync("within 2 to 4");

        await Raise();
        await Raise();
        await Raise();
        await Assess();
        await Expect(Line("within")).ToHaveTextAsync("outside 2 to 4");

        await Lower();
        await Assess();
        await Expect(Line("within")).ToHaveTextAsync("within 2 to 4");
    }

    [Test]
    public async Task truthy_holds_for_any_number_but_zero()
    {
        await Open("/Conditions/Levels");

        await Assess();
        await Expect(Line("in-pain")).ToHaveTextAsync("in pain");

        await Lower();
        await Lower();
        await Lower();
        await Assess();
        await Expect(Line("in-pain")).ToHaveTextAsync("no pain");
    }

    [Test]
    public async Task falsy_holds_only_for_zero()
    {
        await Open("/Conditions/Levels");

        await Assess();
        await Expect(Line("pain-free")).ToHaveTextAsync("not pain-free");

        await Lower();
        await Lower();
        await Lower();
        await Assess();
        await Expect(Line("pain-free")).ToHaveTextAsync("pain-free");
    }
}
