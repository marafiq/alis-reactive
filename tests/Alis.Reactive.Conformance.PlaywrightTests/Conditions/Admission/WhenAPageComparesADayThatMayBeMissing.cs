using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Conditions;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPageComparesADayThatMayBeMissing(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M3_F1.X8_27_0)]
    public async Task a_picked_day_compares_as_a_day_and_a_missing_day_is_neither_ordered_nor_between()
    {
        var booking = await NewWorld();
        await booking.Page.GotoAsync(CarePages.OpenDayBooking);
        await booking.Page.Locator("#care-picker").GetByLabel("Day", new LocatorGetByLabelOptions { Exact = true }).FillAsync("2026-10-26");
        await booking.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(booking.Page.Locator("#before")).ToHaveTextAsync("before the open day");
        await Expect(booking.Page.Locator("#by")).ToHaveTextAsync("by the open day");
        await booking.Page.Locator("#care-picker").GetByLabel("Day", new LocatorGetByLabelOptions { Exact = true }).FillAsync("2026-10-27");
        await booking.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(booking.Page.Locator("#same")).ToHaveTextAsync("the open day");
        await Expect(booking.Page.Locator("#other")).ToHaveTextAsync("the open day itself");
        await Expect(booking.Page.Locator("#by")).ToHaveTextAsync("by the open day");
        await Expect(booking.Page.Locator("#from")).ToHaveTextAsync("from the open day on");
        await Expect(booking.Page.Locator("#week")).ToHaveTextAsync("in the open week");
        await booking.Page.Locator("#care-picker").GetByLabel("Day", new LocatorGetByLabelOptions { Exact = true }).FillAsync("2026-10-28");
        await booking.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(booking.Page.Locator("#other")).ToHaveTextAsync("another day");
        await Expect(booking.Page.Locator("#after")).ToHaveTextAsync("after the open day");
        await Expect(booking.Page.Locator("#from")).ToHaveTextAsync("from the open day on");
        await booking.Page.Locator("#care-picker").GetByLabel("Day", new LocatorGetByLabelOptions { Exact = true }).ClearAsync();
        await booking.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(booking.Page.Locator("#picked")).ToHaveTextAsync("pick a day");
        await Expect(booking.Page.Locator("#to-book")).ToHaveTextAsync("nothing to book");
        await Expect(booking.Page.Locator("#other")).ToHaveTextAsync("another day");
        await Expect(booking.Page.Locator("#before")).ToHaveTextAsync("not before the open day");
        await Expect(booking.Page.Locator("#by")).ToHaveTextAsync("not by the open day");
        await Expect(booking.Page.Locator("#after")).ToHaveTextAsync("not after the open day");
        await Expect(booking.Page.Locator("#from")).ToHaveTextAsync("not from the open day on");
        await Expect(booking.Page.Locator("#week")).ToHaveTextAsync("outside the open week");

        var lateBooking = await NewWorld();
        await lateBooking.Page.GotoAsync(CarePages.OpenDayBooking);
        await lateBooking.Page.Locator("#care-picker").GetByLabel("Day", new LocatorGetByLabelOptions { Exact = true }).FillAsync("2026-10-30");
        await lateBooking.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(lateBooking.Page.Locator("#week")).ToHaveTextAsync("outside the open week");
    }
}
