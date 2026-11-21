using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Declarations;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenANullablePropertyIsSetFromASum(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M11_F1.X7_9_1)]
    public async Task the_sum_of_each_records_count_reaches_the_nullable_number()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.DeclarationsRecordTotal);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Total", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Nullable count", Exact = true })).ToHaveTextAsync("8");

        var empty = await NewWorld();
        await empty.Seed(Seed.TheComponentHoldsNoRecords());
        await empty.Page.GotoAsync(BenchPages.DeclarationsRecordTotal);
        await empty.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Total", Exact = true }).ClickAsync();
        await Expect(empty.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Nullable count", Exact = true })).ToHaveTextAsync("0");
    }
}
