using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALiveFlagsMemberIsSetFromAResponseBody(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F1.X7_20_0)]
    public async Task the_panel_holds_the_days_as_their_number_and_a_request_sends_each_by_its_name()
    {
        var mondayAndTheWeekend = await NewWorld();
        await mondayAndTheWeekend.Page.GotoAsync(CarePages.PanelWeekOfMondayAndTheWeekend);
        await mondayAndTheWeekend.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(mondayAndTheWeekend.Page.Locator("#care-plan")).ToHaveTextAsync("Ada");
        var sent = await mondayAndTheWeekend.Page.RunAndWaitForRequestAsync(() => mondayAndTheWeekend.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send care", Exact = true }).ClickAsync(), "**/record/panel-week/care**");
        Assert.That(sent.Method, Is.EqualTo("GET"));
        Assert.That(sent.Url, Does.EndWith("/care/record/panel-week/care?Care=Mon%2C+Weekend"));
        await Expect(mondayAndTheWeekend.Page.Locator("#care-echo")).ToHaveTextAsync("Mon, Weekend");

        var monday = await NewWorld();
        await monday.Page.GotoAsync(CarePages.PanelWeekOfMonday);
        await monday.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(monday.Page.Locator("#care-plan")).ToHaveTextAsync("Ada");
        var sentMonday = await monday.Page.RunAndWaitForRequestAsync(() => monday.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send care", Exact = true }).ClickAsync(), "**/record/panel-week/care**");
        Assert.That(sentMonday.Url, Does.EndWith("/care/record/panel-week/care?Care=Mon"));
        await Expect(monday.Page.Locator("#care-echo")).ToHaveTextAsync("Mon");
    }
}
