using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAComponentsMemberIsPassedToAMethod(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.source_MethodArgument)]
    public async Task the_method_receives_the_value_the_component_held()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.SourceMethodArgument);
        await Expect(bench.Page.GetByRole(AriaRole.Log, new PageGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Log", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Log, new PageGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "a-1" });
    }
}
