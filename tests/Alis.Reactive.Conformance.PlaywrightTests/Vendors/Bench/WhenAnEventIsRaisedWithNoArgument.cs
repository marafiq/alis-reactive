using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnEventIsRaisedWithNoArgument(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F1.X17_15_0)]
    public async Task a_declared_payload_that_is_missing_stops_the_handler_and_an_event_declared_with_none_runs_it()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.VendorsEmptyPayloads);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise with no payload", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Raises", Exact = true })).ToHaveTextAsync("Raised 1");
        await Expect(bench.Page.Locator("#raise-tried")).ToHaveTextAsync("");
        await Expect(bench.Page.Locator("#raise-text")).ToHaveTextAsync("");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sound the alarm", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#alarm-heard")).ToHaveTextAsync("alarmed");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise with text 7", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#raise-tried")).ToHaveTextAsync("raising");
        await Expect(bench.Page.Locator("#raise-text")).ToHaveTextAsync("7");
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Raises", Exact = true })).ToHaveTextAsync("Raised 2");
    }
}
