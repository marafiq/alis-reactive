using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaCastsANullableToItsValue(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_47_0)]
    public async Task a_cast_in_a_test_drops_the_item_that_holds_null_and_a_cast_in_a_sum_stops_the_pipeline()
    {
        var withAnUnprioritisedDose = await NewWorld();
        await withAnUnprioritisedDose.Page.GotoAsync(CarePages.RoutineDosesWithAnUnprioritisedDose);
        await Expect(withAnUnprioritisedDose.Page.Locator("#off-routine-count")).ToHaveTextAsync("2");
        await Expect(withAnUnprioritisedDose.Page.Locator("#routine-count")).ToHaveTextAsync("2");
        await Expect(withAnUnprioritisedDose.Page.Locator("#priority-total")).ToHaveTextAsync("-");
        await Expect(withAnUnprioritisedDose.Page.Locator("#doses-status")).ToHaveTextAsync("Waiting");

        var withEveryDosePrioritised = await NewWorld();
        await withEveryDosePrioritised.Page.GotoAsync(CarePages.RoutineDosesWithEveryDosePrioritised);
        await Expect(withEveryDosePrioritised.Page.Locator("#priority-total")).ToHaveTextAsync("8");
        await Expect(withEveryDosePrioritised.Page.Locator("#doses-status")).ToHaveTextAsync("Summed");
    }
}
