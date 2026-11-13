using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAChipListReadsItsChosenChips(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F11.M21_X401_0)]
    public async Task the_indexes_of_the_chosen_chips_are_read_by_the_name_the_chip_list_writes()
    {
        var chosen = await NewWorld();
        await chosen.Page.GotoAsync(CarePages.AllergyTags);
        await chosen.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Penicillin", Exact = true }).ClickAsync();
        await chosen.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Peanuts", Exact = true }).ClickAsync();
        await chosen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show selected", Exact = true }).ClickAsync();

        await Expect(chosen.Page.Locator("#chip-status")).ToHaveTextAsync("Indexes held");

        var none = await NewWorld();
        await none.Page.GotoAsync(CarePages.AllergyTags);
        await none.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show selected", Exact = true }).ClickAsync();

        await Expect(none.Page.Locator("#chip-status")).ToHaveTextAsync("No indexes");
    }
}
