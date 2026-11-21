using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHandlerWritesAnObjectThePayloadHoldsAfterAnAwait(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X11_9_0)]
    public async Task the_write_changes_that_object_once_the_awaited_step_settles()
    {
        var record = await NewWorld();
        await record.Page.GotoAsync(CarePages.PanelAcknowledgement);
        var armed = await record.Arm(RunGate.Settle);

        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await Expect(record.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Outer", Exact = true })).ToHaveTextAsync("Outer: Gamma");
        await held.Release();

        await Expect(record.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Outer", Exact = true })).ToHaveTextAsync("Outer: acknowledged");
    }
}
