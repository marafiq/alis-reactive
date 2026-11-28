using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.DefaultHooks))]
[Category(Hosts.DefaultHooksName)]
public sealed class WhenATabIsClosedWhileItsDefaultsFinallyAwaits(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X11_23_0)]
    public async Task the_step_after_the_await_does_not_run_and_the_busy_target_hides()
    {
        var closed = await NewWorld();
        await closed.Page.GotoAsync(CarePages.NotesTab);
        var armed = await closed.Arm(RunGate.Settle, WorldEndpoints.SessionFlush);

        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await Expect(closed.Page.Locator("#notes-refreshed")).ToHaveTextAsync("Refreshed");
        await Expect(closed.Page.Locator("#request-log")).ToHaveTextAsync("defaults finally");
        await Expect(closed.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: record-busy");
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close tab", Exact = true }).ClickAsync();
        await Expect(closed.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Care notes", Exact = true })).ToBeHiddenAsync();
        await Expect(closed.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        await held.Release();

        await Expect(closed.Page.Locator("#flush-line")).ToHaveTextAsync("");

        var kept = await NewWorld();
        await kept.Page.GotoAsync(CarePages.NotesTab);
        var armedToFlush = await kept.Arm(RunGate.Settle, WorldEndpoints.SessionFlush);

        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh", Exact = true }).ClickAsync();
        var heldToFlush = await armedToFlush.Held();
        await Expect(kept.Page.Locator("#notes-refreshed")).ToHaveTextAsync("Refreshed");
        await Expect(kept.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: record-busy");
        await heldToFlush.Release();

        await Expect(kept.Page.Locator("#flush-line")).ToHaveTextAsync("flushed");
        await Expect(kept.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
    }
}
