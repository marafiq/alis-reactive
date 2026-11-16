using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10AndNet48))]
public sealed class WhenATextBoxShowsWhatTheServerCouldNotBind(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M24_F9.M24_X10_0)]
    public async Task the_box_shows_the_text_the_room_did_not_bind_from_and_the_members_value_otherwise()
    {
        var unbound = await NewWorld();
        await unbound.Page.GotoAsync(CarePages.RoomIntakeOfAnUnboundRoom);
        await Expect(unbound.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Room", Exact = true })).ToHaveValueAsync("2a");

        var held = await NewWorld();
        await held.Page.GotoAsync(CarePages.RoomIntakeOfRoom12);
        await Expect(held.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Room", Exact = true })).ToHaveValueAsync("12");
    }
}
