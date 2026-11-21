using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Tracing;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Tracing))]
[Category(Hosts.TracingName)]
public sealed class WhenAnUntracedGetIsRetried(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M26_F3.X20_7_0)]
    public async Task the_retry_carries_a_sampled_traceparent_and_its_resend_count()
    {
        var retried = await NewWorld();
        await retried.Seed(Seed.TheRenderIsNotRecorded());
        await retried.Seed(Seed.TheEndpointAnswers(CareEndpoints.RoundCensusCount, 503, 200));
        await retried.InstallTheClock();
        await retried.Page.GotoAsync(CarePages.RoundCensus);
        await retried.HoldTheClock();
        var armed = await retried.Arm(TracingGate.Respond, CareEndpoints.RoundCensusCount);
        await retried.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load census", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await Expect(retried.Page.Locator("#census")).ToHaveTextAsync("-");
        await held.Release();
        await retried.RunTheClockUntilItSends(CareEndpoints.RoundCensusCount, 2);
        await Expect(retried.Page.Locator("#census")).ToHaveTextAsync("12");
        await retried.Page.GotoAsync(WorldPages.Echo);
        await Expect(retried.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(
        [
            "attempt 1: traceparent none, baggage none",
            "attempt 2: traceparent flags 01 in a trace of its own, baggage http.request.resend_count=1",
        ]);

        var once = await NewWorld();
        await once.Seed(Seed.TheRenderIsNotRecorded());
        await once.Page.GotoAsync(CarePages.RoundCensus);
        await once.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load census", Exact = true }).ClickAsync();
        await Expect(once.Page.Locator("#census")).ToHaveTextAsync("12");
        await once.Page.GotoAsync(WorldPages.Echo);
        await Expect(once.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(["attempt 1: traceparent none, baggage none"]);
    }
}
