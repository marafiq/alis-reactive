using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaComparesVisitTimesTheServerWrote(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_11_0)]
    public async Task each_time_is_compared_as_the_server_wrote_it_in_every_zone_the_browser_runs_in()
    {
        var newYork = await NewWorldInZone(Zones.NewYork);
        await newYork.Page.GotoAsync(CarePages.VisitTimes);
        await Expect(newYork.Page.Locator("#before-three-count")).ToHaveTextAsync("1");
        await Expect(newYork.Page.Locator("#half-past-eight-count")).ToHaveTextAsync("3");
        await Expect(newYork.Page.Locator("#after-half-past-eight-count")).ToHaveTextAsync("1");
        await Expect(newYork.Page.Locator("#seen-count")).ToHaveTextAsync("1");

        var kolkata = await NewWorldInZone(Zones.Kolkata);
        await kolkata.Page.GotoAsync(CarePages.VisitTimes);
        await Expect(kolkata.Page.Locator("#before-three-count")).ToHaveTextAsync("1");
        await Expect(kolkata.Page.Locator("#half-past-eight-count")).ToHaveTextAsync("3");
        await Expect(kolkata.Page.Locator("#after-half-past-eight-count")).ToHaveTextAsync("1");
        await Expect(kolkata.Page.Locator("#seen-count")).ToHaveTextAsync("1");

        var halfPastThree = await NewWorldInZone(Zones.NewYork);
        await halfPastThree.Page.GotoAsync(CarePages.VisitTimesWithTheEarlyVisitAtHalfPastThree);
        await Expect(halfPastThree.Page.Locator("#before-three-count")).ToHaveTextAsync("0");

        var eightThirtyOne = await NewWorldInZone(Zones.NewYork);
        await eightThirtyOne.Page.GotoAsync(CarePages.VisitTimesWithTheUtcVisitAtEightThirtyOne);
        await Expect(eightThirtyOne.Page.Locator("#half-past-eight-count")).ToHaveTextAsync("2");
    }
}
