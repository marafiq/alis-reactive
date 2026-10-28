namespace Alis.Reactive.Framework.PlaywrightTests.Conditions;

/// <summary>
/// A number compared with another value read when the condition runs: the pain score against a comfort
/// goal the nurse moves on a second meter. Both start at 3; the goal is moved between comparisons, so each
/// answer is the goal as it stands when the condition runs.
/// </summary>
[TestFixture]
public sealed class WhenAPageComparesTwoNumbersItReads : AppPageTest
{
    private ILocator Line(string id) => Page.Locator($"#{id}");

    private Task Compare() => Page.GetByRole(AriaRole.Button, new() { Name = "Compare with the goal", Exact = true }).ClickAsync();

    private Task RaiseTheGoal() => Page.Locator("#goal").GetByRole(AriaRole.Button, new() { Name = "Raise", Exact = true }).ClickAsync();

    private Task LowerTheGoal() => Page.Locator("#goal").GetByRole(AriaRole.Button, new() { Name = "Lower", Exact = true }).ClickAsync();

    [Test]
    public async Task equal_holds_when_the_other_value_is_the_same_when_the_condition_runs()
    {
        await Open("/Conditions/Goal");

        await Compare();
        await Expect(Line("on-goal")).ToHaveTextAsync("on the goal");

        await RaiseTheGoal();
        await Compare();
        await Expect(Line("on-goal")).ToHaveTextAsync("not on the goal");
    }

    [Test]
    public async Task not_equal_holds_when_the_other_value_differs_when_the_condition_runs()
    {
        await Open("/Conditions/Goal");

        await Compare();
        await Expect(Line("off-goal")).ToHaveTextAsync("not off the goal");

        await LowerTheGoal();
        await Compare();
        await Expect(Line("off-goal")).ToHaveTextAsync("off the goal");
    }

    [Test]
    public async Task greater_than_holds_only_above_the_other_value()
    {
        await Open("/Conditions/Goal");

        await Compare();
        await Expect(Line("over-goal")).ToHaveTextAsync("not over the goal");

        await LowerTheGoal();
        await Compare();
        await Expect(Line("over-goal")).ToHaveTextAsync("over the goal");

        await RaiseTheGoal();
        await RaiseTheGoal();
        await Compare();
        await Expect(Line("over-goal")).ToHaveTextAsync("not over the goal");
    }

    [Test]
    public async Task greater_or_equal_holds_at_the_other_value_and_above_it()
    {
        await Open("/Conditions/Goal");

        await Compare();
        await Expect(Line("goal-or-over")).ToHaveTextAsync("at or over the goal");

        await RaiseTheGoal();
        await Compare();
        await Expect(Line("goal-or-over")).ToHaveTextAsync("under the goal");

        await LowerTheGoal();
        await LowerTheGoal();
        await Compare();
        await Expect(Line("goal-or-over")).ToHaveTextAsync("at or over the goal");
    }

    [Test]
    public async Task less_than_holds_only_below_the_other_value()
    {
        await Open("/Conditions/Goal");

        await Compare();
        await Expect(Line("under-goal")).ToHaveTextAsync("not under the goal");

        await RaiseTheGoal();
        await Compare();
        await Expect(Line("under-goal")).ToHaveTextAsync("under the goal");

        await LowerTheGoal();
        await LowerTheGoal();
        await Compare();
        await Expect(Line("under-goal")).ToHaveTextAsync("not under the goal");
    }

    [Test]
    public async Task less_or_equal_holds_at_the_other_value_and_below_it()
    {
        await Open("/Conditions/Goal");

        await Compare();
        await Expect(Line("goal-or-under")).ToHaveTextAsync("at or under the goal");

        await LowerTheGoal();
        await Compare();
        await Expect(Line("goal-or-under")).ToHaveTextAsync("over the goal");

        await RaiseTheGoal();
        await RaiseTheGoal();
        await Compare();
        await Expect(Line("goal-or-under")).ToHaveTextAsync("at or under the goal");
    }
}
