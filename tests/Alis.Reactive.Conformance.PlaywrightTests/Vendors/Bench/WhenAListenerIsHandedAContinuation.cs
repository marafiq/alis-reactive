using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAListenerIsHandedAContinuation(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F1.X17_12_0)]
    public async Task the_handler_reads_the_query_and_the_box_lists_the_names_the_page_hands_its_continuation()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.VendorsNameSearch);
        await Expect(bench.Page.Locator("#searched-for")).ToHaveTextAsync("");
        await Expect(bench.Page.Locator("#callbacks").GetByRole(AriaRole.List, new LocatorGetByRoleOptions { Name = "Names", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);
        await bench.Page.Locator("#callbacks").GetByRole(AriaRole.Searchbox, new LocatorGetByRoleOptions { Name = "Search", Exact = true }).FillAsync("Ad");
        await Expect(bench.Page.Locator("#searched-for")).ToHaveTextAsync("Ad");
        await Expect(bench.Page.Locator("#callbacks").GetByRole(AriaRole.List, new LocatorGetByRoleOptions { Name = "Names", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "Ada Byrne", "Adam Cole" });
    }
}
