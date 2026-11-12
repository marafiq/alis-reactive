using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHandlerCallsAMethodAnEventsPayloadCarries(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.event_method)]
    public async Task the_method_the_payload_carries_runs()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.EventMethod);
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Raise", Exact = true })).ToHaveTextAsync("Waiting");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Raise", Exact = true })).ToHaveTextAsync("Acted");

        var plain = await NewWorld();
        await plain.Page.GotoAsync(BenchPages.EventMethod);
        await plain.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise plain", Exact = true }).ClickAsync();
        await Expect(plain.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Raise", Exact = true })).ToHaveTextAsync("Acted");
    }
}
