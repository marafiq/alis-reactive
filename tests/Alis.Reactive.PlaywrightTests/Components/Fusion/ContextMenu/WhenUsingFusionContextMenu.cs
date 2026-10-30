using System.Text.RegularExpressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.ContextMenu;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.ContextMenu;

/// <summary>
/// As the evening charge nurse on the Maple wing, I want to right-click a resident on the room board to move
/// them to an open room, flag them for the night nurse or read their care plan, so that the wing's changes are
/// made on the board the shift already works from, and never into a room someone else holds.
/// </summary>
/// <remarks>
/// Each resident's card is the target of that resident's menu; the Actions button opens the same menu for a
/// nurse on a keyboard or a tablet. The menu checks the rooms it offers as it lists them, holds the room list
/// open while a move saves and closes once the move is saved. During the shift handover the board is
/// read-only: the handover menu opens where the nurse right-clicked. The tests work on Arthur Bell, Maple 215.
/// Not exercised:
/// OnOpen's, OnClose's and BeforeClose's Items: each carries the items of the list opening or closing, the
/// list BeforeOpen's Items carries, which the page reads.
/// A menu item's Items: a submenu's items arrive as BeforeOpen's Items when the submenu opens.
/// BeforeItemRender for the top list: EJ2 renders it when it creates the menu, before the page boots; the page
/// reads BeforeItemRender for the room list, which renders when it opens.
/// </remarks>
[TestFixture]
public class WhenUsingFusionContextMenu : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/ContextMenu";

    // Where the nurse right-clicks Arthur's card: on his name, near the card's top-left corner.
    private const float ClickX = 24;
    private const float ClickY = 16;

    private static readonly Regex Marked = new(@"\bring-2\b");

    private PagePlan<FusionContextMenuModel> _plan = null!;

    private FusionContextMenuLocator ArthursMenu => new(Page, "arthur-actions");
    private FusionContextMenuLocator HandoverMenu => new(Page, "handover-menu");
    private ILocator ArthursCard => Page.Locator("#arthur-card");
    private ILocator ArthursRoom => Page.Locator("#arthur-room");
    private ILocator ArthursFlag => Page.Locator("#arthur-flag");
    private ILocator ArthursActions => Page.Locator("#arthur-actions-btn");
    private ILocator ArthursCarePlan => Page.Locator("#arthur-care-plan");
    private ILocator ArthursCarePlanTitle => ArthursCarePlan.Locator("h2");
    private ILocator Status => Page.Locator("#board-status");
    private ILocator OpenRooms => Page.Locator("#open-rooms");
    private ILocator RoomsNote => Page.Locator("#rooms-note");
    private ILocator HandoverNote => Page.Locator("#handover-note");
    private ILocator HandoverReport => Page.Locator("#handover-report");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<FusionContextMenuModel>.FromPage(Page);
    }

    private async Task RightClickArthur() =>
        await ArthursCard.ClickAsync(new() { Button = MouseButton.Right, Position = new() { X = ClickX, Y = ClickY } });

    private async Task OpenArthursMenu()
    {
        await RightClickArthur();
        await Expect(ArthursMenu.Item("Move to room")).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    private async Task ShowTheRoomList()
    {
        await OpenArthursMenu();
        await ArthursMenu.Item("Move to room").HoverAsync();
        await Expect(ArthursMenu.Item("Maple 216")).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    private async Task FlagArthurFor(string flag)
    {
        await OpenArthursMenu();
        await ArthursMenu.Item("Flag for night nurse").HoverAsync();
        await ClickWhenStable(ArthursMenu.Item(flag));
    }

    private async Task StartTheHandover() => await _plan.NativeCheckBox(m => m.ShiftHandover).Check();

    [Test]
    public async Task right_clicking_a_resident_opens_their_actions_and_marks_their_card()
    {
        await NavigateAndBoot();

        await RightClickArthur();

        await Expect(ArthursMenu.Item("Move to room")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(ArthursMenu.Item("Flag for night nurse")).ToBeVisibleAsync();
        await Expect(ArthursMenu.Item("View care plan")).ToBeVisibleAsync();
        await Expect(ArthursCard).ToHaveClassAsync(Marked);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_actions_button_opens_the_residents_menu_without_a_right_click()
    {
        await NavigateAndBoot();

        await ClickWhenStable(ArthursActions);

        await Expect(ArthursMenu.Item("Move to room")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(ArthursCard).ToHaveClassAsync(Marked);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task closing_the_menu_unmarks_the_card()
    {
        await NavigateAndBoot();
        await OpenArthursMenu();
        await Expect(ArthursCard).ToHaveClassAsync(Marked);

        await Page.Keyboard.PressAsync("Escape");

        await Expect(ArthursMenu.Root).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(ArthursCard).Not.ToHaveClassAsync(Marked);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task moving_from_one_list_to_another_keeps_the_card_marked()
    {
        await NavigateAndBoot();
        await ShowTheRoomList();

        await ArthursMenu.Item("Flag for night nurse").HoverAsync();

        await Expect(ArthursMenu.Item("Fall risk")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(ArthursMenu.Item("Maple 216")).ToBeHiddenAsync();
        await Expect(ArthursCard).ToHaveClassAsync(Marked);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task opening_the_room_list_notes_the_room_held_for_tonights_admission()
    {
        await NavigateAndBoot();
        await OpenArthursMenu();
        await Expect(RoomsNote).ToBeHiddenAsync();

        await ArthursMenu.Item("Move to room").HoverAsync();

        await Expect(RoomsNote).ToHaveTextAsync("Maple 218 is held for tonight's 9:00 PM admission.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_saved_move_closes_the_menu_and_shows_the_resident_in_the_new_room()
    {
        await NavigateAndBoot();
        await ShowTheRoomList();

        await ClickWhenStable(ArthursMenu.Item("Maple 216"));

        await Expect(Status).ToHaveTextAsync("Arthur Bell moved to Maple 216.", new() { Timeout = 10000 });
        await Expect(ArthursRoom).ToHaveTextAsync("Maple 216");
        await Expect(OpenRooms).ToHaveTextAsync("Maple 215, Maple 218");
        await Expect(ArthursMenu.Root).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(ArthursCard).Not.ToHaveClassAsync(Marked);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_refused_move_keeps_the_room_list_open_to_choose_another_room()
    {
        await NavigateAndBoot();
        await ShowTheRoomList();

        await ClickWhenStable(ArthursMenu.Item("Maple 218"));

        await Expect(Status).ToHaveTextAsync(
            "Maple 218 is held for tonight's admission; choose another room for Arthur Bell.", new() { Timeout = 10000 });
        await Expect(ArthursMenu.Item("Maple 216")).ToBeVisibleAsync();
        await Expect(ArthursRoom).ToHaveTextAsync("Maple 215");
        AssertNoConsoleErrorsExcept("the server responded with a status of 409");
    }

    [Test]
    public async Task an_urgent_flag_shows_on_the_card_and_puts_the_resident_first_on_the_rounds()
    {
        await NavigateAndBoot();

        await FlagArthurFor("Fall risk");

        await Expect(ArthursFlag).ToHaveTextAsync("Fall risk", new() { Timeout = 5000 });
        await Expect(ArthursFlag).ToBeVisibleAsync();
        await Expect(Status).ToHaveTextAsync("Arthur Bell is first on the night nurse's rounds.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_routine_flag_shows_on_the_card_without_moving_the_resident_up_the_rounds()
    {
        await NavigateAndBoot();

        await FlagArthurFor("Skipped dinner");

        await Expect(ArthursFlag).ToHaveTextAsync("Skipped dinner", new() { Timeout = 5000 });
        await Expect(ArthursFlag).ToBeVisibleAsync();
        await Expect(Status).ToHaveTextAsync("Arthur Bell's flag is on the night nurse's list.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_resident_flagged_for_fall_risk_is_not_moved_until_the_check_is_done()
    {
        await NavigateAndBoot();
        await FlagArthurFor("Fall risk");
        await Expect(ArthursFlag).ToHaveTextAsync("Fall risk", new() { Timeout = 5000 });
        await OpenArthursMenu();

        await ArthursMenu.Item("Move to room").HoverAsync();

        await Expect(Status).ToHaveTextAsync("Finish Arthur Bell's fall-risk check before moving them.", new() { Timeout = 5000 });
        await Expect(ArthursMenu.Item("Maple 216")).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task viewing_the_care_plan_opens_it_below_the_board()
    {
        await NavigateAndBoot();
        await OpenArthursMenu();
        await Expect(ArthursCarePlan).ToBeHiddenAsync();

        await ClickWhenStable(ArthursMenu.Item("View care plan"));

        await Expect(ArthursCarePlan).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(ArthursCarePlanTitle).ToHaveTextAsync("Arthur Bell's care plan");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task during_the_handover_a_right_click_opens_only_the_handover_menu_where_the_nurse_clicked()
    {
        await NavigateAndBoot();
        await StartTheHandover();
        // Where the card is when the nurse clicks: the handover note that opens above the board then moves it down.
        var card = await ArthursCard.BoundingBoxAsync();

        await RightClickArthur();

        await Expect(HandoverMenu.Item("Open the handover report")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(ArthursMenu.Root).ToBeHiddenAsync();
        await Expect(HandoverNote).ToHaveTextAsync("3 actions for Arthur Bell wait until the handover ends.");
        // The click lands inside the card's 1px border; EJ2 opens the menu one pixel past the pointer.
        var menu = await HandoverMenu.Root.BoundingBoxAsync();
        Assert.That(menu!.X, Is.EqualTo(card!.X + ClickX + 1).Within(3), "the menu's left edge sits at the click");
        Assert.That(menu.Y, Is.EqualTo(card.Y + ClickY + 1).Within(3), "the menu's top edge sits at the click");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_handover_menu_opens_the_handover_report()
    {
        await NavigateAndBoot();
        await StartTheHandover();
        await RightClickArthur();
        await Expect(HandoverReport).ToBeHiddenAsync();

        await ClickWhenStable(HandoverMenu.Item("Open the handover report"));

        await Expect(HandoverReport).ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
