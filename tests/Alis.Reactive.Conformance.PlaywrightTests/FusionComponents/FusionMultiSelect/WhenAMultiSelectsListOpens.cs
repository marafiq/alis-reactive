using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMultiSelectsListOpens(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.OwnCells.event_poc15_component_held_before)]
    public async Task the_list_written_in_its_open_handler_reads_the_offset_written()
    {
        var moved = await NewWorld();
        await moved.Page.GotoAsync(CarePages.AllergyOffsetWritten);
        await moved.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();

        await Expect(moved.Page.Locator("#offset")).ToHaveTextAsync("8");

        var kept = await NewWorld();
        await kept.Page.GotoAsync(CarePages.AllergyOffsetRead);
        await kept.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();

        await Expect(kept.Page.Locator("#offset")).ToHaveTextAsync("1");
    }
}
