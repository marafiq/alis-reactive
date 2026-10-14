using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As the business office manager preparing the quarterly state census, I want to list the residents at or
/// over an age, sort the census by any column and page through it, with the server answering every page, so
/// that the counts I send the state are the community's own records and not a copy in my browser.
/// </summary>
/// <remarks>
/// Maple Grove's census holds 45 residents, by name: Ada Lawson first, Edith Moore first on page two. Ages run
/// 66 to 98; Oscar Flynn, 98, is the oldest, and Ada Lawson and Paul Dunn, 66, the youngest. Nine residents are
/// 90 or older and 25 are 80 or older. The grid asks the server for every page, sort and page size through its
/// DataStateChange event; the age box asks the grid to ask again. The server answers with the page, how many
/// residents the census counts, and what the change was, worked out from what the grid sent: the rows to skip
/// and take, whether it needs the count, the sort, the page moved to and from, the page size, and the age.
/// Not exercised: DataStateChange's Name, which EJ2 writes as the event's own name on every payload
/// (ej2-base observer.js:92), and its Action's Name, Type and Cancel: the Action is the actionBegin payload EJ2
/// already ran (ej2-grids actions/data.js:609), so they read actionBegin, actionBegin and false whenever the
/// event comes, and no server answer can depend on them.
/// </remarks>
[TestFixture]
public class WhenUsingFusionGridCensus : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/Census";

    // The grid numbers its columns from 1.
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
