namespace Alis.Reactive.Poc.PlaywrightTests.Sorts;

[TestFixture]
public sealed class WhenDosesAreSorted : AppPageTest
{
    [Test]
    public async Task order_by_a_number_lists_the_smallest_first()
    {
        await Open("/Doses/Sorts");

        await Page.GetByRole(AriaRole.Button, new() { Name = "List doses by milligrams", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-names li")).ToHaveTextAsync(["Digoxin", "Insulin", "Aspirin", "Paracetamol", "Heparin"]);
    }

    [Test]
    public async Task order_by_descending_keeps_the_list_order_of_equal_keys()
    {
        await Open("/Doses/Sorts");

        await Page.GetByRole(AriaRole.Button, new() { Name = "List doses by route, last first", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-names li")).ToHaveTextAsync(["Aspirin", "Digoxin", "Paracetamol", "Insulin", "Heparin"]);
    }

    [Test]
    public async Task order_by_a_nullable_number_lists_null_first()
    {
        await Open("/Doses/Sorts");

        await Page.GetByRole(AriaRole.Button, new() { Name = "List doses by priority", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-names li")).ToHaveTextAsync(["Heparin", "Aspirin", "Digoxin", "Paracetamol", "Insulin"]);
    }

    [Test]
    public async Task order_by_descending_a_nullable_number_lists_null_last()
    {
        await Open("/Doses/Sorts");

        await Page.GetByRole(AriaRole.Button, new() { Name = "List doses by priority, highest first", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-names li")).ToHaveTextAsync(["Insulin", "Digoxin", "Paracetamol", "Aspirin", "Heparin"]);
    }

    [Test]
    public async Task order_by_text_with_the_ordinal_comparer_orders_by_code_unit()
    {
        await Open("/Doses/Sorts");

        await Page.GetByRole(AriaRole.Button, new() { Name = "List doses by prescriber, code by code", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-names li")).ToHaveTextAsync(["Paracetamol", "Heparin", "Digoxin", "Aspirin", "Insulin"]);
    }

    [Test]
    public async Task order_by_a_bool_lists_false_first()
    {
        await Open("/Doses/Sorts");

        await Page.GetByRole(AriaRole.Button, new() { Name = "List doses, controlled last", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-names li")).ToHaveTextAsync(["Aspirin", "Insulin", "Paracetamol", "Digoxin", "Heparin"]);
    }
}
