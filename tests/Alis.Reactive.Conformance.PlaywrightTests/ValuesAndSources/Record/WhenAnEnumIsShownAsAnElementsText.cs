using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnEnumIsShownAsAnElementsText(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F1.X7_20_5)]
    public async Task the_text_reads_the_name_the_apps_json_writes_where_the_panels_member_takes_the_same_day_as_its_number()
    {
        var saturday = await NewWorld();
        await saturday.Page.GotoAsync(CarePages.PlannedCareOfSaturday);
        await saturday.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(saturday.Page.Locator("#day-planned")).ToHaveTextAsync("sat");

        var sent = await saturday.Page.RunAndWaitForRequestAsync(() => saturday.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send care", Exact = true }).ClickAsync(), "**/record/panel-week/care**");
        Assert.That(sent.Url, Does.EndWith("/care/record/panel-week/care?Care=Sat"));
        await Expect(saturday.Page.Locator("#care-echo")).ToHaveTextAsync("Sat");
    }
}
