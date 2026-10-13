namespace Alis.Reactive.PlaywrightTests.Components.Brought.HoldRelease;

// Journey: an admissions coordinator releases a room held for a resident who is moving in. The
// community asks before a hold is released, in its own confirmation: the page renders a native
// <dialog> in place of the framework's Syncfusion confirm, and the sandbox's script gives it to the
// plan as alis.confirm, the object p.Confirm(..) asks.
[TestFixture]
public class WhenReleasingARoomHold : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/HoldRelease";
    private const string Held = "Room 214 is held for Ruth Alvarez until Friday.";

    private ILocator HoldStatus => Page.Locator("#hold-status");
    private ILocator ReleaseHold => Page.Locator("#release-hold");
    private ILocator CommunityConfirm => Page.Locator("#house-confirm");
    private ILocator SyncfusionConfirm => Page.Locator("#alisConfirmDialog");

    private async Task OpenRoomHolds()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(HoldStatus).ToHaveTextAsync(Held);
    }

    [Test]
    public async Task releasing_the_hold_asks_in_the_communitys_own_dialog_and_opens_the_room()
    {
        await OpenRoomHolds();

        await ReleaseHold.ClickAsync();

        await Expect(CommunityConfirm).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(CommunityConfirm).ToContainTextAsync("Release Ruth Alvarez's hold on room 214?");
        await Expect(SyncfusionConfirm).ToHaveCountAsync(0);
        await Expect(HoldStatus).ToHaveTextAsync(Held);

        await CommunityConfirm.GetByRole(AriaRole.Button, new() { Name = "Confirm" }).ClickAsync();

        await Expect(CommunityConfirm).ToBeHiddenAsync();
        await Expect(HoldStatus).ToHaveTextAsync("Room 214 is open for the waiting list.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task cancelling_in_the_communitys_dialog_keeps_the_room_held()
    {
        await OpenRoomHolds();

        await ReleaseHold.ClickAsync();
        await Expect(CommunityConfirm).ToBeVisibleAsync(new() { Timeout = 10000 });
        await CommunityConfirm.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();

        await Expect(CommunityConfirm).ToBeHiddenAsync();
        await Expect(HoldStatus).ToHaveTextAsync(Held);
        AssertNoConsoleErrors();
    }
}
