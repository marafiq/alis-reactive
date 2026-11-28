using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPartialHoldsItsOwnValidation(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X9_0)]
    public async Task its_save_is_checked_by_its_own_rules_and_by_none_of_the_pages()
    {
        var named = await NewWorld();
        await named.Page.GotoAsync(CarePages.ArrivalDesk);
        await named.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add next of kin", Exact = true }).ClickAsync();
        await named.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Kin's name", Exact = true }).FillAsync("Jo Hart");
        await named.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Kin's phone", Exact = true }).FillAsync("555-0101");
        await named.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save next of kin", Exact = true }).ClickAsync();
        await Expect(named.Page.Locator("#arrival-kin-saved")).ToHaveTextAsync("Next of kin: Jo Hart");

        var unnamed = await NewWorld();
        await unnamed.Page.GotoAsync(CarePages.ArrivalDesk);
        await unnamed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add next of kin", Exact = true }).ClickAsync();
        await unnamed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save next of kin", Exact = true }).ClickAsync();
        await Expect(unnamed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Kin's name", Exact = true })
            .GetByText("Name the next of kin.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(unnamed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Kin's phone", Exact = true })
            .GetByText("Give a phone number.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(unnamed.Page.Locator("#arrival-kin-saved")).ToHaveTextAsync("nobody");
    }
}
