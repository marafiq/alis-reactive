using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenABulletChartIsRenderedWithTheViewsOptions(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F7.M21_X1003_0)]
    public async Task the_chart_shows_the_views_title_and_its_click_runs_the_handler_the_plan_gave_it()
    {
        var intake = await NewWorld();
        await intake.Page.GotoAsync(CarePages.FluidChartOfIntake);
        await Expect(intake.Page.Locator("#fluid-chart").GetByText("Fluid intake", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await intake.Page.Locator("#fluid-chart").ClickAsync();

        await Expect(intake.Page.Locator("#chart-note")).ToHaveTextAsync("Chart read");

        var weight = await NewWorld();
        await weight.Page.GotoAsync(CarePages.FluidChartOfWeight);

        await Expect(weight.Page.Locator("#fluid-chart").GetByText("Weight", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }
}
