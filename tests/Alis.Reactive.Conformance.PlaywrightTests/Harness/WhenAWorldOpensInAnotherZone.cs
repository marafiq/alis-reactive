using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Harness;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAWorldOpensInAnotherZone(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Harness.WhenAWorldOpensInAnotherZone_its_browser_reads_an_instant_on_the_clock_of_the_zone_the_test_names)]
    public async Task its_browser_reads_an_instant_on_the_clock_of_the_zone_the_test_names()
    {
        var kolkata = await NewWorldInZone(Zones.Kolkata);
        await kolkata.Page.GotoAsync(WorldPages.ZoneSlip);

        await Expect(kolkata.Page.GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("17:30");

        var fixtures = await NewWorld();
        await fixtures.Page.GotoAsync(WorldPages.ZoneSlip);

        await Expect(fixtures.Page.GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("04:00");
    }
}
