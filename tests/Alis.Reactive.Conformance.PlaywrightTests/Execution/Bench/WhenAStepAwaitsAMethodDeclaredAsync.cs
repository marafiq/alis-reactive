using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepAwaitsAMethodDeclaredAsync(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X4_11_0)]
    public async Task the_next_step_runs_once_its_promise_resolves_never_when_it_rejects_and_its_result_is_what_it_resolves_to()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.ExecutionSelfTest);
        var armed = await bench.Arm(RunGate.Settle);

        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Run the self-test", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await Expect(bench.Page.Locator("#check-status")).ToHaveTextAsync("Testing");
        await held.Release();
        await Expect(bench.Page.Locator("#check-status")).ToHaveTextAsync("Self-test passed");

        await bench.Seed(Seed.TheSettleEndpointRefuses());
        var armedAgain = await bench.Arm(RunGate.Settle);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Run the self-test", Exact = true }).ClickAsync();
        var heldAgain = await armedAgain.Held();
        await heldAgain.Release();
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Settle", Exact = true })).ToHaveTextAsync("Refused");
        await Expect(bench.Page.Locator("#check-status")).ToHaveTextAsync("Testing");

        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read the count", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#record-count")).ToHaveTextAsync("125");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the remote code", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#record-code")).ToHaveTextAsync("C-750");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read older's count", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#older-count")).ToHaveTextAsync("90");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Take the missing record", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Taken text", Exact = true })).ToHaveTextAsync("(null)");

        var recounted = await NewWorld();
        await recounted.Seed(Seed.TheRecordEndpointsCount(60));
        await recounted.Page.GotoAsync(BenchPages.ExecutionSelfTest);
        await recounted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read the count", Exact = true }).ClickAsync();
        await Expect(recounted.Page.Locator("#record-count")).ToHaveTextAsync("60");
    }
}
