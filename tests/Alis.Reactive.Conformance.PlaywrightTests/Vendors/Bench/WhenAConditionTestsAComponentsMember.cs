using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAConditionTestsAComponentsMember(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.source_ConditionLeft)]
    public async Task the_branch_follows_the_value_the_component_holds()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.SourceConditionLeft);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check" }).ClickAsync();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("Picked first");

        var picked = await NewWorld();
        await picked.Page.GotoAsync(BenchPages.SourceConditionLeft);
        await picked.Page.GetByRole(AriaRole.Listbox, new PageGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await picked.Page.GetByRole(AriaRole.Listbox, new PageGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("Enter");
        await picked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check" }).ClickAsync();
        await Expect(picked.Page.Locator("#shown")).ToHaveTextAsync("Elsewhere");
    }
}
