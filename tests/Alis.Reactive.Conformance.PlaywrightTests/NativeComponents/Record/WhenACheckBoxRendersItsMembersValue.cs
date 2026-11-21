using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACheckBoxRendersItsMembersValue(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M24_F3.M24_X7_0)]
    public async Task the_box_shows_ticked_for_a_member_that_holds_true()
    {
        var free = await NewWorld();
        await free.Page.GotoAsync(CarePages.RoomFreeRecordWithTheRoomFree);
        await Expect(free.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Room free", Exact = true })).ToBeCheckedAsync();

        var taken = await NewWorld();
        await taken.Page.GotoAsync(CarePages.RoomFreeRecordWithTheRoomTaken);
        await Expect(taken.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Room free", Exact = true })).Not.ToBeCheckedAsync();
    }
}
