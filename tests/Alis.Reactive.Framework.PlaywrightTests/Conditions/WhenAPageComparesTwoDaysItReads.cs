namespace Alis.Reactive.Framework.PlaywrightTests.Conditions;

/// <summary>
/// A day that may be missing compared with another such day read when the condition runs: the day a
/// resident leaves against the day they arrive. The leaving day stays 2026-10-28; the arriving day moves.
/// </summary>
[TestFixture]
public sealed class WhenAPageComparesTwoDaysItReads : AppPageTest
{
    private ILocator Line(string id) => Page.Locator($"#{id}");

    private Task Arrive(string day) =>
        Page.Locator("#arrive").GetByRole(AriaRole.Button, new() { Name = $"Pick {day}", Exact = true }).ClickAsync();

    private Task Leave(string day) =>
        Page.Locator("#leave").GetByRole(AriaRole.Button, new() { Name = $"Pick {day}", Exact = true }).ClickAsync();

    private Task CheckTheStay() => Page.GetByRole(AriaRole.Button, new() { Name = "Check the stay", Exact = true }).ClickAsync();

    [Test]
    public async Task greater_than_holds_only_for_a_day_after_the_other()
    {
        await Open("/Conditions/Stay");
        await Leave("2026-10-28");

        await Arrive("2026-10-28");
        await CheckTheStay();
        await Expect(Line("later")).ToHaveTextAsync("does not leave after arriving");

        await Arrive("2026-10-27");
        await CheckTheStay();
        await Expect(Line("later")).ToHaveTextAsync("leaves after arriving");

        await Arrive("2026-10-29");
        await CheckTheStay();
        await Expect(Line("later")).ToHaveTextAsync("does not leave after arriving");
    }

    [Test]
    public async Task greater_or_equal_holds_for_the_same_day_as_the_other_and_after_it()
    {
        await Open("/Conditions/Stay");
        await Leave("2026-10-28");

        await Arrive("2026-10-28");
        await CheckTheStay();
        await Expect(Line("same-or-later")).ToHaveTextAsync("leaves on or after arriving");

        await Arrive("2026-10-29");
        await CheckTheStay();
        await Expect(Line("same-or-later")).ToHaveTextAsync("leaves before arriving");

        await Arrive("2026-10-27");
        await CheckTheStay();
        await Expect(Line("same-or-later")).ToHaveTextAsync("leaves on or after arriving");
    }

    [Test]
    public async Task less_than_holds_only_for_a_day_before_the_other()
    {
        await Open("/Conditions/Stay");
        await Leave("2026-10-28");

        await Arrive("2026-10-28");
        await CheckTheStay();
        await Expect(Line("earlier")).ToHaveTextAsync("does not leave before arriving");

        await Arrive("2026-10-29");
        await CheckTheStay();
        await Expect(Line("earlier")).ToHaveTextAsync("leaves before arriving");

        await Arrive("2026-10-27");
        await CheckTheStay();
        await Expect(Line("earlier")).ToHaveTextAsync("does not leave before arriving");
    }

    [Test]
    public async Task less_or_equal_holds_for_the_same_day_as_the_other_and_before_it()
    {
        await Open("/Conditions/Stay");
        await Leave("2026-10-28");

        await Arrive("2026-10-28");
        await CheckTheStay();
        await Expect(Line("same-or-earlier")).ToHaveTextAsync("leaves on or before arriving");

        await Arrive("2026-10-27");
        await CheckTheStay();
        await Expect(Line("same-or-earlier")).ToHaveTextAsync("leaves after arriving");

        await Arrive("2026-10-29");
        await CheckTheStay();
        await Expect(Line("same-or-earlier")).ToHaveTextAsync("leaves on or before arriving");
    }
}
