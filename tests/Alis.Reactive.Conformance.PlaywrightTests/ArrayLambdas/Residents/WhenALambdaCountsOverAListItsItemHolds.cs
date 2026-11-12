using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaCountsOverAListItsItemHolds(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_20_0)]
    public async Task the_inner_lambda_reads_its_own_item_and_the_outer_one_and_counts_as_dotnet_counts()
    {
        var round = await NewWorld();
        await round.Page.GotoAsync(CarePages.DoseChecksOfTheRound);
        await Expect(round.Page.Locator("#checked-by-carer-count")).ToHaveTextAsync("1");
        await Expect(round.Page.Locator("#all-signed-count")).ToHaveTextAsync("2");
        await Expect(round.Page.Locator("#checked-count")).ToHaveTextAsync("2");

        var withBenChecking = await NewWorld();
        await withBenChecking.Page.GotoAsync(CarePages.DoseChecksWithBenChecking);
        await Expect(withBenChecking.Page.Locator("#checked-by-carer-count")).ToHaveTextAsync("2");
    }
}
