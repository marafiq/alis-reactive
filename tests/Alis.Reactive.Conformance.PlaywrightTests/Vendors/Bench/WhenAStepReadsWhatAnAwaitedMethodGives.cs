using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepReadsWhatAnAwaitedMethodGives(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.method_async_value)]
    public async Task the_value_the_promise_gives_is_shown_once_it_settles()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.MethodAsyncValue);
        var armed = await bench.Arm(RunGate.Settle);

        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show" }).ClickAsync();
        var held = await armed.Held();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("");
        await held.Release();

        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("3");

        var refused = await NewWorld();
        await refused.Seed(Seed.TheCountEndpointRefuses());
        await refused.Page.GotoAsync(BenchPages.MethodAsyncValue);
        await refused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show" }).ClickAsync();
        await Expect(refused.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Settle", Exact = true })).ToHaveTextAsync("Refused");
        await Expect(refused.Page.Locator("#shown")).ToHaveTextAsync("");
    }
}
