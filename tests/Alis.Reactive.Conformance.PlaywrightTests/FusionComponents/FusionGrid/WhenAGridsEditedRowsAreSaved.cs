using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAGridsEditedRowsAreSaved(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F23.M21_X208_0)]
    public async Task the_changed_rows_are_posted_as_the_grid_holds_them_each_date_as_its_wire_text()
    {
        var edited = await NewWorld();
        await edited.Page.GotoAsync(CarePages.ChargeCorrections);
        await edited.Page.GetByRole(AriaRole.Gridcell, new PageGetByRoleOptions { Name = "11/11/2026", Exact = true }).DblClickAsync();
        await Expect(edited.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("11/11/2026");
        await edited.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Combobox).FillAsync("11/20/2026");
        await edited.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Combobox).PressAsync("Enter");
        var saved = await edited.Page.RunAndWaitForRequestAsync(() => edited.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save corrections", Exact = true }).ClickAsync(), "**/charge-corrections/saved");
        Assert.That(saved.Method, Is.EqualTo("POST"));
        Assert.That(saved.PostData, Is.EqualTo("{\"changed\":[{\"residentId\":\"R-102\",\"name\":\"Rose\",\"dueOn\":\"2026-11-20\",\"amount\":40,\"status\":1}]}"));
        await Expect(edited.Page.Locator("#saved")).ToHaveTextAsync("Saved Rose due 2026-11-20T00:00 at 40 Paid");

        var unedited = await NewWorld();
        await unedited.Page.GotoAsync(CarePages.ChargeCorrections);
        var nothing = await unedited.Page.RunAndWaitForRequestAsync(() => unedited.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save corrections", Exact = true }).ClickAsync(), "**/charge-corrections/saved");
        Assert.That(nothing.PostData, Is.EqualTo("{\"changed\":[]}"));
        await Expect(unedited.Page.Locator("#saved")).ToHaveTextAsync("No charges changed");
    }
}
