using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMultiColumnComboBoxsRowsAreLoadedFromAnEndpoint(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F31.M21_X512_1)]
    public async Task the_box_shows_the_chosen_rows_text_by_the_name_the_endpoint_writes()
    {
        var loaded = await NewWorld();
        await loaded.Page.GotoAsync(CarePages.OpenBeds);
        await loaded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load beds", Exact = true }).ClickAsync();
        await Expect(loaded.Page.Locator("#bed-list")).ToHaveTextAsync("Loaded");
        await loaded.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await loaded.Page.GetByRole(AriaRole.Gridcell, new PageGetByRoleOptions { Name = "12A", Exact = true }).ClickAsync();

        await Expect(loaded.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("12A Maple");

        var unloaded = await NewWorld();
        await unloaded.Page.GotoAsync(CarePages.OpenBeds);
        await unloaded.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");

        await Expect(unloaded.Page.GetByRole(AriaRole.Dialog).GetByText("No records found", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(unloaded.Page.GetByRole(AriaRole.Row)).ToHaveCountAsync(0);
    }
}
