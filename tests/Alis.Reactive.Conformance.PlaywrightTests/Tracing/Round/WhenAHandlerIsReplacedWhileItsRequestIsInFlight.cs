using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Tracing;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Tracing))]
[Category(Hosts.TracingName)]
public sealed class WhenAHandlerIsReplacedWhileItsRequestIsInFlight(string host) : ConformanceFixture(host)
{
    private const string Render = "alis.render";
    private const string Run = Render + " > alis.run";
    private const string Send = Run + " > alis.send";
    private const string FreeBeds = "GET " + CareEndpoints.FreeBeds;

    [Test]
    [FailsFor(Promises.M26_F2.X20_3_0)]
    public async Task the_replaced_run_ends_replaced_with_its_send_and_its_attempt()
    {
        var replaced = await NewWorld();
        await replaced.Page.GotoAsync(CarePages.BedFinder);
        var armedFirst = await replaced.Arm(TracingGate.Respond, CareEndpoints.FreeBeds);
        var armedSecond = await replaced.Arm(TracingGate.Respond);
        await replaced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Find beds", Exact = true }).ClickAsync();
        var heldFirst = await armedFirst.Held();
        await Expect(replaced.Page.Locator("#beds")).ToHaveTextAsync("-");
        await replaced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Find beds", Exact = true }).ClickAsync();
        var heldSecond = await armedSecond.Held();
        await Expect(replaced.Page.Locator("#beds")).ToHaveTextAsync("-");
        await heldSecond.Release();
        await Expect(replaced.Page.Locator("#beds")).ToHaveTextAsync("4");
        await heldFirst.Release();
        await replaced.Page.GotoAsync(WorldPages.Collector);
        await Expect(replaced.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(
        [
            Render,
            Render + " > alis.boot completed",
            Run + " replaced",
            Send + " replaced",
            Send + " > " + FreeBeds + " replaced",
            Run + " completed",
            Send + " completed",
            Send + " > " + FreeBeds + " completed 200",
        ]);

        var once = await NewWorld();
        await once.Page.GotoAsync(CarePages.BedFinder);
        var armed = await once.Arm(TracingGate.Respond, CareEndpoints.FreeBeds);
        await once.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Find beds", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await Expect(once.Page.Locator("#beds")).ToHaveTextAsync("-");
        await held.Release();
        await Expect(once.Page.Locator("#beds")).ToHaveTextAsync("4");
        await once.Page.GotoAsync(WorldPages.Collector);
        await Expect(once.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(
        [
            Render,
            Render + " > alis.boot completed",
            Run + " completed",
            Send + " completed",
            Send + " > " + FreeBeds + " completed 200",
        ]);
    }
}
