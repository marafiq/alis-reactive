using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Declarations;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAComponentLacksADeclaredProperty(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M11_F1.X4_2_0)]
    public async Task the_read_stops_its_own_pipeline_and_the_steps_after_it_never_run()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.DeclarationsOlderReadOnlyText);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read older's read-only text", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show older's text", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#text-read")).ToHaveTextAsync("Alpha");
        await Expect(bench.Page.Locator("#tried")).ToHaveTextAsync("reading");
        await Expect(bench.Page.Locator("#read")).ToHaveTextAsync("");
        await Expect(bench.Page.Locator("#done")).ToHaveTextAsync("");

        var current = await NewWorld();
        await current.Page.GotoAsync(BenchPages.DeclarationsOlderReadOnlyText);
        await current.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read the read-only text", Exact = true }).ClickAsync();
        await Expect(current.Page.Locator("#b-read")).ToHaveTextAsync("RO-2026-11-10");
        await Expect(current.Page.Locator("#b-done")).ToHaveTextAsync("read");
    }
}
