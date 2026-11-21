using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Realtime;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACardThatSharesAHubIsClosed(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M10_F2.X12_5_0)]
    public async Task the_page_and_the_open_card_still_hear_the_hub()
    {
        var closed = await NewWorld();
        await closed.Page.GotoAsync(CarePages.LiveStatusBoard);
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open Ada's card", Exact = true }).ClickAsync();
        await Expect(closed.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Ada", Exact = true })).ToBeVisibleAsync();
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open Ben's card", Exact = true }).ClickAsync();
        await Expect(closed.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Ben", Exact = true })).ToBeVisibleAsync();
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close Ada's card", Exact = true }).ClickAsync();
        await Expect(closed.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Ada", Exact = true })).ToHaveCountAsync(0);
        await closed.Opened(CareEndpoints.WardLine);
        await closed.Push(CareEndpoints.WardLine, "statusChanged", "[{\"resident\":\"Ben\",\"status\":\"Resting\"}]");
        await Expect(closed.Page.Locator("#page-status")).ToHaveTextAsync("Ben");
        await Expect(closed.Page.Locator("#ben-status")).ToHaveTextAsync("Resting");

        var both = await NewWorld();
        await both.Page.GotoAsync(CarePages.LiveStatusBoard);
        await both.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open Ada's card", Exact = true }).ClickAsync();
        await Expect(both.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Ada", Exact = true })).ToBeVisibleAsync();
        await both.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open Ben's card", Exact = true }).ClickAsync();
        await Expect(both.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Ben", Exact = true })).ToBeVisibleAsync();
        await both.Opened(CareEndpoints.WardLine);
        await both.Push(CareEndpoints.WardLine, "statusChanged", "[{\"resident\":\"Ada\",\"status\":\"Awake\"}]");
        await Expect(both.Page.Locator("#ada-status")).ToHaveTextAsync("Awake");
        await Expect(both.Page.Locator("#ben-status")).ToBeEmptyAsync();
    }
}
