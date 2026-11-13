using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAComboBoxsItemsAreHandedOverWithTheView(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F13.M21_X505_0)]
    public async Task the_list_shows_each_languages_text_by_the_name_its_member_carries()
    {
        var intake = await NewWorld();
        await intake.Page.GotoAsync(CarePages.LanguageIntake);
        await intake.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Preferred language", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");

        await Expect(intake.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Tagalog", "Punjabi" });

        var chosen = await NewWorld();
        await chosen.Page.GotoAsync(CarePages.LanguageIntake);
        await chosen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Preferred language", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("ArrowDown");
        await chosen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Preferred language", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("ArrowDown");

        await Expect(chosen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Preferred language", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("Punjabi");

        await chosen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();

        await Expect(chosen.Page.Locator("#saved")).ToHaveTextAsync("Language: pa");
    }
}
