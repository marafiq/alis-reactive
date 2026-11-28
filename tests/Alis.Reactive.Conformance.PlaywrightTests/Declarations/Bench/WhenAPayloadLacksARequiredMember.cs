using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Declarations;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPayloadLacksARequiredMember(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M11_F2.X4_3_0)]
    public async Task the_handler_stops_at_the_member_and_data_past_a_member_it_holds_reads_its_start()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.DeclarationsRequiredPayloadText);
        await bench.Page.Locator("#conformance").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Raise without text", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Raises", Exact = true })).ToHaveTextAsync("Raised 1");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the text", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#text-read")).ToHaveTextAsync("Alpha");
        await Expect(bench.Page.Locator("#record-count")).ToHaveTextAsync("0");
        await Expect(bench.Page.Locator("#raised-text")).ToHaveTextAsync("");
        await Expect(bench.Page.Locator("#raised-recorded")).ToHaveTextAsync("");

        var named = await NewWorld();
        await named.Page.GotoAsync(BenchPages.DeclarationsRequiredPayloadText);
        await named.Page.Locator("#conformance").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Raise Ada", Exact = true }).ClickAsync();
        await Expect(named.Page.Locator("#record-count")).ToHaveTextAsync("0");
        await Expect(named.Page.Locator("#raised-text")).ToHaveTextAsync("Ada Byron");
        await Expect(named.Page.Locator("#raised-recorded")).ToHaveTextAsync("recorded");
    }
}
