using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepCallsAComponentsMethod(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.method_void)]
    public async Task the_method_runs_once()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.MethodVoid);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ping", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Ping", Exact = true })).ToHaveTextAsync("Pinged 1 time");

        var unpinged = await NewWorld();
        await unpinged.Page.GotoAsync(BenchPages.MethodVoid);
        await Expect(unpinged.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Ping", Exact = true })).ToHaveTextAsync("Pinged 0 times");
    }
}
