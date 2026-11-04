namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridColumnFit : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/ColumnFit";
    private const string GridId = "med-pass-sheet";

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
