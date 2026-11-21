using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASubscribeReturnsItsUnsubscriber(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F1.X17_11_0)]
    public async Task the_closed_cards_listener_is_removed_by_the_function_its_subscribe_returned()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.VendorsStoreUpdates);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the card", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#store").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Subscribers", Exact = true })).ToHaveTextAsync("subscribers: 2");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Post an update", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#card-heard")).ToHaveTextAsync("1");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the card", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#store").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Subscribers", Exact = true })).ToHaveTextAsync("subscribers: 1");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Post an update", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#last-update")).ToHaveTextAsync("Bed 4 needs water");
        await Expect(bench.Page.Locator("#card-heard")).ToHaveTextAsync("1");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the card", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#store").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Subscribers", Exact = true })).ToHaveTextAsync("subscribers: 2");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Post an update", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#card-heard")).ToHaveTextAsync("2");
    }
}
