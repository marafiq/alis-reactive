using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Declarations;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenANullablePropertyIsSetFromACount(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M11_F1.X7_9_0)]
    public async Task the_count_of_the_records_reaches_the_nullable_number()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.DeclarationsRecordCount);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Nullable count", Exact = true })).ToHaveTextAsync("12");

        var empty = await NewWorld();
        await empty.Seed(Seed.TheComponentHoldsNoRecords());
        await empty.Page.GotoAsync(BenchPages.DeclarationsRecordCount);
        await empty.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count", Exact = true }).ClickAsync();
        await Expect(empty.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Nullable count", Exact = true })).ToHaveTextAsync("0");
    }
}
