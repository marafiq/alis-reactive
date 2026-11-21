using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMultiSelectsChangeSaysWhoMadeIt(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.OwnCells.event_scalar)]
    public async Task a_persons_choice_reads_user_and_a_write_reads_system()
    {
        var chosen = await NewWorld();
        await chosen.Page.GotoAsync(CarePages.AllergySource);
        await chosen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await chosen.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Latex", Exact = true }).ClickAsync();
        await chosen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Tab");

        await Expect(chosen.Page.Locator("#source")).ToHaveTextAsync("user");

        var loaded = await NewWorld();
        await loaded.Page.GotoAsync(CarePages.AllergySource);
        await loaded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load from discharge", Exact = true }).ClickAsync();

        await Expect(loaded.Page.Locator("#source")).ToHaveTextAsync("system");
    }
}
