using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.DefaultHooks))]
[Category(Hosts.DefaultHooksName)]
public sealed class WhenARefreshIsReplacedWhileItsRoutesRequestIsInFlight(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X11_15_0)]
    public async Task each_finally_layer_runs_once_from_the_inner_request_out_and_the_old_route_writes_nothing()
    {
        var replaced = await NewWorld();
        await replaced.Page.GotoAsync(CarePages.RosterShiftsUnderReplace);
        var armedFirst = await replaced.Arm(LanesGate.Respond, CareEndpoints.RosterShifts);
        await replaced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh", Exact = true }).ClickAsync();
        var heldFirst = await armedFirst.Held();
        var armedSecond = await replaced.Arm(LanesGate.Respond, CareEndpoints.RosterShifts);
        await replaced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh", Exact = true }).ClickAsync();
        var heldSecond = await armedSecond.Held();
        await Expect(replaced.Page.Locator("#request-log")).ToHaveTextAsync("inner finally defaults finally outer finally defaults finally");
        await heldFirst.Release();
        await heldSecond.Release();

        await Expect(replaced.Page.Locator("#request-log")).ToHaveTextAsync(
            "inner finally defaults finally outer finally defaults finally inner finally defaults finally outer finally defaults finally");
        await Expect(replaced.Page.Locator("#roster-status")).ToHaveTextAsync("shifts shown");

        var overlapped = await NewWorld();
        await overlapped.Page.GotoAsync(CarePages.RosterShiftsUnderOverlap);
        var armedOnce = await overlapped.Arm(LanesGate.Respond, CareEndpoints.RosterShifts);
        await overlapped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh", Exact = true }).ClickAsync();
        var heldOnce = await armedOnce.Held();
        await overlapped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh", Exact = true }).ClickAsync();
        await heldOnce.Release();

        await Expect(overlapped.Page.Locator("#roster-status")).ToHaveTextAsync("shifts shown shifts shown");
        await Expect(overlapped.Page.Locator("#request-log")).ToHaveTextAsync(
            "inner finally defaults finally outer finally defaults finally inner finally defaults finally outer finally defaults finally");
    }
}
