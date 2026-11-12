using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnAnswerStopsWhileTheComponentAsks(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F1.X17_22_0)]
    public async Task the_components_call_fails_and_the_component_keeps_the_answer_it_drew()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.VendorsAnswerAskedByTheComponent);
        await bench.Page.Locator("#conformance").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Ask", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Answer", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "Ward 3" });
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Error", Exact = true })).ToHaveTextAsync("");
        await bench.Page.Locator("#conformance-source").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Remove the read-only text", Exact = true }).ClickAsync();
        await bench.Page.Locator("#conformance").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Ask", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Error", Exact = true })).Not.ToBeEmptyAsync();
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Answer", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "Ward 3" });

        var kept = await NewWorld();
        await kept.Page.GotoAsync(BenchPages.VendorsAnswerAskedByTheComponent);
        await kept.Page.Locator("#conformance").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Ask", Exact = true }).ClickAsync();
        await Expect(kept.Page.Locator("#conformance").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Answer", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "Ward 3" });
        await kept.Page.Locator("#conformance").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Ask", Exact = true }).ClickAsync();
        await Expect(kept.Page.Locator("#conformance").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Error", Exact = true })).ToHaveTextAsync("");
        await Expect(kept.Page.Locator("#conformance").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Answer", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "Ward 3" });
    }
}
