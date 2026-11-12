using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Conditions;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPageComparesTwoDaysItsComponentsHold(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M3_F1.X8_27_1)]
    public async Task each_ordering_holds_as_the_two_days_are_ordered_and_one_day_is_neither_before_nor_after_itself()
    {
        var stay = await NewWorld();
        await stay.Page.GotoAsync(CarePages.Stay);
        await stay.Page.Locator("#leave").GetByLabel("Day", new LocatorGetByLabelOptions { Exact = true }).FillAsync("2026-10-28");
        await stay.Page.Locator("#arrive").GetByLabel("Day", new LocatorGetByLabelOptions { Exact = true }).FillAsync("2026-10-27");
        await stay.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(stay.Page.Locator("#later")).ToHaveTextAsync("leaves after arriving");
        await Expect(stay.Page.Locator("#same-or-earlier")).ToHaveTextAsync("leaves after arriving");
        await stay.Page.Locator("#arrive").GetByLabel("Day", new LocatorGetByLabelOptions { Exact = true }).FillAsync("2026-10-28");
        await stay.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(stay.Page.Locator("#same-or-earlier")).ToHaveTextAsync("leaves on or before arriving");
        await Expect(stay.Page.Locator("#later")).ToHaveTextAsync("does not leave after arriving");
        await stay.Page.Locator("#arrive").GetByLabel("Day", new LocatorGetByLabelOptions { Exact = true }).FillAsync("2026-10-29");
        await stay.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(stay.Page.Locator("#earlier")).ToHaveTextAsync("leaves before arriving");
        await Expect(stay.Page.Locator("#same-or-earlier")).ToHaveTextAsync("leaves on or before arriving");
        await Expect(stay.Page.Locator("#later")).ToHaveTextAsync("does not leave after arriving");

        var sameDay = await NewWorld();
        await sameDay.Page.GotoAsync(CarePages.Stay);
        await sameDay.Page.Locator("#leave").GetByLabel("Day", new LocatorGetByLabelOptions { Exact = true }).FillAsync("2026-10-28");
        await sameDay.Page.Locator("#arrive").GetByLabel("Day", new LocatorGetByLabelOptions { Exact = true }).FillAsync("2026-10-28");
        await sameDay.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(sameDay.Page.Locator("#same-or-later")).ToHaveTextAsync("leaves on or after arriving");
        await Expect(sameDay.Page.Locator("#same-or-earlier")).ToHaveTextAsync("leaves on or before arriving");
        await Expect(sameDay.Page.Locator("#later")).ToHaveTextAsync("does not leave after arriving");
        await Expect(sameDay.Page.Locator("#earlier")).ToHaveTextAsync("does not leave before arriving");
    }
}
