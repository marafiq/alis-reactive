using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPluginsUnsubscribeThrows(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F1.X12_16_0)]
    public async Task the_plugins_event_runs_the_reopened_cards_handler_once()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.VendorsClockTicks);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Break off", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the card", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#card").GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("The card listens to the clock.");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the card", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#card")).ToBeEmptyAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the card", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#card").GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("The card listens to the clock.");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Tick", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#ticks-heard")).ToHaveTextAsync("1");

        var unbroken = await NewWorld();
        await unbroken.Page.GotoAsync(BenchPages.VendorsClockTicks);
        await unbroken.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the card", Exact = true }).ClickAsync();
        await Expect(unbroken.Page.Locator("#card").GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("The card listens to the clock.");
        await unbroken.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the card", Exact = true }).ClickAsync();
        await Expect(unbroken.Page.Locator("#card")).ToBeEmptyAsync();
        await unbroken.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Tick", Exact = true }).ClickAsync();
        await Expect(unbroken.Page.Locator("#ticks-heard")).ToHaveTextAsync("");
    }
}
