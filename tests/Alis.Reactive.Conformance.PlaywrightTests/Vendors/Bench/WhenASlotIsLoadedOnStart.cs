using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASlotIsLoadedOnStart(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.lane_slot_load)]
    public async Task the_loaded_cards_handler_runs_on_its_components_change()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.LaneSlotLoad);
        await Expect(bench.Page.Locator("#card").GetByRole(AriaRole.Heading)).ToHaveTextAsync("Card component");
        await Expect(bench.Page.Locator("#picked")).ToHaveTextAsync("");
        await bench.Page.Locator("#card").GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await bench.Page.Locator("#card").GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("Enter");
        await Expect(bench.Page.Locator("#picked")).ToHaveTextAsync("a-2");
    }
}
