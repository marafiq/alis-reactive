using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Declarations;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPlainMethodsPromiseRejects(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M11_F1.X11_3_0)]
    public async Task the_step_after_the_call_never_runs_and_another_pipeline_still_does()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.DeclarationsRefusedPing);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ping promised", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show promised's text", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#text-read")).ToHaveTextAsync("Alpha");
        await Expect(bench.Page.Locator("#ping-tried")).ToHaveTextAsync("pinging");
        await Expect(bench.Page.Locator("#ping-done")).ToHaveTextAsync("");

        var settled = await NewWorld();
        await settled.Page.GotoAsync(BenchPages.DeclarationsRefusedPing);
        await settled.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Settle", Exact = true }).ClickAsync();
        await Expect(settled.Page.Locator("#settle-done")).ToHaveTextAsync("settled");
    }
}
