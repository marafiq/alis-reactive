namespace Alis.Reactive.Poc.PlaywrightTests.Lists;

[TestFixture]
public sealed class WhenCareRoundsAreListed : AppPageTest
{
    [Test]
    public async Task only_the_rounds_still_due_are_kept_and_each_reads_its_carer_by_the_name_the_apps_json_options_give_it()
    {
        await Open("/Stations/Census");

        await Page.GetByRole(AriaRole.Button, new() { Name = "List carers on due rounds" }).ClickAsync();

        await Expect(Page.Locator("#census .census-names li")).ToHaveTextAsync(["Ruth", "Abena"]);
    }

    [Test]
    public async Task a_round_that_lacks_its_carer_reads_no_name_and_the_rounds_around_it_keep_theirs()
    {
        await Open("/Stations/Census");

        await Page.GetByRole(AriaRole.Button, new() { Name = "List carers on west rounds" }).ClickAsync();

        await Expect(Page.Locator("#census .census-names li")).ToHaveTextAsync(["Yaw", "", "Esi"]);
        await Expect(Page.Locator("#census-status")).ToHaveTextAsync("Listed west");
    }

    [Test]
    public async Task the_census_counts_every_round_the_schedule_holds()
    {
        await Open("/Stations/Census");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count north rounds" }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task rounds_the_schedule_lacks_count_nothing_and_the_steps_after_go_on()
    {
        await Open("/Stations/Census");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count east rounds" }).ClickAsync();

        await Expect(Page.Locator("#census-status")).ToHaveTextAsync("Counted east");
        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("");
    }

    [Test]
    public async Task rounds_held_as_a_paged_result_instead_of_a_list_stop_the_count_and_the_steps_after_it()
    {
        await Open("/Stations/Census");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count south rounds" }).ClickAsync();

        await Expect(Page.Locator("#census-status")).ToHaveTextAsync("Counting south");
        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("unknown");
    }
}
