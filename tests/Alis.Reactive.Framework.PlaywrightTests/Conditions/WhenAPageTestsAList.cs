namespace Alis.Reactive.Framework.PlaywrightTests.Conditions;

/// <summary>
/// A list a component holds, tested: whether it holds an item, and whether it is empty. The slate's rows
/// are the residents on the round; its own Add row button adds "Row 3".
/// </summary>
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
