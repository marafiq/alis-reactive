using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARouteReadsItsRepliesHeaders(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F2.M6_X4_0)]
    public async Task a_name_reads_in_any_case_a_repeated_header_joins_its_values_and_a_missing_one_shows_nothing()
    {
        var twoWards = await NewWorld();
        await twoWards.Page.GotoAsync(CarePages.WardNoticeOfTwoWards);
        await twoWards.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(twoWards.Page.Locator("#notice-version")).ToHaveTextAsync("\"v7\"");
        await Expect(twoWards.Page.Locator("#notice-wards")).ToHaveTextAsync("East, West");
        await Expect(twoWards.Page.Locator("#notice-missing")).ToHaveTextAsync("");

        var oneWard = await NewWorld();
        await oneWard.Page.GotoAsync(CarePages.WardNoticeOfOneWard);
        await oneWard.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(oneWard.Page.Locator("#notice-wards")).ToHaveTextAsync("East");
    }
}
