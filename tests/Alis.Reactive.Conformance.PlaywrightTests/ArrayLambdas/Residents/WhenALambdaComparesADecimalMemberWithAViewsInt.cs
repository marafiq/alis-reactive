using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaComparesADecimalMemberWithAViewsInt(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_51_0)]
    public async Task the_doses_charged_above_the_views_int_are_counted()
    {
        var free = await NewWorld();
        await free.Page.GotoAsync(CarePages.ChargedDosesAboveNoCharge);
        await Expect(free.Page.Locator("#charged-count")).ToHaveTextAsync("2");

        var aboveThree = await NewWorld();
        await aboveThree.Page.GotoAsync(CarePages.ChargedDosesAboveThree);
        await Expect(aboveThree.Page.Locator("#charged-count")).ToHaveTextAsync("1");
    }
}
