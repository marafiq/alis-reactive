using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPartialRendersAnEntryUnderItsMembersPrefix(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.validation_prefix)]
    public async Task the_form_sends_the_first_code_under_its_prefix_and_the_server_binds_it()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.ValidationPrefix);
        await bench.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "First entry", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await bench.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "First entry", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("Enter");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("Saved a-1");

        var unpicked = await NewWorld();
        await unpicked.Page.GotoAsync(BenchPages.ValidationPrefix);
        await unpicked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(unpicked.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "First entry", Exact = true }).GetByText("Pick a code.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(unpicked.Page.Locator("#shown")).ToBeEmptyAsync();
    }
}
