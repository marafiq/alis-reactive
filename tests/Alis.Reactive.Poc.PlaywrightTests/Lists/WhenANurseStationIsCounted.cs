namespace Alis.Reactive.Poc.PlaywrightTests.Lists;

[TestFixture]
public sealed class WhenANurseStationIsCounted : AppPageTest
{
    [Test]
    public async Task a_bed_reached_by_its_position_takes_a_write_and_the_bed_before_it_keeps_its_own()
    {
        await Open("/Stations/Census");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Mark bed 2 for cleaning" }).ClickAsync();

        await Expect(Page.Locator("#north-station .bed-status")).ToHaveTextAsync(["Ready", "Cleaning", "Ready"]);
    }

    [Test]
    public async Task each_bed_reads_its_resident_in_bed_order()
    {
        await Open("/Stations/Census");

        await Page.GetByRole(AriaRole.Button, new() { Name = "List residents" }).ClickAsync();

        await Expect(Page.Locator("#census .census-names li")).ToHaveTextAsync(["Ama Owusu", "", "Kwame Mensah"]);
    }

    [Test]
    public async Task only_the_beds_that_are_occupied_are_kept()
    {
        await Open("/Stations/Census");

        await Page.GetByRole(AriaRole.Button, new() { Name = "List occupied beds" }).ClickAsync();

        await Expect(Page.Locator("#census .census-names li")).ToHaveTextAsync(["Ama Owusu", "Kwame Mensah"]);
    }

    [Test]
    public async Task the_census_counts_every_bed_the_station_holds()
    {
        await Open("/Stations/Census");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count north beds" }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task each_bed_counts_the_alerts_its_own_list_holds()
    {
        await Open("/Stations/Census");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count alerts per bed" }).ClickAsync();

        await Expect(Page.Locator("#census .census-counts li")).ToHaveTextAsync(["1", "0", "2"]);
    }

    [Test]
    public async Task a_bed_that_lacks_a_member_the_declaration_names_stops_the_list_and_the_steps_after_it()
    {
        await Open("/Stations/Census");

        await Page.GetByRole(AriaRole.Button, new() { Name = "List south occupied beds" }).ClickAsync();

        await Expect(Page.Locator("#census-status")).ToHaveTextAsync("Listing south");
        await Expect(Page.Locator("#census .census-names li")).ToHaveCountAsync(0);
    }
}
