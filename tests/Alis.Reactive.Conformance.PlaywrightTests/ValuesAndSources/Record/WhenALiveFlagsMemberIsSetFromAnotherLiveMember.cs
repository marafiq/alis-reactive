using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALiveFlagsMemberIsSetFromAnotherLiveMember(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F1.X7_20_4)]
    public async Task the_days_arrive_as_the_first_panel_holds_them_and_a_request_sends_each_by_its_name()
    {
        var mondayAndTheWeekend = await NewWorld();
        await mondayAndTheWeekend.Page.GotoAsync(CarePages.NightPanelOfMondayAndTheWeekend);
        await mondayAndTheWeekend.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Copy the day panel's days", Exact = true }).ClickAsync();
        await Expect(mondayAndTheWeekend.Page.Locator("#days-copied")).ToHaveTextAsync("yes");
        var sent = await mondayAndTheWeekend.Page.RunAndWaitForRequestAsync(() => mondayAndTheWeekend.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send night care", Exact = true }).ClickAsync(), "**/record/panel-week/care**");
        Assert.That(sent.Url, Does.EndWith("/care/record/panel-week/care?Care=Mon%2C+Weekend"));
        await Expect(mondayAndTheWeekend.Page.Locator("#care-echo")).ToHaveTextAsync("Mon, Weekend");

        var monday = await NewWorld();
        await monday.Page.GotoAsync(CarePages.NightPanelOfMonday);
        await monday.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Copy the day panel's days", Exact = true }).ClickAsync();
        await Expect(monday.Page.Locator("#days-copied")).ToHaveTextAsync("yes");
        var sentMonday = await monday.Page.RunAndWaitForRequestAsync(() => monday.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send night care", Exact = true }).ClickAsync(), "**/record/panel-week/care**");
        Assert.That(sentMonday.Url, Does.EndWith("/care/record/panel-week/care?Care=Mon"));
        await Expect(monday.Page.Locator("#care-echo")).ToHaveTextAsync("Mon");
    }
}
