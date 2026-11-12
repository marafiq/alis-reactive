using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASelectsFieldReadsNothing(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_55_0)]
    public async Task a_field_that_holds_null_is_null_in_its_list_and_one_that_holds_none_stops_the_count()
    {
        var missing = await NewWorld();
        await missing.Page.GotoAsync(CarePages.RoundVialsWithASupplyMissing);
        await Expect(missing.Page.Locator("#unsupplied-count")).ToHaveTextAsync("1");
        await Expect(missing.Page.Locator("#supply-count")).ToHaveTextAsync("-");
        await Expect(missing.Page.Locator("#vial-status")).ToHaveTextAsync("Waiting");

        var supplied = await NewWorld();
        await supplied.Page.GotoAsync(CarePages.RoundVialsWithEverySupply);
        await Expect(supplied.Page.Locator("#supply-count")).ToHaveTextAsync("3");
        await Expect(supplied.Page.Locator("#vial-status")).ToHaveTextAsync("Counted");
    }
}
