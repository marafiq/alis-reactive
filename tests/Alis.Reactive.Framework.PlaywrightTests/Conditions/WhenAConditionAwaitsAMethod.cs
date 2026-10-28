namespace Alis.Reactive.Framework.PlaywrightTests.Conditions;

/// <summary>
/// A condition whose term is a method's awaited result (M4, M5 in a condition): the result settles before
/// the condition reads it, and the method is asked only when its term is reached: after the arms before
/// it failed, after the terms before an And held, after the terms before an Or failed. The picker shows
/// each check it is asked for, so a check that was not asked leaves "Availability not checked". Of its
/// days, 2026-10-27 is free and 2026-10-28 is booked; the pain starts at 2.
/// </summary>
[TestFixture]
public sealed class WhenAConditionAwaitsAMethod : AppPageTest
{
    private ILocator Line(string id) => Page.Locator($"#{id}");

    private ILocator Picker => Page.Locator("#visit");

    private Task Pick(string day) => Picker.GetByRole(AriaRole.Button, new() { Name = $"Pick {day}", Exact = true }).ClickAsync();

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();

    private Task RaiseThePain() => Page.Locator("#pain").GetByRole(AriaRole.Button, new() { Name = "Raise", Exact = true }).ClickAsync();

    private Task LowerThePain() => Page.Locator("#pain").GetByRole(AriaRole.Button, new() { Name = "Lower", Exact = true }).ClickAsync();

    [Test]
    public async Task an_awaited_result_is_settled_before_the_condition_reads_it()
    {
        await Open("/Conditions/Availability");

        await Pick("2026-10-27");
        await Click("Is the day free?");
        await Expect(Line("free")).ToHaveTextAsync("free to book");

        await Pick("2026-10-28");
        await Click("Is the day free?");
        await Expect(Line("free")).ToHaveTextAsync("already booked");
    }

    [Test]
    public async Task an_awaited_term_of_an_else_if_is_asked_only_when_the_arms_before_it_do_not_hold()
    {
        await Open("/Conditions/Availability");

        await Click("Book the visit");
        await Expect(Line("booking")).ToHaveTextAsync("pick a day first");
        await Expect(Picker.GetByText("Availability not checked")).ToBeVisibleAsync();

        await Pick("2026-10-27");
        await Click("Book the visit");
        await Expect(Line("booking")).ToHaveTextAsync("visit booked");
        await Expect(Picker.GetByText("2026-10-27 is free")).ToBeVisibleAsync();
    }

    [Test]
    public async Task an_awaited_term_after_and_is_asked_only_when_the_terms_before_it_hold()
    {
        await Open("/Conditions/Availability");
        await Pick("2026-10-27");
        await RaiseThePain(); await RaiseThePain();

        await Click("Book transport");
        await Expect(Line("transport")).ToHaveTextAsync("no transport");
        await Expect(Picker.GetByText("Availability not checked")).ToBeVisibleAsync();

        await LowerThePain();
        await Click("Book transport");
        await Expect(Line("transport")).ToHaveTextAsync("transport booked");
        await Expect(Picker.GetByText("2026-10-27 is free")).ToBeVisibleAsync();
    }

    [Test]
    public async Task an_awaited_term_after_or_is_asked_only_when_the_terms_before_it_do_not_hold()
    {
        await Open("/Conditions/Availability");
        await Pick("2026-10-28");
        await RaiseThePain(); await RaiseThePain();

        await Click("Go ahead with the visit");
        await Expect(Line("go-ahead")).ToHaveTextAsync("the visit goes ahead");
        await Expect(Picker.GetByText("Availability not checked")).ToBeVisibleAsync();

        await LowerThePain();
        await Click("Go ahead with the visit");
        await Expect(Line("go-ahead")).ToHaveTextAsync("find another day");
        await Expect(Picker.GetByText("2026-10-28 is booked")).ToBeVisibleAsync();
    }

    [Test]
    public async Task not_over_an_awaited_term_holds_when_the_answer_is_no()
    {
        await Open("/Conditions/Availability");

        await Pick("2026-10-28");
        await Click("Is the day taken?");
        await Expect(Line("taken")).ToHaveTextAsync("that day is taken");

        await Pick("2026-10-27");
        await Click("Is the day taken?");
        await Expect(Line("taken")).ToHaveTextAsync("that day is open");
    }
}
