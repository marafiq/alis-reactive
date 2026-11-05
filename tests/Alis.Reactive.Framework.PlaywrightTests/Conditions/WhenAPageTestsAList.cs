namespace Alis.Reactive.Framework.PlaywrightTests.Conditions;

[TestFixture]
public sealed class WhenAPageTestsAList : AppPageTest
{
    private ILocator Line(string id) => Page.Locator($"#{id}");

    private Task CheckTheRound() => Page.GetByRole(AriaRole.Button, new() { Name = "Check the round", Exact = true }).ClickAsync();

    [Test]
    public async Task a_list_holds_an_item_once_the_item_is_added()
    {
        await Open("/Conditions/Round");

        await CheckTheRound();
        await Expect(Line("has-row-3")).ToHaveTextAsync("Row 3 is not on the round");

        await Page.Locator("#round").GetByRole(AriaRole.Button, new() { Name = "Add row", Exact = true }).ClickAsync();
        await CheckTheRound();
        await Expect(Line("has-row-3")).ToHaveTextAsync("Row 3 is on the round");
    }

    [TestCase("array-row-3", TestName = "a_list_declared_as_an_array_holds_an_item_once_it_is_added")]
    [TestCase("list-row-3", TestName = "a_list_declared_as_a_list_holds_an_item_once_it_is_added")]
    [TestCase("sequence-row-3", TestName = "a_list_declared_as_a_sequence_holds_an_item_once_it_is_added")]
    [TestCase("indexed-list-row-3", TestName = "a_list_declared_as_an_indexed_list_holds_an_item_once_it_is_added")]
    [TestCase("collection-row-3", TestName = "a_list_declared_as_a_collection_holds_an_item_once_it_is_added")]
    [TestCase("read-only-collection-row-3", TestName = "a_list_declared_as_a_read_only_collection_holds_an_item_once_it_is_added")]
    [TestCase("hash-set-row-3", TestName = "a_list_declared_as_a_hash_set_holds_an_item_once_it_is_added")]
    [TestCase("set-row-3", TestName = "a_list_declared_as_a_set_holds_an_item_once_it_is_added")]
    public async Task a_list_holds_an_item_whatever_collection_it_is_declared_as(string line)
    {
        await Open("/Conditions/Round");

        await CheckTheRound();
        await Expect(Line(line)).ToHaveTextAsync("not held");

        await Page.Locator("#round").GetByRole(AriaRole.Button, new() { Name = "Add row", Exact = true }).ClickAsync();
        await CheckTheRound();
        await Expect(Line(line)).ToHaveTextAsync("held");
    }

    [Test]
    public async Task is_empty_holds_for_a_list_with_no_items()
    {
        await Open("/Conditions/Round");

        await CheckTheRound();
        await Expect(Line("round-empty")).ToHaveTextAsync("the round has residents");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Clear the round", Exact = true }).ClickAsync();
        await CheckTheRound();
        await Expect(Line("round-empty")).ToHaveTextAsync("nobody is on the round");
    }
}
