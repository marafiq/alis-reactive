namespace Alis.Reactive.Poc.PlaywrightTests.Lists;

[TestFixture]
public sealed class WhenANurseStationIsAsked : AppPageTest
{
    [Test]
    public async Task a_count_with_a_predicate_counts_only_the_beds_it_keeps()
    {
        await Open("/Stations/Questions");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count occupied beds" }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task a_station_that_holds_beds_answers_yes_to_any()
    {
        await Open("/Stations/Questions");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Any north beds" }).ClickAsync();

        await Expect(Page.Locator("#census .census-answer")).ToHaveTextAsync("yes");
    }

    [Test]
    public async Task a_station_that_holds_no_bed_answers_no_to_any()
    {
        await Open("/Stations/Questions");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Any closed beds" }).ClickAsync();

        await Expect(Page.Locator("#census .census-answer")).ToHaveTextAsync("no");
    }

    [Test]
    public async Task any_with_a_predicate_answers_yes_when_one_bed_holds_it()
    {
        await Open("/Stations/Questions");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Any north bed occupied" }).ClickAsync();

        await Expect(Page.Locator("#census .census-answer")).ToHaveTextAsync("yes");
    }

    [Test]
    public async Task any_with_a_predicate_answers_no_when_no_bed_holds_it()
    {
        await Open("/Stations/Questions");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Any quiet bed occupied" }).ClickAsync();

        await Expect(Page.Locator("#census .census-answer")).ToHaveTextAsync("no");
    }

    [Test]
    public async Task all_answers_yes_when_every_bed_holds_the_predicate()
    {
        await Open("/Stations/Questions");

        await Page.GetByRole(AriaRole.Button, new() { Name = "All west beds occupied" }).ClickAsync();

        await Expect(Page.Locator("#census .census-answer")).ToHaveTextAsync("yes");
    }

    [Test]
    public async Task all_answers_no_when_one_bed_fails_the_predicate()
    {
        await Open("/Stations/Questions");

        await Page.GetByRole(AriaRole.Button, new() { Name = "All north beds occupied" }).ClickAsync();

        await Expect(Page.Locator("#census .census-answer")).ToHaveTextAsync("no");
    }

    [Test]
    public async Task the_first_bed_a_find_keeps_takes_the_write_and_the_others_keep_theirs()
    {
        await Open("/Stations/Questions");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Check first occupied north bed" }).ClickAsync();

        await Expect(Page.Locator("#north-station .bed-status")).ToHaveTextAsync(["Ready", "Checked", "Ready"]);
    }

    [Test]
    public async Task a_write_through_a_find_that_keeps_no_bed_stops_the_steps_after_it()
    {
        await Open("/Stations/Questions");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Check first occupied quiet bed" }).ClickAsync();

        await Expect(Page.Locator("#station-status")).ToHaveTextAsync("Checking quiet");
        await Expect(Page.Locator("#quiet-station .bed-status")).ToHaveTextAsync(["Ready", "Ready"]);
    }

    [Test]
    public async Task a_find_with_a_field_reads_the_field_of_the_first_bed_it_keeps()
    {
        await Open("/Stations/Questions");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Name first occupied north bed" }).ClickAsync();

        await Expect(Page.Locator("#census .census-lead")).ToHaveTextAsync("Ama Owusu");
    }

    [Test]
    public async Task a_find_with_a_field_that_keeps_no_bed_reads_nothing_and_the_steps_after_go_on()
    {
        await Open("/Stations/Questions");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Name first occupied quiet bed" }).ClickAsync();

        await Expect(Page.Locator("#station-status")).ToHaveTextAsync("Named quiet");
        await Expect(Page.Locator("#census .census-lead")).ToHaveTextAsync("");
    }
}
