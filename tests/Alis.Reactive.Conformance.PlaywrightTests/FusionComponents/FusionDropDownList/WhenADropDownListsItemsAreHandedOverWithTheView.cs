using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenADropDownListsItemsAreHandedOverWithTheView(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F20.M21_X507_0)]
    public async Task the_list_shows_each_wards_text_by_the_name_its_member_carries()
    {
        var placement = await NewWorld();
        await placement.Page.GotoAsync(CarePages.WardPlacement);
        await placement.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();

        await Expect(placement.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Maple", "Cedar" });

        var chosen = await NewWorld();
        await chosen.Page.GotoAsync(CarePages.WardPlacement);
        await chosen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("c");

        await Expect(chosen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Cedar");

        await chosen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();

        await Expect(chosen.Page.Locator("#saved")).ToHaveTextAsync("Ward: C2");
    }
}
