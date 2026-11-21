using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAJsonMemberReadsNothing(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_8_0)]
    public async Task the_member_is_left_out_of_the_body_and_the_endpoint_binds_its_start()
    {
        var wardless = await NewWorld();
        await wardless.Page.GotoAsync(CarePages.BedCountOfACensusWithNoWard);
        await wardless.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load census", Exact = true }).ClickAsync();
        await Expect(wardless.Page.Locator("#census-wing")).ToHaveTextAsync("North wing");
        await wardless.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send", Exact = true }).ClickAsync();
        await Expect(wardless.Page.Locator("#recorded-beds")).ToHaveTextAsync("Beds 1, valid");

        var warded = await NewWorld();
        await warded.Page.GotoAsync(CarePages.BedCountOfACensusWithAWard);
        await warded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load census", Exact = true }).ClickAsync();
        await Expect(warded.Page.Locator("#census-wing")).ToHaveTextAsync("North wing");
        await warded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send", Exact = true }).ClickAsync();
        await Expect(warded.Page.Locator("#recorded-beds")).ToHaveTextAsync("Beds 3, valid");
    }
}
