using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAFormsSummaryListsItsFailures(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.validation_summary)]
    public async Task the_summary_lists_pick_a_code()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.ValidationSummary);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Pick a code." });

        var picked = await NewWorld();
        await picked.Page.GotoAsync(BenchPages.ValidationSummary);
        await picked.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Code", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await picked.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Code", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("Enter");
        await picked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(picked.Page.Locator("#shown")).ToHaveTextAsync("Saved a-1");
        await Expect(picked.Page.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);
    }
}
