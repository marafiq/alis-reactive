using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Tracing;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Tracing))]
[Category(Hosts.TracingName)]
public sealed class WhenAnUntracedPageSavesADose(string host) : ConformanceFixture(host)
{
    private const string Render = "alis.render";
    private const string Run = Render + " > alis.run";
    private const string Send = Run + " > alis.send";
    private const string Doses = "POST " + CareEndpoints.DoseSaveDoses;

    [Test]
    [FailsFor(Promises.M26_F1.X20_1_0)]
    public async Task the_save_lists_no_span_and_its_endpoint_hears_no_traceparent()
    {
        var untraced = await NewWorld();
        await untraced.Seed(Seed.TheRenderIsNotRecorded());
        await untraced.Page.GotoAsync(CarePages.DoseSave);
        var armed = await untraced.Arm(TracingGate.Respond, CareEndpoints.DoseSaveDoses);
        await untraced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await Expect(untraced.Page.Locator("#echo")).ToHaveTextAsync("-");
        await held.Release();
        await Expect(untraced.Page.Locator("#echo")).ToHaveTextAsync("traceparent none, baggage none");
        await untraced.Page.GotoAsync(WorldPages.Collector);
        await Expect(untraced.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Collector", Exact = true })).ToBeVisibleAsync();
        await Expect(untraced.Page.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);

        var traced = await NewWorld();
        await traced.Page.GotoAsync(CarePages.DoseSave);
        var armedTraced = await traced.Arm(TracingGate.Respond, CareEndpoints.DoseSaveDoses);
        await traced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        var heldTraced = await armedTraced.Held();
        await Expect(traced.Page.Locator("#echo")).ToHaveTextAsync("-");
        await heldTraced.Release();
        await Expect(traced.Page.Locator("#echo")).ToHaveTextAsync("traceparent flags 01 in the page's trace, baggage none");
        await traced.Page.GotoAsync(WorldPages.Collector);
        await Expect(traced.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(
        [
            Render,
            Render + " > alis.boot completed",
            Run + " completed",
            Send + " completed",
            Send + " > " + Doses + " completed 200",
        ]);
    }
}
