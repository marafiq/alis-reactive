using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenRequestsRunInParallel(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.http_parallel)]
    public async Task each_branch_sets_the_component_when_its_own_answer_arrives()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.HttpParallel);
        var armedAtCode = await bench.Arm(LanesGate.Respond, BenchEndpoints.Code);
        var armedAtItems = await bench.Arm(LanesGate.Respond, BenchEndpoints.Items);

        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        var heldAtCode = await armedAtCode.Held();
        var heldAtItems = await armedAtItems.Held();
        await heldAtCode.Release();
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Value", Exact = true })).ToHaveTextAsync("a-2");
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Heading)).ToHaveTextAsync("Alpha");
        await heldAtItems.Release();

        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Heading)).ToHaveTextAsync("3 items");

        var refused = await NewWorld();
        await refused.Seed(Seed.TheCodeEndpointRefuses());
        await refused.Page.GotoAsync(BenchPages.HttpParallel);
        await refused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(refused.Page.Locator("#shown")).ToHaveTextAsync("Answered");
        await Expect(refused.Page.Locator("#conformance").GetByRole(AriaRole.Heading)).ToHaveTextAsync("3 items");
        await Expect(refused.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Value", Exact = true })).ToHaveTextAsync("a-1");
    }
}
