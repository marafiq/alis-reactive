using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Elements;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepCountsATablesSeatsAndFlags(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M5_F1.X6_56_0)]
    public async Task the_seats_and_flags_are_counted_a_fall_risk_added_is_counted_in_the_next_step_and_a_paged_result_stops_at_the_count()
    {
        var lunch = await NewWorld();
        await lunch.Page.GotoAsync(CarePages.LunchSeating);
        await Expect(lunch.Page.Locator("#table-4-seated")).ToHaveTextAsync("4");
        await Expect(lunch.Page.Locator("#table-4-close-supervision")).ToHaveTextAsync("2");
        await Expect(lunch.Page.Locator("#table-4-risk-count")).ToHaveTextAsync("1");
        await lunch.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Table 4", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Fall risk", Exact = true }).ClickAsync();
        await Expect(lunch.Page.Locator("#table-4-risk-count")).ToHaveTextAsync("2");

        var station = await NewWorld();
        await station.Page.GotoAsync(CarePages.LunchSeating);
        await station.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count the plates", Exact = true }).ClickAsync();
        await Expect(station.Page.Locator("#plates-on-order")).ToHaveTextAsync("3");
        await Expect(station.Page.Locator("#plates-ready")).ToHaveTextAsync("-");
        await Expect(station.Page.Locator("#station-status")).ToHaveTextAsync("Not counted");
    }
}
