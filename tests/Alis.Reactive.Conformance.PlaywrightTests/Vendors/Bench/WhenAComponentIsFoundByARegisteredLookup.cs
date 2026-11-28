using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAComponentIsFoundByARegisteredLookup(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F1.X17_10_0)]
    public async Task the_gauge_is_read_written_called_and_heard_and_a_gauge_never_made_stops_only_its_own_pipeline()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.VendorsRegisteredGauges);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read the first", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#reading-1")).ToHaveTextAsync("97");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Set the threshold to 92", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#registered-1").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Threshold", Exact = true })).ToHaveTextAsync("92");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise the alarm", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#registered-1").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Alarm", Exact = true })).ToHaveTextAsync("Alarm raised");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Lower the alarm", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#registered-1").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Alarm", Exact = true })).ToHaveTextAsync("Alarm lowered");
        await bench.Page.Locator("#registered-1").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Sound the alarm", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#alarm-heard")).ToHaveTextAsync("alarmed");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read the second", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#second-tried")).ToHaveTextAsync("reading");
        await Expect(bench.Page.Locator("#reading-2")).ToHaveTextAsync("");
        await Expect(bench.Page.Locator("#second-done")).ToHaveTextAsync("");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise the alarm", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#registered-1").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Alarm", Exact = true })).ToHaveTextAsync("Alarm raised");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Make the second", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read the second", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#reading-2")).ToHaveTextAsync("88");
        await Expect(bench.Page.Locator("#second-done")).ToHaveTextAsync("read");
    }
}
