using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As the business office manager closing October's billing, I want to page, sort and narrow the month's
/// charges on the server, correct a rate in the grid, raise or bill the residents I pick, open a new resident's
/// charges and email the families their statements, so that every family is billed what the community charges
/// and the board always shows what is still outstanding.
/// </summary>
/// <remarks>
/// The board holds 36 residents, twelve to a page by name: Abigail Moore (Independent, $3,200 a month, pending)
/// first and Grace Bennett last on page one, Priya Shah first on page three. Care levels run in turn, so nine
/// residents are in Memory Care, and a third each are pending, billed and paid; $147,000 is outstanding. Each
/// change goes to the server, which answers in a toast, and the grid then asks the server again for the page,
/// sort and filters it shows (its DataStateChange, on Refresh). The charges live in the browser's session, so
/// every test starts from the month as filed.
/// Not exercised:
/// ToolbarClick's Item Text, Cancel and Name: the board tells its Email statements item apart by Id; Cancel
/// stops only the grid's built-in items (toolbar.js:504-549), and the board's column chooser item is one the
/// page leaves to the grid; Name is the event's own name (ej2-base observer.js:92).
/// DataStateChange's Name, RequiresCounts, Group, Where, Search and Action: the board pages and sorts on the
/// server and keeps its own filters; WhenUsingFusionGridCensus and WhenUsingFusionGridDirectory read the rest.
/// The exports, Print, AutoFitColumns and OpenColumnChooser the old board called are proven where a page uses
/// them: WhenUsingFusionGridExport, WhenUsingFusionGridPrintableRoster, WhenUsingFusionGridColumnFit and
/// WhenUsingFusionGridTooling.
/// </remarks>
[TestFixture]
public class WhenUsingFusionGridBilling : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/Billing";
    private const string GridId = "billing-grid";
    private const string WholeBoard = "36 residents: $147,000 outstanding.";

    // The grid numbers its columns from 1.
    private const int ResidentColumn = 2;
    private const int MonthlyRateColumn = 5;
    private const int BalanceDueColumn = 7;
    private const int StatusColumn = 8;

    private PagePlan<ResidentBillingViewModel> _plan = null!;

    private FusionGridLocator Board => new(Page, GridId);
    private ILocator Summary => Page.Locator("#billing-summary");
    private ILocator Toast => Page.Locator("#alisFusionToast .e-toast");
    private ILocator RateEditor => Page.Locator($"#{GridId}monthlyRate");
    private ILocator EmailStatements => Page.Locator($"#{GridId} .e-toolbar-item").Filter(new() { HasText = "Email statements" });
    private ILocator ApplyFilters => Page.Locator("#billing-apply-filters");
    private ILocator UnbilledOnly => Page.Locator("#billing-quick-unbilled");
    private ILocator MemoryCare => Page.Locator("#billing-quick-memory");
    private ILocator ClearFilters => Page.Locator("#billing-clear-filters");
    private ILocator AddCharges => Page.Locator("#billing-add-open");
    private ILocator OpenCharges => Page.Locator("#add-charge-save");
    private ILocator CancelCharges => Page.Locator("#add-charge-cancel");
    private ILocator AddChargeDialog => Page.Locator("#add-charge-dialog");
    private ILocator SaveRateChanges => Page.Locator("#billing-save-all");
    private ILocator RaiseSelectedRates => Page.Locator("#billing-bulk-increase");
    private ILocator MarkSelectedBilled => Page.Locator("#billing-mark-billed");

    private DropDownListLocator CareLevelFilter => _plan.DropDownList(m => m.FilterCareLevel);
    private DropDownListLocator WingFilter => _plan.DropDownList(m => m.FilterWing);
    private DropDownListLocator StatusFilter => _plan.DropDownList(m => m.FilterStatus);
    private FusionTextBoxLocator NewResidentName => new(Page, IdGenerator.For<ResidentBillingViewModel>(m => m.NewResidentName));
    private DropDownListLocator NewCareLevel => _plan.DropDownList(m => m.NewCareLevel);
    private NumericTextBoxLocator NewMonthlyRate => _plan.NumericTextBox(m => m.NewMonthlyRate);
    private NumericTextBoxLocator NewAddOnCharges => _plan.NumericTextBox(m => m.NewAddOnCharges);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<ResidentBillingViewModel>.FromPage(Page);
        await Expect(Summary).ToHaveTextAsync(WholeBoard, new() { Timeout = 10000 });
        await Expect(Board.Rows).ToHaveCountAsync(12, new() { Timeout = 10000 });
    }

    // A click on a resident's name selects their row; the name's cell is not one the board edits.
    private async Task Select(string resident)
    {
        await ClickWhenStable(Board.Cell(resident, ResidentColumn));
        await Expect(Board.SelectedRows.Filter(new() { HasText = resident })).ToHaveCountAsync(1, new() { Timeout = 10000 });
    }

    private async Task FillInCharges(string resident, string careLevel, string rate, string addOns)
    {
        await ClickWhenStable(AddCharges);
        await Expect(AddChargeDialog).ToBeVisibleAsync(new() { Timeout = 10000 });
        await NewResidentName.FillAndBlur(resident);
        await NewCareLevel.Select(careLevel);
        await NewMonthlyRate.FillAndBlur(rate);
        await NewAddOnCharges.FillAndBlur(addOns);
    }

    [Test]
    public async Task the_board_opens_on_the_first_twelve_residents_by_name_with_what_is_outstanding()
    {
        await NavigateAndBoot();

        await Expect(Board.Rows.First).ToContainTextAsync("Abigail Moore");
        await Expect(Board.Rows.Last).ToContainTextAsync("Grace Bennett");
        await Expect(Board.Cell("Abigail Moore", MonthlyRateColumn)).ToHaveTextAsync("$3,200");
        await Expect(Board.Cell("Arthur Bell", BalanceDueColumn)).ToHaveTextAsync("$0");
        await Expect(Board.Cell("Arthur Bell", StatusColumn)).ToHaveTextAsync("Paid");
        AssertNoConsoleErrors();
    }

    // The last page starts 24 residents in, twice the twelve a page shows.
    [Test]
    public async Task the_last_page_shows_the_last_twelve_residents()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Board.PagerPage(3));

        await Expect(Board.Rows.First).ToContainTextAsync("Priya Shah", new() { Timeout = 10000 });
        await Expect(Board.Rows.Last).ToContainTextAsync("Yusuf Haddad");
        await Expect(Board.Rows).ToHaveCountAsync(12);
        AssertNoConsoleErrors();
    }

    // Arthur Bell leads the Memory Care residents by name; Frank Delgado and Vera Lindqvist pay its lowest rate.
    [Test]
    public async Task sorting_the_memory_care_residents_by_rate_lists_the_lowest_rate_first_and_keeps_to_memory_care()
    {
        await NavigateAndBoot();
        await CareLevelFilter.Select("Memory Care");
        await ClickWhenStable(ApplyFilters);
        await Expect(Board.Rows.First).ToContainTextAsync("Arthur Bell", new() { Timeout = 10000 });

        await ClickWhenStable(Board.Header("Monthly Rate"));

        await Expect(Board.Rows.First).ToContainTextAsync("Frank Delgado", new() { Timeout = 10000 });
        await Expect(Board.Rows.Nth(1)).ToContainTextAsync("Vera Lindqvist");
        await Expect(Board.Rows.Last).ToContainTextAsync("Yara Ahmed");
        await Expect(Board.Rows).ToHaveCountAsync(9);
        await Expect(Summary).ToHaveTextAsync("9 residents (Memory Care): $41,790 outstanding.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task applying_the_filters_shows_only_the_residents_they_match()
    {
        await NavigateAndBoot();
        await CareLevelFilter.Select("Skilled Nursing");
        await WingFilter.Select("East");

        await ClickWhenStable(ApplyFilters);

        await Expect(Summary).ToHaveTextAsync("5 residents (Skilled Nursing, East wing): $26,475 outstanding.", new() { Timeout = 10000 });
        await Expect(Board.Rows).ToHaveCountAsync(5);
        await Expect(Board.Rows.First).ToContainTextAsync("Beatrice Lane");
        await Expect(Board.Rows.Last).ToContainTextAsync("Yusuf Haddad");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task unbilled_only_narrows_the_board_to_the_charges_still_pending()
    {
        await NavigateAndBoot();

        await ClickWhenStable(UnbilledOnly);

        await Expect(Summary).ToHaveTextAsync("12 residents (Pending): $73,485 outstanding.", new() { Timeout = 10000 });
        await Expect(StatusFilter.Input).ToHaveValueAsync("Pending");
        await Expect(Board.Rows.First).ToContainTextAsync("Abigail Moore");
        await Expect(Board.Rows.Last).ToContainTextAsync("Wendy Price");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task memory_care_narrows_the_board_to_the_memory_care_residents()
    {
        await NavigateAndBoot();

        await ClickWhenStable(MemoryCare);

        await Expect(Summary).ToHaveTextAsync("9 residents (Memory Care): $41,790 outstanding.", new() { Timeout = 10000 });
        await Expect(CareLevelFilter.Input).ToHaveValueAsync("Memory Care");
        await Expect(Board.Rows).ToHaveCountAsync(9);
        await Expect(Board.Rows.First).ToContainTextAsync("Arthur Bell");
        AssertNoConsoleErrors();
    }

    // Each filter left set would keep the board narrowed: all three are set before they are cleared.
    [Test]
    public async Task clearing_the_filters_brings_back_every_resident()
    {
        await NavigateAndBoot();
        await CareLevelFilter.Select("Skilled Nursing");
        await WingFilter.Select("East");
        await ClickWhenStable(UnbilledOnly);
        await Expect(Summary).ToHaveTextAsync("2 residents (Skilled Nursing, East wing, Pending): $17,555 outstanding.", new() { Timeout = 10000 });

        await ClickWhenStable(ClearFilters);

        await Expect(Summary).ToHaveTextAsync(WholeBoard, new() { Timeout = 10000 });
        await Expect(CareLevelFilter.Input).ToHaveValueAsync("");
        await Expect(WingFilter.Input).ToHaveValueAsync("");
        await Expect(StatusFilter.Input).ToHaveValueAsync("");
        await Expect(Board.Rows.Nth(1)).ToContainTextAsync("Amina Patel");
        AssertNoConsoleErrors();
    }

    // The rate's editor is still open when Save Rate Changes is clicked: the page saves the open cell into the
    // pending changes before it sends them. The server works out the new balance due, which the grid then shows.
    [Test]
    public async Task a_rate_changed_in_the_grid_is_saved_while_its_cell_is_still_open()
    {
        await NavigateAndBoot();
        await Board.Cell("Beatrice Lane", MonthlyRateColumn).DblClickAsync();
        // The grid focuses its numeric editor a moment after showing it and rewrites "8,425.00" to "8425";
        // a fill before then lands after the number.
        await Expect(RateEditor).ToHaveValueAsync("8425", new() { Timeout = 10000 });
        await RateEditor.FillAsync("5000");

        await ClickWhenStable(SaveRateChanges);

        await Expect(Toast).ToContainTextAsync(
            "Saved Beatrice Lane's charges: $5,000 a month and $300 in add-ons, $5,300 due.", new() { Timeout = 10000 });
        await Expect(Board.Cell("Beatrice Lane", BalanceDueColumn)).ToHaveTextAsync("$5,300", new() { Timeout = 10000 });
        await Expect(Summary).ToHaveTextAsync("36 residents: $143,575 outstanding.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task raising_a_selected_residents_rate_five_percent_shows_the_new_rate()
    {
        await NavigateAndBoot();
        await Select("Abigail Moore");

        await ClickWhenStable(RaiseSelectedRates);

        await Expect(Toast).ToContainTextAsync("Raised Abigail Moore's monthly rate 5% to $3,360.", new() { Timeout = 10000 });
        await Expect(Board.Cell("Abigail Moore", MonthlyRateColumn)).ToHaveTextAsync("$3,360", new() { Timeout = 10000 });
        await Expect(Summary).ToHaveTextAsync("36 residents: $147,160 outstanding.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task raising_rates_with_nobody_selected_raises_no_rate()
    {
        await NavigateAndBoot();

        await ClickWhenStable(RaiseSelectedRates);

        await Expect(Toast).ToContainTextAsync("Select the residents whose rate goes up, then apply the increase.", new() { Timeout = 10000 });
        await Expect(Summary).ToHaveTextAsync(WholeBoard);
        await Expect(Board.Cell("Abigail Moore", MonthlyRateColumn)).ToHaveTextAsync("$3,200");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task marking_a_selected_pending_resident_billed_shows_them_billed()
    {
        await NavigateAndBoot();
        await Select("Abigail Moore");

        await ClickWhenStable(MarkSelectedBilled);

        await Expect(Toast).ToContainTextAsync("Marked Abigail Moore billed.", new() { Timeout = 10000 });
        await Expect(Board.Cell("Abigail Moore", StatusColumn)).ToHaveTextAsync("Billed", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    // Arthur Bell has paid for the month; billing him again would ask his family to pay twice.
    [Test]
    public async Task a_resident_who_has_paid_is_not_billed_again()
    {
        await NavigateAndBoot();
        await Select("Arthur Bell");

        await ClickWhenStable(MarkSelectedBilled);

        await Expect(Toast).ToContainTextAsync("None of the selected residents is waiting to be billed.", new() { Timeout = 10000 });
        await Expect(Board.Cell("Arthur Bell", StatusColumn)).ToHaveTextAsync("Paid");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task empty_charges_are_refused_field_by_field()
    {
        await NavigateAndBoot();
        await ClickWhenStable(AddCharges);
        await Expect(AddChargeDialog).ToBeVisibleAsync(new() { Timeout = 10000 });

        await ClickWhenStable(OpenCharges);

        await Expect(_plan.ErrorFor(m => m.NewResidentName)).ToHaveTextAsync("Enter the resident's name.", new() { Timeout = 10000 });
        await Expect(_plan.ErrorFor(m => m.NewCareLevel)).ToHaveTextAsync("Choose the resident's care level.");
        await Expect(_plan.ErrorFor(m => m.NewMonthlyRate)).ToHaveTextAsync("Enter the monthly rate.");
        await Expect(_plan.ErrorFor(m => m.NewAddOnCharges)).ToHaveTextAsync("Enter the add-on charges, or 0 for none.");
        await Expect(AddChargeDialog).ToBeVisibleAsync();
        await Expect(Summary).ToHaveTextAsync(WholeBoard);
        AssertNoConsoleErrors();
    }

    // Agnes Whitlock files second by name, so her new charges show on the board's first page.
    [Test]
    public async Task a_new_residents_charges_open_on_the_board()
    {
        await NavigateAndBoot();
        await FillInCharges("Agnes Whitlock", "Assisted Living", "5200", "250");

        await ClickWhenStable(OpenCharges);

        await Expect(Toast).ToContainTextAsync(
            "Opened Agnes Whitlock's charges: $5,200 a month and $250 in add-ons, $5,450 due.", new() { Timeout = 10000 });
        await Expect(AddChargeDialog).ToBeHiddenAsync();
        await Expect(Board.Rows.Nth(1)).ToContainTextAsync("Agnes Whitlock", new() { Timeout = 10000 });
        await Expect(Board.Cell("Agnes Whitlock", BalanceDueColumn)).ToHaveTextAsync("$5,450");
        await Expect(Summary).ToHaveTextAsync("37 residents: $152,450 outstanding.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task charges_for_a_resident_already_billed_this_month_are_refused_by_the_server()
    {
        await NavigateAndBoot();
        await FillInCharges("Amina Patel", "Assisted Living", "4875", "180");

        await ClickWhenStable(OpenCharges);

        await Expect(_plan.ErrorFor(m => m.NewResidentName)).ToHaveTextAsync("Amina Patel already has charges this month.", new() { Timeout = 10000 });
        await Expect(AddChargeDialog).ToBeVisibleAsync();
        await Expect(Summary).ToHaveTextAsync(WholeBoard);
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task cancelling_new_charges_closes_the_form()
    {
        await NavigateAndBoot();
        await ClickWhenStable(AddCharges);
        await Expect(AddChargeDialog).ToBeVisibleAsync(new() { Timeout = 10000 });
        await NewResidentName.FillAndBlur("Agnes Whitlock");

        await ClickWhenStable(CancelCharges);

        await Expect(AddChargeDialog).ToBeHiddenAsync(new() { Timeout = 10000 });
        await Expect(Summary).ToHaveTextAsync(WholeBoard);
        AssertNoConsoleErrors();
    }

    // Six of the nine Memory Care families owe this month; the other three have paid.
    [Test]
    public async Task email_statements_reaches_every_family_on_the_board_with_a_balance_due()
    {
        await NavigateAndBoot();
        await ClickWhenStable(MemoryCare);
        await Expect(Summary).ToHaveTextAsync("9 residents (Memory Care): $41,790 outstanding.", new() { Timeout = 10000 });

        await ClickWhenStable(EmailStatements);

        await Expect(Toast).ToContainTextAsync("Emailed statements to the 6 families with a balance due.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }
}
