using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMultiSelectSelectsEveryItem(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.OwnCells.method_arg_scalar)]
    public async Task every_allergy_is_chosen_and_none_after_select_none()
    {
        var every = await NewWorld();
        await every.Page.GotoAsync(CarePages.AllergySelectionEmpty);
        await every.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Select every allergy", Exact = true }).ClickAsync();

        await Expect(every.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByText("Penicillin, Latex, Peanut", new LocatorGetByTextOptions { Exact = true }).Filter(new LocatorFilterOptions { Visible = true })).ToBeVisibleAsync();

        var none = await NewWorld();
        await none.Page.GotoAsync(CarePages.AllergySelectionOfLatex);
        await Expect(none.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByText("Latex", new LocatorGetByTextOptions { Exact = true }).Filter(new LocatorFilterOptions { Visible = true })).ToBeVisibleAsync();
        await none.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Select none", Exact = true }).ClickAsync();

        await none.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await Expect(none.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Penicillin", "Latex", "Peanut" });
    }
}
