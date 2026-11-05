namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridTooling : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/GridTooling";
    private const string GridId = "shift-schedule";

    private const int DayCell = 1;
    private const int ShiftCell = 2;
    private const int NurseCell = 3;
    private const int AideCell = 4;

    private ILocator Rows => Page.Locator($"#{GridId} .e-row");
    private ILocator Cell(int row, int cell) => Rows.Nth(row).Locator(".e-rowcell").Nth(cell);
    private ILocator Headers => Page.Locator($"#{GridId} .e-headercell:visible");
    private ILocator CurrentPage => Page.Locator($"#{GridId} .e-pager .e-numericitem.e-active");
    private ILocator Weekend => Page.Locator("#shift-schedule-weekend");
    private ILocator ChooseColumns => Page.Locator("#shift-schedule-columns");
    private ILocator ColumnChooser => Page.Locator($"#{GridId}_ccdlg");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Cell(0, DayCell)).ToHaveTextAsync("Mon, Oct 19", new() { Timeout = 10000 });
    }

    [Test]
    public async Task the_schedule_opens_on_mondays_three_shifts()
    {
        await NavigateAndBoot();

        await Expect(Rows).ToHaveCountAsync(3);
        await Expect(Cell(0, ShiftCell)).ToHaveTextAsync("Day");
        await Expect(Cell(2, ShiftCell)).ToHaveTextAsync("Night");
        await Expect(Cell(2, DayCell)).ToHaveTextAsync("Mon, Oct 19");
        await Expect(CurrentPage).ToHaveTextAsync("1");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task weekend_shows_saturdays_shifts_with_their_open_places()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Weekend);

        await Expect(Cell(0, DayCell)).ToHaveTextAsync("Sat, Oct 24", new() { Timeout = 10000 });
        await Expect(Cell(2, DayCell)).ToHaveTextAsync("Sat, Oct 24");
        await Expect(Cell(1, AideCell)).ToHaveTextAsync("Open");
        await Expect(Cell(2, NurseCell)).ToHaveTextAsync("Open");
        await Expect(CurrentPage).ToHaveTextAsync("6");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_columns_lets_the_scheduler_hide_the_aides()
    {
        await NavigateAndBoot();
        await Expect(Headers).ToHaveTextAsync(new[] { "Day", "Shift", "Nurse", "Aide" });

        await ClickWhenStable(ChooseColumns);
        await Expect(ColumnChooser).ToBeVisibleAsync(new() { Timeout = 10000 });
        await ColumnChooser.GetByText("Aide", new() { Exact = true }).ClickAsync();
        await ColumnChooser.GetByRole(AriaRole.Button, new() { Name = "OK" }).ClickAsync();

        await Expect(ColumnChooser).ToBeHiddenAsync(new() { Timeout = 10000 });
        await Expect(Headers).ToHaveTextAsync(new[] { "Day", "Shift", "Nurse" });
        await Expect(Cell(0, DayCell)).ToHaveTextAsync("Mon, Oct 19");
        AssertNoConsoleErrors();
    }
}
