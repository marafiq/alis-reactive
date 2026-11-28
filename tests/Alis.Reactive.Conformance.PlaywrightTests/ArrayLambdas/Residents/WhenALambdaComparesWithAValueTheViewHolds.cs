using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaComparesWithAValueTheViewHolds(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_5_0)]
    public async Task the_count_follows_the_models_ward_read_from_the_model_and_from_a_local()
    {
        var east = await NewWorld();
        await east.Page.GotoAsync(CarePages.CensusOfTheEastWard);
        await Expect(east.Page.Locator("#ward-count")).ToHaveTextAsync("2");
        await Expect(east.Page.Locator("#round-count")).ToHaveTextAsync("2");
        await Expect(east.Page.Locator("#east-count")).ToHaveTextAsync("2");

        var west = await NewWorld();
        await west.Page.GotoAsync(CarePages.CensusOfTheWestWard);
        await Expect(west.Page.Locator("#ward-count")).ToHaveTextAsync("3");
        await Expect(west.Page.Locator("#round-count")).ToHaveTextAsync("3");
        await Expect(west.Page.Locator("#east-count")).ToHaveTextAsync("2");
    }
}
