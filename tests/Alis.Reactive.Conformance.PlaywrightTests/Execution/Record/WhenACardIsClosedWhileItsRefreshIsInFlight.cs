using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACardIsClosedWhileItsRefreshIsInFlight(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X11_5_0)]
    public async Task the_refresh_runs_no_route_and_its_busy_target_hides()
    {
        var closed = await NewWorld();
        await closed.Page.GotoAsync(CarePages.WardRefresh);
        var armed = await closed.Arm(CompositionGate.Answer);

        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await Expect(closed.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: ward-busy");
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close card", Exact = true }).ClickAsync();
        await Expect(closed.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Resident card", Exact = true })).ToBeHiddenAsync();
        await Expect(closed.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        await held.Release();

        await Expect(closed.Page.Locator("#ward-status")).ToHaveTextAsync("");

        var kept = await NewWorld();
        await kept.Page.GotoAsync(CarePages.WardRefresh);
        var armedToAnswer = await kept.Arm(CompositionGate.Answer);

        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh", Exact = true }).ClickAsync();
        var heldToAnswer = await armedToAnswer.Held();
        await Expect(kept.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: ward-busy");
        await heldToAnswer.Release();

        await Expect(kept.Page.Locator("#ward-status")).ToHaveTextAsync("Refreshed");
        await Expect(kept.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
    }
}
