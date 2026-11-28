using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenATreesFieldsAreNamedByItsNodesMembers(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F21.M21_X1050_0)]
    public async Task the_box_shows_the_rooms_text_by_the_name_its_member_carries()
    {
        var north = await NewWorld();
        await north.Page.GotoAsync(CarePages.RoomPicker);

        await Expect(north.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Room", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("North 2");

        var south = await NewWorld();
        await south.Page.GotoAsync(CarePages.RoomPickerToTheSouth);

        await Expect(south.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Room", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("South 1");
    }
}
