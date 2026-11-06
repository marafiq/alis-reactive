namespace Alis.Reactive.Poc.PlaywrightTests.Lookups;

[TestFixture]
public sealed class WhenADoseIsLookedUp : AppPageTest
{
    [Test]
    public async Task contains_over_an_array_the_view_holds_keeps_the_items_it_lists()
    {
        await Open("/Doses/Lookups");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses in the listed wards", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task contains_over_an_enum_array_compares_as_the_apps_json_writes_it()
    {
        await Open("/Doses/Lookups");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses with a listed status", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task contains_over_a_list_the_view_holds_keeps_the_items_it_lists()
    {
        await Open("/Doses/Lookups");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses in the southern wards", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task contains_over_an_array_typed_as_a_sequence_keeps_the_items_it_lists()
    {
        await Open("/Doses/Lookups");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses at an urgent priority", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task contains_a_char_finds_it_in_a_text()
    {
        await Open("/Doses/Lookups");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count medicines with the letter o", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task a_lists_count_property_counts_its_items()
    {
        await Open("/Doses/Lookups");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses checked more than once", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task an_arrays_length_counts_its_items()
    {
        await Open("/Doses/Lookups");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses given more than once a day", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }
}
