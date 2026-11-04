using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.BulletChart;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.BulletChart;

[TestFixture]
public class WhenUsingFusionBulletChart : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionBulletChart";
    private const int Birch = 1;

    private PagePlan<FusionBulletChartModel> _plan = null!;

    private FusionBulletChartLocator NursingHours => new(Page, "nursing-hours-chart");
    private FusionBulletChartLocator AgencyHours => new(Page, "agency-hours-chart");
    private ILocator NursingReadout => Page.Locator("#nursing-hours-readout");
    private ILocator AgencyReadout => Page.Locator("#agency-hours-readout");
    private ILocator ShiftsPanel => Page.Locator("#wing-shifts");
    private ILocator ShiftsHint => Page.Locator("#wing-shifts-hint");
    private ILocator UpNext => Page.Locator("#huddle-next");
    private ILocator Reviewed(string wing) =>
        _plan.Element(_plan.FindComponent(m => m.ReviewedWings)!.ElementId)
            .GetByRole(AriaRole.Checkbox, new() { Name = wing, Exact = true });

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<FusionBulletChartModel>.FromPage(Page);
        await Expect(NursingHours.Bar(Birch)).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(AgencyHours.Bar(Birch)).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    [Test]
    public async Task pointing_at_a_wing_shows_the_hours_it_worked_against_its_budget()
    {
        await NavigateAndBoot();

        await NursingHours.Bar(Birch).HoverAsync();

        await Expect(NursingReadout).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(NursingReadout).ToHaveTextAsync("Worked 868 of 840 budgeted hours.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_wings_tooltip_says_a_click_opens_its_shifts()
    {
        await NavigateAndBoot();

        await NursingHours.Bar(Birch).HoverAsync();

        await Expect(NursingHours.Tooltip).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(NursingHours.Tooltip).ToHaveTextAsync("Click the bar for this wing's shifts");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_a_wings_bar_opens_its_shifts_for_the_week()
    {
        await NavigateAndBoot();

        await NursingHours.Bar(Birch).ClickAsync(new() { Force = true });

        await Expect(ShiftsPanel).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#wing-shifts-wing")).ToHaveTextAsync("Birch");
        await Expect(Page.Locator("#wing-shifts-summary"))
            .ToHaveTextAsync("868 of 840 budgeted hours. Three night shifts ran long while a resident was on fall watch.");
        await Expect(ShiftsHint).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_the_chart_away_from_the_bars_says_where_to_click_and_opens_no_wing()
    {
        await NavigateAndBoot();

        await NursingHours.Root.ClickAsync(new() { Position = new() { X = 4, Y = 4 } });

        await Expect(ShiftsHint).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(ShiftsPanel).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task pointing_at_a_wings_agency_hours_repeats_them_under_the_chart()
    {
        await NavigateAndBoot();

        await AgencyHours.Bar(Birch).HoverAsync();

        await Expect(AgencyReadout).ToHaveTextAsync("96 h used of the 120 h cap", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_huddle_names_the_wing_after_the_ones_reviewed()
    {
        await NavigateAndBoot();

        await Reviewed("Maple").CheckAsync();
        await Reviewed("Birch").CheckAsync();

        await Expect(UpNext).ToHaveTextAsync("Cedar", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task once_every_wing_is_reviewed_the_round_starts_over_at_maple()
    {
        await NavigateAndBoot();
        await Reviewed("Maple").CheckAsync();
        await Reviewed("Birch").CheckAsync();
        await Reviewed("Cedar").CheckAsync();
        await Expect(UpNext).ToHaveTextAsync("Memory Care", new() { Timeout = 5000 });

        await Reviewed("Memory Care").CheckAsync();

        await Expect(UpNext).ToHaveTextAsync("Maple", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
