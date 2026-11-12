using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Declarations;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMemberIsNotWhatItsDeclarationSays(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M11_F1.X5_12_0)]
    public async Task each_call_stops_its_own_pipeline_and_a_null_reaches_a_method_as_null()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.DeclarationsMembersOffTheirDeclaration);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Log older", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show older's text", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#text-read")).ToHaveTextAsync("Alpha");
        await Expect(bench.Page.Locator("#tried")).ToHaveTextAsync("Log older");
        await Expect(bench.Page.Locator("#done")).ToHaveTextAsync("");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Echo older", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show older's text", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#text-read")).ToHaveTextAsync("Alpha");
        await Expect(bench.Page.Locator("#tried")).ToHaveTextAsync("Echo older");
        await Expect(bench.Page.Locator("#done")).ToHaveTextAsync("");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ping promised", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show promised's text", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#text-read")).ToHaveTextAsync("Alpha");
        await Expect(bench.Page.Locator("#tried")).ToHaveTextAsync("Ping promised");
        await Expect(bench.Page.Locator("#done")).ToHaveTextAsync("");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Total promised", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show promised's text", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#text-read")).ToHaveTextAsync("Alpha");
        await Expect(bench.Page.Locator("#tried")).ToHaveTextAsync("Total promised");
        await Expect(bench.Page.Locator("#done")).ToHaveTextAsync("");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Take nothing", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Taken text", Exact = true })).ToHaveTextAsync("(null)");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Take the snapshot's name", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Taken text", Exact = true })).ToHaveTextAsync("(null)");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Take the missing item's name", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Taken text", Exact = true })).ToHaveTextAsync("(null)");

        var pinged = await NewWorld();
        await pinged.Page.GotoAsync(BenchPages.DeclarationsMembersOffTheirDeclaration);
        await pinged.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ping", Exact = true }).ClickAsync();
        await Expect(pinged.Page.Locator("#b-done")).ToHaveTextAsync("pinged");
    }
}
