namespace Alis.Reactive.Framework.PlaywrightTests.Conditions;

/// <summary>
/// A condition that asks the user (<c>p.Confirm</c>, and <c>g.Confirm</c> in a group): Then runs when they
/// agree and Else when they decline, and a question in a group is asked only when its term is reached.
/// The app answers <c>alis.confirm</c> with its own dialog. The pain starts at 3.
/// </summary>
[TestFixture]
public sealed class WhenAConditionAsksTheUser : AppPageTest
{
    private ILocator Line(string id) => Page.Locator($"#{id}");

    private ILocator Dialog => Page.GetByRole(AriaRole.Dialog, new() { Name = "Confirm" });

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();

    private Task Answer(string answer) => Dialog.GetByRole(AriaRole.Button, new() { Name = answer, Exact = true }).ClickAsync();

    private Task RaiseThePain() => Page.Locator("#pain").GetByRole(AriaRole.Button, new() { Name = "Raise", Exact = true }).ClickAsync();

    private Task LowerThePain() => Page.Locator("#pain").GetByRole(AriaRole.Button, new() { Name = "Lower", Exact = true }).ClickAsync();

    [Test]
    public async Task a_question_runs_then_when_the_user_agrees_and_else_when_they_decline()
    {
        await Open("/Conditions/Decisions");

        await Click("Discharge");
        await Expect(Dialog).ToContainTextAsync("Discharge the resident?");
        await Answer("Yes");
        await Expect(Line("discharge")).ToHaveTextAsync("discharged");

        await Click("Discharge");
        await Answer("No");
        await Expect(Line("discharge")).ToHaveTextAsync("kept in");
    }

    [Test]
    public async Task a_question_after_and_is_asked_only_when_the_terms_before_it_hold()
    {
        await Open("/Conditions/Decisions");

        await Click("Escalate");
        await Expect(Line("doctor")).ToHaveTextAsync("no call");
        await Expect(Dialog).ToBeHiddenAsync();

        await RaiseThePain();
        await Click("Escalate");
        await Expect(Dialog).ToContainTextAsync("Call the doctor?");
        await Answer("Yes");
        await Expect(Line("doctor")).ToHaveTextAsync("doctor called");
    }

    [Test]
    public async Task a_question_after_or_is_asked_only_when_the_terms_before_it_do_not_hold()
    {
        await Open("/Conditions/Decisions");
        await LowerThePain(); await LowerThePain();

        await Click("Plan pain relief");
        await Expect(Line("relief")).ToHaveTextAsync("relief skipped");
        await Expect(Dialog).ToBeHiddenAsync();

        await RaiseThePain();
        await Click("Plan pain relief");
        await Expect(Dialog).ToContainTextAsync("Skip the pain relief?");
        await Answer("No");
        await Expect(Line("relief")).ToHaveTextAsync("relief given");
    }
}
