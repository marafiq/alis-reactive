using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenADropDownListsItemsAreLoadedFromAnEndpoint(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F20.M21_X507_1)]
    public async Task the_list_shows_each_wards_text_by_the_name_the_endpoint_writes()
    {
        var loaded = await NewWorld();
        await loaded.Page.GotoAsync(CarePages.HomeWards);
        await loaded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load wards", Exact = true }).ClickAsync();
        await Expect(loaded.Page.Locator("#ward-list")).ToHaveTextAsync("Loaded");
        await loaded.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();

        await Expect(loaded.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Maple", "Cedar" });

        var unloaded = await NewWorld();
        await unloaded.Page.GotoAsync(CarePages.HomeWards);
        await unloaded.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();

        await Expect(unloaded.Page.GetByRole(AriaRole.Dialog).GetByText("No records found", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(unloaded.Page.GetByRole(AriaRole.Option)).ToHaveCountAsync(0);
    }
}
