using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnAutoCompletesItemsAreLoadedFromAnEndpoint(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F5.M21_X503_1)]
    public async Task the_suggestions_show_each_items_text_by_the_name_the_endpoint_writes()
    {
        var loaded = await NewWorld();
        await loaded.Page.GotoAsync(CarePages.WardMedications);
        await loaded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load medications", Exact = true }).ClickAsync();
        await Expect(loaded.Page.Locator("#ward-list")).ToHaveTextAsync("Loaded");
        await loaded.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Current medication", Exact = true }).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("war");

        await Expect(loaded.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Warfarin" });

        var unloaded = await NewWorld();
        await unloaded.Page.GotoAsync(CarePages.WardMedications);
        await unloaded.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Current medication", Exact = true }).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("war");

        await Expect(unloaded.Page.GetByRole(AriaRole.Dialog).GetByText("No records found", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(unloaded.Page.GetByRole(AriaRole.Option)).ToHaveCountAsync(0);
    }
}
