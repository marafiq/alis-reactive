using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMultiSelectsItemsAreHandedOverWithTheView(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F1.M21_X2_0)]
    public async Task the_list_shows_each_items_text_by_the_name_its_member_carries()
    {
        var intake = await NewWorld();
        await intake.Page.GotoAsync(CarePages.AllergyIntake);
        await intake.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();

        await Expect(intake.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Penicillin", "Latex" });

        var chosen = await NewWorld();
        await chosen.Page.GotoAsync(CarePages.AllergyIntake);
        await chosen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await chosen.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Latex", Exact = true }).ClickAsync();
        await chosen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();

        await Expect(chosen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true })
            .GetByText("Latex", new LocatorGetByTextOptions { Exact = true }).Filter(new LocatorFilterOptions { Visible = true })).ToBeVisibleAsync();
        await Expect(chosen.Page.Locator("#saved")).ToHaveTextAsync("Allergies: 9");
    }
}
