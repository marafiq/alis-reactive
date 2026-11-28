using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenDataLacksAMemberItsTypeRequires(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X4_9_0)]
    public async Task a_read_through_it_stops_and_a_read_through_a_whole_binding_reads_on()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.ExecutionStrictBindings);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "List the strict names", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#binding-status")).ToHaveTextAsync("Listing strict");
        await Expect(bench.Page.Locator("#binding-list")).ToHaveTextAsync("unknown");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "List the sealed names", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#binding-status")).ToHaveTextAsync("Listing sealed");
        await Expect(bench.Page.Locator("#binding-list")).ToHaveTextAsync("unknown");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "List the polymorphic names", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#binding-status")).ToHaveTextAsync("Listing polymorphic");
        await Expect(bench.Page.Locator("#binding-list")).ToHaveTextAsync("unknown");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the shaped number", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#binding-status")).ToHaveTextAsync("Shaping");
        await Expect(bench.Page.Locator("#shaped-number")).ToHaveTextAsync("unknown");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the binding's count", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#binding-status")).ToHaveTextAsync("Counting");
        await Expect(bench.Page.Locator("#binding-count")).ToHaveTextAsync("unknown");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show Alpha's strict name", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#alpha-strict")).ToHaveTextAsync("A-1");

        var whole = await NewWorld();
        await whole.Seed(Seed.TheBindingsWhole());
        await whole.Page.GotoAsync(BenchPages.ExecutionStrictBindings);
        await whole.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "List the strict names", Exact = true }).ClickAsync();
        await Expect(whole.Page.Locator("#binding-list")).ToHaveTextAsync("A-1,B-2");
        await whole.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "List the sealed names", Exact = true }).ClickAsync();
        await Expect(whole.Page.Locator("#binding-list")).ToHaveTextAsync("S-1,S-2");
        await whole.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "List the polymorphic names", Exact = true }).ClickAsync();
        await Expect(whole.Page.Locator("#binding-list")).ToHaveTextAsync("P-1,P-2");
        await whole.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the shaped number", Exact = true }).ClickAsync();
        await Expect(whole.Page.Locator("#shaped-number")).ToHaveTextAsync("4410");
        await whole.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the binding's count", Exact = true }).ClickAsync();
        await Expect(whole.Page.Locator("#binding-count")).ToHaveTextAsync("42");
    }
}
