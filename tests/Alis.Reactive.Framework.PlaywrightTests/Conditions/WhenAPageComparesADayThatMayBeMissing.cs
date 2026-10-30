namespace Alis.Reactive.Framework.PlaywrightTests.Conditions;

/// <summary>
/// A day that may be missing (a <c>DateTime?</c> the picker holds, nothing until a day is picked), tested
/// for null and compared with a fixed day, the open day 2026-10-27: as a calendar day, ordered by its
/// time, in a range, and, when missing, unequal to any day and neither before nor after one. The page
/// tests the day each time the picker changes.
/// </summary>
[TestFixture]
public sealed class WhenAPageComparesADayThatMayBeMissing : AppPageTest
{
    private ILocator Line(string id) => Page.Locator($"#{id}");

    private Task Pick(string day) =>
        Page.Locator("#visit").GetByRole(AriaRole.Button, new() { Name = $"Pick {day}", Exact = true }).ClickAsync();

    private Task ClearTheDay() =>
        Page.Locator("#visit").GetByRole(AriaRole.Button, new() { Name = "Clear the day", Exact = true }).ClickAsync();

    [Test]
    public async Task is_null_holds_while_no_day_is_picked()
    {
        await Open("/Conditions/Visit");

        await Pick("2026-10-27");
        await Expect(Line("none-picked")).ToHaveTextAsync("a day is picked");

        await ClearTheDay();
        await Expect(Line("none-picked")).ToHaveTextAsync("pick a day");
    }

    [Test]
    public async Task not_null_holds_once_a_day_is_picked()
    {
        await Open("/Conditions/Visit");

        await Pick("2026-10-27");
        await Expect(Line("some-picked")).ToHaveTextAsync("ready to book");

        await ClearTheDay();
        await Expect(Line("some-picked")).ToHaveTextAsync("nothing to book");
    }

    [Test]
    public async Task a_day_equals_the_same_calendar_day()
    {
        await Open("/Conditions/Visit");

        await Pick("2026-10-27");
        await Expect(Line("open-day")).ToHaveTextAsync("the open day");

        await Pick("2026-10-28");
        await Expect(Line("open-day")).ToHaveTextAsync("not the open day");
    }

    [Test]
    public async Task not_equal_holds_for_another_day_and_for_no_day()
    {
        await Open("/Conditions/Visit");

        await Pick("2026-10-28");
        await Expect(Line("another-day")).ToHaveTextAsync("another day");

        await Pick("2026-10-27");
        await Expect(Line("another-day")).ToHaveTextAsync("the open day itself");

        await ClearTheDay();
        await Expect(Line("another-day")).ToHaveTextAsync("another day");
    }

    [Test]
    public async Task less_than_holds_only_for_an_earlier_day()
    {
        await Open("/Conditions/Visit");

        await Pick("2026-10-27");
        await Expect(Line("before")).ToHaveTextAsync("not before the open day");

        await Pick("2026-10-26");
        await Expect(Line("before")).ToHaveTextAsync("before the open day");

        await Pick("2026-10-28");
        await Expect(Line("before")).ToHaveTextAsync("not before the open day");
    }

    [Test]
    public async Task less_or_equal_holds_for_the_day_and_an_earlier_one()
    {
        await Open("/Conditions/Visit");

        await Pick("2026-10-27");
        await Expect(Line("by")).ToHaveTextAsync("by the open day");

        await Pick("2026-10-28");
        await Expect(Line("by")).ToHaveTextAsync("not by the open day");

        await Pick("2026-10-26");
        await Expect(Line("by")).ToHaveTextAsync("by the open day");
    }

    [Test]
    public async Task greater_than_holds_only_for_a_later_day()
    {
        await Open("/Conditions/Visit");

        await Pick("2026-10-27");
        await Expect(Line("after")).ToHaveTextAsync("not after the open day");

        await Pick("2026-10-28");
        await Expect(Line("after")).ToHaveTextAsync("after the open day");

        await Pick("2026-10-26");
        await Expect(Line("after")).ToHaveTextAsync("not after the open day");
    }

    [Test]
    public async Task greater_or_equal_holds_for_the_day_and_a_later_one()
    {
        await Open("/Conditions/Visit");

        await Pick("2026-10-27");
        await Expect(Line("from")).ToHaveTextAsync("from the open day on");

        await Pick("2026-10-26");
        await Expect(Line("from")).ToHaveTextAsync("not from the open day on");

        await Pick("2026-10-28");
        await Expect(Line("from")).ToHaveTextAsync("from the open day on");
    }

    [Test]
    public async Task a_missing_day_is_neither_before_nor_after_any_day()
    {
        await Open("/Conditions/Visit");

        await Pick("2026-10-26");
        await Expect(Line("before")).ToHaveTextAsync("before the open day");
        await Expect(Line("by")).ToHaveTextAsync("by the open day");
        await ClearTheDay();
        await Expect(Line("before")).ToHaveTextAsync("not before the open day");
        await Expect(Line("by")).ToHaveTextAsync("not by the open day");

        await Pick("2026-10-28");
        await Expect(Line("after")).ToHaveTextAsync("after the open day");
        await Expect(Line("from")).ToHaveTextAsync("from the open day on");
        await ClearTheDay();
        await Expect(Line("after")).ToHaveTextAsync("not after the open day");
        await Expect(Line("from")).ToHaveTextAsync("not from the open day on");
    }

    [Test]
    public async Task between_holds_from_the_first_day_to_the_last_both_included_and_never_for_no_day()
    {
        await Open("/Conditions/Visit");

        await Pick("2026-10-26");
        await Expect(Line("open-week")).ToHaveTextAsync("outside the open week");

        await Pick("2026-10-27");
        await Expect(Line("open-week")).ToHaveTextAsync("in the open week");

        await Pick("2026-10-30");
        await Expect(Line("open-week")).ToHaveTextAsync("outside the open week");

        await Pick("2026-10-29");
        await Expect(Line("open-week")).ToHaveTextAsync("in the open week");

        await ClearTheDay();
        await Expect(Line("open-week")).ToHaveTextAsync("outside the open week");
    }
}
