using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnAppBringsItsOwnComponent(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F2.X17_4_0)]
    public async Task a_read_a_write_a_call_and_an_event_each_reach_the_brought_component()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.VendorsBroughtComponent);
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Amount", Exact = true })).ToHaveTextAsync("30");
        await Expect(bench.Page.Locator("#amount-read")).ToHaveTextAsync("");
        await Expect(bench.Page.Locator("#raised-for")).ToHaveTextAsync("");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Set 15", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read the amount", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#amount-read")).ToHaveTextAsync("15");
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Amount", Exact = true })).ToHaveTextAsync("15");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add 15", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read the amount", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#amount-read")).ToHaveTextAsync("30");
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Amount", Exact = true })).ToHaveTextAsync("30");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#raised-for")).ToHaveTextAsync("Ada");
    }
}
