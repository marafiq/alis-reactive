using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Declarations;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPayloadLeavesOutAnOptionalMember(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M11_F2.X5_7_0)]
    public async Task the_optional_member_reads_nothing_and_the_handler_goes_on_while_a_required_one_stops_it()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.DeclarationsOptionalPayloadText);
        await bench.Page.Locator("#conformance-a").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Raise 4", Exact = true }).ClickAsync();
        await bench.Page.Locator("#conformance-b").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Raise the silent", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#a-done")).ToHaveTextAsync("raised");
        await Expect(bench.Page.Locator("#a-note")).ToHaveTextAsync("");
        await Expect(bench.Page.Locator("#a-text")).ToHaveTextAsync("4");
        await Expect(bench.Page.Locator("#b-note")).ToHaveTextAsync("Fell asleep");
        await Expect(bench.Page.Locator("#conformance-b").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Raises", Exact = true })).ToHaveTextAsync("Raised 1");
        await Expect(bench.Page.Locator("#b-text")).ToHaveTextAsync("");
        await Expect(bench.Page.Locator("#b-done")).ToHaveTextAsync("");
        await Expect(bench.Page.Locator("#conformance-a").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Raises", Exact = true })).ToHaveTextAsync("Raised 1");

        var noted = await NewWorld();
        await noted.Page.GotoAsync(BenchPages.DeclarationsOptionalPayloadText);
        await noted.Page.Locator("#conformance-a").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Raise 4 with a note", Exact = true }).ClickAsync();
        await Expect(noted.Page.Locator("#a-note")).ToHaveTextAsync("Water please");
        await Expect(noted.Page.Locator("#a-text")).ToHaveTextAsync("4");
    }
}
