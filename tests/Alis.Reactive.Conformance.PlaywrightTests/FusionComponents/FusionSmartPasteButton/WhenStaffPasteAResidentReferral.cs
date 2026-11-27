using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenStaffPasteAResidentReferral(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F42.M21_X803_0)]
    public async Task the_protected_intake_fills_the_given_name_after_staff_paste()
    {
        var world = await NewWorldWithClipboard();
        await world.Page.GotoAsync(CarePages.SmartPasteReferral);
        var note = world.Page.GetByLabel("Referral note", new PageGetByLabelOptions { Exact = true });
        var givenName = world.Page.GetByLabel("Given name", new PageGetByLabelOptions { Exact = true });
        await note.PressAsync("ControlOrMeta+a");
        await note.PressAsync("ControlOrMeta+c");
        await Expect(givenName).ToHaveValueAsync("");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Smart Paste", Exact = true }).ClickAsync();
        await Expect(givenName).ToHaveValueAsync("Rose");
    }

    [Test]
    [FailsFor(Promises.M21_F42.M21_X804_0)]
    public async Task an_intake_loaded_into_the_page_fills_the_given_name_after_staff_paste()
    {
        var world = await NewWorldWithClipboard();
        await world.Page.GotoAsync(CarePages.SmartPasteReferralSlot);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load intake", Exact = true }).ClickAsync();
        var note = world.Page.GetByLabel("Referral note", new PageGetByLabelOptions { Exact = true });
        var givenName = world.Page.GetByLabel("Given name", new PageGetByLabelOptions { Exact = true });
        await note.PressAsync("ControlOrMeta+a");
        await note.PressAsync("ControlOrMeta+c");
        await Expect(givenName).ToHaveValueAsync("");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Smart Paste", Exact = true }).ClickAsync();
        await Expect(givenName).ToHaveValueAsync("Rose");
    }
}
