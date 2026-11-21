using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Conditions;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaJoinsTermsOverACarerThatHoldsNull(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M3_F2.X8_8_0)]
    public async Task or_keeps_an_item_whose_right_term_holds_and_not_holds_where_and_reads_false()
    {
        var north = await NewWorld();
        await north.Page.GotoAsync(CarePages.VisitCountsWithAVisitInTheNorthWard);
        await north.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count", Exact = true }).ClickAsync();
        await Expect(north.Page.Locator("#ruth-or-north")).ToHaveTextAsync("2");
        await Expect(north.Page.Locator("#not-ruth-in-south")).ToHaveTextAsync("1");

        var south = await NewWorld();
        await south.Page.GotoAsync(CarePages.VisitCountsWithEveryVisitInTheSouthWard);
        await south.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count", Exact = true }).ClickAsync();
        await Expect(south.Page.Locator("#ruth-or-north")).ToHaveTextAsync("1");
        await Expect(south.Page.Locator("#not-ruth-in-south")).ToHaveTextAsync("0");
    }
}
