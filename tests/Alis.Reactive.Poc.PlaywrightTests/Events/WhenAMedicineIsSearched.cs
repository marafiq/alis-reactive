namespace Alis.Reactive.Poc.PlaywrightTests.Events;

[TestFixture]
public sealed class WhenAMedicineIsSearched : AppPageTest
{
    [Test]
    public async Task a_payload_member_at_a_position_reads_that_argument_of_the_listener()
    {
        await Open("/Signals/Search");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Search for In" }).ClickAsync();

        await Expect(Page.Locator("#query")).ToHaveTextAsync("In");
    }

    [Test]
    public async Task a_method_at_a_position_is_the_vendors_continuation_and_runs_with_the_pages_items()
    {
        await Open("/Signals/Search");
        await Expect(Page.Locator("#search .search-results")).ToHaveTextAsync("no results");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Search for In" }).ClickAsync();

        await Expect(Page.Locator("#search .search-results")).ToHaveTextAsync("Insulin, Heparin");
    }

    [Test]
    public async Task an_argument_at_a_position_read_on_a_component_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Signals/ArgumentOfAComponent");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: An argument at a position is what an event's listener receives: declare it on the event's payload.");
    }
}
