using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASlotThatRenderedAnInputUnloads(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X12_4_0)]
    public async Task the_pages_rule_still_checks_the_member_and_admit_sends_nothing()
    {
        var cleared = await NewWorld();
        await cleared.Page.GotoAsync(CarePages.RoomAdmission);
        await cleared.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Resident", Exact = true }).FillAsync("Ann");
        await cleared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Choose a room", Exact = true }).ClickAsync();
        await cleared.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Room", Exact = true }).FillAsync("12");
        await cleared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Clear the room", Exact = true }).ClickAsync();
        await cleared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Admit", Exact = true }).ClickAsync();
        await Expect(cleared.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Choose the resident's room." });
        await Expect(cleared.Page.Locator("#admitted")).ToBeEmptyAsync();

        var chosen = await NewWorld();
        await chosen.Page.GotoAsync(CarePages.RoomAdmission);
        await chosen.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Resident", Exact = true }).FillAsync("Ann");
        await chosen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Choose a room", Exact = true }).ClickAsync();
        await chosen.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Room", Exact = true }).FillAsync("12");
        await chosen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Admit", Exact = true }).ClickAsync();
        await Expect(chosen.Page.Locator("#admitted")).ToHaveTextAsync("Ann admitted to room 12");
    }
}
