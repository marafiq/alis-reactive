using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.DefaultHooks))]
[Category(Hosts.DefaultHooksName)]
public sealed class WhenAStopInARoutesRequestUnwindsItsFinallyLayers(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X11_20_0)]
    public async Task every_finally_layer_runs_from_the_inner_request_out_and_no_step_after_either_request_runs()
    {
        var stopped = await NewWorld();
        await stopped.Page.GotoAsync(CarePages.CareLevelAdmissionWithAnOlderPicker);
        await stopped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Admit", Exact = true }).ClickAsync();
        await Expect(stopped.Page.Locator("#request-log")).ToHaveTextAsync("inner finally defaults finally outer finally defaults finally");

        var read = await NewWorld();
        await read.Page.GotoAsync(CarePages.CareLevelAdmissionWithTheCurrentPicker);
        await read.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Admit", Exact = true }).ClickAsync();
        await Expect(read.Page.Locator("#request-log")).ToHaveTextAsync("inner finally defaults finally after beds outer finally defaults finally after admission");
        await Expect(read.Page.Locator("#care-level-shown")).ToHaveTextAsync("Residential");
    }
}
