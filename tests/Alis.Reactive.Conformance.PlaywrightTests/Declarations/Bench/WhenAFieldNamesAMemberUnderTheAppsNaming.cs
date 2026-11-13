using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Declarations;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.SnakeCase))]
[Category(Hosts.SnakeCaseName)]
public sealed class WhenAFieldNamesAMemberUnderTheAppsNaming(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M11_F1.M11_X2_0)]
    public async Task the_component_orders_its_records_by_the_member_the_app_names()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.DeclarationsRecordsSortedByAField);
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.List, new LocatorGetByRoleOptions { Name = "Records", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(["Ada", "Ben", "Cy"]);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sort by multi-word", Exact = true }).ClickAsync();

        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.List, new LocatorGetByRoleOptions { Name = "Records", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(["Ben", "Ada", "Cy"]);
    }
}
