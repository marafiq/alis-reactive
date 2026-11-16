using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PathsAndWire;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenADateTimeOfEitherKindIsWrittenIntoThePlan(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M15_F1.X3_13_0)]
    public async Task each_shows_its_wall_time_with_no_offset_in_another_zone_and_binds_it_as_unspecified()
    {
        var kolkata = await NewWorldInZone(Zones.Kolkata);
        await kolkata.Page.GotoAsync(CarePages.ShiftClock);
        await Expect(kolkata.Page.Locator("#server-utc")).ToHaveTextAsync("2026-11-09T14:30:05");
        await Expect(kolkata.Page.Locator("#server-local")).ToHaveTextAsync("2026-11-09T09:30:05");
        await kolkata.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the times", Exact = true }).ClickAsync();
        await Expect(kolkata.Page.Locator("#utc-shown")).ToHaveTextAsync("2026-11-09T14:30:05");
        await Expect(kolkata.Page.Locator("#local-shown")).ToHaveTextAsync("2026-11-09T09:30:05");
        await kolkata.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Stamp", Exact = true }).ClickAsync();
        await Expect(kolkata.Page.Locator("#stamped-at")).ToHaveTextAsync("2026-11-09T14:30:05 Unspecified");
        await Expect(kolkata.Page.Locator("#stamped-seen")).ToHaveTextAsync("2026-11-09T09:30:05 Unspecified");

        var stampedOnTheHomesClock = await NewWorldInZone(Zones.Kolkata);
        await stampedOnTheHomesClock.Page.GotoAsync(CarePages.ShiftClockStampedOnTheHomesClock);
        await stampedOnTheHomesClock.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Stamp", Exact = true }).ClickAsync();
        await Expect(stampedOnTheHomesClock.Page.Locator("#stamped-at")).ToHaveTextAsync("2026-11-09T09:30:05 Unspecified");
    }
}
