using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As the evening nursing supervisor, I want to find any resident in the community directory by name, wing,
/// care level, risk or nurse, and open their card, so that I can answer a family's call and plan tonight's
/// rounds without walking to the nurses' station.
/// </summary>
/// <remarks>
/// The directory lists Maple Grove's 48 residents, surname first, eight to a page; the server answers every
/// page, sort, filter, search and grouping the grid asks for, says how many residents match and what the last
/// change did, and refuses a request it cannot read. Abbott, Margaret (North wing, Independent) heads page 1;
/// Brooks, Harold is the first resident on the East wing. Picking a resident opens their card; Next resident
/// steps down the page, and the card says how it was opened.
/// Not exercised:
/// DataStateChange's, RecordClick's and RowSelected's Name: EJ2 writes each event's own name on its payload
/// (ej2-base observer.js:92); a handler is attached to one event and has nothing to decide by it.
/// DataStateChange's Action Name, Type and Cancel: Action is the actionBegin payload EJ2 already ran
/// (actions/data.js:607), so they read actionBegin and false whenever the event comes.
/// RecordClick's RowIndex: the row a click lands on is the row the click selects, whose index the page reads
/// from RowSelected (selection.js:292-296, grid.js:2575-2583).
/// </remarks>
[TestFixture]
public class WhenUsingFusionGridDirectory : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/Directory";
    private const string GridId = "resident-directory-grid";

    // The grid numbers a row's cells from 0.
    private const int ResidentCell = 1;
    private const int CareLevelCell = 3;
    private const int WingCell = 4;
    private const int RiskCell = 6;

    private PagePlan<DirectoryViewModel> _plan = null!;

    private ILocator Rows => Page.Locator($"#{GridId} .e-row");
    private ILocator Cell(int row, int cell) => Rows.Nth(row).Locator(".e-rowcell").Nth(cell);
    private ILocator Captions => Page.Locator($"#{GridId} .e-groupcaption");
    private ILocator PagerPage(string number) => Page.Locator($"#{GridId} .e-pager .e-numericitem").Filter(new() { HasText = number });
    private ILocator FilterBar(string field) => Page.Locator($"#{field}_filterBarcell");
    private ILocator Summary => Page.Locator("#directory-summary");
    private ILocator Change => Page.Locator("#directory-change");
    private ILocator Card => Page.Locator("#resident-card");
    private ILocator CardName => Page.Locator("#card-name");
    private ILocator CardContext => Page.Locator("#card-context");
    private ILocator CardTip => Page.Locator("#card-tip");
    private ILocator RoundsStatus => Page.Locator("#rounds-status");
    private FusionTextBoxLocator Find => new(Page, _plan.FindComponent(m => m.Find)!.ElementId);
    private ILocator GroupByCareLevel => _plan.NativeCheckBox(m => m.GroupByCareLevel).Input;
    private ILocator GroupByWing => _plan.NativeCheckBox(m => m.GroupByWing).Input;

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<DirectoryViewModel>.FromPage(Page);
        await Expect(Summary).ToHaveTextAsync("Showing 1–8 of 48 residents.", new() { Timeout = 10000 });
        await Expect(Cell(0, ResidentCell)).ToHaveTextAsync("Abbott, Margaret", new() { Timeout = 10000 });
    }

    private async Task EveryShownResident(int cell, string text)
    {
        await Expect(Rows).ToHaveCountAsync(8, new() { Timeout = 10000 });
        for (var row = 0; row < 8; row++)
            await Expect(Cell(row, cell)).ToHaveTextAsync(text);
    }

    private async Task PickResident(int row, string name)
    {
        await ClickWhenStable(Cell(row, ResidentCell));
        await Expect(CardName).ToHaveTextAsync(name, new() { Timeout = 10000 });
    }

    [Test]
    public async Task the_directory_opens_on_the_first_eight_residents_by_surname()
    {
        await NavigateAndBoot();

        await Expect(Rows).ToHaveCountAsync(8);
        await Expect(Cell(7, ResidentCell)).ToHaveTextAsync("Diaz, Samuel");
        await Expect(Change).ToHaveTextAsync("Every resident, in surname order.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_pager_shows_the_next_eight_residents()
    {
        await NavigateAndBoot();

        await ClickWhenStable(PagerPage("2"));

        await Expect(Summary).ToHaveTextAsync("Showing 9–16 of 48 residents.", new() { Timeout = 10000 });
        await Expect(Change).ToHaveTextAsync("Page 2 of 6.");
        await Expect(Cell(0, ResidentCell)).ToHaveTextAsync("Dubois, Renée", new() { Timeout = 10000 });
        await Expect(Cell(7, ResidentCell)).ToHaveTextAsync("Hughes, Frank");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_s_to_z_tab_opens_the_page_where_those_surnames_start()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Page.Locator("#surnames-s-z"));

        await Expect(Summary).ToHaveTextAsync("Showing 33–40 of 48 residents.", new() { Timeout = 10000 });
        await Expect(Change).ToHaveTextAsync("Page 5 of 6.");
        await Expect(Cell(4, ResidentCell)).ToHaveTextAsync("Shah, Priya", new() { Timeout = 10000 });
        await Expect(PagerPage("5")).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("e-active"));
        AssertNoConsoleErrors();
    }

    // The grid keeps several sorts at once; Highest risk first replaces the name sort instead of adding to it.
    [Test]
    public async Task highest_risk_first_replaces_the_name_sort_with_the_high_risk_residents_on_top()
    {
        await NavigateAndBoot();
        await ClickWhenStable(Page.Locator($"#{GridId} .e-headercell").Filter(new() { HasText = "Resident" }));
        await Expect(Change).ToHaveTextAsync("Sorted by name, A to Z.", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#highest-risk-first"));

        await Expect(Change).ToHaveTextAsync("Sorted by risk, highest first.", new() { Timeout = 10000 });
        await Expect(Cell(0, ResidentCell)).ToHaveTextAsync("Brooks, Harold", new() { Timeout = 10000 });
        await EveryShownResident(RiskCell, "High");
        await Expect(Summary).ToHaveTextAsync("Showing 1–8 of 48 residents.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task surname_order_undoes_the_risk_sort()
    {
        await NavigateAndBoot();
        await ClickWhenStable(Page.Locator("#highest-risk-first"));
        await Expect(Cell(0, ResidentCell)).ToHaveTextAsync("Brooks, Harold", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#surname-order"));

        await Expect(Change).ToHaveTextAsync("Back in surname order.", new() { Timeout = 10000 });
        await Expect(Cell(0, ResidentCell)).ToHaveTextAsync("Abbott, Margaret", new() { Timeout = 10000 });
        await Expect(Cell(0, RiskCell)).ToHaveTextAsync("Low");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_north_wing_view_lists_only_north_wing_residents()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Page.Locator("#north-wing"));

        await Expect(Summary).ToHaveTextAsync("Showing 1–8 of 12 residents.", new() { Timeout = 10000 });
        await Expect(Change).ToHaveTextAsync("Filtered: wing is \"North\".");
        await EveryShownResident(WingCell, "North");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task every_wing_brings_the_whole_directory_back()
    {
        await NavigateAndBoot();
        await ClickWhenStable(Page.Locator("#north-wing"));
        await Expect(Summary).ToHaveTextAsync("Showing 1–8 of 12 residents.", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#every-wing"));

        await Expect(Summary).ToHaveTextAsync("Showing 1–8 of 48 residents.", new() { Timeout = 10000 });
        await Expect(Change).ToHaveTextAsync("Filters cleared.");
        await Expect(Cell(4, WingCell)).ToHaveTextAsync("East", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    // The filter bar compares from the start of the wing, whatever the case typed: "s" is South only.
    [Test]
    public async Task typing_a_lowercase_s_in_the_wing_filter_shows_the_south_wing()
    {
        await NavigateAndBoot();

        await FilterBar("wing").PressSequentiallyAsync("s");
        await FilterBar("wing").PressAsync("Enter");

        await Expect(Summary).ToHaveTextAsync("Showing 1–8 of 12 residents.", new() { Timeout = 10000 });
        await Expect(Change).ToHaveTextAsync("Filtered: wing starts with \"s\".");
        await EveryShownResident(WingCell, "South");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_surname_typed_without_its_accent_finds_the_resident()
    {
        await NavigateAndBoot();

        await FilterBar("residentName").PressSequentiallyAsync("alv");
        await FilterBar("residentName").PressAsync("Enter");

        await Expect(Summary).ToHaveTextAsync("Showing 1–1 of 1 resident.", new() { Timeout = 10000 });
        await Expect(Rows).ToHaveCountAsync(1, new() { Timeout = 10000 });
        await Expect(Cell(0, ResidentCell)).ToHaveTextAsync("Álvarez, José");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task finding_memory_care_lists_only_memory_care_residents()
    {
        await NavigateAndBoot();

        await Find.FillAndBlur("memory care");

        await Expect(Summary).ToHaveTextAsync("Showing 1–8 of 12 residents.", new() { Timeout = 10000 });
        await Expect(Change).ToHaveTextAsync("Searched for \"memory care\".");
        await EveryShownResident(CareLevelCell, "Memory Care");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_name_typed_without_its_accent_is_found()
    {
        await NavigateAndBoot();

        await Find.FillAndBlur("renee");

        await Expect(Summary).ToHaveTextAsync("Showing 1–1 of 1 resident.", new() { Timeout = 10000 });
        await Expect(Rows).ToHaveCountAsync(1, new() { Timeout = 10000 });
        await Expect(Cell(0, ResidentCell)).ToHaveTextAsync("Dubois, Renée");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_the_search_brings_every_resident_back()
    {
        await NavigateAndBoot();
        await Find.FillAndBlur("memory care");
        await Expect(Summary).ToHaveTextAsync("Showing 1–8 of 12 residents.", new() { Timeout = 10000 });

        await Find.FillAndBlur("");

        await Expect(Summary).ToHaveTextAsync("Showing 1–8 of 48 residents.", new() { Timeout = 10000 });
        await Expect(Change).ToHaveTextAsync("Search cleared.");
        await Expect(Cell(0, CareLevelCell)).ToHaveTextAsync("Independent", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task grouping_by_care_level_lists_every_resident_under_their_care_level()
    {
        await NavigateAndBoot();

        await GroupByCareLevel.CheckAsync();

        await Expect(Summary).ToHaveTextAsync("Showing all 48 residents.", new() { Timeout = 10000 });
        await Expect(Change).ToHaveTextAsync("Grouped by care level.");
        await Expect(Captions).ToHaveTextAsync(new[]
        {
            "Care level: Assisted Living - 12 items",
            "Care level: Independent - 12 items",
            "Care level: Memory Care - 12 items",
            "Care level: Skilled Nursing - 12 items"
        }, new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_the_care_level_box_returns_the_plain_list()
    {
        await NavigateAndBoot();
        await GroupByCareLevel.CheckAsync();
        await Expect(Captions).ToHaveCountAsync(4, new() { Timeout = 10000 });

        await GroupByCareLevel.UncheckAsync();

        await Expect(Change).ToHaveTextAsync("Grouping cleared.", new() { Timeout = 10000 });
        await Expect(Summary).ToHaveTextAsync("Showing 1–8 of 48 residents.");
        await Expect(Captions).ToHaveCountAsync(0, new() { Timeout = 10000 });
        await Expect(Cell(0, ResidentCell)).ToHaveTextAsync("Abbott, Margaret");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task no_grouping_clears_both_groupings_and_both_boxes()
    {
        await NavigateAndBoot();
        await GroupByCareLevel.CheckAsync();
        await Expect(Change).ToHaveTextAsync("Grouped by care level.", new() { Timeout = 10000 });
        await GroupByWing.CheckAsync();
        await Expect(Change).ToHaveTextAsync("Grouped by care level then wing.", new() { Timeout = 10000 });
        await Expect(Captions.Filter(new() { HasText = "Wing: East - 3 items" }).First).ToBeVisibleAsync(new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#no-grouping"));

        await Expect(Change).ToHaveTextAsync("Grouping cleared.", new() { Timeout = 10000 });
        await Expect(Captions).ToHaveCountAsync(0, new() { Timeout = 10000 });
        await Expect(Summary).ToHaveTextAsync("Showing 1–8 of 48 residents.");
        await Expect(GroupByCareLevel).Not.ToBeCheckedAsync();
        await Expect(GroupByWing).Not.ToBeCheckedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task picking_a_resident_opens_their_card_without_filtering_the_directory()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Cell(0, ResidentCell));

        await Expect(Card).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(CardName).ToHaveTextAsync("Margaret Abbott");
        await Expect(Page.Locator("#card-details"))
            .ToHaveTextAsync("Independent · North wing, suite N-101 · 0 open tasks · primary nurse Nora Ellis");
        await Expect(CardContext).ToHaveTextAsync("Opened from the directory.");
        await Expect(CardTip).ToBeVisibleAsync();
        await Expect(Summary).ToHaveTextAsync("Showing 1–8 of 48 residents.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task picking_a_second_resident_retires_the_rounds_tip()
    {
        await NavigateAndBoot();
        await PickResident(0, "Margaret Abbott");
        await Expect(CardTip).ToBeVisibleAsync();

        await PickResident(1, "Yara Ahmed");

        await Expect(CardTip).ToBeHiddenAsync();
        await Expect(CardContext).ToHaveTextAsync("Opened from the directory.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task next_resident_opens_the_card_of_the_resident_below()
    {
        await NavigateAndBoot();
        await PickResident(0, "Margaret Abbott");

        await ClickWhenStable(Page.Locator("#next-resident"));

        await Expect(CardName).ToHaveTextAsync("Yara Ahmed", new() { Timeout = 10000 });
        await Expect(CardContext).ToHaveTextAsync("Next on your rounds: resident 2 on this page.");
        await Expect(CardTip).ToBeHiddenAsync();
        await Expect(Rows.Nth(1)).ToHaveAttributeAsync("aria-selected", "true");
        await Expect(Rows.Nth(0)).Not.ToHaveAttributeAsync("aria-selected", "true");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task adding_the_resident_to_tonights_rounds_names_them()
    {
        await NavigateAndBoot();
        await PickResident(0, "Margaret Abbott");

        await ClickWhenStable(Page.Locator("#add-to-rounds"));

        await Expect(RoundsStatus).ToHaveTextAsync("Margaret Abbott is on tonight's rounds.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task closing_the_card_takes_the_highlight_off_the_resident()
    {
        await NavigateAndBoot();
        await PickResident(0, "Margaret Abbott");
        await Expect(Rows.Nth(0)).ToHaveAttributeAsync("aria-selected", "true");

        await ClickWhenStable(Page.Locator("#close-card"));

        await Expect(Card).ToBeHiddenAsync();
        await Expect(Rows.Nth(0)).Not.ToHaveAttributeAsync("aria-selected", "true", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_a_residents_wing_shows_everyone_on_that_wing()
    {
        await NavigateAndBoot();
        await Expect(Cell(4, ResidentCell)).ToHaveTextAsync("Brooks, Harold");

        await ClickWhenStable(Cell(4, WingCell));

        await Expect(Summary).ToHaveTextAsync("Showing 1–8 of 12 residents.", new() { Timeout = 10000 });
        await Expect(Change).ToHaveTextAsync("Filtered: wing is \"East\".");
        await EveryShownResident(WingCell, "East");
        AssertNoConsoleErrors();
    }
}
