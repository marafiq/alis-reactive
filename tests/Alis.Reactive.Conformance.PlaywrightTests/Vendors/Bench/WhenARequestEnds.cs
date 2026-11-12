using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARequestEnds(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.http_finally)]
    public async Task its_finally_block_writes_the_component_after_either_outcome()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.HttpFinally);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Heading)).ToHaveTextAsync("Finished");

        var failed = await NewWorld();
        await failed.Seed(Seed.TheMoveEndpointAnswers(500));
        await failed.Page.GotoAsync(BenchPages.HttpFinally);
        await failed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(failed.Page.Locator("#conformance").GetByRole(AriaRole.Heading)).ToHaveTextAsync("Finished");
    }
}
