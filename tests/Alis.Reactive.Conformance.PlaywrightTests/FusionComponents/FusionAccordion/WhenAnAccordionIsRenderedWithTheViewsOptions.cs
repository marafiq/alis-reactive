using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnAccordionIsRenderedWithTheViewsOptions(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F4.M21_X1001_0)]
    public async Task the_panels_open_as_the_view_sets_them_and_a_step_reaches_the_accordion_at_its_id()
    {
        var allergies = await NewWorld();
        await allergies.Page.GotoAsync(CarePages.CareSectionsAtAllergies);

        await Expect(allergies.Page.GetByText("Penicillin and latex", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(allergies.Page.GetByText("Metformin with breakfast", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await Expect(allergies.Page.GetByText("Soft food, no added salt", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();

        await allergies.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open diet", Exact = true }).ClickAsync();

        await Expect(allergies.Page.GetByText("Soft food, no added salt", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        var medications = await NewWorld();
        await medications.Page.GotoAsync(CarePages.CareSectionsAtMedications);

        await Expect(medications.Page.GetByText("Metformin with breakfast", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(medications.Page.GetByText("Penicillin and latex", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();
    }
}
