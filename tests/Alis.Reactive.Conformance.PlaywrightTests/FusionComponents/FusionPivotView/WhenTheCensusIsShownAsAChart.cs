using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheCensusIsShownAsAChart(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F34.M21_X1020_0)]
    public async Task the_pivot_shows_and_reads_the_view_its_view_set()
    {
        var chart = await NewWorld();
        await chart.Page.GotoAsync(CarePages.WardCensus);
        await chart.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Which view", Exact = true }).ClickAsync();

        await Expect(chart.Page.GetByRole(AriaRole.Img, new PageGetByRoleOptions { Name = "Interactive chart", Exact = true })).ToBeVisibleAsync();
        await Expect(chart.Page.Locator("#view-name")).ToHaveTextAsync("Chart");

        var table = await NewWorld();
        await table.Page.GotoAsync(CarePages.WardCensusAsTable);
        await table.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Which view", Exact = true }).ClickAsync();

        await Expect(table.Page.GetByRole(AriaRole.Grid)).ToBeVisibleAsync();
        await Expect(table.Page.Locator("#view-name")).ToHaveTextAsync("Table");
    }
}
