namespace Alis.Reactive.Poc.PlaywrightTests.Sorts;

[TestFixture]
public sealed class WhenDosesAreSortedByTheViewsCulture : AppPageTest
{
    [Test]
    public async Task text_with_no_comparer_orders_by_the_views_culture()
    {
        await Open("/Doses/SortedByCulture?culture=en-US");

        await Page.GetByRole(AriaRole.Button, new() { Name = "List doses by prescriber", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-names li")).ToHaveTextAsync(["Paracetamol", "Digoxin", "Aspirin", "Insulin", "Heparin"]);
    }

    [Test]
    public async Task text_orders_by_the_turkish_alphabet_when_the_view_renders_under_turkish()
    {
        await Open("/Doses/SortedByCulture?culture=tr-TR");

        await Page.GetByRole(AriaRole.Button, new() { Name = "List doses by prescriber", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-names li")).ToHaveTextAsync(["Digoxin", "Paracetamol", "Aspirin", "Insulin", "Heparin"]);
    }

    [Test]
    public async Task the_invariant_culture_orders_text_as_english_does()
    {
        await Open("/Doses/SortedByCulture");

        await Page.GetByRole(AriaRole.Button, new() { Name = "List doses by prescriber", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-names li")).ToHaveTextAsync(["Paracetamol", "Digoxin", "Aspirin", "Insulin", "Heparin"]);
    }

    [Test]
    public async Task descending_lists_the_last_text_in_the_views_culture_first()
    {
        await Open("/Doses/SortedByCulture?culture=tr-TR");

        await Page.GetByRole(AriaRole.Button, new() { Name = "List doses by prescriber, last first", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-names li")).ToHaveTextAsync(["Heparin", "Insulin", "Aspirin", "Paracetamol", "Digoxin"]);
    }

    [Test]
    public async Task the_current_cultures_comparer_orders_as_no_comparer_does()
    {
        await Open("/Doses/SortedByCulture?culture=tr-TR");

        await Page.GetByRole(AriaRole.Button, new() { Name = "List doses by prescriber with the current culture's comparer", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-names li")).ToHaveTextAsync(["Digoxin", "Paracetamol", "Aspirin", "Insulin", "Heparin"]);
    }

    [Test]
    public async Task a_culture_the_browser_has_no_collation_for_stops_the_sort_and_the_steps_after_it()
    {
        await Open("/Doses/SortedByCulture?culture=haw-US");

        await Page.GetByRole(AriaRole.Button, new() { Name = "List doses by prescriber", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census-status")).ToHaveTextAsync("Listing by prescriber");
        await Expect(Page.Locator("#census .census-names li")).ToHaveCountAsync(0);
    }
}
