using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALoadedNoticesPlanIsNotJson(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X4_0)]
    public async Task the_slot_keeps_its_notice_and_its_handlers_and_the_step_stops_before_it_writes()
    {
        var torn = await NewWorld();
        await torn.Page.GotoAsync(CarePages.Noticeboard);
        await torn.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Pin the notice", Exact = true }).ClickAsync();
        await Expect(torn.Page.Locator("#pinned")).ToHaveTextAsync("the notice");
        await torn.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Pin the torn notice", Exact = true }).ClickAsync();
        await Expect(torn.Page.Locator("#pin-ended")).ToHaveTextAsync("the torn notice");
        await Expect(torn.Page.Locator("#pinning")).ToHaveTextAsync("the torn notice");
        await Expect(torn.Page.Locator("#pinned")).ToHaveTextAsync("the notice");
        await Expect(torn.Page.Locator("#notice-slot").GetByText("Visiting hours change on Sunday.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await torn.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Mark the notice read", Exact = true }).ClickAsync();
        await Expect(torn.Page.Locator("#notice-read")).ToHaveTextAsync("read");

        var repinned = await NewWorld();
        await repinned.Page.GotoAsync(CarePages.Noticeboard);
        await repinned.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Pin the notice", Exact = true }).ClickAsync();
        await Expect(repinned.Page.Locator("#pinned")).ToHaveTextAsync("the notice");
        await repinned.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Pin the notice", Exact = true }).ClickAsync();
        await Expect(repinned.Page.Locator("#notice-slot").GetByText("Visiting hours change on Sunday.", new LocatorGetByTextOptions { Exact = true })).ToHaveCountAsync(1);
        await Expect(repinned.Page.Locator("#pinned")).ToHaveTextAsync("the notice");
    }
}
