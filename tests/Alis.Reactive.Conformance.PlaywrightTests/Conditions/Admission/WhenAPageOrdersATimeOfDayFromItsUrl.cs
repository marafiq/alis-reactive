using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Conditions;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPageOrdersATimeOfDayFromItsUrl(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M3_F1.X8_27_2)]
    public async Task a_time_before_noon_is_a_morning_shift_and_a_time_after_noon_is_not()
    {
        var shift = await NewWorld();
        await shift.Page.GotoAsync(CarePages.ShiftStartingAtSeven);
        await Expect(shift.Page.Locator("#shift")).ToHaveTextAsync("a morning shift");
        await shift.Page.GotoAsync(CarePages.ShiftStartingAtHalfPastTwo);
        await Expect(shift.Page.Locator("#shift")).ToHaveTextAsync("not a morning shift");

        var beforeNoon = await NewWorld();
        await beforeNoon.Page.GotoAsync(CarePages.ShiftStartingATickBeforeNoon);
        await Expect(beforeNoon.Page.Locator("#shift")).ToHaveTextAsync("a morning shift");
    }
}
