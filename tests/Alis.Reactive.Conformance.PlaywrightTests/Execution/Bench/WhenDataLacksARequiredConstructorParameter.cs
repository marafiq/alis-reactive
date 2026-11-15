using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.JsonContract))]
[Category(Hosts.JsonContractName)]
public sealed class WhenDataLacksARequiredConstructorParameter(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X4_10_0)]
    public async Task a_read_through_it_stops_and_a_read_through_a_whole_binding_reads_on()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.ExecutionRequiredBindings);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count the zero counts", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#binding-status")).ToHaveTextAsync("Counting");
        await Expect(bench.Page.Locator("#zero-count")).ToHaveTextAsync("unknown");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show Gamma's count", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#gamma-count")).ToHaveTextAsync("0");

        var lenient = await NewWorldUnder(Hosts.Net10Name);
        await lenient.Page.GotoAsync(BenchPages.ExecutionRequiredBindings);
        await lenient.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count the zero counts", Exact = true }).ClickAsync();
        await Expect(lenient.Page.Locator("#zero-count")).ToHaveTextAsync("2");
    }
}
