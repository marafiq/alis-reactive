using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAValueIsChosenBetweenALiteralAndAMethodsResult(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F1.X7_6_3)]
    public async Task the_first_value_is_chosen_when_the_test_holds_and_the_method_runs_only_when_it_is_chosen()
    {
        var both = await NewWorld();
        await both.Page.GotoAsync(CarePages.FluidOrders);
        await both.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Order for Ada Hart", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Set target", Exact = true }).ClickAsync();
        await Expect(both.Page.Locator("#fluid-target")).ToHaveTextAsync("1000");
        await both.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Order for Ben Okoro", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Set target", Exact = true }).ClickAsync();
        await Expect(both.Page.Locator("#fluid-target")).ToHaveTextAsync("850");
        await Expect(both.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Calls", Exact = true })).ToHaveTextAsync("1");

        var restricted = await NewWorld();
        await restricted.Page.GotoAsync(CarePages.FluidOrders);
        await restricted.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Order for Ada Hart", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Set target", Exact = true }).ClickAsync();
        await Expect(restricted.Page.Locator("#fluid-target")).ToHaveTextAsync("1000");
        await Expect(restricted.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Calls", Exact = true })).ToHaveTextAsync("0");
    }
}
