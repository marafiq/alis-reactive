using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Declarations;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.SnakeCase))]
[Category(Hosts.SnakeCaseName)]
public sealed class WhenTheAppsJsonOptionsNameMembersInSnakeCase(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M11_F1.M11_X1_0)]
    public async Task a_declared_method_keeps_its_name_and_a_data_read_follows_the_apps_naming()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.DeclarationsNamesUnderTheAppsNaming);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Take 25", Exact = true }).ClickAsync();
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read the record", Exact = true }).ClickAsync();

        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Took", Exact = true })).ToHaveTextAsync("Took 25");
        await Expect(bench.Page.Locator("#take-done")).ToHaveTextAsync("taken");
        await Expect(bench.Page.Locator("#multi-word")).ToHaveTextAsync("40");

        var camel = await NewWorld();
        await camel.Page.GotoAsync(BenchPages.DeclarationsNamesUnderTheAppsNaming);
        await camel.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read the camel record", Exact = true }).ClickAsync();

        await Expect(camel.Page.Locator("#camel-word")).ToHaveTextAsync("0");
    }
}
