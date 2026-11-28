using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepPassesAnObjectToAComponentsMethod(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.method_arg_object)]
    public async Task the_component_receives_the_records_name_and_code()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.MethodArgObject);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Take", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Records", Exact = true })).ToHaveTextAsync("Alpha in C7");

        var untaken = await NewWorld();
        await untaken.Page.GotoAsync(BenchPages.MethodArgObject);
        await Expect(untaken.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Records", Exact = true })).ToHaveTextAsync("None");
    }
}
