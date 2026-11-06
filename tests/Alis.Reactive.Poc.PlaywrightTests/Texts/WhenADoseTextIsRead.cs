namespace Alis.Reactive.Poc.PlaywrightTests.Texts;

[TestFixture]
public sealed class WhenADoseTextIsRead : AppPageTest
{
    [Test]
    public async Task a_texts_length_reads_nothing_when_the_text_reads_nothing()
    {
        await Open("/Doses/Texts");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses whose carer's name is not four letters long", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task trim_removes_the_white_space_dotnet_removes_and_keeps_what_it_keeps()
    {
        await Open("/Doses/Texts");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count labels that trim to the medicine", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("4");
    }

    [Test]
    public async Task trim_start_removes_white_space_at_the_start_only()
    {
        await Open("/Doses/Texts");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count labels that trim at the start to the medicine", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task trim_end_removes_white_space_at_the_end_only()
    {
        await Open("/Doses/Texts");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count labels that trim at the end to the medicine", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task trim_end_given_no_characters_removes_white_space_as_dotnet_does()
    {
        await Open("/Doses/Texts");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count labels that trim at the end of no characters to the medicine", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task upper_invariant_writes_each_letter_as_dotnet_writes_it()
    {
        await Open("/Doses/Texts");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count prescribers written in capitals as .NET writes them", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task lower_invariant_writes_each_letter_as_dotnet_writes_it()
    {
        await Open("/Doses/Texts");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count prescribers written in small letters as .NET writes them", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task contains_finds_text_anywhere_in_a_text()
    {
        await Open("/Doses/Texts");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count medicines with ri in their name", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task not_of_contains_does_not_hold_when_the_text_is_the_null_start_of_a_member_the_dose_lacks()
    {
        await Open("/Doses/Texts");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses whose ward name has no th", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task starts_with_by_ordinal_compares_the_first_characters_as_they_are()
    {
        await Open("/Doses/Texts");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count labels that start with Heparin or Digoxin", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task ends_with_by_ordinal_compares_the_last_characters_as_they_are()
    {
        await Open("/Doses/Texts");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count labels that end with Aspirin or Insulin", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task is_null_or_empty_holds_on_null_and_on_an_empty_text()
    {
        await Open("/Doses/Texts");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses whose note is null or empty", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task is_null_or_empty_holds_on_a_text_that_reads_nothing()
    {
        await Open("/Doses/Texts");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses whose carer's name is null or empty", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task is_null_or_white_space_holds_on_the_white_space_dotnet_accepts()
    {
        await Open("/Doses/Texts");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses whose note is blank", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("4");
    }

    [Test]
    public async Task a_text_test_given_null_to_look_for_stops_the_steps_after_it()
    {
        await Open("/Doses/Texts");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count medicines that contain their note", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census-status")).ToHaveTextAsync("Counting notes");
        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("unknown");
    }
}
