using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridCensus : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/Census";

    private const int NameColumn = 1;
    private const int AgeColumn = 2;

    private PagePlan<AgeCensusViewModel> _plan = null!;

    private FusionGridLocator Census => new(Page, "age-census");
    private ILocator Names => Census.Rows.Locator($"td[aria-colindex='{NameColumn}']");
    private ILocator Ages => Census.Rows.Locator($"td[aria-colindex='{AgeColumn}']");
    private ILocator Summary => Page.Locator("#census-summary");
    private ILocator Change => Page.Locator("#census-change");
    private NumericTextBoxLocator AgedAtLeast => _plan.NumericTextBox(m => m.MinAge);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<AgeCensusViewModel>.FromPage(Page);
        await Expect(Summary).ToHaveTextAsync("Showing 1–10 of 45 residents.", new() { Timeout = 10000 });
        await Expect(Names.First).ToHaveTextAsync("Ada Lawson", new() { Timeout = 10000 });
    }

    [Test]
    public async Task the_census_opens_on_the_first_ten_residents_by_name()
    {
        await NavigateAndBoot();

        await Expect(Census.Rows).ToHaveCountAsync(10);
        await Expect(Names.Last).ToHaveTextAsync("Dorothy Hale");
        await Expect(Change).ToHaveTextAsync("Every resident, by name.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_pager_shows_the_next_ten_residents()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Census.PagerPage(2));

        await Expect(Change).ToHaveTextAsync("On to page 2 of 5.", new() { Timeout = 10000 });
        await Expect(Summary).ToHaveTextAsync("Showing 11–20 of 45 residents.");
        await Expect(Names.First).ToHaveTextAsync("Edith Moore", new() { Timeout = 10000 });
        await Expect(Names.Last).ToHaveTextAsync("Irene Morgan");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task turning_back_a_page_says_so()
    {
        await NavigateAndBoot();
        await ClickWhenStable(Census.PagerPage(3));
        await Expect(Change).ToHaveTextAsync("On to page 3 of 5.", new() { Timeout = 10000 });
        await Expect(Summary).ToHaveTextAsync("Showing 21–30 of 45 residents.");

        await ClickWhenStable(Census.PagerPage(2));

        await Expect(Change).ToHaveTextAsync("Back to page 2 of 5.", new() { Timeout = 10000 });
        await Expect(Summary).ToHaveTextAsync("Showing 11–20 of 45 residents.");
        await Expect(Names.First).ToHaveTextAsync("Edith Moore", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task twenty_five_a_page_lists_twenty_five_residents_on_two_pages()
    {
        await NavigateAndBoot();

        await Census.ChoosePageSize(25);

        await Expect(Change).ToHaveTextAsync("25 residents a page; page 1 of 2.", new() { Timeout = 10000 });
        await Expect(Summary).ToHaveTextAsync("Showing 1–25 of 45 residents.");
        await Expect(Census.Rows).ToHaveCountAsync(25, new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sorting_by_age_lists_the_youngest_residents_first()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Census.Header("Age"));

        await Expect(Change).ToHaveTextAsync("Sorted by age, youngest first.", new() { Timeout = 10000 });
        await Expect(Ages).ToHaveTextAsync(new[] { "66", "66", "67", "67", "68", "68", "69", "69", "70", "70" }, new() { Timeout = 10000 });
        await Expect(Names.First).ToHaveTextAsync("Ada Lawson");
        await Expect(Names.Nth(1)).ToHaveTextAsync("Paul Dunn");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sorting_by_age_again_lists_the_oldest_residents_first()
    {
        await NavigateAndBoot();
        await ClickWhenStable(Census.Header("Age"));
        await Expect(Change).ToHaveTextAsync("Sorted by age, youngest first.", new() { Timeout = 10000 });

        await ClickWhenStable(Census.Header("Age"));

        await Expect(Change).ToHaveTextAsync("Sorted by age, oldest first.", new() { Timeout = 10000 });
        await Expect(Ages).ToHaveTextAsync(new[] { "98", "97", "96", "95", "94", "93", "92", "91", "90", "89" }, new() { Timeout = 10000 });
        await Expect(Names.First).ToHaveTextAsync("Oscar Flynn");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_oldest_first_order_holds_on_the_next_page()
    {
        await NavigateAndBoot();
        await ClickWhenStable(Census.Header("Age"));
        await Expect(Change).ToHaveTextAsync("Sorted by age, youngest first.", new() { Timeout = 10000 });
        await ClickWhenStable(Census.Header("Age"));
        await Expect(Change).ToHaveTextAsync("Sorted by age, oldest first.", new() { Timeout = 10000 });

        await ClickWhenStable(Census.PagerPage(2));

        await Expect(Change).ToHaveTextAsync("On to page 2 of 5.", new() { Timeout = 10000 });
        await Expect(Ages).ToHaveTextAsync(new[] { "88", "88", "87", "87", "86", "86", "85", "85", "84", "84" }, new() { Timeout = 10000 });
        await Expect(Names.First).ToHaveTextAsync("Edward Nash");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task listing_residents_aged_ninety_and_over_shows_only_them()
    {
        await NavigateAndBoot();

        await AgedAtLeast.FillAndBlur("90");

        await Expect(Change).ToHaveTextAsync("Residents aged 90 and over.", new() { Timeout = 10000 });
        await Expect(Summary).ToHaveTextAsync("Showing 1–9 of 9 residents aged 90 and over.");
        await Expect(Ages).ToHaveTextAsync(new[] { "90", "91", "92", "93", "94", "95", "96", "97", "98" }, new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_age_limit_holds_on_the_next_page()
    {
        await NavigateAndBoot();
        await AgedAtLeast.FillAndBlur("80");
        await Expect(Summary).ToHaveTextAsync("Showing 1–10 of 25 residents aged 80 and over.", new() { Timeout = 10000 });

        await ClickWhenStable(Census.PagerPage(2));

        await Expect(Change).ToHaveTextAsync("On to page 2 of 3.", new() { Timeout = 10000 });
        await Expect(Summary).ToHaveTextAsync("Showing 11–20 of 25 residents aged 80 and over.");
        await Expect(Ages).ToHaveTextAsync(new[] { "93", "94", "95", "96", "80", "97", "81", "98", "82", "83" }, new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_the_age_lists_every_resident_again()
    {
        await NavigateAndBoot();
        await AgedAtLeast.FillAndBlur("90");
        await Expect(Summary).ToHaveTextAsync("Showing 1–9 of 9 residents aged 90 and over.", new() { Timeout = 10000 });

        await AgedAtLeast.Clear();
        await AgedAtLeast.Blur();

        await Expect(Change).ToHaveTextAsync("Every resident.", new() { Timeout = 10000 });
        await Expect(Summary).ToHaveTextAsync("Showing 1–10 of 45 residents.");
        await Expect(Names.First).ToHaveTextAsync("Ada Lawson", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }
}
