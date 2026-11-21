using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHandlerWritesAHeldObjectsWriteOnlyMemberAfterAnAwait(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.event_poc15_writeOnly_held_after)]
    public async Task the_write_after_the_await_lands_on_the_object_the_payload_holds()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.Poc15WriteOnlyHeldAfter);
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Held", Exact = true })).ToHaveTextAsync("Held: none");
        var armed = await bench.Arm(RunGate.Settle);

        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Held", Exact = true })).ToHaveTextAsync("Held: none");
        await held.Release();

        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Held", Exact = true })).ToHaveTextAsync("Held: Kappa");
    }
}
