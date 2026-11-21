using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenDataLacksAMemberItsHolderInitializes(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X4_7_0)]
    public async Task a_populated_and_a_getter_only_member_read_the_holders_start_and_a_replaced_one_reads_its_types()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.ExecutionSpotStarts);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the spots", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#populated-place")).ToHaveTextAsync("binding start");
        await Expect(bench.Page.Locator("#getter-place")).ToHaveTextAsync("binding start");
        await Expect(bench.Page.Locator("#replaced-place")).ToHaveTextAsync("own start");

        var placed = await NewWorld();
        await placed.Seed(Seed.TheComponentsStartsPlace("Ward 4"));
        await placed.Page.GotoAsync(BenchPages.ExecutionSpotStarts);
        await placed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the spots", Exact = true }).ClickAsync();
        await Expect(placed.Page.Locator("#populated-place")).ToHaveTextAsync("Ward 4");
        await Expect(placed.Page.Locator("#getter-place")).ToHaveTextAsync("Ward 4");
        await Expect(placed.Page.Locator("#replaced-place")).ToHaveTextAsync("Ward 4");
    }
}
