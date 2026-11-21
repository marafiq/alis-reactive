using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACardListensToThePeerItsPanelHands(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F1.X12_10_0)]
    public async Task the_listener_on_the_peer_leaves_with_the_card()
    {
        var record = await NewWorld();
        await record.Page.GotoAsync(CarePages.PanelHandover);
        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the panel card", Exact = true }).ClickAsync();
        await Expect(record.Page.GetByText("The card counts the changes of the peer the panel hands.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Hand a peer", Exact = true }).ClickAsync();
        await record.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Peer", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Change", Exact = true }).ClickAsync();
        await Expect(record.Page.Locator("#changed-count")).ToHaveTextAsync("1");
        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the panel card", Exact = true }).ClickAsync();
        await Expect(record.Page.GetByText("The card counts the changes of the peer the panel hands.", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await record.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Peer", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Change", Exact = true }).ClickAsync();
        await Expect(record.Page.Locator("#changed-count")).ToHaveTextAsync("1");

        var kept = await NewWorld();
        await kept.Page.GotoAsync(CarePages.PanelHandover);
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the panel card", Exact = true }).ClickAsync();
        await Expect(kept.Page.GetByText("The card counts the changes of the peer the panel hands.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Hand a peer", Exact = true }).ClickAsync();
        await kept.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Peer", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Change", Exact = true }).ClickAsync();
        await Expect(kept.Page.Locator("#changed-count")).ToHaveTextAsync("1");
        await kept.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Peer", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Change", Exact = true }).ClickAsync();
        await Expect(kept.Page.Locator("#changed-count")).ToHaveTextAsync("2");
    }
}
