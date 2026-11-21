using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMultiSelectsChangeHandsItsValues(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.OwnCells.event_list)]
    public async Task the_values_are_read_as_the_bound_members_codes()
    {
        var peanut = await NewWorld();
        await peanut.Page.GotoAsync(CarePages.PeanutCheck);
        await peanut.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await peanut.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Peanut", Exact = true }).ClickAsync();
        await peanut.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Tab");

        await Expect(peanut.Page.Locator("#peanut")).ToHaveTextAsync("Peanut on file");

        var latex = await NewWorld();
        await latex.Page.GotoAsync(CarePages.PeanutCheck);
        await latex.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await latex.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Latex", Exact = true }).ClickAsync();
        await latex.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Tab");

        await Expect(latex.Page.Locator("#peanut")).ToHaveTextAsync("No peanut");
    }
}
