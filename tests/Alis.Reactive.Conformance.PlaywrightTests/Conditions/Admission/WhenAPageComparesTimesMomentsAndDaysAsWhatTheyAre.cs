using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Conditions;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPageComparesTimesMomentsAndDaysAsWhatTheyAre(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M3_F1.X8_27_3)]
    public async Task a_time_a_moment_and_a_day_each_equal_their_literal_by_value_and_not_by_text()
    {
        var desk = await NewWorld();
        await desk.Page.GotoAsync(CarePages.NoonStartAtTwelve);
        await Expect(desk.Page.Locator("#noon")).ToHaveTextAsync("noon");
        await desk.Page.GotoAsync(CarePages.NoonStartATickAfterTwelve);
        await Expect(desk.Page.Locator("#noon")).ToHaveTextAsync("not noon");
        await desk.Page.GotoAsync(CarePages.HandoverDesk);
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load the first handover", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#moment")).ToHaveTextAsync("the same moment");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load the second handover", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#moment")).ToHaveTextAsync("another moment");
        await desk.Page.GotoAsync(CarePages.OpenDays);
        await desk.Page.Locator("#care-picker").GetByLabel("Day", new LocatorGetByLabelOptions { Exact = true }).FillAsync("2026-10-27");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#open")).ToHaveTextAsync("an open day");
        await desk.Page.Locator("#care-picker").GetByLabel("Day", new LocatorGetByLabelOptions { Exact = true }).FillAsync("2026-10-28");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#open")).ToHaveTextAsync("a closed day");
        await desk.Page.Locator("#care-picker").GetByLabel("Day", new LocatorGetByLabelOptions { Exact = true }).FillAsync("2026-10-29");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#open")).ToHaveTextAsync("an open day");

        var tickless = await NewWorld();
        await tickless.Page.GotoAsync(CarePages.NoonStartAtTwelveToTheTick);
        await Expect(tickless.Page.Locator("#noon")).ToHaveTextAsync("noon");
    }
}
