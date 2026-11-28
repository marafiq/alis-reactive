using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenDatesAndTimesAreOrdered(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_29_0)]
    public async Task each_list_orders_as_dotnet_orders_its_dates_instants_and_spans()
    {
        var newYork = await NewWorldInZone(Zones.NewYork);
        await newYork.Page.GotoAsync(CarePages.ResidentTimelineInNewYork);
        await Expect(newYork.Page.Locator("#timeline-visits")).ToHaveTextAsync("01:30,02:30,03:15");
        await Expect(newYork.Page.Locator("#timeline-seen")).ToHaveTextAsync("10:00:00-08:00,13:00:00-05:00,18:00:00.0000001+00:00,12:00:00-08:00");
        await Expect(newYork.Page.Locator("#timeline-stays")).ToHaveTextAsync("1.02:00:00,23:00:00,-00:00:00.5000000");

        var latestFirst = await NewWorldInZone(Zones.NewYork);
        await latestFirst.Page.GotoAsync(CarePages.ResidentTimelineLatestVisitFirstInNewYork);
        await Expect(latestFirst.Page.Locator("#timeline-visits")).ToHaveTextAsync("03:15,02:30,01:30");
    }
}
