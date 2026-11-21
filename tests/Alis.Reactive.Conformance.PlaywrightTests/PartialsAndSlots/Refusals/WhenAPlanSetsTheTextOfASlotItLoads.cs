using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPlanSetsTheTextOfASlotItLoads(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.X12_11_0)]
    public async Task the_page_is_refused_where_the_view_renders_and_told_to_unload_the_slot_and_write_beside_it()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.PassDeskThatSetsItsSlotsText);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: The plan sets the text of element 'card', a slot this plan loads or unloads, so what the slot loaded would keep "
                + "running under content it no longer shows. Empty the slot with Unload(\"card\"), and write the text into an element beside it.",
            Exact = true,
        })).ToBeVisibleAsync();

        var beside = await NewWorld();
        await beside.Page.GotoAsync(RefusalPages.PassDeskThatWritesBesideItsSlot);
        await beside.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the card" }).ClickAsync();
        await beside.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign out" }).ClickAsync();
        await Expect(beside.Page.Locator("#card-note")).ToHaveTextAsync("Signed out");
        await beside.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Print the pass" }).ClickAsync();
        await Expect(beside.Page.Locator("#card-pass")).ToHaveTextAsync("Pass printed");
    }
}
