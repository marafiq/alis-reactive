using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHandlerWritesItsPayloadBeforeItsFirstAwait(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X11_8_0)]
    public async Task the_component_reads_the_write_when_its_listeners_return_and_the_success_route_calls_the_payloads_method()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.ExecutionNameFilter);
        await bench.Page.Locator("#conformance").GetByRole(AriaRole.Searchbox).FillAsync("al");
        await bench.Page.Locator("#conformance").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Note", Exact = true })).ToHaveTextAsync("Filtered");
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.List, new LocatorGetByRoleOptions { Name = "Names", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "Alpha", "Alpine" });

        var other = await NewWorld();
        await other.Page.GotoAsync(BenchPages.ExecutionNameFilter);
        await other.Page.Locator("#conformance").GetByRole(AriaRole.Searchbox).FillAsync("be");
        await other.Page.Locator("#conformance").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(other.Page.Locator("#conformance").GetByRole(AriaRole.List, new LocatorGetByRoleOptions { Name = "Names", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "Beta" });
    }
}
