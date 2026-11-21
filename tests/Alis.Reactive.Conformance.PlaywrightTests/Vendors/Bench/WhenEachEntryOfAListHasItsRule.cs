using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenEachEntryOfAListHasItsRule(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.validation_listItem)]
    public async Task the_message_shows_beside_the_second_component_only_and_nothing_is_sent()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.ValidationListItem);
        await bench.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "First entry", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await bench.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "First entry", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("Enter");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Second entry", Exact = true }).GetByText("Pick a code.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "First entry", Exact = true }).GetByText("Pick a code.", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await Expect(bench.Page.Locator("#shown")).ToBeEmptyAsync();

        var both = await NewWorld();
        await both.Page.GotoAsync(BenchPages.ValidationListItem);
        await both.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "First entry", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await both.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "First entry", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("Enter");
        await both.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Second entry", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await both.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Second entry", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await both.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Second entry", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("Enter");
        await both.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(both.Page.Locator("#shown")).ToHaveTextAsync("Saved a-1 and a-2");
    }
}
