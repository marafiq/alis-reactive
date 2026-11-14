using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASortedListIsSetIntoAnotherComponent(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.M4_X3_0)]
    public async Task the_other_component_lists_the_new_order_and_the_census_keeps_its_own()
    {
        var byName = await NewWorld();
        await byName.Page.GotoAsync(CarePages.CensusSortedByName);
        await byName.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sort", Exact = true }).ClickAsync();
        await Expect(byName.Page.Locator("#census-sorted").GetByRole(AriaRole.List, new LocatorGetByRoleOptions { Name = "Records", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(["Adams", "Lee", "Martin"]);
        await Expect(byName.Page.Locator("#first-name")).ToHaveTextAsync("Martin");

        var descending = await NewWorld();
        await descending.Page.GotoAsync(CarePages.CensusSortedByNameDescending);
        await descending.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sort", Exact = true }).ClickAsync();
        await Expect(descending.Page.Locator("#census-sorted").GetByRole(AriaRole.List, new LocatorGetByRoleOptions { Name = "Records", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(["Martin", "Lee", "Adams"]);
        await Expect(descending.Page.Locator("#first-name")).ToHaveTextAsync("Martin");
    }
}
