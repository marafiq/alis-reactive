namespace Alis.Reactive.Poc.PlaywrightTests.DotNotation;

[TestFixture]
public sealed class WhenARosterIsHandedOver : AppPageTest
{
    [Test]
    public async Task a_label_written_through_the_shift_object_the_roster_holds_shows_on_the_roster()
    {
        await Open("/Rosters/Handover");

        await Page.GetByLabel("Shift name").FillAsync("Late shift");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Rename night shift" }).ClickAsync();

        await Expect(Page.Locator("#night-roster .roster-shift")).ToHaveTextAsync("Late shift");
        await Expect(Page.Locator("#shift-status")).ToHaveTextAsync("Renamed");
    }

    [Test]
    public async Task a_member_of_data_the_roster_holds_reads_by_the_name_the_apps_json_options_give_it()
    {
        await Open("/Rosters/Handover");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Page night on-call" }).ClickAsync();

        await Expect(Page.Locator("#night-pager")).ToHaveTextAsync("4471");
        await Expect(Page.Locator("#page-status")).ToHaveTextAsync("Paged night");
    }

    [Test]
    public async Task a_member_an_override_implements_reads_by_the_name_the_member_it_overrides_has()
    {
        await Open("/Rosters/ReadThroughAnOverride");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Show night on-call role" }).ClickAsync();

        await Expect(Page.Locator("#night-role")).ToHaveTextAsync("Charge nurse");
    }

    [Test]
    public async Task a_member_past_one_the_data_lacks_whose_start_is_null_reads_nothing_and_the_pipeline_goes_on()
    {
        await Open("/Rosters/Handover");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Page day on-call" }).ClickAsync();

        await Expect(Page.Locator("#day-pager")).ToHaveTextAsync("");
        await Expect(Page.Locator("#page-status")).ToHaveTextAsync("Paged day");
    }

    [Test]
    public async Task a_member_the_data_lacks_reads_the_start_the_endpoint_binds_for_it()
    {
        await Open("/Rosters/Handover");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Show day callback" }).ClickAsync();

        await Expect(Page.Locator("#day-callback")).ToHaveTextAsync("switchboard");
    }

    [Test]
    public async Task a_declared_member_the_roster_lacks_stops_a_read_of_data_past_it()
    {
        await Open("/Rosters/Handover");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Page relief on-call" }).ClickAsync();

        await Expect(Page.Locator("#page-status")).ToHaveTextAsync("Paging relief");
        await Expect(Page.Locator("#relief-pager")).ToHaveTextAsync("unknown");
    }
}
