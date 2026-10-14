using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.DropDownTree;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.DropDownTree;

/// <summary>
/// As a move-in coordinator, I want to give each resident moving in a room, chosen on the building plan by
/// building, wing and room or typed by its number, with the room their deposit holds filled in for me, so
/// that every resident has exactly one room held for them before move-in day.
/// </summary>
/// <remarks>
/// Moving in this week: Ruth Alvarez, whose deposit holds Maple 106 (move-in October 21), and Walter Chen,
/// with no room held. The building plan is Maple House (North wing: Maple 104, Maple 106; South wing:
/// Maple 121, Maple 123) and Oak House (Memory care wing: Oak 110, Oak 112). Choosing a resident opens the
/// plan for the coordinator; a room the deposit holds is filled in and the plan closes on it. Every change
/// of room tells the front office which room is released and which is held, and a room picked on the plan,
/// not filled in by the page, writes its number in the room number box.
/// Not exercised: a read of Change's e field could not be told from IsInteracted, which EJ2 sets from it.
/// The render helper without a build step is not used: a plan rendered with no buildings has no room to
/// choose.
/// </remarks>
[TestFixture]
public class WhenUsingFusionDropDownTree : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionDropDownTree";

    private PagePlan<FusionDropDownTreeModel> _plan = null!;

    private FusionDropDownTreeLocator BuildingPlan => new(Page, _plan.FindComponent(m => m.Room)!.ElementId);
    private NativeDropDownLocator MovingIn => _plan.NativeDropDown(m => m.Resident);
    private NativeTextBoxLocator RoomNumber => _plan.TextBox(m => m.RoomNumber);
    private ILocator DepositHeld => Page.Locator("#deposit-held");
    private ILocator NoDeposit => Page.Locator("#no-deposit");
    private ILocator UnknownRoom => Page.Locator("#unknown-room");
    private ILocator HoldStatus => Page.Locator("#hold-status");
    private ILocator ReleaseRoom => Page.Locator("#release-room-btn");
    private ILocator AssignRoom => Page.Locator("#assign-room-btn");
    private ILocator AssignStatus => Page.Locator("#assign-status");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<FusionDropDownTreeModel>.FromPage(Page);
        await Expect(BuildingPlan.Input).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    // Walter Chen has no room held, so the plan stays open for the coordinator to choose one.
    private async Task ChooseWalterChen()
    {
        await MovingIn.Select("Walter Chen");
        await Expect(NoDeposit).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    // Ruth Alvarez's deposit holds Maple 106, which the page fills in and holds for her.
    private async Task ChooseRuthAlvarez()
    {
        await MovingIn.Select("Ruth Alvarez");
        await Expect(HoldStatus).ToHaveTextAsync("Maple 106 is held for Ruth Alvarez.", new() { Timeout = 5000 });
    }

    private async Task TypeRoomNumber(string number)
    {
        await RoomNumber.Fill(number);
        await RoomNumber.Input.PressAsync("Enter");
    }

    [Test]
    public async Task choosing_a_resident_with_no_room_held_opens_the_building_plan()
    {
        await NavigateAndBoot();
        await Expect(BuildingPlan.Popup).ToBeHiddenAsync();

        await MovingIn.Select("Walter Chen");

        await Expect(NoDeposit).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(BuildingPlan.Popup).ToBeVisibleAsync();
        await Expect(BuildingPlan.TreeItemText("Oak 110")).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    // The page opens the plan when the resident is chosen; filling in the deposit's room does not close it,
    // only the page does.
    [Test]
    public async Task the_room_a_deposit_holds_is_filled_in_and_the_building_plan_closes_on_it()
    {
        await NavigateAndBoot();

        await MovingIn.Select("Ruth Alvarez");

        await Expect(BuildingPlan.Input).ToHaveValueAsync("Maple 106", new() { Timeout = 5000 });
        await Expect(DepositHeld).ToHaveTextAsync("The deposit holds Maple 106.");
        await Expect(DepositHeld).ToBeVisibleAsync();
        await Expect(BuildingPlan.Popup).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task picking_a_room_on_the_plan_holds_it_for_the_resident()
    {
        await NavigateAndBoot();
        await ChooseWalterChen();

        await BuildingPlan.SelectItem("Oak 112");

        await Expect(BuildingPlan.Input).ToHaveValueAsync("Oak 112", new() { Timeout = 5000 });
        await Expect(HoldStatus).ToHaveTextAsync("Oak 112 is held for Walter Chen.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_room_picked_on_the_plan_writes_its_number()
    {
        await NavigateAndBoot();
        await ChooseWalterChen();

        await BuildingPlan.SelectItem("Oak 112");

        await Expect(RoomNumber.Input).ToHaveValueAsync("Oak 112", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // The hold is posted after the number would have been written, so the box is read once it is settled.
    [Test]
    public async Task the_room_a_deposit_holds_does_not_write_its_number()
    {
        await NavigateAndBoot();

        await ChooseRuthAlvarez();

        await Expect(RoomNumber.Input).ToHaveValueAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task picking_another_room_releases_the_one_held_before()
    {
        await NavigateAndBoot();
        await ChooseRuthAlvarez();
        await BuildingPlan.Open();

        await BuildingPlan.SelectItem("Oak 110");

        await Expect(HoldStatus).ToHaveTextAsync("Maple 106 is released; Oak 110 is held for Ruth Alvarez.", new() { Timeout = 5000 });
        await Expect(BuildingPlan.Input).ToHaveValueAsync("Oak 110");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task picking_a_wing_instead_of_a_room_is_refused()
    {
        await NavigateAndBoot();
        await ChooseWalterChen();

        await BuildingPlan.SelectItem("North wing");

        await Expect(HoldStatus).ToHaveTextAsync("North wing is not a room. Choose one of its rooms.", new() { Timeout = 5000 });
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task typing_a_room_number_picks_that_room_on_the_plan()
    {
        await NavigateAndBoot();
        await ChooseWalterChen();

        await TypeRoomNumber("Oak 110");

        await Expect(BuildingPlan.Input).ToHaveValueAsync("Oak 110", new() { Timeout = 5000 });
        await Expect(HoldStatus).ToHaveTextAsync("Oak 110 is held for Walter Chen.", new() { Timeout = 5000 });
        await Expect(UnknownRoom).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_room_number_the_plan_does_not_have_is_refused()
    {
        await NavigateAndBoot();

        await TypeRoomNumber("Oak 999");

        await Expect(UnknownRoom).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(UnknownRoom).ToHaveTextAsync("No room is numbered Oak 999.");
        await Expect(BuildingPlan.Input).ToHaveValueAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task releasing_the_room_empties_the_plan_and_frees_the_room()
    {
        await NavigateAndBoot();
        await ChooseRuthAlvarez();

        await ClickWhenStable(ReleaseRoom);

        await Expect(BuildingPlan.Input).ToHaveValueAsync("", new() { Timeout = 5000 });
        await Expect(HoldStatus).ToHaveTextAsync("Maple 106 is released.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task releasing_with_no_room_held_says_there_is_none_to_release()
    {
        await NavigateAndBoot();

        await ClickWhenStable(ReleaseRoom);

        await Expect(HoldStatus).ToHaveTextAsync("No room is held to release.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task assigning_without_a_room_asks_for_one()
    {
        await NavigateAndBoot();
        await ChooseWalterChen();

        await ClickWhenStable(AssignRoom);

        await Expect(_plan.ErrorFor(m => m.Room)).ToHaveTextAsync("Choose a room on the building plan.", new() { Timeout = 5000 });
        await Expect(AssignStatus).ToHaveTextAsync("No move-in booked yet.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task picking_a_room_takes_away_the_request_for_one()
    {
        await NavigateAndBoot();
        await ChooseWalterChen();
        await ClickWhenStable(AssignRoom);
        await Expect(_plan.ErrorFor(m => m.Room)).ToHaveTextAsync("Choose a room on the building plan.", new() { Timeout = 5000 });
        await BuildingPlan.Open();

        await BuildingPlan.SelectItem("Oak 112");

        await Expect(_plan.ErrorFor(m => m.Room)).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task assigning_the_held_room_books_the_move_in()
    {
        await NavigateAndBoot();
        await ChooseRuthAlvarez();

        await ClickWhenStable(AssignRoom);

        await Expect(AssignStatus).ToHaveTextAsync("Ruth Alvarez moves into Maple 106 on October 21.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
