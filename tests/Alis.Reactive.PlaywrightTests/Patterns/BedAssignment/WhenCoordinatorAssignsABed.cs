using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.BedAssignment;

namespace Alis.Reactive.PlaywrightTests.Patterns.BedAssignment;

/// <summary>
/// As an admissions coordinator, I want the room list to offer only the rooms of the wing I choose that have
/// an open bed, and the bed list only that room's open beds, so that I can only assign a new resident a bed
/// that is free in the room and wing I chose.
/// </summary>
/// <remarks>
/// Maple has rooms 112 (beds A and B open) and 118 (bed B); Oak has 204 (bed A), 207 (A and B) and 211 (B);
/// Cedar has 305 (A and B). Bed A of a room is by the window, bed B by the door. The wing list comes with
/// the page; the server answers the rooms of the wing chosen, and the open beds of the room chosen in that
/// wing, so a beds lookup sent without the wing finds none. Choosing another wing replaces the rooms: the
/// room list clears the room chosen before, which the new list does not hold, and raises its change, and
/// the page empties the bed list for it. Not exercised: overlapping lookups from a coordinator who changes
/// the wing before the rooms arrive (the latest answer is not guaranteed to win, and no page here shows
/// which did).
/// </remarks>
[TestFixture]
public class WhenCoordinatorAssignsABed : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Patterns/BedAssignment";

    private PagePlan<BedAssignmentModel> _plan = null!;

    private FusionDropDownListLocator Wing => new(Page, _plan.FindComponent(m => m.Wing)!.ElementId);
    private FusionDropDownListLocator Room => new(Page, _plan.FindComponent(m => m.Room)!.ElementId);
    private FusionDropDownListLocator Bed => new(Page, _plan.FindComponent(m => m.Bed)!.ElementId);
    private ILocator RoomSummary => Page.Locator("#room-summary");
    private ILocator BedSummary => Page.Locator("#bed-summary");
    private ILocator BedHold => Page.Locator("#bed-hold");
    private ILocator AssignBed => Page.Locator("#assign-bed-btn");
    private ILocator AssignmentStatus => Page.Locator("#assignment-status");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<BedAssignmentModel>.FromPage(Page);
        await Expect(Wing.DropDownButton).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    private async Task ChooseTheOakWing()
    {
        await Wing.Choose("Oak");
        await Expect(RoomSummary).ToHaveTextAsync("Oak has 3 rooms with an open bed.", new() { Timeout = 5000 });
    }

    private async Task ChooseRoom207InOak()
    {
        await ChooseTheOakWing();
        await Room.Choose("Room 207");
        await Expect(BedSummary).ToHaveTextAsync("Room 207 has 2 open beds.", new() { Timeout = 5000 });
    }

    private async Task HoldBed207B()
    {
        await ChooseRoom207InOak();
        await Bed.Choose("Bed 207-B, by the door");
        await Expect(BedHold).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    [Test]
    public async Task the_wing_list_offers_the_communitys_wings()
    {
        await NavigateAndBoot();

        await Wing.Open();

        await Expect(Wing.Items).ToHaveTextAsync(new[] { "Maple", "Oak", "Cedar" });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_room_list_offers_nothing_until_a_wing_is_chosen()
    {
        await NavigateAndBoot();

        await Room.Open();

        await Expect(Room.Items).ToHaveCountAsync(0);
        await Expect(RoomSummary).ToHaveTextAsync("Choose a wing to see its rooms with an open bed.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_a_wing_lists_its_rooms_with_an_open_bed()
    {
        await NavigateAndBoot();

        await ChooseTheOakWing();

        await Room.Open();
        await Expect(Room.Items).ToHaveTextAsync(new[] { "Room 204", "Room 207", "Room 211" });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_a_room_lists_the_open_beds_of_that_room_in_that_wing()
    {
        await NavigateAndBoot();

        await ChooseRoom207InOak();

        await Bed.Open();
        await Expect(Bed.Items).ToHaveTextAsync(new[] { "Bed 207-A, by the window", "Bed 207-B, by the door" });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_a_bed_holds_it_for_the_resident()
    {
        await NavigateAndBoot();

        await HoldBed207B();

        await Expect(BedHold).ToHaveTextAsync("Bed 207-B is held for Margaret Thompson until you assign it.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_another_room_replaces_its_beds_and_lets_go_of_the_bed_held()
    {
        await NavigateAndBoot();
        await HoldBed207B();

        await Room.Choose("Room 211");

        await Expect(BedSummary).ToHaveTextAsync("Room 211 has 1 open bed.", new() { Timeout = 5000 });
        await Expect(Bed.Input).ToHaveValueAsync("");
        await Expect(BedHold).ToBeHiddenAsync();
        await Bed.Open();
        await Expect(Bed.Items).ToHaveTextAsync(new[] { "Bed 211-B, by the door" });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_another_wing_replaces_the_rooms_and_clears_the_room_and_bed_chosen()
    {
        await NavigateAndBoot();
        await HoldBed207B();

        await Wing.Choose("Maple");

        await Expect(RoomSummary).ToHaveTextAsync("Maple has 2 rooms with an open bed.", new() { Timeout = 5000 });
        await Expect(Room.Input).ToHaveValueAsync("", new() { Timeout = 5000 });
        await Expect(Bed.Input).ToHaveValueAsync("");
        await Expect(BedHold).ToBeHiddenAsync();
        await Expect(BedSummary).ToHaveTextAsync("Choose a room to see its open beds.");
        await Room.Open();
        await Expect(Room.Items).ToHaveTextAsync(new[] { "Room 112", "Room 118" });
        await Page.Keyboard.PressAsync("Escape");
        await Bed.Open();
        await Expect(Bed.Items).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task assigning_the_bed_confirms_the_wing_room_and_bed()
    {
        await NavigateAndBoot();
        await HoldBed207B();

        await ClickWhenStable(AssignBed);

        await Expect(AssignmentStatus).ToHaveTextAsync(
            "Margaret Thompson is assigned bed 207-B in room 207, Oak wing.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task assigning_before_a_bed_is_chosen_asks_for_one()
    {
        await NavigateAndBoot();
        await ChooseRoom207InOak();

        await ClickWhenStable(AssignBed);

        await Expect(AssignmentStatus).ToHaveTextAsync("Choose a bed for Margaret Thompson.", new() { Timeout = 5000 });
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    // Gather-pipeline check: no answer can show what the rooms lookup left out, so the request itself is read.
    [Test]
    public async Task the_rooms_lookup_sends_only_the_wing_chosen()
    {
        await NavigateAndBoot();
        await HoldBed207B();

        var lookup = await Page.RunAndWaitForRequestAsync(
            async () => await Wing.Choose("Cedar"),
            request => request.Url.Contains("/Sandbox/Patterns/BedAssignment/Rooms"));

        Assert.That(new Uri(lookup.Url).Query, Is.EqualTo("?Wing=cedar"));
        AssertNoConsoleErrors();
    }
}
