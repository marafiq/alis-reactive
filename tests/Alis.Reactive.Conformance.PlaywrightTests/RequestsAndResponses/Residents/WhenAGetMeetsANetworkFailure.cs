using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAGetMeetsANetworkFailure(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F2.X10_3_0)]
    public async Task its_catch_all_and_its_finally_run_and_its_card_is_no_longer_busy()
    {
        var offline = await NewWorld();
        await offline.Seed(Seed.TheSummaryEndpointDropsItsConnection());
        await offline.Page.GotoAsync(CarePages.ResidentSummaryCard);
        await offline.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load summary", Exact = true }).ClickAsync();
        await Expect(offline.Page.Locator("#summary-error")).ToHaveTextAsync("Offline");
        await Expect(offline.Page.Locator("#summary-status")).ToHaveTextAsync("Done");
        await Expect(offline.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy calls", Exact = true }))
            .ToHaveTextAsync("Busy calls: show resident-card; hide resident-card");
        await Expect(offline.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        await Expect(offline.Page.Locator("#resident-card")).Not.ToHaveAttributeAsync("aria-busy", "true");
        await Expect(offline.Page.Locator("#resident-card")).ToHaveTextAsync("-");

        var online = await NewWorld();
        await online.Page.GotoAsync(CarePages.ResidentSummaryCard);
        await online.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load summary", Exact = true }).ClickAsync();
        await Expect(online.Page.Locator("#resident-card")).ToHaveTextAsync("Ada Hart");
        await Expect(online.Page.Locator("#summary-status")).ToHaveTextAsync("Done");
        await Expect(online.Page.Locator("#summary-error")).ToBeEmptyAsync();
    }
}
