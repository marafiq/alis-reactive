using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaLooksAMemberUpInAListTheViewHolds(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_26_0)]
    public async Task each_lookup_keeps_the_items_the_view_lists_and_a_letter_a_count_and_a_length_are_read()
    {
        var eastAndNorth = await NewWorld();
        await eastAndNorth.Page.GotoAsync(CarePages.WardLookupsOfEastAndNorth);
        await Expect(eastAndNorth.Page.Locator("#rota-ward-count")).ToHaveTextAsync("3");
        await Expect(eastAndNorth.Page.Locator("#listed-ward-count")).ToHaveTextAsync("3");
        await Expect(eastAndNorth.Page.Locator("#route-ward-count")).ToHaveTextAsync("3");
        await Expect(eastAndNorth.Page.Locator("#wing-count")).ToHaveTextAsync("3");
        await Expect(eastAndNorth.Page.Locator("#letter-count")).ToHaveTextAsync("1");
        await Expect(eastAndNorth.Page.Locator("#hours-count")).ToHaveTextAsync("2");
        await Expect(eastAndNorth.Page.Locator("#codes-count")).ToHaveTextAsync("1");

        var east = await NewWorld();
        await east.Page.GotoAsync(CarePages.WardLookupsOfEast);
        await Expect(east.Page.Locator("#rota-ward-count")).ToHaveTextAsync("2");
        await Expect(east.Page.Locator("#listed-ward-count")).ToHaveTextAsync("2");
        await Expect(east.Page.Locator("#route-ward-count")).ToHaveTextAsync("2");
        await Expect(east.Page.Locator("#wing-count")).ToHaveTextAsync("2");
    }
}
