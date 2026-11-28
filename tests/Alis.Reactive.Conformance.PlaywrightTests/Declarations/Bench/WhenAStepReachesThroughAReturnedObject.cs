using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Declarations;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepReachesThroughAReturnedObject(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M11_F1.M11_X3_0)]
    public async Task a_write_to_a_member_it_lacks_and_a_read_through_no_object_each_stop_their_own_pipeline()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.DeclarationsReturnedItems);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Write the value", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read the empty current", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the text", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#text-read")).ToHaveTextAsync("Alpha");
        await Expect(bench.Page.Locator("#write-tried")).ToHaveTextAsync("writing");
        await Expect(bench.Page.Locator("#write-done")).ToHaveTextAsync("");
        await Expect(bench.Page.Locator("#read-tried")).ToHaveTextAsync("reading");
        await Expect(bench.Page.Locator("#current-name")).ToHaveTextAsync("");
        await Expect(bench.Page.Locator("#read-done")).ToHaveTextAsync("");

        var held = await NewWorld();
        await held.Page.GotoAsync(BenchPages.DeclarationsReturnedItems);
        await held.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read the held current", Exact = true }).ClickAsync();
        await Expect(held.Page.Locator("#held-name")).ToHaveTextAsync("One");
    }
}
