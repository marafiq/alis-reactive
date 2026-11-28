namespace Alis.Reactive.Poc.PlaywrightTests.Lists;

[TestFixture]
public sealed class WhenCareRoundsAreAsked : AppPageTest
{
    [Test]
    public async Task a_count_with_a_predicate_counts_only_the_rounds_it_keeps()
    {
        await Open("/Stations/Questions");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count due rounds" }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task a_schedule_that_holds_rounds_answers_yes_to_any()
    {
        await Open("/Stations/Questions");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Any north rounds" }).ClickAsync();

        await Expect(Page.Locator("#census .census-answer")).ToHaveTextAsync("yes");
    }

    [Test]
    public async Task a_schedule_whose_rounds_are_empty_answers_no_to_any()
    {
        await Open("/Stations/Questions");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Any closed rounds" }).ClickAsync();

        await Expect(Page.Locator("#census .census-answer")).ToHaveTextAsync("no");
    }

    [Test]
    public async Task any_over_rounds_the_schedule_lacks_reads_nothing_and_the_steps_after_go_on()
    {
        await Open("/Stations/Questions");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Any east rounds" }).ClickAsync();

        await Expect(Page.Locator("#station-status")).ToHaveTextAsync("Asked east");
        await Expect(Page.Locator("#census .census-answer")).ToHaveTextAsync("");
    }

    [Test]
    public async Task any_with_a_predicate_answers_yes_when_one_round_holds_it()
    {
        await Open("/Stations/Questions");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Any north round due" }).ClickAsync();

        await Expect(Page.Locator("#census .census-answer")).ToHaveTextAsync("yes");
    }

    [Test]
    public async Task any_with_a_predicate_answers_no_when_no_round_holds_it()
    {
        await Open("/Stations/Questions");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Any quiet round due" }).ClickAsync();

        await Expect(Page.Locator("#census .census-answer")).ToHaveTextAsync("no");
    }

    [Test]
    public async Task all_answers_yes_when_every_round_holds_the_predicate()
    {
        await Open("/Stations/Questions");

        await Page.GetByRole(AriaRole.Button, new() { Name = "All west rounds due" }).ClickAsync();

        await Expect(Page.Locator("#census .census-answer")).ToHaveTextAsync("yes");
    }

    [Test]
    public async Task all_answers_no_when_one_round_fails_the_predicate()
    {
        await Open("/Stations/Questions");

        await Page.GetByRole(AriaRole.Button, new() { Name = "All north rounds due" }).ClickAsync();

        await Expect(Page.Locator("#census .census-answer")).ToHaveTextAsync("no");
    }

    [Test]
    public async Task a_find_reads_the_first_round_it_keeps()
    {
        await Open("/Stations/Questions");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Show the next due north round" }).ClickAsync();

        await Expect(Page.Locator("#census .census-next-round")).ToHaveTextAsync("08:00");
    }

    [Test]
    public async Task a_find_that_keeps_no_round_reads_nothing_and_the_steps_after_go_on()
    {
        await Open("/Stations/Questions");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Show the next due quiet round" }).ClickAsync();

        await Expect(Page.Locator("#station-status")).ToHaveTextAsync("Found quiet round");
        await Expect(Page.Locator("#census .census-next-round")).ToHaveTextAsync("");
    }

    [Test]
    public async Task a_find_with_a_field_reads_the_field_of_the_first_round_it_keeps()
    {
        await Open("/Stations/Questions");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Carer on next north round due" }).ClickAsync();

        await Expect(Page.Locator("#census .census-lead")).ToHaveTextAsync("Ruth");
    }

    [Test]
    public async Task a_find_with_a_field_that_keeps_no_round_reads_nothing_and_the_steps_after_go_on()
    {
        await Open("/Stations/Questions");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Carer on next quiet round due" }).ClickAsync();

        await Expect(Page.Locator("#station-status")).ToHaveTextAsync("Named quiet carer");
        await Expect(Page.Locator("#census .census-lead")).ToHaveTextAsync("");
    }
}
