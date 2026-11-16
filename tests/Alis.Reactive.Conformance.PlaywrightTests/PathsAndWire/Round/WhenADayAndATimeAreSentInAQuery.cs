using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PathsAndWire;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenADayAndATimeAreSentInAQuery(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M15_F1.X3_14_0)]
    public async Task the_endpoint_binds_the_day_and_the_time_and_an_element_shows_each_as_its_text()
    {
        var newYork = await NewWorldInZone(Zones.NewYork);
        await newYork.Page.GotoAsync(CarePages.RoundLookup);
        await newYork.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Find the round", Exact = true }).ClickAsync();
        await Expect(newYork.Page.Locator("#round-found")).ToHaveTextAsync("Round on 2026-11-09 at 13:30:00");
        await Expect(newYork.Page.Locator("#day-text")).ToHaveTextAsync("2026-11-09");
        await Expect(newYork.Page.Locator("#time-text")).ToHaveTextAsync("13:30:00");

        var firstOfDecember = await NewWorldInZone(Zones.NewYork);
        await firstOfDecember.Page.GotoAsync(CarePages.RoundLookupOfTheFirstOfDecember);
        await firstOfDecember.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Find the round", Exact = true }).ClickAsync();
        await Expect(firstOfDecember.Page.Locator("#round-found")).ToHaveTextAsync("Round on 2026-12-01 at 07:05:00");
    }
}
