using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Declarations;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAReturnedObjectLacksADeclaredMethod(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M11_F1.X5_3_0)]
    public async Task the_call_stops_its_own_pipeline_at_the_call()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.DeclarationsItemWithoutAct);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Act on lacking", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show lacking's text", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#text-read")).ToHaveTextAsync("Alpha");
        await Expect(bench.Page.Locator("#act-tried")).ToHaveTextAsync("acting");
        await Expect(bench.Page.Locator("#lacking-done")).ToHaveTextAsync("");

        var acted = await NewWorld();
        await acted.Page.GotoAsync(BenchPages.DeclarationsItemWithoutAct);
        await acted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Act on current", Exact = true }).ClickAsync();
        await Expect(acted.Page.Locator("#conformance").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Item One", Exact = true })).ToHaveTextAsync("One, acted");
        await Expect(acted.Page.Locator("#act-done")).ToHaveTextAsync("acted");
    }
}
