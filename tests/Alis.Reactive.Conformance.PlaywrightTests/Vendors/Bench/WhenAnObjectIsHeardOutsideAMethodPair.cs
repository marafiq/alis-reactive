using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnObjectIsHeardOutsideAMethodPair(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F1.X17_17_0)]
    public async Task a_callback_property_and_a_subscribe_that_returns_its_unsubscriber_each_reach_their_handler()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.VendorsCallbackAndStore);
        await Expect(bench.Page.Locator("#raised-room")).ToHaveTextAsync("");
        await Expect(bench.Page.Locator("#posted-message")).ToHaveTextAsync("");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#raised-room")).ToHaveTextAsync("7");
        await Expect(bench.Page.Locator("#callbacks").GetByRole(AriaRole.Log, new LocatorGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "own" });
        await Expect(bench.Page.Locator("#posted-message")).ToHaveTextAsync("");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Post an update", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#posted-message")).ToHaveTextAsync("Bed 4 needs water");
    }
}
