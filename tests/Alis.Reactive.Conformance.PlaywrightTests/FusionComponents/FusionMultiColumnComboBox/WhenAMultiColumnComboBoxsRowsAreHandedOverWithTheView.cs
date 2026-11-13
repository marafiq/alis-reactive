using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMultiColumnComboBoxsRowsAreHandedOverWithTheView(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F31.M21_X512_0)]
    public async Task the_box_shows_the_chosen_rows_text_by_the_name_its_member_carries()
    {
        var allocation = await NewWorld();
        await allocation.Page.GotoAsync(CarePages.BedAllocation);
        await allocation.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await allocation.Page.GetByRole(AriaRole.Gridcell, new PageGetByRoleOptions { Name = "12A", Exact = true }).ClickAsync();

        await Expect(allocation.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("12A Maple");

        var chosen = await NewWorld();
        await chosen.Page.GotoAsync(CarePages.BedAllocation);
        await chosen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await chosen.Page.GetByRole(AriaRole.Gridcell, new PageGetByRoleOptions { Name = "14B", Exact = true }).ClickAsync();

        await Expect(chosen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("14B Cedar");

        await chosen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();

        await Expect(chosen.Page.Locator("#saved")).ToHaveTextAsync("Bed: 14B");
    }
}
