using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.JsonContract))]
[Category(Hosts.JsonContractName)]
public sealed class WhenTheAppsOptionsOmitAMemberAtItsDefault(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X4_6_0)]
    public async Task each_omitted_member_reads_its_start()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.ExecutionBindingDefaults);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count the zero counts", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count the started", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show Beta's started", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the binding's started", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#zero-count")).ToHaveTextAsync("2");
        await Expect(bench.Page.Locator("#started-count")).ToHaveTextAsync("2");
        await Expect(bench.Page.Locator("#beta-started")).ToHaveTextAsync("start");
        await Expect(bench.Page.Locator("#binding-started")).ToHaveTextAsync("start");

        var held = await NewWorld();
        await held.Seed(Seed.TheBindingsBeta(1, "other"));
        await held.Seed(Seed.TheBindingsStarted("bound"));
        await held.Page.GotoAsync(BenchPages.ExecutionBindingDefaults);
        await held.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count the zero counts", Exact = true }).ClickAsync();
        await held.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count the started", Exact = true }).ClickAsync();
        await held.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show Beta's started", Exact = true }).ClickAsync();
        await held.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the binding's started", Exact = true }).ClickAsync();
        await Expect(held.Page.Locator("#zero-count")).ToHaveTextAsync("1");
        await Expect(held.Page.Locator("#started-count")).ToHaveTextAsync("1");
        await Expect(held.Page.Locator("#beta-started")).ToHaveTextAsync("other");
        await Expect(held.Page.Locator("#binding-started")).ToHaveTextAsync("bound");
    }
}
