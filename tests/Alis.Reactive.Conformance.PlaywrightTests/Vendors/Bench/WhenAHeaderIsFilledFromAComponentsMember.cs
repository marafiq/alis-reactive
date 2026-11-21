using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHeaderIsFilledFromAComponentsMember(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.source_Header)]
    public async Task the_endpoint_receives_the_header_the_value_fills()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.SourceHeader);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send" }).ClickAsync();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("X-Code a-1");

        var picked = await NewWorld();
        await picked.Page.GotoAsync(BenchPages.SourceHeader);
        await picked.Page.GetByRole(AriaRole.Listbox, new PageGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await picked.Page.GetByRole(AriaRole.Listbox, new PageGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("Enter");
        await picked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send" }).ClickAsync();
        await Expect(picked.Page.Locator("#shown")).ToHaveTextAsync("X-Code a-2");
    }
}
