using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Mention;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Mention;

/// <summary>
/// As a night nurse, I want to name the people my shift handoff note is for with @ and see who it notifies,
/// and to find the day nurse I hand the resident to, so that the right people read about Margaret Hale's night.
/// </summary>
/// <remarks>
/// The note is Margaret Hale's; her care team is Dr. Amir Patel, Rosa Reyes, Nora Lind and Omar Haddad, and the
/// day shift is Karen Holt, Luis Ortega, Kalani Brooks and Meera Shah. EJ2 opens the note's list when @ is typed;
/// the page shows how to pick while it is open and offers to open it again when it closes without a pick. The
/// hand-off field finds day nurses by the word typed in it (Search), at the point of the page the page names.
/// Clear is pressed from the keyboard: EJ2 closes a Mention's list on any mousedown outside it, so a mouse click
/// would close it even if HidePopup did nothing.
/// Change's IsInteracted is not proven: EJ2 raises Change only for a pick made with a key or a click, so it is
/// always true (mention.js:1231, 1553). Opened and Closed carry no data.
/// </remarks>
[TestFixture]
public class WhenUsingFusionMention : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Mention";

    private PagePlan<MentionModel> _plan = null!;

    private ILocator Note => _plan.TextBox(m => m.ShiftNote).Input;
    private ILocator DayNurse => _plan.TextBox(m => m.NextNurse).Input;
    private FusionMentionLocator CareTeam => new(Page, _plan.FindComponent(m => m.ShiftNote)!.ElementId);
    private FusionMentionLocator DayShift => new(Page, _plan.FindComponent(m => m.NextNurse)!.ElementId);
    private ILocator Hint => Page.Locator("#mention-hint");
    private ILocator ChooseWhoToNotify => Page.Locator("#choose-who-to-notify-btn");
    private ILocator NoOneNotified => Page.Locator("#notified-none");
    private ILocator NotifiedPhysician => Page.Locator("#notified-s-101");
    private ILocator NotifiedChargeNurse => Page.Locator("#notified-s-114");
    private ILocator Find => Page.Locator("#find-day-nurse-btn");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<MentionModel>.FromPage(Page);
    }

    // The Mention reads each key as it is released, so the note is typed key by key.
    private async Task TypeInTheNote(string text)
    {
        await Note.ClickAsync();
        await Note.PressSequentiallyAsync(text, new() { Delay = 30 });
    }

    // Typed after @, a name's first letters narrow the list to that person; the pick waits for the narrowed list.
    private async Task MentionByFirstLetters(string letters, string name)
    {
        await Note.PressSequentiallyAsync("@" + letters, new() { Delay = 30 });
        await Expect(CareTeam.Items).ToHaveCountAsync(1, new() { Timeout = 5000 });
        await CareTeam.Item(name).ClickAsync();
    }

    private async Task FindDayNurses(string word)
    {
        await DayNurse.ClickAsync();
        await DayNurse.PressSequentiallyAsync(word, new() { Delay = 30 });
        await Find.ClickAsync();
        await Expect(DayShift.Popup).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    [Test]
    public async Task typing_an_at_sign_in_the_note_lists_her_care_team_and_says_how_to_pick()
    {
        await NavigateAndBoot();

        await TypeInTheNote("Refused her 9pm dose. @");

        await Expect(CareTeam.Items).ToHaveTextAsync(new[] { "Dr. Amir Patel", "Rosa Reyes", "Nora Lind", "Omar Haddad" }, new() { Timeout = 5000 });
        await Expect(Hint).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task picking_the_physician_lists_him_as_notified()
    {
        await NavigateAndBoot();
        await TypeInTheNote("Refused her 9pm dose. ");

        await MentionByFirstLetters("Pat", "Dr. Amir Patel");

        await Expect(Note).ToHaveValueAsync("Refused her 9pm dose. @Dr. Amir Patel");
        await Expect(NotifiedPhysician).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(NotifiedPhysician).ToHaveTextAsync("Dr. Amir Patel, attending physician");
        await Expect(NotifiedChargeNurse).ToBeHiddenAsync();
        await Expect(NoOneNotified).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task mentioning_the_physician_and_the_charge_nurse_lists_both_as_notified()
    {
        await NavigateAndBoot();
        await TypeInTheNote("Refused her 9pm dose. ");
        await MentionByFirstLetters("Pat", "Dr. Amir Patel");

        await Note.PressSequentiallyAsync(" and ", new() { Delay = 30 });
        await MentionByFirstLetters("Ros", "Rosa Reyes");

        await Expect(Note).ToHaveValueAsync("Refused her 9pm dose. @Dr. Amir Patel and @Rosa Reyes");
        await Expect(NotifiedPhysician).ToBeVisibleAsync();
        await Expect(NotifiedChargeNurse).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(NotifiedChargeNurse).ToHaveTextAsync("Rosa Reyes, charge nurse");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task pressing_escape_closes_the_list_and_offers_to_choose_who_to_notify()
    {
        await NavigateAndBoot();
        await TypeInTheNote("Please review @");
        await Expect(CareTeam.Popup).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Note.PressAsync("Escape");

        await Expect(CareTeam.Popup).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(Hint).ToBeHiddenAsync();
        await Expect(ChooseWhoToNotify).ToBeVisibleAsync();
        await Expect(Note).ToHaveValueAsync("Please review @");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_who_to_notify_after_escape_finishes_the_mention()
    {
        await NavigateAndBoot();
        await TypeInTheNote("Please review @");
        await Expect(CareTeam.Popup).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Note.PressAsync("Escape");
        await Expect(CareTeam.Popup).ToBeHiddenAsync(new() { Timeout = 5000 });

        await ChooseWhoToNotify.ClickAsync();
        await Expect(CareTeam.Items).ToHaveCountAsync(4, new() { Timeout = 5000 });
        await CareTeam.Item("Rosa Reyes").ClickAsync();

        await Expect(Note).ToHaveValueAsync("Please review @Rosa Reyes");
        await Expect(NotifiedChargeNurse).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(ChooseWhoToNotify).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task finding_a_day_nurse_lists_only_the_nurses_whose_names_hold_the_typed_word()
    {
        await NavigateAndBoot();

        await FindDayNurses("ka");

        await Expect(DayShift.Items).ToHaveTextAsync(new[] { "Karen Holt", "Kalani Brooks" }, new() { Timeout = 5000 });
        // EJ2 places the list at the point of the page the Find button names: 64 pixels across, 640 down.
        await Expect(DayShift.Popup).ToHaveCSSAsync("left", "64px");
        await Expect(DayShift.Popup).ToHaveCSSAsync("top", "640px");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task picking_the_day_nurse_found_hands_the_resident_to_her()
    {
        await NavigateAndBoot();
        await FindDayNurses("ka");

        await DayShift.Item("Karen Holt").ClickAsync();

        await Expect(DayNurse).ToHaveValueAsync("Karen Holt", new() { Timeout = 5000 });
        await Expect(DayShift.Popup).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_the_hand_off_from_the_keyboard_closes_the_list_of_day_nurses()
    {
        await NavigateAndBoot();
        await FindDayNurses("ka");

        // From Find, Tab reaches Clear; Enter presses it without a click that would close the list itself.
        await Page.Keyboard.PressAsync("Tab");
        await Expect(Page.Locator("#clear-day-nurse-btn")).ToBeFocusedAsync();
        await Expect(DayShift.Popup).ToBeVisibleAsync();
        await Page.Keyboard.PressAsync("Enter");

        await Expect(DayShift.Popup).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(DayNurse).ToHaveValueAsync("");
        AssertNoConsoleErrors();
    }
}
