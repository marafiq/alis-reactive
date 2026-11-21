using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMultiSelectIsRenderedUnderAPartialsPrefix(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.OwnCells.validation_prefix)]
    public async Task the_endpoint_binds_the_guardians_allergies_and_an_empty_box_is_refused()
    {
        var latex = await NewWorld();
        await latex.Page.GotoAsync(CarePages.GuardianAllergiesOfLatex);
        await latex.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();

        await Expect(latex.Page.Locator("#saved")).ToHaveTextAsync("Guardian allergies 9");

        var unrecorded = await NewWorld();
        await unrecorded.Page.GotoAsync(CarePages.GuardianAllergiesUnrecorded);
        await unrecorded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();

        await Expect(unrecorded.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Guardian's allergies", Exact = true }).GetByText("Record the guardian's allergies", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(unrecorded.Page.Locator("#saved")).ToHaveTextAsync("Not saved");
    }
}
