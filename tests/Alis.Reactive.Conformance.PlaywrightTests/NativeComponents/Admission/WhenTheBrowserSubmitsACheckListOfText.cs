using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheBrowserSubmitsACheckListOfText(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M24_F4.M24_X14_0)]
    public async Task the_list_sends_its_checked_values_as_one_text()
    {
        var twoDays = await NewWorld();
        await twoDays.Page.GotoAsync(CarePages.VisitRoutine);
        await twoDays.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Mon", Exact = true }).CheckAsync();
        await twoDays.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Wed", Exact = true }).CheckAsync();
        await twoDays.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(twoDays.Page.Locator("#visit-days")).ToHaveTextAsync("Visit days: Mon,Wed");

        var noDays = await NewWorld();
        await noDays.Page.GotoAsync(CarePages.VisitRoutine);
        await noDays.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(noDays.Page.Locator("#visit-days")).ToHaveTextAsync("No visit days");
    }
}
