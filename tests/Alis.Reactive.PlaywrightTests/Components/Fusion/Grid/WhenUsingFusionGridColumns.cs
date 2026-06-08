namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridColumns : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/CareStaffColumns";

    private ILocator VisibleHeaders =>
        Page.Locator("#care-staff-columns-grid .e-headercell:visible .e-headertext");

    private async Task NavigateColumns()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#column-status"))
            .ToHaveTextAsync("loaded", new() { Timeout = 10000 });
        await Expect(Page.Locator("#care-staff-columns-grid .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task hiding_care_columns_removes_their_headers()
    {
        await NavigateColumns();
        await Expect(VisibleHeaders).ToHaveTextAsync(
            new[] { "Resident", "Risk", "Primary Nurse", "Next Review", "Wing", "Care Level" },
            new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#hide-care-columns"));
        await Expect(Page.Locator("#column-status"))
            .ToHaveTextAsync("care columns hidden", new() { Timeout = 10000 });
        await Expect(VisibleHeaders).ToHaveTextAsync(
            new[] { "Resident", "Risk", "Wing", "Care Level" },
            new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task showing_care_columns_restores_their_headers()
    {
        await NavigateColumns();
        await ClickWhenStable(Page.Locator("#hide-care-columns"));
        await Expect(VisibleHeaders).ToHaveTextAsync(
            new[] { "Resident", "Risk", "Wing", "Care Level" },
            new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#show-care-columns"));
        await Expect(Page.Locator("#column-status"))
            .ToHaveTextAsync("care columns shown", new() { Timeout = 10000 });
        await Expect(VisibleHeaders).ToHaveTextAsync(
            new[] { "Resident", "Risk", "Primary Nurse", "Next Review", "Wing", "Care Level" },
            new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reordering_moves_risk_before_resident()
    {
        await NavigateColumns();
        await Expect(VisibleHeaders.First)
            .ToHaveTextAsync("Resident", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#reorder-risk-first"));
        await Expect(Page.Locator("#column-status"))
            .ToHaveTextAsync("risk moved before resident", new() { Timeout = 10000 });
        await Expect(VisibleHeaders).ToHaveTextAsync(
            new[] { "Risk", "Resident", "Primary Nurse", "Next Review", "Wing", "Care Level" },
            new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
