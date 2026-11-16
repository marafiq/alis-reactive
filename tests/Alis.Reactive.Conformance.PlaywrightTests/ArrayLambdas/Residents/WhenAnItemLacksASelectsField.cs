using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnItemLacksASelectsField(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.M4_X2_0)]
    public async Task the_field_reads_its_start_and_a_field_that_holds_null_reads_null()
    {
        var leftOut = await NewWorld();
        await leftOut.Page.GotoAsync(CarePages.CensusCodesWithACodeLeftOut);
        await Expect(leftOut.Page.Locator("#c0-count")).ToHaveTextAsync("2");

        var sentNull = await NewWorld();
        await sentNull.Page.GotoAsync(CarePages.CensusCodesWithACodeSentNull);
        await Expect(sentNull.Page.Locator("#c0-count")).ToHaveTextAsync("1");
    }
}
