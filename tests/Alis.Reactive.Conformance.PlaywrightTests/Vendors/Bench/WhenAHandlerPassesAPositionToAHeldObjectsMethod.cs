using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHandlerPassesAPositionToAHeldObjectsMethod(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.event_poc15_returnedObject_held_before)]
    public async Task the_name_of_the_item_at_the_position_passed_is_shown()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.Poc15ReturnedObjectHeldBefore);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("First");
        await Expect(bench.Page.Locator("#next")).ToHaveTextAsync("Second");
    }
}
