using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARedirectEndsOnAnotherOrigin(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F2.M6_X1_0)]
    public async Task the_catch_all_runs_with_the_outcome_redirect_and_no_other_route_runs()
    {
        var moved = await NewWorld();
        await moved.Page.GotoAsync(CarePages.TransferSummaryMovedToAnotherOrigin);
        await moved.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(moved.Page.Locator("#summary-status")).ToHaveTextAsync("redirect");

        var kept = await NewWorld();
        await kept.Page.GotoAsync(CarePages.TransferSummaryMovedWithinThisOrigin);
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(kept.Page.Locator("#summary-status")).ToHaveTextAsync("Loaded");
    }
}
