using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenThreePanelsRaiseWhileOneHandlerAwaits(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X11_11_0)]
    public async Task each_panel_reads_what_its_handlers_wrote_before_their_first_await_and_the_awaiting_handler_writes_calls_and_reads_the_payload_after_it()
    {
        var record = await NewWorld();
        await record.Page.GotoAsync(CarePages.AlarmPanels);
        var armed = await record.Arm(RunGate.Settle);

        await record.Page.Locator("#first-panel").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await record.Page.Locator("#second-panel").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await record.Page.Locator("#third-panel").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(record.Page.Locator("#first-panel").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Payload", Exact = true })).ToHaveTextAsync("Payload text: Kappa");
        await Expect(record.Page.Locator("#ack-status")).ToHaveTextAsync("Writing");
        await Expect(record.Page.Locator("#second-panel").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Payload", Exact = true })).ToHaveTextAsync("Payload text: Kappa");
        await Expect(record.Page.Locator("#third-panel").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Payload", Exact = true })).ToHaveTextAsync("Payload text: none");
        await Expect(record.Page.Locator("#first-panel").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Outer", Exact = true })).ToHaveTextAsync("Outer: Gamma");
        await Expect(record.Page.Locator("#first-panel").GetByText("Payload acted", new LocatorGetByTextOptions { Exact = true })).ToHaveCountAsync(0);
        await Expect(record.Page.Locator("#ack-total")).ToHaveTextAsync("");
        await held.Release();

        await Expect(record.Page.Locator("#first-panel").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Outer", Exact = true })).ToHaveTextAsync("Outer: acknowledged");
        await Expect(record.Page.Locator("#first-panel").GetByText("Payload acted", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(record.Page.Locator("#ack-total")).ToHaveTextAsync("2");
        await Expect(record.Page.Locator("#ack-status")).ToHaveTextAsync("Settled");
    }
}
