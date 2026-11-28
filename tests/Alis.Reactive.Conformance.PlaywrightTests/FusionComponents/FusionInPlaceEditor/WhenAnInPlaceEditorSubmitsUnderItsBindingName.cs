using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnInPlaceEditorSubmitsUnderItsBindingName(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F24.M21_X1037_0)]
    public async Task the_submission_is_named_by_the_members_binding_name()
    {
        var room = await NewWorld();
        await room.Page.GotoAsync(CarePages.RoomMove);
        await room.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Room", Exact = true }).GetByText("North 12", new LocatorGetByTextOptions { Exact = true }).ClickAsync();
        await room.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Room", Exact = true }).GetByRole(AriaRole.Textbox).PressAsync("Enter");

        await Expect(room.Page.Locator("#saved-as")).ToHaveTextAsync("Room");

        var bed = await NewWorld();
        await bed.Page.GotoAsync(CarePages.RoomMove);
        await bed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed", Exact = true }).GetByText("B2", new LocatorGetByTextOptions { Exact = true }).ClickAsync();
        await bed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed", Exact = true }).GetByRole(AriaRole.Textbox).PressAsync("Enter");

        await Expect(bed.Page.Locator("#saved-as")).ToHaveTextAsync("Bedside.Bed");
    }
}
