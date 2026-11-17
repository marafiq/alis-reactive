using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAServerPushesIntoTheComponent(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.lane_push)]
    public async Task the_component_shows_the_member_the_push_carried()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.LanePush);
        await bench.Opened(BenchEndpoints.Stream);
        await bench.Push(BenchEndpoints.Stream, "renamed", "{\"name\":\"Beta\"}");
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Heading)).ToHaveTextAsync("Beta");

        var untouched = await NewWorld();
        await untouched.Page.GotoAsync(BenchPages.LanePush);
        await untouched.Opened(BenchEndpoints.Stream);
        await Expect(untouched.Page.Locator("#conformance").GetByRole(AriaRole.Heading)).ToHaveTextAsync("Alpha");
    }
}
