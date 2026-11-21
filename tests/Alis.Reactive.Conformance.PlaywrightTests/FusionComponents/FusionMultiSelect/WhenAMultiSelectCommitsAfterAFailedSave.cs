using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMultiSelectCommitsAfterAFailedSave(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.OwnCells.validation_live)]
    public async Task choosing_an_allergy_clears_the_message_and_removing_it_shows_it_again()
    {
        var helen = await NewWorld();
        await helen.Page.GotoAsync(CarePages.IntakeCheck);
        await helen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByText("Record at least one allergy", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await helen.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Latex", Exact = true }).ClickAsync();
        await helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Tab");

        await Expect(helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByText("Record at least one allergy", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await Expect(helen.Page.Locator("#saved")).ToHaveTextAsync("Not saved");

        await helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Backspace");
        await helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Tab");

        await Expect(helen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByText("Record at least one allergy", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }
}
