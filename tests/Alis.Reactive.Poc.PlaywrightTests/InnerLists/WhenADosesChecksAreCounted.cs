namespace Alis.Reactive.Poc.PlaywrightTests.InnerLists;

[TestFixture]
public sealed class WhenADosesChecksAreCounted : AppPageTest
{
    [Test]
    public async Task an_inner_lambda_reads_the_outer_item_by_its_own_name()
    {
        await Open("/Doses/Checks");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses their carer also checked", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task any_with_a_predicate_inside_a_lambda_keeps_the_items_an_inner_item_matches()
    {
        await Open("/Doses/Checks");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses Kofi checked", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task all_inside_a_lambda_holds_when_every_inner_item_matches_and_on_an_empty_list()
    {
        await Open("/Doses/Checks");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses Kofi did not check", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task count_inside_a_lambda_counts_the_inner_items()
    {
        await Open("/Doses/Checks");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses checked more than once", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task any_inside_a_lambda_holds_on_a_list_with_an_item()
    {
        await Open("/Doses/Checks");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses anyone checked", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("4");
    }

    [Test]
    public async Task count_with_a_predicate_inside_a_lambda_counts_the_inner_items_it_keeps()
    {
        await Open("/Doses/Checks");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses checked once by someone other than Kofi", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("4");
    }
}
