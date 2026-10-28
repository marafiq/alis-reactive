namespace Alis.Reactive.Framework.PlaywrightTests.Conditions;

/// <summary>
/// Groups at every place a condition takes one: after an And chain and after an Or chain, a group inside a
/// group after And and after Or, Not over one comparison in a group, and And and Or chains that continue
/// inside a group with a comparison or another group. Each line names its condition; pain and fall risk
/// start at 0 and the ward starts empty.
/// </summary>
[TestFixture]
public sealed class WhenConditionsAreGrouped : AppPageTest
{
    private ILocator Line(string id) => Page.Locator($"#{id}");

    private Task Decide() => Page.GetByRole(AriaRole.Button, new() { Name = "Decide", Exact = true }).ClickAsync();

    private async Task Raise(string meter, int times = 1)
    {
        for (var i = 0; i < times; i++)
            await Page.Locator($"#{meter}").GetByRole(AriaRole.Button, new() { Name = "Raise", Exact = true }).ClickAsync();
    }

    private async Task Lower(string meter, int times = 1)
    {
        for (var i = 0; i < times; i++)
            await Page.Locator($"#{meter}").GetByRole(AriaRole.Button, new() { Name = "Lower", Exact = true }).ClickAsync();
    }

    private Task Ward(string ward) => Page.GetByLabel("Ward").FillAsync(ward);

    private async Task ExpectDecided(string line, string text)
    {
        await Decide();
        await Expect(Line(line)).ToHaveTextAsync(text);
    }

    [Test]
    public async Task a_group_after_an_and_chain_is_one_more_term()
    {
        await Open("/Conditions/Groups");
        await Raise("pain", 2); await Raise("risk", 2);
        await ExpectDecided("watch", "no watch");

        await Raise("risk", 3);
        await ExpectDecided("watch", "watch");

        await Lower("pain");
        await ExpectDecided("watch", "no watch");
    }

    [Test]
    public async Task a_group_after_an_or_chain_is_one_more_alternative()
    {
        await Open("/Conditions/Groups");
        await Ward("South");
        await Raise("pain", 3); await Raise("risk", 2);
        await ExpectDecided("check-in", "no check-in");

        await Raise("risk");
        await ExpectDecided("check-in", "check in");

        await Lower("pain", 3); await Lower("risk", 3); await Ward("Isolation");
        await ExpectDecided("check-in", "check in");
    }

    [Test]
    public async Task a_group_inside_a_group_after_and_binds_its_or_first()
    {
        await Open("/Conditions/Groups");
        await Ward("South"); await Raise("pain", 3);
        await ExpectDecided("ease", "no ease");

        await Ward("North");
        await ExpectDecided("ease", "ease");

        await Ward("South"); await Raise("risk", 3);
        await ExpectDecided("ease", "ease");

        await Lower("pain");
        await ExpectDecided("ease", "no ease");
    }

    [Test]
    public async Task a_group_inside_a_group_after_or_binds_its_and_first()
    {
        await Open("/Conditions/Groups");
        await Ward("South"); await Raise("risk", 4);
        await ExpectDecided("alert", "no alert");

        await Ward("North");
        await ExpectDecided("alert", "alert");

        await Lower("risk");
        await ExpectDecided("alert", "no alert");

        await Ward("South"); await Raise("pain", 5);
        await ExpectDecided("alert", "alert");
    }

    [Test]
    public async Task not_inside_a_group_negates_its_one_comparison()
    {
        await Open("/Conditions/Groups");
        await Ward("South"); await Raise("pain", 3);
        await ExpectDecided("calm", "calm");

        await Raise("pain");
        await ExpectDecided("calm", "not calm");
    }

    [Test]
    public async Task an_and_chain_continues_inside_a_group_with_another_comparison()
    {
        await Open("/Conditions/Groups");
        await Ward("South"); await Raise("pain", 2); await Raise("risk", 2);
        await ExpectDecided("steady", "steady");

        await Raise("pain", 3);
        await ExpectDecided("steady", "not steady");

        await Lower("pain", 3); await Lower("risk");
        await ExpectDecided("steady", "not steady");
    }

    [Test]
    public async Task an_and_chain_continues_inside_a_group_with_another_group()
    {
        await Open("/Conditions/Groups");
        await Ward("East"); await Raise("pain", 2); await Raise("risk", 2);
        await ExpectDecided("round", "no round");

        await Ward("North");
        await ExpectDecided("round", "round");

        await Ward("South");
        await ExpectDecided("round", "round");
    }

    [Test]
    public async Task an_or_chain_continues_inside_a_group_with_another_comparison()
    {
        await Open("/Conditions/Groups");
        await Ward("South");
        await ExpectDecided("flag", "no flag");

        await Ward("Isolation");
        await ExpectDecided("flag", "flag");

        await Ward("South"); await Raise("risk", 5);
        await ExpectDecided("flag", "flag");
    }

    [Test]
    public async Task an_or_chain_continues_inside_a_group_with_another_group()
    {
        await Open("/Conditions/Groups");
        await Ward("Isolation");
        await ExpectDecided("visit", "no visit");

        await Raise("pain");
        await ExpectDecided("visit", "visit");

        await Ward("South");
        await ExpectDecided("visit", "no visit");
    }
}
