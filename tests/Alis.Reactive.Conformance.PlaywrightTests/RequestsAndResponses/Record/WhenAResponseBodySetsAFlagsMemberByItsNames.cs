using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAResponseBodySetsAFlagsMemberByItsNames(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_24_0)]
    public async Task the_url_holds_each_flag_by_its_name_and_the_endpoint_reads_the_same_days()
    {
        var mondayAndTheWeekend = await NewWorld();
        await mondayAndTheWeekend.Page.GotoAsync(CarePages.CareWeekOfMondayAndTheWeekend);
        await mondayAndTheWeekend.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(mondayAndTheWeekend.Page.Locator("#care-plan")).ToHaveTextAsync("Ada");
        var sent = await mondayAndTheWeekend.Page.RunAndWaitForRequestAsync(() => mondayAndTheWeekend.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send care", Exact = true }).ClickAsync(), "**/record/care-week/care**");
        Assert.That(sent.Method, Is.EqualTo("GET"));
        Assert.That(sent.Url, Does.EndWith("/care/record/care-week/care?Care=Mon%2C+Weekend"));
        await Expect(mondayAndTheWeekend.Page.Locator("#care-echo")).ToHaveTextAsync("Mon, Weekend");

        var monday = await NewWorld();
        await monday.Page.GotoAsync(CarePages.CareWeekOfMonday);
        await monday.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(monday.Page.Locator("#care-plan")).ToHaveTextAsync("Ada");
        var sentMonday = await monday.Page.RunAndWaitForRequestAsync(() => monday.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send care", Exact = true }).ClickAsync(), "**/record/care-week/care**");
        Assert.That(sentMonday.Url, Does.EndWith("/care/record/care-week/care?Care=Mon"));
        await Expect(monday.Page.Locator("#care-echo")).ToHaveTextAsync("Mon");
    }
}
