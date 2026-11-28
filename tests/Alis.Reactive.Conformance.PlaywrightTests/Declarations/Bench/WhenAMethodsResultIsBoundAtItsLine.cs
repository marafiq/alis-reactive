using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Declarations;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMethodsResultIsBoundAtItsLine(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M11_F1.X5_11_0)]
    public async Task a_bound_result_is_called_once_each_call_runs_and_a_lambda_calls_for_each_item_at_its_step()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.DeclarationsBoundTotals);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read the total twice", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Calls", Exact = true })).ToHaveTextAsync("1");
        await Expect(bench.Page.Locator("#total-1")).ToHaveTextAsync("25");
        await Expect(bench.Page.Locator("#total-2")).ToHaveTextAsync("25");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Call the total twice", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Calls", Exact = true })).ToHaveTextAsync("3");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Hold the total", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#held-total")).ToHaveTextAsync("25");
        await Expect(bench.Page.Locator("#new-total")).ToHaveTextAsync("40");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count the active items", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#active")).ToHaveTextAsync("2");
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Checks", Exact = true })).ToHaveTextAsync(new[] { "1", "1", "1" });

        await bench.Page.Locator("#conformance").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Deactivate last", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count the active items", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#active")).ToHaveTextAsync("1");
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Checks", Exact = true })).ToHaveTextAsync(new[] { "2", "2", "2" });
    }
}
