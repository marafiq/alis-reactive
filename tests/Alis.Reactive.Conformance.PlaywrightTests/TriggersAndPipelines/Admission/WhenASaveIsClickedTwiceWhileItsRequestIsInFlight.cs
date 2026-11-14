using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.TriggersAndPipelines;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASaveIsClickedTwiceWhileItsRequestIsInFlight(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M1_F1.X11_7_0)]
    public async Task under_skip_the_save_sends_one_request()
    {
        var clickedTwice = await NewWorld();
        await clickedTwice.Page.GotoAsync(CarePages.ChartSave);
        var armed = await clickedTwice.Arm(LanesGate.Respond, CareEndpoints.ChartSaves);
        await clickedTwice.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await clickedTwice.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(clickedTwice.Page.Locator("#chart-status")).ToHaveTextAsync("Not saved");
        await held.Release();
        await Expect(clickedTwice.Page.Locator("#chart-status")).ToHaveTextAsync("Saved once");

        var clickedAfter = await NewWorld();
        await clickedAfter.Page.GotoAsync(CarePages.ChartSave);
        var armedAgain = await clickedAfter.Arm(LanesGate.Respond, CareEndpoints.ChartSaves);
        await clickedAfter.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        var heldAgain = await armedAgain.Held();
        await heldAgain.Release();
        await Expect(clickedAfter.Page.Locator("#chart-status")).ToHaveTextAsync("Saved once");
        await clickedAfter.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(clickedAfter.Page.Locator("#chart-status")).ToHaveTextAsync("Saved twice");
    }
}
