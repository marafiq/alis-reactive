using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Tracing;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Tracing))]
[Category(Hosts.TracingName)]
public sealed class WhenATracedPageAsksAnotherOrigin(string host) : ConformanceFixture(host)
{
    private const string Render = "alis.render";
    private const string Run = Render + " > alis.run";
    private const string Send = Run + " > alis.send";
    private const string WardStock = "GET " + CareEndpoints.WardStock;

    [Test]
    [FailsFor(Promises.M26_F3.X20_4_0)]
    public async Task the_other_origin_receives_no_trace_header_and_no_preflight()
    {
        var pharmacy = await NewWorld();
        await pharmacy.Page.GotoAsync(CarePages.PharmacyCheck);
        var armed = await pharmacy.Arm(TracingGate.Respond, WorldEndpoints.PharmacyStock);
        await pharmacy.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check pharmacy", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await Expect(pharmacy.Page.Locator("#pharmacy-echo")).ToHaveTextAsync("-");
        await held.Release();
        await Expect(pharmacy.Page.Locator("#pharmacy-echo")).ToHaveTextAsync("traceparent none");
        await pharmacy.Page.GotoAsync(WorldPages.SecondOriginLog);
        await Expect(pharmacy.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(["GET " + WorldEndpoints.PharmacyStock]);

        var ward = await NewWorld();
        await ward.Page.GotoAsync(CarePages.PharmacyCheck);
        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check ward", Exact = true }).ClickAsync();
        await Expect(ward.Page.Locator("#ward-echo")).ToHaveTextAsync("traceparent flags 01 in the page's trace");
        await ward.Page.GotoAsync(WorldPages.Collector);
        await Expect(ward.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(
        [
            Render,
            Render + " > alis.boot completed",
            Run + " completed",
            Send + " completed",
            Send + " > " + WardStock + " completed 200",
        ]);
        await ward.Page.GotoAsync(WorldPages.Echo);
        await Expect(ward.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(["attempt 1: traceparent flags 01 naming " + WardStock + " completed 200, baggage none"]);
    }
}
