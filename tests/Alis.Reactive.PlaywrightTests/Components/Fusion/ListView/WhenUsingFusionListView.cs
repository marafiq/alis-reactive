using System.Text.RegularExpressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.ListView;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.ListView;

/// <summary>
/// As the day charge nurse on Maple wing, I want to walk the evening nurse through the residents in round order
/// and tick off what I finish before I leave, so that no resident is missed at the handoff and nothing is left
/// for the evening shift that I should have done.
/// </summary>
/// <remarks>
/// The round is Arthur Bell, Ruth Coleman, Helen Park, Walter Dean and Grace Liu. Helen Park is at St. Mary's
/// Hospital, so the round does not open her and skips her when it moves on. Opening a resident the nurse clicks
/// fetches their note by their place on the round; handing a resident off moves the round on to the next resident
/// in the building, whose note comes with the handoff's answer, and after Grace Liu the round is complete.
/// A resident the page selects after a handoff raises Select without IsInteracted or Index (list-view.js:1084-1110,
/// 1130-1150), which is how the round tells its own move from the nurse's click. Before you leave is a checkbox
/// list of the day nurse's last tasks; its Select carries the task and whether it is now checked.
/// </remarks>
[TestFixture]
public class WhenUsingFusionListView : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionListView";
    private static readonly Regex Selected = new(@"\be-active\b");

    private FusionListViewLocator Round => new(Page, "handoff-round");
    private FusionListViewLocator BeforeYouLeave => new(Page, "leaving-checklist");
    private ILocator RoundStatus => Page.Locator("#round-status");
    private ILocator AwayNotice => Page.Locator("#away-notice");
    private ILocator RoundComplete => Page.Locator("#round-complete");
    private ILocator Note => Page.Locator("#handoff-note");
    private ILocator NoteResident => Page.Locator("#handoff-note-resident");
    private ILocator NoteText => Page.Locator("#handoff-note-text");
    private ILocator HandedOff => Page.Locator("#handed-off-btn");
    private ILocator ChecklistStatus => Page.Locator("#checklist-status");
    private ILocator AllCovered => Page.Locator("#all-covered-btn");
    private ILocator ClearChecklist => Page.Locator("#clear-checklist-btn");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Round.Item("Arthur Bell")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(BeforeYouLeave.Item("Call lights tested")).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    private async Task OpenResident(string name)
    {
        await Round.ClickItem(name);
        await Expect(NoteResident).ToHaveTextAsync(name, new() { Timeout = 5000 });
    }

    [Test]
    public async Task clicking_a_resident_opens_their_handoff_note()
    {
        await NavigateAndBoot();

        await Round.ClickItem("Ruth Coleman");

        await Expect(Note).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(NoteResident).ToHaveTextAsync("Ruth Coleman");
        await Expect(NoteText).ToHaveTextAsync("New walker delivered today. Physical therapy at 4 PM; encourage fluids.");
        await Expect(RoundStatus).ToHaveTextAsync("Opened Ruth Coleman");
        await Expect(Round.Item("Ruth Coleman")).ToHaveClassAsync(Selected);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task handing_off_a_resident_moves_the_round_on_to_the_next_one_in_the_building()
    {
        await NavigateAndBoot();
        await OpenResident("Ruth Coleman");

        await ClickWhenStable(HandedOff);

        await Expect(Round.Item("Walter Dean")).ToHaveClassAsync(Selected, new() { Timeout = 5000 });
        await Expect(Round.SelectedItems).ToHaveCountAsync(1);
        await Expect(RoundStatus).ToHaveTextAsync("Up next: Walter Dean");
        await Expect(NoteResident).ToHaveTextAsync("Walter Dean");
        await Expect(NoteText).ToHaveTextAsync("Refused breakfast and lunch. His daughter visits at 6 PM; let her know.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task handing_off_again_hands_off_the_resident_the_round_moved_on_to()
    {
        await NavigateAndBoot();
        await OpenResident("Ruth Coleman");
        await ClickWhenStable(HandedOff);
        await Expect(RoundStatus).ToHaveTextAsync("Up next: Walter Dean", new() { Timeout = 5000 });

        await ClickWhenStable(HandedOff);

        await Expect(Round.Item("Grace Liu")).ToHaveClassAsync(Selected, new() { Timeout = 5000 });
        await Expect(RoundStatus).ToHaveTextAsync("Up next: Grace Liu");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_resident_out_of_the_building_is_not_opened_on_the_round()
    {
        await NavigateAndBoot();

        await Round.ClickItem("Helen Park");

        await Expect(AwayNotice).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(AwayNotice).ToHaveTextAsync("Helen Park is at St. Mary's Hospital until Thursday. The hospital has the handoff.");
        await Expect(Round.Item("Helen Park")).Not.ToHaveClassAsync(Selected);
        await Expect(Note).ToBeHiddenAsync();
        await Expect(RoundStatus).ToHaveTextAsync("Up next: Arthur Bell");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task handing_off_the_last_resident_completes_the_round_and_leaves_no_one_open()
    {
        await NavigateAndBoot();
        await OpenResident("Grace Liu");
        await Expect(Round.Item("Grace Liu")).ToHaveClassAsync(Selected);

        await ClickWhenStable(HandedOff);

        await Expect(RoundComplete).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Round.Item("Grace Liu")).Not.ToHaveClassAsync(Selected);
        await Expect(Round.SelectedItems).ToHaveCountAsync(0);
        await Expect(Note).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task ticking_a_task_notes_it_as_covered()
    {
        await NavigateAndBoot();

        await BeforeYouLeave.ClickItem("Call lights tested");

        await Expect(BeforeYouLeave.CheckMark("Call lights tested")).ToHaveCountAsync(1, new() { Timeout = 5000 });
        await Expect(ChecklistStatus).ToHaveTextAsync("Covered: Call lights tested");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task unticking_a_task_notes_it_as_reopened()
    {
        await NavigateAndBoot();
        await BeforeYouLeave.ClickItem("Call lights tested");
        await Expect(ChecklistStatus).ToHaveTextAsync("Covered: Call lights tested", new() { Timeout = 5000 });

        await BeforeYouLeave.ClickItem("Call lights tested");

        await Expect(BeforeYouLeave.CheckMark("Call lights tested")).ToHaveCountAsync(0, new() { Timeout = 5000 });
        await Expect(ChecklistStatus).ToHaveTextAsync("Reopened: Call lights tested");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task all_covered_ticks_every_task()
    {
        await NavigateAndBoot();

        await ClickWhenStable(AllCovered);

        foreach (var task in MapleHandoff.BeforeLeaving)
            await Expect(BeforeYouLeave.CheckMark(task)).ToHaveCountAsync(1, new() { Timeout = 5000 });
        await Expect(ChecklistStatus).ToHaveTextAsync("Covered: every item");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_the_checklist_unticks_every_task()
    {
        await NavigateAndBoot();
        await BeforeYouLeave.ClickItem("Narcotics count matched");
        await BeforeYouLeave.ClickItem("Incidents logged");
        await Expect(BeforeYouLeave.CheckMarks).ToHaveCountAsync(2, new() { Timeout = 5000 });

        await ClickWhenStable(ClearChecklist);

        await Expect(BeforeYouLeave.CheckMarks).ToHaveCountAsync(0, new() { Timeout = 5000 });
        await Expect(ChecklistStatus).ToHaveTextAsync("Nothing covered yet.");
        AssertNoConsoleErrors();
    }
}
