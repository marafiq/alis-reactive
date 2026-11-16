using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.DefaultHooks))]
[Category(Hosts.DefaultHooksName)]
public sealed class WhenARefreshIsReplacedWhileItsFinallyAwaits(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X11_22_0)]
    public async Task the_old_finally_goes_on_to_its_end_once_its_defaults_finally_runs_after_it_and_no_step_after_the_old_request_runs()
    {
        var replaced = await NewWorld();
        await replaced.Page.GotoAsync(CarePages.RosterRecountUnderReplace);
        var armedFirst = await replaced.Arm(RunGate.Settle, WorldEndpoints.Settle);
        await replaced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh", Exact = true }).ClickAsync();
        var heldFirst = await armedFirst.Held();
        await Expect(replaced.Page.Locator("#request-log")).ToHaveTextAsync("finally start 1");
        var armedSecond = await replaced.Arm(RunGate.Settle, WorldEndpoints.Settle);
        await replaced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh", Exact = true }).ClickAsync();
        var heldSecond = await armedSecond.Held();
        await Expect(replaced.Page.Locator("#request-log")).ToHaveTextAsync("finally start 1 finally start 2");
        await heldFirst.Release();

        await Expect(replaced.Page.Locator("#flush-line")).ToHaveTextAsync("flushed");
        await Expect(replaced.Page.Locator("#request-log")).ToHaveTextAsync("finally start 1 finally start 2 finally end 1 defaults finally");
        await heldSecond.Release();

        await Expect(replaced.Page.Locator("#request-log")).ToHaveTextAsync("finally start 1 finally start 2 finally end 1 defaults finally finally end 2 defaults finally after 2");

        var overlapped = await NewWorld();
        await overlapped.Page.GotoAsync(CarePages.RosterRecountUnderOverlap);
        var armedOnce = await overlapped.Arm(RunGate.Settle, WorldEndpoints.Settle);
        await overlapped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh", Exact = true }).ClickAsync();
        var heldOnce = await armedOnce.Held();
        await Expect(overlapped.Page.Locator("#request-log")).ToHaveTextAsync("finally start 1");
        var armedTwice = await overlapped.Arm(RunGate.Settle, WorldEndpoints.Settle);
        await overlapped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh", Exact = true }).ClickAsync();
        var heldTwice = await armedTwice.Held();
        await Expect(overlapped.Page.Locator("#request-log")).ToHaveTextAsync("finally start 1 finally start 2");
        await heldOnce.Release();

        await Expect(overlapped.Page.Locator("#flush-line")).ToHaveTextAsync("flushed");
        await Expect(overlapped.Page.Locator("#request-log")).ToHaveTextAsync("finally start 1 finally start 2 finally end 1 defaults finally after 1");
        await heldTwice.Release();

        await Expect(overlapped.Page.Locator("#request-log")).ToHaveTextAsync("finally start 1 finally start 2 finally end 1 defaults finally after 1 finally end 2 defaults finally after 2");
    }
}
