using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepPassesATextToAComponentsMethod(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.method_arg_scalar)]
    public async Task the_component_receives_the_text_itself()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.MethodArgScalar);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Log", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Log, new PageGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "Checked at 14:00" });

        var unlogged = await NewWorld();
        await unlogged.Page.GotoAsync(BenchPages.MethodArgScalar);
        await Expect(unlogged.Page.GetByRole(AriaRole.Log, new PageGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveCountAsync(0);
    }
}
