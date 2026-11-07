namespace Alis.Reactive.Poc.PlaywrightTests.Patterns;

[TestFixture]
public sealed class WhenADoseTextIsMatched : AppPageTest
{
    [Test]
    public async Task a_pattern_keeps_the_doses_whose_text_it_matches()
    {
        await Open("/Doses/Patterns");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses whose medicine starts with A to D", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task an_end_anchor_matches_before_a_final_line_break_as_dotnet_reads_it()
    {
        await Open("/Doses/Patterns");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses whose instruction ends with food", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task a_text_that_holds_null_reads_nothing()
    {
        await Open("/Doses/Patterns");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses whose note is blank", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task the_culture_invariant_option_matches_as_no_option_does()
    {
        await Open("/Doses/Patterns");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses whose medicine starts with A to D, culture invariant", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task an_escaped_dollar_sign_matches_the_character()
    {
        await Open("/Doses/Patterns");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses whose instruction holds a dollar sign", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task a_pattern_the_view_holds_in_a_local_matches_as_one_written_in_place()
    {
        await Open("/Doses/Patterns");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses whose medicine starts with the initials the view holds", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task a_lookbehind_matches_as_dotnet_reads_it()
    {
        await Open("/Doses/Patterns");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses whose prescriber is a Dr I", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("1");
    }
}
