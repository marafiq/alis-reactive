using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACallbacksAnswerReadsThePage(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F1.X17_20_0)]
    public async Task each_call_reads_the_page_as_it_is_and_a_methods_answer_is_the_key_the_component_sorts_by()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.VendorsLiveAnswers);
        await bench.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Unit name", Exact = true }).FillAsync("East");
        await bench.Page.Locator("#conformance-caption").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Ask", Exact = true }).ClickAsync();
        await bench.Page.Locator("#conformance-footer").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Ask", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#conformance-caption").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Answer", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "East", "East", "East" });
        await Expect(bench.Page.Locator("#conformance-footer").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Answer", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "Day" });
        await bench.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Unit name", Exact = true }).FillAsync("West");
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Set the text to Night", Exact = true }).ClickAsync();
        await bench.Page.Locator("#conformance-caption").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Ask", Exact = true }).ClickAsync();
        await bench.Page.Locator("#conformance-footer").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Ask", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#conformance-caption").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Answer", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "West", "West", "West" });
        await Expect(bench.Page.Locator("#conformance-footer").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Answer", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "Night" });
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sort by value", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#conformance-caption").GetByRole(AriaRole.List, new LocatorGetByRoleOptions { Name = "Items", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "Cy 1", "Ada 2", "Bea 4" });

        var untyped = await NewWorld();
        await untyped.Page.GotoAsync(BenchPages.VendorsLiveAnswers);
        await Expect(untyped.Page.Locator("#conformance-caption").GetByRole(AriaRole.List, new LocatorGetByRoleOptions { Name = "Items", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "Ada 2", "Bea 4", "Cy 1" });
        await untyped.Page.Locator("#conformance-caption").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Ask", Exact = true }).ClickAsync();
        await untyped.Page.Locator("#conformance-footer").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Ask", Exact = true }).ClickAsync();
        await Expect(untyped.Page.Locator("#conformance-footer").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Answer", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "Day" });
        await Expect(untyped.Page.Locator("#conformance-caption").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Answer", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "", "", "" });
    }
}
