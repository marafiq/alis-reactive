using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.TriggersAndPipelines;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenABellsRouteRingsTheBellAgain(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M1_F1.X11_24_0)]
    public async Task under_replace_the_old_runs_finally_runs_before_the_new_runs_first_step_and_no_step_after_the_old_request_runs()
    {
        var replaced = await NewWorld();
        await replaced.Page.GotoAsync(CarePages.BedsideBellUnderReplace);
        await replaced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ring the bell", Exact = true }).ClickAsync();
        await Expect(replaced.Page.Locator("#bell-log")).ToHaveTextAsync("rang 1 finally 1 rang 2 finally 2 after 2");

        var overlapped = await NewWorld();
        await overlapped.Page.GotoAsync(CarePages.BedsideBellUnderOverlap);
        await overlapped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ring the bell", Exact = true }).ClickAsync();
        await Expect(overlapped.Page.Locator("#bell-log")).ToHaveTextAsync("rang 1 rang 2 finally 1 after 1 finally 2 after 2");
    }
}
