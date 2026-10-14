using System.Text.RegularExpressions;
using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.PivotView;

/// <summary>
/// As the executive director of Maple Grove, I want my monthly occupancy and care-hours report by wing and
/// care level, as a table or a chart, so that I can see what each figure counts, bring in the month that
/// just closed, and take the report to the board as a snapshot or in the board packet.
/// </summary>
/// <remarks>
/// The report opens on July to September. North holds Assisted Living and Memory Care, South holds
/// Assisted Living and Independent. In the table, column 1 holds the row headings, then each month its
/// residents and its care hours: July 2 and 3, August 4 and 5, September 6 and 7; once October loads,
/// August is 2 and 3 and October 6 and 7. Picking a figure says what it counts; drawing the report again
/// (refresh, new data, a snapshot) clears the figure picked. A snapshot keeps the figures; the board packet
/// takes the layout without them. Snapshots live in the browser's session, so each test keeps its own.
/// Not exercised:
/// writing <c>CurrentView</c>: EJ2 keeps it as a plain field that nothing on screen follows; its own
/// toolbar switches the view, and Refresh resets the field from the display option.
/// Reading <c>DataSourceSettings.DataSource</c>: the page never needs the rows back.
/// The cell's <c>ActualValue</c>: it differs from <c>Value</c> only in a drilled row whose subtotals are
/// hidden, and this report shows every subtotal.
/// The cell's <c>RowIndex</c> and <c>ColIndex</c>: positions in EJ2's cell matrix, which no figure names.
/// </remarks>
[TestFixture]
public class WhenUsingFusionPivotView : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/PivotView?community=maple-grove";

    // The table numbers its columns from 1; column 1 holds the row headings.
    private const int JulyResidents = 2;
    private const int AugustResidents = 4;
    private const int SeptemberCareHours = 7;
    private const int OctoberResidents = 6;

    private FusionPivotViewLocator Report => new(Page, "occupancy-report");
    private ILocator Period => Page.Locator("#report-period");
    private ILocator Hint => Page.Locator("#figure-hint");
    private ILocator SelectedFigure => Page.Locator("#selected-figure");
    private ILocator LoadOctober => Page.Locator("#load-october-btn");
    private ILocator Refresh => Page.Locator("#refresh-report-btn");
    private ILocator HighlightFigures => Page.Locator("#highlight-figures-btn");
    private ILocator AddMeasure => Page.Locator("#add-measure-btn");
    private ILocator SaveSnapshot => Page.Locator("#save-snapshot-btn");
    private ILocator OpenSnapshot => Page.Locator("#open-snapshot-btn");
    private ILocator AddToBoardPacket => Page.Locator("#add-to-board-packet-btn");
    private ILocator BoardStatus => Page.Locator("#board-status");

    // The layout's toast container holds one element per toast shown.
    private ILocator Toast => Page.Locator("#alisFusionToast .e-toast");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Report.Figure("Memory Care", AugustResidents)).ToHaveTextAsync("9", new() { Timeout = 10000 });
    }

    private async Task PickFigure(string rowHeading, int column)
    {
        await ClickWhenStable(Report.Figure(rowHeading, column));
        await Expect(SelectedFigure).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    [Test]
    public async Task the_report_opens_on_the_table_of_residents_and_care_hours_by_wing_and_care_level()
    {
        await NavigateAndBoot();

        await Expect(Period).ToHaveTextAsync("July to September");
        await Expect(Report.ColumnHeading("July")).ToBeVisibleAsync();
        await Expect(Report.ColumnHeading("September")).ToBeVisibleAsync();
        await Expect(Report.RowHeading("North")).ToBeVisibleAsync();
        await Expect(Report.RowHeading("Independent")).ToBeVisibleAsync();
        await Expect(Report.Figure("Grand Total", AugustResidents)).ToHaveTextAsync("50");
        await Expect(Report.Chart).ToBeHiddenAsync();
        await Expect(Hint).ToHaveTextAsync("Pick a figure to see what it counts.");
        await Expect(SelectedFigure).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task picking_a_figure_says_which_wing_care_level_and_month_it_counts()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Report.Figure("Memory Care", AugustResidents));

        await Expect(SelectedFigure).ToHaveTextAsync("North · Memory Care in August: 9 residents", new() { Timeout = 5000 });
        await Expect(Hint).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task picking_a_care_hours_figure_gives_it_in_care_hours()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Report.Figure("Memory Care", SeptemberCareHours));

        await Expect(SelectedFigure).ToHaveTextAsync("North · Memory Care in September: 2140 care hours", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_figure_of_one_resident_reads_in_the_singular()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Report.Figure("Independent", AugustResidents));

        await Expect(SelectedFigure).ToHaveTextAsync("South · Independent in August: 1 resident", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task picking_a_wing_total_names_the_wing()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Report.Figure("North", AugustResidents));

        await Expect(SelectedFigure).ToHaveTextAsync("North wing in August: 27 residents", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task picking_the_community_total_names_all_wings()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Report.Figure("Grand Total", AugustResidents));

        await Expect(SelectedFigure).ToHaveTextAsync("All wings in August: 50 residents", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task picking_a_heading_instead_of_a_figure_asks_for_a_figure()
    {
        await NavigateAndBoot();
        await PickFigure("Memory Care", AugustResidents);

        await ClickWhenStable(Report.RowHeading("Memory Care"));

        await Expect(SelectedFigure).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(Hint).ToHaveTextAsync("Pick a figure to see what it counts.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task refreshing_the_report_clears_the_figure_that_was_picked()
    {
        await NavigateAndBoot();
        await PickFigure("Memory Care", AugustResidents);

        await ClickWhenStable(Refresh);

        await Expect(SelectedFigure).ToBeHiddenAsync(new() { Timeout = 10000 });
        await Expect(Hint).ToBeVisibleAsync();
        await Expect(Report.Figure("Memory Care", AugustResidents)).ToHaveTextAsync("9");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task loading_october_moves_the_report_to_the_latest_three_months()
    {
        await NavigateAndBoot();

        await ClickWhenStable(LoadOctober);

        await Expect(Period).ToHaveTextAsync("August to October", new() { Timeout = 10000 });
        await Expect(Report.ColumnHeading("October")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Report.ColumnHeading("July")).ToBeHiddenAsync();
        await Expect(Report.Figure("Memory Care", OctoberResidents)).ToHaveTextAsync("10");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task adding_the_chart_to_the_board_packet_names_the_chart()
    {
        await NavigateAndBoot();
        await Report.ShowChart("Column");
        await Expect(Report.Chart).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Report.Table).ToBeHiddenAsync();

        await ClickWhenStable(AddToBoardPacket);

        await Expect(BoardStatus).ToHaveTextAsync("Added the chart to Maple Grove's board packet.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task adding_the_table_to_the_board_packet_names_the_table()
    {
        await NavigateAndBoot();

        await ClickWhenStable(AddToBoardPacket);

        await Expect(BoardStatus).ToHaveTextAsync("Added the table to Maple Grove's board packet.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_a_snapshot_confirms_it_in_a_brief_toast_that_goes_away_by_itself()
    {
        await NavigateAndBoot();

        await ClickWhenStable(SaveSnapshot);

        await Expect(Toast).ToHaveCountAsync(1, new() { Timeout = 10000 });
        await Expect(Toast).ToContainTextAsync("Board snapshot saved for Maple Grove.");
        await Expect(Toast.Locator(".e-toast-progress")).ToHaveCountAsync(1);
        await Expect(Toast.Locator(".e-toast-close-icon")).ToHaveCountAsync(0);

        // It counts down 2.5 seconds and fades; a toast left at the default five seconds would still show.
        await Expect(Toast).ToHaveCountAsync(0, new() { Timeout = 4500 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task opening_the_snapshot_brings_back_the_months_it_was_saved_with()
    {
        await NavigateAndBoot();
        await ClickWhenStable(SaveSnapshot);
        await Expect(Toast).ToContainTextAsync("Board snapshot saved for Maple Grove.", new() { Timeout = 10000 });
        await ClickWhenStable(LoadOctober);
        await Expect(Report.ColumnHeading("October")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Report.ColumnHeading("July")).ToBeHiddenAsync();

        await ClickWhenStable(OpenSnapshot);

        await Expect(BoardStatus).ToHaveTextAsync("Showing the board snapshot saved for Maple Grove.", new() { Timeout = 10000 });
        await Expect(Report.ColumnHeading("July")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Report.ColumnHeading("October")).ToBeHiddenAsync();
        await Expect(Report.Figure("Memory Care", JulyResidents)).ToHaveTextAsync("9");
        await Expect(Period).ToHaveTextAsync("July to September");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task opening_a_snapshot_before_one_is_saved_says_there_is_none()
    {
        await NavigateAndBoot();

        await ClickWhenStable(OpenSnapshot);

        await Expect(BoardStatus).ToHaveTextAsync("No board snapshot is saved for Maple Grove yet.", new() { Timeout = 10000 });
        await Expect(Report.ColumnHeading("July")).ToBeVisibleAsync();
        AssertNoConsoleErrorsExcept("the server responded with a status of 404");
    }

    [Test]
    public async Task highlighting_figures_opens_the_conditional_formatting_dialog()
    {
        await NavigateAndBoot();
        await Expect(Report.ConditionalFormattingDialog).ToBeHiddenAsync();

        await ClickWhenStable(HighlightFigures);

        await Expect(Report.ConditionalFormattingDialog).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Report.ConditionalFormattingDialogTitle).ToHaveTextAsync("Conditional Formatting");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task adding_a_measure_opens_the_calculated_field_dialog()
    {
        await NavigateAndBoot();
        await Expect(Report.CalculatedFieldDialog).ToBeHiddenAsync();

        await ClickWhenStable(AddMeasure);

        await Expect(Report.CalculatedFieldDialog).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Report.CalculatedFieldDialogTitle).ToHaveTextAsync("Create Calculated Field");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_components_index_opens_the_occupancy_report()
    {
        await NavigateTo("/Sandbox/Components");
        var link = Page.Locator("a[href='/Sandbox/Components/PivotView/Index']");

        await ClickWhenStable(link);

        await Page.WaitForURLAsync(new Regex("/Sandbox/Components/PivotView.*community=maple-grove"), new() { Timeout = 10000 });
        await Expect(Report.Figure("Memory Care", AugustResidents)).ToHaveTextAsync("9", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }
}
