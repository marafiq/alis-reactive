namespace Alis.Reactive.Poc.PlaywrightTests.Comparisons;

[TestFixture]
public sealed class WhenASourceIsReadInItsOwnJson : AppPageTest
{
    [Test]
    public async Task a_found_member_its_own_converter_writes_equals_a_value_written_as_that_converter_writes_it()
    {
        await Open("/Doses/OwnJsonSources");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Check the digoxin's chart", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#verdict")).ToHaveTextAsync("charted as given");
    }

    [Test]
    public async Task each_item_of_a_list_a_select_takes_from_such_a_member_is_compared_in_its_form()
    {
        await Open("/Doses/OwnJsonSources");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count the charts that read given", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task a_fallback_for_such_a_member_is_written_in_its_form()
    {
        await Open("/Doses/OwnJsonSources");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Check an unfound dose's chart", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#verdict")).ToHaveTextAsync("charted as given");
    }

    [Test]
    public async Task a_value_chosen_beside_such_a_member_is_written_in_its_form()
    {
        await Open("/Doses/OwnJsonSources");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Check the chosen chart", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#verdict")).ToHaveTextAsync("charted as given");
    }

    [Test]
    public async Task a_components_data_read_to_such_a_member_is_compared_in_its_form()
    {
        await Open("/Doses/OwnJsonSources");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Check the featured dose's chart", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#verdict")).ToHaveTextAsync("charted as given");
    }
}
