namespace Alis.Reactive.Poc.InvariantGlobalization.PlaywrightTests.Sorts;

[TestFixture]
public sealed class WhenDotnetComparesTextInInvariantGlobalizationMode : AppPageTest
{
    [Test]
    public async Task text_with_no_comparer_orders_by_code_unit_as_dotnet_compares_it()
    {
        await Open("/Doses/SortedByCulture?culture=tr-TR");

        await Page.GetByRole(AriaRole.Button, new() { Name = "List doses by prescriber", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-names li")).ToHaveTextAsync(["Paracetamol", "Heparin", "Digoxin", "Aspirin", "Insulin"]);
    }

    [Test]
    public async Task the_current_cultures_comparer_orders_by_code_unit_as_dotnet_compares_it()
    {
        await Open("/Doses/SortedByCulture?culture=tr-TR");

        await Page.GetByRole(AriaRole.Button, new() { Name = "List doses by prescriber with the current culture's comparer", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-names li")).ToHaveTextAsync(["Paracetamol", "Heparin", "Digoxin", "Aspirin", "Insulin"]);
    }
}
