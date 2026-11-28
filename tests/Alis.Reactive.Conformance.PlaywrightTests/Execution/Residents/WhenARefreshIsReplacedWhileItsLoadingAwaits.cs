using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.DefaultHooks))]
[Category(Hosts.DefaultHooksName)]
public sealed class WhenARefreshIsReplacedWhileItsLoadingAwaits(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X11_21_0)]
    public async Task the_replaced_request_runs_its_defaults_finally_and_neither_its_own_finally_nor_its_route_and_never_shows_its_busy_target()
    {
        var replaced = await NewWorld();
        await replaced.Page.GotoAsync(CarePages.RosterRefreshUnderReplace);
        var armedFirst = await replaced.Arm(RunGate.Settle, WorldEndpoints.Settle);
        await replaced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh", Exact = true }).ClickAsync();
        var heldFirst = await armedFirst.Held();
        var armedSecond = await replaced.Arm(RunGate.Settle, WorldEndpoints.Settle);
        await replaced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh", Exact = true }).ClickAsync();
        var heldSecond = await armedSecond.Held();
        await Expect(replaced.Page.Locator("#flush-line")).ToHaveTextAsync("flushed");
        await Expect(replaced.Page.Locator("#request-log")).ToHaveTextAsync("defaults finally");
        await Expect(replaced.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        await heldFirst.Release();

        await Expect(replaced.Page.Locator("#census").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Settle", Exact = true })).ToHaveTextAsync("Answered");
        await Expect(replaced.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        await Expect(replaced.Page.Locator("#request-log")).ToHaveTextAsync("defaults finally");
        await heldSecond.Release();

        await Expect(replaced.Page.Locator("#request-log")).ToHaveTextAsync("defaults finally route 2 finally 2 defaults finally");
        await Expect(replaced.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy calls", Exact = true })).ToHaveTextAsync("Busy calls: show roster-busy; hide roster-busy");

        var overlapped = await NewWorld();
        await overlapped.Page.GotoAsync(CarePages.RosterRefreshUnderOverlap);
        var armedOnce = await overlapped.Arm(RunGate.Settle, WorldEndpoints.Settle);
        await overlapped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh", Exact = true }).ClickAsync();
        var heldOnce = await armedOnce.Held();
        var armedTwice = await overlapped.Arm(RunGate.Settle, WorldEndpoints.Settle);
        await overlapped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh", Exact = true }).ClickAsync();
        var heldTwice = await armedTwice.Held();
        await heldOnce.Release();

        await Expect(overlapped.Page.Locator("#request-log")).ToHaveTextAsync("route 1 finally 1 defaults finally");
        await heldTwice.Release();

        await Expect(overlapped.Page.Locator("#request-log")).ToHaveTextAsync("route 1 finally 1 defaults finally route 2 finally 2 defaults finally");
    }
}
