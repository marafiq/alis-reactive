namespace Alis.Reactive.Framework.PlaywrightTests.Conditions;

[TestFixture]
public sealed class WhenAConditionCallsAMethod : AppPageTest
{
    private ILocator Line(string id) => Page.Locator($"#{id}");

    private ILocator Picker => Page.Locator("#visit");

    private ILocator Dispenser => Page.Locator("#dispenser");

    private Task Pick(string day) => Picker.GetByRole(AriaRole.Button, new() { Name = $"Pick {day}", Exact = true }).ClickAsync();

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();

    private Task RaiseThePain() => Page.Locator("#pain").GetByRole(AriaRole.Button, new() { Name = "Raise", Exact = true }).ClickAsync();

    private Task LowerThePain() => Page.Locator("#pain").GetByRole(AriaRole.Button, new() { Name = "Lower", Exact = true }).ClickAsync();

    [Test]
    public async Task a_method_term_of_an_else_if_is_called_only_when_the_arms_before_it_do_not_hold()
    {
        await Open("/Conditions/Calendar");

        await Click("Plan the visit");
        await Expect(Line("visit-plan")).ToHaveTextAsync("pick a day first");
        await Expect(Picker.GetByText("Availability not checked")).ToBeVisibleAsync();

        await Pick("2026-10-27");
        await Click("Plan the visit");
        await Expect(Line("visit-plan")).ToHaveTextAsync("visit planned");
        await Expect(Picker.GetByText("2026-10-27 is open")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_method_term_after_and_is_called_only_when_the_terms_before_it_hold()
    {
        await Open("/Conditions/Calendar");
        await Pick("2026-10-27");
        await RaiseThePain(); await RaiseThePain();

        await Click("Book transport");
        await Expect(Line("transport")).ToHaveTextAsync("no transport");
        await Expect(Picker.GetByText("Availability not checked")).ToBeVisibleAsync();

        await LowerThePain();
        await Click("Book transport");
        await Expect(Line("transport")).ToHaveTextAsync("transport booked");
        await Expect(Picker.GetByText("2026-10-27 is open")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_method_term_after_or_is_called_only_when_the_terms_before_it_do_not_hold()
    {
        await Open("/Conditions/Calendar");
        await Pick("2026-10-28");
        await RaiseThePain(); await RaiseThePain();

        await Click("Go ahead with the visit");
        await Expect(Line("go-ahead")).ToHaveTextAsync("the visit goes ahead");
        await Expect(Picker.GetByText("Availability not checked")).ToBeVisibleAsync();

        await LowerThePain();
        await Click("Go ahead with the visit");
        await Expect(Line("go-ahead")).ToHaveTextAsync("find another day");
        await Expect(Picker.GetByText("2026-10-28 is closed")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_method_term_inside_a_group_is_called_only_when_the_group_is_reached()
    {
        await Open("/Conditions/Calendar");
        await Pick("2026-10-27");
        await RaiseThePain(); await RaiseThePain();

        await Click("Book an escort");
        await Expect(Line("escort")).ToHaveTextAsync("no escort");
        await Expect(Picker.GetByText("Availability not checked")).ToBeVisibleAsync();

        await LowerThePain();
        await Click("Book an escort");
        await Expect(Line("escort")).ToHaveTextAsync("escort booked");
        await Expect(Picker.GetByText("2026-10-27 is open")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_method_called_on_its_own_line_runs_there_whether_or_not_the_condition_reaches_it()
    {
        await Open("/Conditions/Calendar");
        await Pick("2026-10-27");
        await RaiseThePain(); await RaiseThePain();

        await Click("Set a reminder");
        await Expect(Line("reminder")).ToHaveTextAsync("no reminder");
        await Expect(Picker.GetByText("2026-10-27 is open")).ToBeVisibleAsync();

        await LowerThePain();
        await Click("Set a reminder");
        await Expect(Line("reminder")).ToHaveTextAsync("reminder set");
    }

    [Test]
    public async Task a_result_its_condition_and_its_then_both_read_is_called_once_each_time_the_pipeline_runs()
    {
        await Open("/Conditions/Queue");

        await Click("Take a number");
        await Expect(Line("your-number")).ToHaveTextAsync("1");
        await Expect(Dispenser).ToHaveTextAsync("Next number: 2");

        await Click("Take a number");
        await Expect(Line("your-number")).ToHaveTextAsync("2");
        await Expect(Dispenser).ToHaveTextAsync("Next number: 3");
    }
}
