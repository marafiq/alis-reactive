using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenATabIsRenderedWithTheViewsOptions(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F47.M21_X1028_0)]
    public async Task a_step_reads_the_selected_item_the_view_set_at_the_tabs_id()
    {
        var medications = await NewWorld();
        await medications.Page.GotoAsync(CarePages.CareTabsAtMedications);
        await Expect(medications.Page.GetByRole(AriaRole.Tab, new PageGetByRoleOptions { Name = "Medications", Exact = true, Selected = true })).ToBeVisibleAsync();
        await medications.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Which tab", Exact = true }).ClickAsync();

        await Expect(medications.Page.Locator("#tab-shown")).ToHaveTextAsync("2");

        var visits = await NewWorld();
        await visits.Page.GotoAsync(CarePages.CareTabsAtVisits);
        await Expect(visits.Page.GetByRole(AriaRole.Tab, new PageGetByRoleOptions { Name = "Visits", Exact = true, Selected = true })).ToBeVisibleAsync();
        await visits.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Which tab", Exact = true }).ClickAsync();

        await Expect(visits.Page.Locator("#tab-shown")).ToHaveTextAsync("1");
    }
}
