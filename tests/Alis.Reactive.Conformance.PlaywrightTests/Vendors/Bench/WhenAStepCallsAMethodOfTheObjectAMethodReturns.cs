using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepCallsAMethodOfTheObjectAMethodReturns(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.method_object_methodCall)]
    public async Task the_item_the_method_returned_acts_and_no_other()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.MethodObjectMethodCall);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Act", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Item One", Exact = true })).ToHaveTextAsync("One, acted");
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Item Two", Exact = true })).ToHaveTextAsync("Two, not acted");

        var unacted = await NewWorld();
        await unacted.Page.GotoAsync(BenchPages.MethodObjectMethodCall);
        await Expect(unacted.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Item One", Exact = true })).ToHaveTextAsync("One, not acted");
    }
}
