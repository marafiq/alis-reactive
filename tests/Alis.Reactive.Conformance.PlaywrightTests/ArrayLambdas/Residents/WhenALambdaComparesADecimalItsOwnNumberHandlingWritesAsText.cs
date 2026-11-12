using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaComparesADecimalItsOwnNumberHandlingWritesAsText(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_53_0)]
    public async Task the_copays_are_compared_as_decimals_and_a_text_that_is_no_number_stops_the_counts()
    {
        var round = await NewWorld();
        await round.Page.GotoAsync(CarePages.RoundCopays);
        await Expect(round.Page.Locator("#equal-count")).ToHaveTextAsync("2");
        await Expect(round.Page.Locator("#unequal-count")).ToHaveTextAsync("1");
        await Expect(round.Page.Locator("#copay-status")).ToHaveTextAsync("Counted");

        var unread = await NewWorld();
        await unread.Page.GotoAsync(CarePages.RoundCopaysWithOneUnread);
        await Expect(unread.Page.Locator("#dose-count")).ToHaveTextAsync("5");
        await Expect(unread.Page.Locator("#equal-count")).ToHaveTextAsync("-");
        await Expect(unread.Page.Locator("#unequal-count")).ToHaveTextAsync("-");
        await Expect(unread.Page.Locator("#copay-status")).ToHaveTextAsync("Waiting");
    }
}
