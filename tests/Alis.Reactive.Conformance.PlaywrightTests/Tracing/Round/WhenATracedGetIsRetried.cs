using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Tracing;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Tracing))]
[Category(Hosts.TracingName)]
public sealed class WhenATracedGetIsRetried(string host) : ConformanceFixture(host)
{
    private const string Render = "alis.render";
    private const string Run = Render + " > alis.run";
    private const string Send = Run + " > alis.send";
    private const string Census = "GET " + CareEndpoints.RoundCensusCount;

    [Test]
    [FailsFor(Promises.M26_F3.X20_2_0)]
    public async Task each_attempt_is_a_span_of_its_send_and_its_traceparent_names_that_span()
    {
        var retried = await NewWorld();
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
        await retried.Page.GotoAsync(WorldPages.Collector);
        await Expect(retried.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(
        [
            Render,
            Render + " > alis.boot completed",
            Run + " completed",
            Send + " completed",
            Send + " > " + Census + " completed 503",
            Send + " > " + Census + " completed 200 resend 1",
        ]);
        await retried.Page.GotoAsync(WorldPages.Echo);
        await Expect(retried.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(
        [
            "attempt 1: traceparent flags 01 naming " + Census + " completed 503, baggage none",
            "attempt 2: traceparent flags 01 naming " + Census + " completed 200 resend 1, baggage http.request.resend_count=1",
        ]);

        var once = await NewWorld();
        await once.Page.GotoAsync(CarePages.RoundCensus);
        await once.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load census", Exact = true }).ClickAsync();
        await Expect(once.Page.Locator("#census")).ToHaveTextAsync("12");
        await once.Page.GotoAsync(WorldPages.Collector);
        await Expect(once.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(
        [
            Render,
            Render + " > alis.boot completed",
            Run + " completed",
            Send + " completed",
            Send + " > " + Census + " completed 200",
        ]);
    }
}
