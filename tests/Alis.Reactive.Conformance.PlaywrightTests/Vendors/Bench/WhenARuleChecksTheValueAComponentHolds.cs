using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARuleChecksTheValueAComponentHolds(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.validation_rule)]
    public async Task the_message_shows_beside_the_component_and_nothing_is_sent()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.ValidationRule);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Code", Exact = true }).GetByText("Pick a code.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(bench.Page.Locator("#shown")).ToBeEmptyAsync();

        var picked = await NewWorld();
        await picked.Page.GotoAsync(BenchPages.ValidationRule);
        await picked.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Code", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await picked.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Code", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("Enter");
        var sent = await picked.Page.RunAndWaitForRequestAsync(() => picked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync(), "**/form");
        Assert.That(sent.Method, Is.EqualTo("POST"));
        await Expect(picked.Page.Locator("#shown")).ToHaveTextAsync("Saved a-1");
    }
}
