using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenADateTimePickerStartsAtItsMembersTime(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F17.M21_X1010_0)]
    public async Task the_box_shows_the_day_and_time_the_member_holds()
    {
        var booked = await NewWorld();
        await booked.Page.GotoAsync(CarePages.FirstVisitBooked);

        await Expect(booked.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "First visit", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-14 09:30");

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.FirstVisitOpen);

        await Expect(open.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "First visit", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("");
    }
}
