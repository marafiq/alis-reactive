using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepAwaitsAComponentsMethod(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.method_async_void)]
    public async Task the_step_after_the_call_waits_until_its_promise_settles()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.MethodAsyncVoid);
        var armed = await bench.Arm(RunGate.Settle);

        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Settle" }).ClickAsync();
        var held = await armed.Held();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("Settling");
        await held.Release();

        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("Settled");

        var refused = await NewWorld();
        await refused.Seed(Seed.TheSettleEndpointRefuses());
        await refused.Page.GotoAsync(BenchPages.MethodAsyncVoid);
        await refused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Settle" }).ClickAsync();
        await Expect(refused.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Settle", Exact = true })).ToHaveTextAsync("Refused");
        await Expect(refused.Page.Locator("#shown")).ToHaveTextAsync("Settling");
    }
}
