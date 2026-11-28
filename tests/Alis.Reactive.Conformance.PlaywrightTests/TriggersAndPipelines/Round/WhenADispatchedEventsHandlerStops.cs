using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.TriggersAndPipelines;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenADispatchedEventsHandlerStops(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M1_F2.M1_X2_0)]
    public async Task every_handler_runs_before_the_dispatchers_next_step_and_the_stop_ends_neither_the_dispatch_nor_the_dispatcher()
    {
        var logged = await NewWorld();
        await logged.Page.GotoAsync(CarePages.CallDesk);
        await logged.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Log the call", Exact = true }).ClickAsync();
        await Expect(logged.Page.Locator("#call-log")).ToHaveTextAsync("Logged");
        await Expect(logged.Page.Locator("#pager")).ToHaveTextAsync("Paged");
        await Expect(logged.Page.Locator("#desk-status")).ToHaveTextAsync("Paged");

        var unheard = await NewWorld();
        await unheard.Page.GotoAsync(CarePages.CallDeskWithNoLogHandler);
        await unheard.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Log the call", Exact = true }).ClickAsync();
        await Expect(unheard.Page.Locator("#desk-status")).ToHaveTextAsync("Idle");
    }
}
