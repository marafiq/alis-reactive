using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARoutesAwaitedMethodRejects(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X11_4_0)]
    public async Task its_pipeline_stops_its_requests_finally_runs_and_its_busy_target_hides()
    {
        var refused = await NewWorld();
        await refused.Seed(Seed.TheSettleEndpointRefuses());
        await refused.Page.GotoAsync(CarePages.OrderStart);
        var armed = await refused.Arm(RunGate.Settle);

        await refused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Start", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await Expect(refused.Page.Locator("#settle-status")).ToHaveTextAsync("Settling");
        await Expect(refused.Page.Locator("#order-busy")).ToBeVisibleAsync();
        await held.Release();

        await Expect(refused.Page.Locator("#order-status")).ToHaveTextAsync("Finished");
        await Expect(refused.Page.Locator("#settle-status")).ToHaveTextAsync("Settling");
        await Expect(refused.Page.Locator("#order-busy")).ToBeHiddenAsync();

        var answered = await NewWorld();
        await answered.Page.GotoAsync(CarePages.OrderStart);
        var armedToAnswer = await answered.Arm(RunGate.Settle);

        await answered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Start", Exact = true }).ClickAsync();
        var heldToAnswer = await armedToAnswer.Held();
        await Expect(answered.Page.Locator("#settle-status")).ToHaveTextAsync("Settling");
        await Expect(answered.Page.Locator("#order-busy")).ToBeVisibleAsync();
        await heldToAnswer.Release();

        await Expect(answered.Page.Locator("#settle-status")).ToHaveTextAsync("Settled");
        await Expect(answered.Page.Locator("#order-status")).ToHaveTextAsync("Finished");
        await Expect(answered.Page.Locator("#order-busy")).ToBeHiddenAsync();
    }
}
