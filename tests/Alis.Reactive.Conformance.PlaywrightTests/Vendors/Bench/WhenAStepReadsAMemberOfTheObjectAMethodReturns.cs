using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepReadsAMemberOfTheObjectAMethodReturns(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.method_object_memberRead)]
    public async Task the_name_of_the_item_the_method_returned_is_shown()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.MethodObjectMemberRead);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("One");

        var withTwoCurrent = await NewWorld();
        await withTwoCurrent.Seed(Seed.TheComponentsCurrentItem("Two"));
        await withTwoCurrent.Page.GotoAsync(BenchPages.MethodObjectMemberRead);
        await withTwoCurrent.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show", Exact = true }).ClickAsync();
        await Expect(withTwoCurrent.Page.Locator("#shown")).ToHaveTextAsync("Two");
    }
}
