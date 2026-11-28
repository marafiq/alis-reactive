using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheBrowserSubmitsACheckList(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M24_F4.M24_X5_0)]
    public async Task the_list_binds_one_entry_for_each_checked_box()
    {
        var twoDays = await NewWorld();
        await twoDays.Page.GotoAsync(CarePages.VisitWeek);
        await twoDays.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Mon", Exact = true }).CheckAsync();
        await twoDays.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Wed", Exact = true }).CheckAsync();
        await twoDays.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(twoDays.Page.Locator("#visit-days")).ToHaveTextAsync("2 days: Mon, Wed");

        var noDays = await NewWorld();
        await noDays.Page.GotoAsync(CarePages.VisitWeek);
        await noDays.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(noDays.Page.Locator("#visit-days")).ToHaveTextAsync("0 days");
    }
}
