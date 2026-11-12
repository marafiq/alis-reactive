using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenATypedCatchAllReadsAFailure(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F2.X10_29_0)]
    public async Task it_reads_the_body_of_a_status_no_route_names_and_nothing_after_a_network_failure()
    {
        var ward = await NewWorld();
        await ward.Page.GotoAsync(CarePages.BedCommit);
        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(ward.Page.Locator("#bed-error")).ToHaveTextAsync("Bed taken");
        await Expect(ward.Page.Locator("#bed-outcome")).ToHaveTextAsync("status");

        await ward.Seed(Seed.TheBedsEndpointDropsItsConnection());
        await ward.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(ward.Page.Locator("#bed-outcome")).ToHaveTextAsync("network");
        await Expect(ward.Page.Locator("#bed-error")).ToBeEmptyAsync();

        var routed = await NewWorld();
        await routed.Page.GotoAsync(CarePages.BedCommitWithAConflictRoute);
        await routed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(routed.Page.Locator("#bed-outcome")).ToHaveTextAsync("Choose another bed");
        await Expect(routed.Page.Locator("#bed-error")).ToBeEmptyAsync();
    }
}
