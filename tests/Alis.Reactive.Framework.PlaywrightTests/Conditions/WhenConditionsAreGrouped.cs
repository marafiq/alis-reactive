namespace Alis.Reactive.Framework.PlaywrightTests.Conditions;

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

    [Test]
    public async Task a_group_after_an_and_chain_is_one_more_term()
    {
        await Open("/Conditions/Groups");
        await Raise("pain", 2); await Raise("risk", 2);
        await Decide();
        await Expect(Line("watch")).ToHaveTextAsync("no watch");

        await Raise("risk", 3);
        await Decide();
        await Expect(Line("watch")).ToHaveTextAsync("watch");

        await Lower("pain");
        await Decide();
        await Expect(Line("watch")).ToHaveTextAsync("no watch");
    }

    [Test]
    public async Task a_group_after_an_or_chain_is_one_more_alternative()
    {
        await Open("/Conditions/Groups");
        await Ward("South");
        await Raise("pain", 3); await Raise("risk", 2);
        await Decide();
        await Expect(Line("check-in")).ToHaveTextAsync("no check-in");

        await Raise("risk");
        await Decide();
        await Expect(Line("check-in")).ToHaveTextAsync("check in");

        await Lower("pain", 3); await Lower("risk", 3); await Ward("Isolation");
        await Decide();
        await Expect(Line("check-in")).ToHaveTextAsync("check in");
    }

    [Test]
    public async Task a_group_inside_a_group_after_and_binds_its_or_first()
    {
        await Open("/Conditions/Groups");
        await Ward("South"); await Raise("pain", 3);
        await Decide();
        await Expect(Line("ease")).ToHaveTextAsync("no ease");

        await Ward("North");
        await Decide();
        await Expect(Line("ease")).ToHaveTextAsync("ease");

        await Ward("South"); await Raise("risk", 3);
        await Decide();
        await Expect(Line("ease")).ToHaveTextAsync("ease");

        await Lower("pain");
        await Decide();
        await Expect(Line("ease")).ToHaveTextAsync("no ease");
    }

    [Test]
    public async Task a_group_inside_a_group_after_or_binds_its_and_first()
    {
        await Open("/Conditions/Groups");
        await Ward("South"); await Raise("risk", 4);
        await Decide();
        await Expect(Line("alert")).ToHaveTextAsync("no alert");

        await Ward("North");
        await Decide();
        await Expect(Line("alert")).ToHaveTextAsync("alert");

        await Lower("risk");
        await Decide();
        await Expect(Line("alert")).ToHaveTextAsync("no alert");

        await Ward("South"); await Raise("pain", 5);
        await Decide();
        await Expect(Line("alert")).ToHaveTextAsync("alert");
    }

    [Test]
    public async Task not_inside_a_group_negates_its_one_comparison()
    {
        await Open("/Conditions/Groups");
        await Ward("South"); await Raise("pain", 3);
        await Decide();
        await Expect(Line("calm")).ToHaveTextAsync("calm");

        await Raise("pain");
        await Decide();
        await Expect(Line("calm")).ToHaveTextAsync("not calm");
    }

    [Test]
    public async Task an_and_chain_continues_inside_a_group_with_another_comparison()
    {
        await Open("/Conditions/Groups");
        await Ward("South"); await Raise("pain", 2); await Raise("risk", 2);
        await Decide();
        await Expect(Line("steady")).ToHaveTextAsync("steady");

        await Raise("pain", 3);
        await Decide();
        await Expect(Line("steady")).ToHaveTextAsync("not steady");

        await Lower("pain", 3); await Lower("risk");
        await Decide();
        await Expect(Line("steady")).ToHaveTextAsync("not steady");
    }

    [Test]
    public async Task an_and_chain_continues_inside_a_group_with_another_group()
    {
        await Open("/Conditions/Groups");
        await Ward("East"); await Raise("pain", 2); await Raise("risk", 2);
        await Decide();
        await Expect(Line("round")).ToHaveTextAsync("no round");

        await Ward("North");
        await Decide();
        await Expect(Line("round")).ToHaveTextAsync("round");

        await Ward("South");
        await Decide();
        await Expect(Line("round")).ToHaveTextAsync("round");
    }

    [Test]
    public async Task an_or_chain_continues_inside_a_group_with_another_comparison()
    {
        await Open("/Conditions/Groups");
        await Ward("South");
        await Decide();
        await Expect(Line("flag")).ToHaveTextAsync("no flag");

        await Ward("Isolation");
        await Decide();
        await Expect(Line("flag")).ToHaveTextAsync("flag");

        await Ward("South"); await Raise("risk", 5);
        await Decide();
        await Expect(Line("flag")).ToHaveTextAsync("flag");
    }

    [Test]
    public async Task an_or_chain_continues_inside_a_group_with_another_group()
    {
        await Open("/Conditions/Groups");
        await Ward("Isolation");
        await Decide();
        await Expect(Line("visit")).ToHaveTextAsync("no visit");

        await Raise("pain");
        await Decide();
        await Expect(Line("visit")).ToHaveTextAsync("visit");

        await Ward("South");
        await Decide();
        await Expect(Line("visit")).ToHaveTextAsync("no visit");
    }
}
