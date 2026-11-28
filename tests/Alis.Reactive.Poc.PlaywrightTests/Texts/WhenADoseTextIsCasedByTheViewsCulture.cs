namespace Alis.Reactive.Poc.PlaywrightTests.Texts;

[TestFixture]
public sealed class WhenADoseTextIsCasedByTheViewsCulture : AppPageTest
{
    [Test]
    public async Task to_upper_under_a_culture_capitalises_the_dotless_i()
    {
        await Open("/Doses/Casing?culture=en-US");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count prescribers written DR I\u015EIK or DR WEI\u00DF in capitals", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task to_upper_under_a_turkish_culture_dots_the_capital_i()
    {
        await Open("/Doses/Casing?culture=tr-TR");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count prescribers written DR I\u015EIK or DR WE\u0130\u00DF in capitals", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task to_upper_under_the_invariant_culture_keeps_the_dotless_i()
    {
        await Open("/Doses/Casing");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count prescribers written DR I\u015E\u0131K in capitals", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task to_lower_under_a_culture_lowers_the_dotted_capital_i()
    {
        await Open("/Doses/Casing?culture=en-US");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count prescribers written dr ince or dr i\u015F\u0131k in small letters", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task to_lower_under_a_turkish_culture_lowers_the_capital_i_to_the_dotless_i()
    {
        await Open("/Doses/Casing?culture=tr-TR");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count prescribers written dr ince or dr \u0131\u015F\u0131k in small letters", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task to_lower_under_the_invariant_culture_keeps_the_dotted_capital_i()
    {
        await Open("/Doses/Casing");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count prescribers written dr \u0130nce in small letters", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("1");
    }
}
