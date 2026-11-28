using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMultiSelectsPlaceholderIsWritten(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.OwnCells.prop_scalar_write)]
    public async Task the_empty_box_shows_the_placeholder_written_to_it()
    {
        var hinted = await NewWorld();
        await hinted.Page.GotoAsync(CarePages.HintChange);
        await hinted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Penicillin only", Exact = true }).ClickAsync();

        await Expect(hinted.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("placeholder", "Helen: penicillin only");

        var cleared = await NewWorld();
        await cleared.Page.GotoAsync(CarePages.HintChange);
        await cleared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Clear hint", Exact = true }).ClickAsync();

        await Expect(cleared.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveAttributeAsync("placeholder", "");
    }
}
