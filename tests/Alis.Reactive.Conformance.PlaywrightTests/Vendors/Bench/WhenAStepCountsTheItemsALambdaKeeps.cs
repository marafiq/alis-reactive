using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepCountsTheItemsALambdaKeeps(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.prop_list_lambda)]
    public async Task the_items_whose_member_holds_are_counted()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.PropListLambda);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count" }).ClickAsync();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("3");

        var quiet = await NewWorld();
        await quiet.Seed(Seed.TheComponentsActiveItems(1));
        await quiet.Page.GotoAsync(BenchPages.PropListLambda);
        await quiet.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count" }).ClickAsync();
        await Expect(quiet.Page.Locator("#shown")).ToHaveTextAsync("1");
    }
}
