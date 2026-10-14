namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As the medication aide on the evening pass, I want to widen the cut-off columns of the med pass sheet to
/// read each name in full, so that I never give a dose by a guess at a medication or resident name.
/// </summary>
/// <remarks>
/// The sheet is laid out for the med cart's tablet, so Margaret Abernathy-Whitfield's name and her Donepezil
/// hydrochloride 10 mg tablet are cut short in the first row. The resident and medication columns cut a name
/// that does not fit with an ellipsis and show it whole in a tooltip on hover; the grid opens that tooltip
/// only when the text is wider than its cell (grid.js:5621-5644, 5655-5676), so no tooltip on hover means
/// the name fits. The ellipsis itself is painted text, which a test cannot read. Fitting runs in the click's
/// own pipeline, so the fitted cells are measured wider right after the click; that the tooltip stays shut
/// then reads the name as whole. A tooltip already open on another cell closes when the pointer leaves it.
/// Not exercised: fitting also narrows a column wider than its content (resize.js:157); a narrower column
/// shows the aide nothing new, so the tests assert names shown in full.
/// </remarks>
[TestFixture]
public class WhenUsingFusionGridColumnFit : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/ColumnFit";
    private const string GridId = "med-pass-sheet";

    // The grid numbers a row's cells from 0, counting the hidden ID column.
    private const int RoomCell = 1;
    private const int ResidentCell = 2;
    private const int MedicationCell = 3;
    private const int DueCell = 5;

    private ILocator Rows => Page.Locator($"#{GridId} .e-row");
    private ILocator Cell(int row, int cell) => Rows.Nth(row).Locator(".e-rowcell").Nth(cell);
    private ILocator FullName => Page.Locator(".e-tooltip-wrap.e-popup-open");
    private ILocator ShowFullMedicationNames => Page.Locator("#fit-medication");
    private ILocator FitEveryColumn => Page.Locator("#fit-every-column");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Rows).ToHaveCountAsync(4, new() { Timeout = 10000 });
    }

    [Test]
    public async Task the_sheet_lists_tonights_doses_by_room()
    {
        await NavigateAndBoot();

        await Expect(Cell(0, RoomCell)).ToHaveTextAsync("214");
        await Expect(Cell(0, MedicationCell)).ToHaveTextAsync("Donepezil hydrochloride 10 mg tablet");
        await Expect(Cell(3, ResidentCell)).ToHaveTextAsync("Walter Grant");
        await Expect(Cell(3, DueCell)).ToHaveTextAsync("9:00 PM");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_medication_name_cut_short_shows_in_full_when_hovered()
    {
        await NavigateAndBoot();

        await Cell(0, MedicationCell).HoverAsync();

        await Expect(FullName).ToHaveTextAsync("Donepezil hydrochloride 10 mg tablet", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task showing_full_medication_names_widens_only_the_medication_column()
    {
        await NavigateAndBoot();
        var cutShort = await Cell(0, MedicationCell).BoundingBoxAsync();

        await ClickWhenStable(ShowFullMedicationNames);

        var fitted = await Cell(0, MedicationCell).BoundingBoxAsync();
        Assert.That(fitted!.Width, Is.GreaterThan(cutShort!.Width), "the medication column is wider");
        await Cell(0, ResidentCell).HoverAsync();
        await Expect(FullName).ToHaveTextAsync("Margaret Abernathy-Whitfield", new() { Timeout = 10000 });
        await Cell(0, MedicationCell).HoverAsync();
        await Expect(FullName).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task fitting_every_column_shows_every_name_in_full()
    {
        await NavigateAndBoot();
        var residentCutShort = await Cell(0, ResidentCell).BoundingBoxAsync();
        var medicationCutShort = await Cell(0, MedicationCell).BoundingBoxAsync();

        await ClickWhenStable(FitEveryColumn);

        Assert.That((await Cell(0, ResidentCell).BoundingBoxAsync())!.Width, Is.GreaterThan(residentCutShort!.Width),
            "the resident column is wider");
        Assert.That((await Cell(0, MedicationCell).BoundingBoxAsync())!.Width, Is.GreaterThan(medicationCutShort!.Width),
            "the medication column is wider");
        await Cell(0, ResidentCell).HoverAsync();
        await Expect(FullName).ToHaveCountAsync(0);
        await Cell(0, MedicationCell).HoverAsync();
        await Expect(FullName).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }
}
