using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASuccessBodyIsLoadedIntoAnElement(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.http_into)]
    public async Task the_loaded_partials_component_shows_and_its_handler_runs()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.HttpInto);
        await Expect(bench.Page.Locator("#card")).ToBeEmptyAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open card", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#card").GetByRole(AriaRole.Heading)).ToHaveTextAsync("Card component");
        await bench.Page.Locator("#card").GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await bench.Page.Locator("#card").GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("Enter");
        await Expect(bench.Page.Locator("#picked")).ToHaveTextAsync("a-2");
    }
}
