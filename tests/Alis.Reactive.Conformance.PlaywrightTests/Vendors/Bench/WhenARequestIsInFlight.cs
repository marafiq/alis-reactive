using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARequestIsInFlight(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.http_whileLoading)]
    public async Task its_while_loading_block_writes_the_component_before_the_answer()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.HttpWhileLoading);
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Heading)).ToHaveTextAsync("Alpha");
        var armed = await bench.Arm(RequestGate.Route);

        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Heading)).ToHaveTextAsync("Loading items");
        await held.Release();

        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Heading)).ToHaveTextAsync("Alpha, 3 items");

        var stopped = await NewWorld();
        await stopped.Seed(Seed.TheComponentsVersion(ComponentVersion.Older));
        await stopped.Page.GotoAsync(BenchPages.HttpWhileLoading);
        await stopped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(stopped.Page.Locator("#conformance").GetByRole(AriaRole.Heading)).ToHaveTextAsync("Alpha");
    }
}
