using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARetryWaitsOnTheClock(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F2.X10_8_0)]
    public async Task the_request_loads_and_holds_its_target_once_across_its_attempts()
    {
        var retried = await NewWorld();
        await retried.Seed(Seed.TheEndpointAnswers(CareEndpoints.RetriedCensus, 503, 200));
        await retried.InstallTheClock();
        await retried.Page.GotoAsync(CarePages.CensusRetried);
        await retried.HoldTheClock();
        var armedFirst = await retried.Arm(CompositionGate.Answer, CareEndpoints.RetriedCensus);
        await retried.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load census", Exact = true }).ClickAsync();
        var heldFirst = await armedFirst.Held();
        await Expect(retried.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: census");
        await Expect(retried.Page.Locator("#census-log")).ToHaveTextAsync("loading");
        await heldFirst.Release();
        var armedSecond = await retried.Arm(CompositionGate.Answer, CareEndpoints.RetriedCensus);
        await retried.RunTheClockUntilItSends(CareEndpoints.RetriedCensus, 2);
        var heldSecond = await armedSecond.Held();
        await Expect(retried.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: census");
        await Expect(retried.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy calls", Exact = true })).ToHaveTextAsync("Busy calls: show census");
        await Expect(retried.Page.Locator("#census")).ToHaveAttributeAsync("aria-busy", "true");
        await Expect(retried.Page.Locator("#census-log")).ToHaveTextAsync("loading");
        await heldSecond.Release();
        await Expect(retried.Page.Locator("#census")).ToHaveTextAsync("12");
        await Expect(retried.Page.Locator("#census-log")).ToHaveTextAsync("loading done");
        await Expect(retried.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        await Expect(retried.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy calls", Exact = true })).ToHaveTextAsync("Busy calls: show census; hide census");
        await Expect(retried.Page.Locator("#census")).Not.ToHaveAttributeAsync("aria-busy", "true");

        var once = await NewWorld();
        await once.Seed(Seed.TheEndpointAnswers(CareEndpoints.RetriedCensus, 503, 200));
        await once.Page.GotoAsync(CarePages.CensusRetriedOnce);
        await once.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load census", Exact = true }).ClickAsync();
        await Expect(once.Page.Locator("#census-error")).ToHaveTextAsync("Unavailable");
        await Expect(once.Page.Locator("#census")).ToHaveTextAsync("-");
    }
}
