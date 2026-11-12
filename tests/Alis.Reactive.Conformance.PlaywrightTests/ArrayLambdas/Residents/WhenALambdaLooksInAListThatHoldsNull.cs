using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaLooksInAListThatHoldsNull(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_35_0)]
    public async Task null_is_found_in_the_list_and_an_item_that_reads_nothing_is_not()
    {
        var withAnUnsetWard = await NewWorld();
        await withAnUnsetWard.Page.GotoAsync(CarePages.RoomWardsLookedUpWithAnUnsetWard);
        await Expect(withAnUnsetWard.Page.Locator("#ward-dose-count")).ToHaveTextAsync("2");

        var inTheEastWard = await NewWorld();
        await inTheEastWard.Page.GotoAsync(CarePages.RoomWardsLookedUpInTheEastWard);
        await Expect(inTheEastWard.Page.Locator("#ward-dose-count")).ToHaveTextAsync("1");
    }
}
