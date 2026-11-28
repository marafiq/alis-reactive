using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepPassesAListToAComponentsMethod(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.method_arg_list)]
    public async Task the_component_receives_both_records_as_one_list()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.MethodArgList);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Take both", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Records", Exact = true })).ToHaveTextAsync("Alpha in C7; Beta in C8");

        var untaken = await NewWorld();
        await untaken.Page.GotoAsync(BenchPages.MethodArgList);
        await Expect(untaken.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Records", Exact = true })).ToHaveTextAsync("None");
    }
}
