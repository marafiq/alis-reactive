using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAComponentCommitsAValueAfterAFailedSave(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.validation_live)]
    public async Task the_message_clears_without_another_save()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.ValidationLive);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Code", Exact = true }).GetByText("Pick a code.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await bench.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Code", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await bench.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Code", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("Enter");
        await Expect(bench.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Code", Exact = true }).GetByText("Pick a code.", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await Expect(bench.Page.Locator("#shown")).ToBeEmptyAsync();

        var unpicked = await NewWorld();
        await unpicked.Page.GotoAsync(BenchPages.ValidationLive);
        await unpicked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await unpicked.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Code", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("Enter");
        await Expect(unpicked.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Code", Exact = true }).GetByText("Pick a code.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }
}
