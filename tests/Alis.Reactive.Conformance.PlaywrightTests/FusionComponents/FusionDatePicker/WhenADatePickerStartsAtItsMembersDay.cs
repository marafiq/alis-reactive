using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenADatePickerStartsAtItsMembersDay(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F15.M21_X1008_0)]
    public async Task the_box_shows_the_day_the_member_holds()
    {
        var known = await NewWorld();
        await known.Page.GotoAsync(CarePages.ArrivalDayKnown);

        await Expect(known.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-14");

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.ArrivalDayOpen);

        await Expect(open.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("");
    }
}
