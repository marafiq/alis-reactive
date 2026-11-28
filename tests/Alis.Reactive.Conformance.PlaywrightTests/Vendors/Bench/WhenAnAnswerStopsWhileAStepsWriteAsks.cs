using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnAnswerStopsWhileAStepsWriteAsks(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F1.X17_21_0)]
    public async Task the_pipeline_stops_before_its_next_step_and_the_component_keeps_the_answer_it_drew()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.VendorsAnswerAskedByAWrite);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Set the text North", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#north-done")).ToHaveTextAsync("set");
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Answer", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "Ward 3" });
        await bench.Page.Locator("#conformance-source").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Remove the read-only text", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Set the text South", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#south-tried")).ToHaveTextAsync("trying");
        await Expect(bench.Page.Locator("#south-done")).ToHaveTextAsync("");
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Answer", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "Ward 3" });

        var kept = await NewWorld();
        await kept.Page.GotoAsync(BenchPages.VendorsAnswerAskedByAWrite);
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Set the text North", Exact = true }).ClickAsync();
        await Expect(kept.Page.Locator("#north-done")).ToHaveTextAsync("set");
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Set the text South", Exact = true }).ClickAsync();
        await Expect(kept.Page.Locator("#south-done")).ToHaveTextAsync("set");
        await Expect(kept.Page.Locator("#conformance").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Answer", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "Ward 3" });
    }
}
