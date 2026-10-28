namespace Alis.Reactive.Framework.PlaywrightTests.Conditions;

/// <summary>
/// Comparisons composed: an And chain holds only when every term does, an Or chain when any does, Not
/// negates its comparison, a group after And or Or is one term whose own Or or And binds first, and Not
/// over a group holds when the group does not. Pain and fall risk start at 0; the ward starts empty.
/// </summary>
[TestFixture]
public sealed class WhenAPageComposesConditions : AppPageTest
{
    private ILocator Line(string id) => Page.Locator($"#{id}");

    private Task Triage() => Page.GetByRole(AriaRole.Button, new() { Name = "Triage", Exact = true }).ClickAsync();

    private Task Raise(string meter) => Page.Locator($"#{meter}").GetByRole(AriaRole.Button, new() { Name = "Raise", Exact = true }).ClickAsync();

    private Task Lower(string meter) => Page.Locator($"#{meter}").GetByRole(AriaRole.Button, new() { Name = "Lower", Exact = true }).ClickAsync();

    private Task Ward(string ward) => Page.GetByLabel("Ward").FillAsync(ward);

    [Test]
    public async Task and_holds_only_when_every_term_holds()
    {
        await Open("/Conditions/Triage");
        await Ward("North");
        await Raise("pain"); await Raise("pain"); await Raise("pain"); await Raise("pain");
        await Raise("risk"); await Raise("risk"); await Raise("risk");

        await Triage();
        await Expect(Line("urgent")).ToHaveTextAsync("urgent");

        await Lower("pain");
        await Triage();
        await Expect(Line("urgent")).ToHaveTextAsync("not urgent");

        await Raise("pain");
        await Triage();
        await Expect(Line("urgent")).ToHaveTextAsync("urgent");

        await Lower("risk");
        await Triage();
        await Expect(Line("urgent")).ToHaveTextAsync("not urgent");

        await Raise("risk");
        await Triage();
        await Expect(Line("urgent")).ToHaveTextAsync("urgent");

        await Ward("");
        await Triage();
        await Expect(Line("urgent")).ToHaveTextAsync("not urgent");
    }

    [Test]
    public async Task or_holds_when_any_term_holds_and_not_when_none_does()
    {
        await Open("/Conditions/Triage");

        await Triage();
        await Expect(Line("review")).ToHaveTextAsync("no review");

        await Raise("pain"); await Raise("pain"); await Raise("pain"); await Raise("pain");
        await Triage();
        await Expect(Line("review")).ToHaveTextAsync("review");

        await Lower("pain");
        await Raise("risk"); await Raise("risk"); await Raise("risk");
        await Triage();
        await Expect(Line("review")).ToHaveTextAsync("review");

        await Lower("risk");
        await Triage();
        await Expect(Line("review")).ToHaveTextAsync("no review");

        await Ward("Isolation");
        await Triage();
        await Expect(Line("review")).ToHaveTextAsync("review");
    }

    [Test]
    public async Task not_holds_when_its_comparison_does_not()
    {
        await Open("/Conditions/Triage");

        await Triage();
        await Expect(Line("comfortable")).ToHaveTextAsync("comfortable");

        await Raise("pain"); await Raise("pain");
        await Triage();
        await Expect(Line("comfortable")).ToHaveTextAsync("not comfortable");

        await Lower("pain");
        await Triage();
        await Expect(Line("comfortable")).ToHaveTextAsync("comfortable");
    }

    [Test]
    public async Task a_group_after_and_is_one_term_so_its_or_binds_first()
    {
        await Open("/Conditions/Triage");
        await Ward("South");
        await Raise("risk"); await Raise("risk"); await Raise("risk"); await Raise("risk"); await Raise("risk");

        await Triage();
        await Expect(Line("escalate")).ToHaveTextAsync("do not escalate");

        await Ward("North");
        await Triage();
        await Expect(Line("escalate")).ToHaveTextAsync("escalate");

        await Lower("risk");
        await Triage();
        await Expect(Line("escalate")).ToHaveTextAsync("do not escalate");

        await Raise("pain"); await Raise("pain"); await Raise("pain"); await Raise("pain"); await Raise("pain");
        await Triage();
        await Expect(Line("escalate")).ToHaveTextAsync("escalate");
    }

    [Test]
    public async Task a_group_after_or_is_one_term_so_its_and_binds_first()
    {
        await Open("/Conditions/Triage");
        await Ward("South");
        await Raise("risk"); await Raise("risk"); await Raise("risk"); await Raise("risk");

        await Triage();
        await Expect(Line("call-doctor")).ToHaveTextAsync("no call");

        await Ward("North");
        await Triage();
        await Expect(Line("call-doctor")).ToHaveTextAsync("call the doctor");

        await Lower("risk");
        await Triage();
        await Expect(Line("call-doctor")).ToHaveTextAsync("no call");

        await Ward("South");
        await Raise("pain"); await Raise("pain"); await Raise("pain"); await Raise("pain"); await Raise("pain");
        await Triage();
        await Expect(Line("call-doctor")).ToHaveTextAsync("call the doctor");
    }

    [Test]
    public async Task not_over_a_group_holds_when_the_group_does_not()
    {
        await Open("/Conditions/Triage");
        await Ward("North");

        await Triage();
        await Expect(Line("settled")).ToHaveTextAsync("settled");

        await Raise("risk");
        await Triage();
        await Expect(Line("settled")).ToHaveTextAsync("not settled");

        await Lower("risk");
        await Triage();
        await Expect(Line("settled")).ToHaveTextAsync("settled");

        await Raise("pain");
        await Triage();
        await Expect(Line("settled")).ToHaveTextAsync("not settled");
    }
}
