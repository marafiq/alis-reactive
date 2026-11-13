using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnAutoCompletesItemsAreHandedOverWithTheView(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F5.M21_X503_0)]
    public async Task the_suggestions_show_each_items_text_by_the_name_its_member_carries()
    {
        var intake = await NewWorld();
        await intake.Page.GotoAsync(CarePages.MedicationIntake);
        await intake.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Current medication", Exact = true }).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("war");

        await Expect(intake.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Warfarin" });

        var chosen = await NewWorld();
        await chosen.Page.GotoAsync(CarePages.MedicationIntake);
        await chosen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Current medication", Exact = true }).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("met");

        await Expect(chosen.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Metformin" });

        await chosen.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Metformin", Exact = true }).ClickAsync();
        await chosen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();

        await Expect(chosen.Page.Locator("#saved")).ToHaveTextAsync("Medication: Metformin");
    }
}
