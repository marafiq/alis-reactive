using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.TextArea;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.TextArea;

/// <summary>
/// As a day-shift caregiver, I want to write Margaret Hale's daily care log note, starting from what the night
/// shift wrote if I like, sign it as complete and save it to her daily log, so that the evening shift reads an
/// accurate, signed account of her day.
/// </summary>
/// <remarks>
/// The note starts empty under the night shift's note. Each time the caregiver leaves the note the log keeps a
/// draft and says how long it is; the daily log answers a save with the note's length. A signed note is locked:
/// the cursor does not stay in it, and a note that changes is no longer signed. Starting from the night shift's
/// note writes it in at once with <c>DataBind</c>, so the note is already unsigned when <c>FocusIn</c> puts the
/// cursor in it; without <c>DataBind</c> the note is still signed when the cursor arrives and is pushed out again.
/// <c>FocusOut</c> runs from the note's own focus event, which a click on another control would not prove.
/// </remarks>
[TestFixture]
public class WhenUsingFusionTextArea : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/TextArea";
    private const string NightShiftNote =
        "Slept through until 5:40, then asked for tea. Took her evening medications with applesauce.";

    private PagePlan<DailyCareLogModel> _plan = null!;

    private FusionTextAreaLocator Note => new(Page, _plan.FindComponent(m => m.CareNote)!.ElementId);
    private NativeCheckBoxLocator Signed => _plan.NativeCheckBox(m => m.Signed);
    private ILocator Status => Page.Locator("#care-log-status");
    private ILocator DraftStatus => Page.Locator("#draft-status");
    private ILocator WritingTips => Page.Locator("#writing-tips");
    private ILocator IncidentReminder => Page.Locator("#incident-reminder");
    private ILocator SignedNoteLocked => Page.Locator("#signed-note-locked");
    private ILocator NightShiftNoteText => Page.Locator("#night-shift-note");
    private ILocator StartFromNightNote => Page.Locator("#start-from-night-note-btn");
    private ILocator SaveToDailyLog => Page.Locator("#save-care-log-btn");

    private async Task OpenTheCareLog()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<DailyCareLogModel>.FromPage(Page);
        await Expect(Note.TextArea).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    private async Task WriteAndLeaveTheNote(string note)
    {
        await Note.FillAndBlur(note);
        await Expect(DraftStatus).ToHaveTextAsync(new System.Text.RegularExpressions.Regex("^(Draft kept|Nothing to keep)"), new() { Timeout = 10000 });
    }

    private async Task WriteAndSignTheNote(string note)
    {
        await WriteAndLeaveTheNote(note);
        await Signed.Check();
        await Expect(Signed.Input).ToBeCheckedAsync();
    }

    [Test]
    public async Task writing_that_margaret_fell_reminds_the_caregiver_to_file_an_incident_report()
    {
        await OpenTheCareLog();
        await Expect(IncidentReminder).ToBeHiddenAsync();

        await Note.Fill("After lunch she fell in the hallway.");
        await Expect(IncidentReminder).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Note.Fill("After lunch she walked the hallway.");
        await Expect(IncidentReminder).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_the_note_while_writing_says_it_was_cleared()
    {
        await OpenTheCareLog();
        await Note.Fill("Ate a full breakfast.");

        await Note.Clear();

        await Expect(Status).ToHaveTextAsync("You cleared the note.", new() { Timeout = 5000 });
        await Expect(Note.TextArea).ToBeFocusedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task leaving_an_edited_note_asks_the_caregiver_to_sign_it()
    {
        await OpenTheCareLog();

        await Note.FillAndBlur("Ate a full breakfast.");

        await Expect(Status).ToHaveTextAsync("Edited for the day shift. Sign it when it is complete.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task updating_the_night_shifts_note_says_it_was_updated()
    {
        await OpenTheCareLog();
        await ClickWhenStable(StartFromNightNote);
        await Expect(Note.TextArea).ToHaveValueAsync(NightShiftNote, new() { Timeout = 5000 });

        await Note.FillAndBlur("Slept through until 5:40. Ate a full breakfast.");

        await Expect(Status).ToHaveTextAsync(
            "Updated the night shift's note for the day shift. Sign it when it is complete.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task erasing_the_note_says_nothing_is_written_for_the_day_shift()
    {
        await OpenTheCareLog();
        await WriteAndLeaveTheNote("Ate a full breakfast.");
        await Expect(Status).ToHaveTextAsync("Edited for the day shift. Sign it when it is complete.", new() { Timeout = 5000 });

        await Note.FillAndBlur("");

        await Expect(Status).ToHaveTextAsync("Nothing written for the day shift yet.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task starting_from_the_night_shifts_note_puts_it_in_the_note_with_the_cursor()
    {
        await OpenTheCareLog();
        await Expect(NightShiftNoteText).ToHaveTextAsync(NightShiftNote);

        await ClickWhenStable(StartFromNightNote);

        await Expect(Note.TextArea).ToHaveValueAsync(NightShiftNote, new() { Timeout = 5000 });
        await Expect(Note.TextArea).ToBeFocusedAsync();
        await Expect(Status).ToHaveTextAsync("This is the night shift's note. Update it for the day shift.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task starting_a_signed_note_over_from_the_night_shifts_note_unsigns_it_and_opens_it_for_writing()
    {
        await OpenTheCareLog();
        await WriteAndSignTheNote("Ate a full breakfast.");

        await ClickWhenStable(StartFromNightNote);

        await Expect(Note.TextArea).ToHaveValueAsync(NightShiftNote, new() { Timeout = 5000 });
        await Expect(Signed.Input).Not.ToBeCheckedAsync();
        await Expect(Note.TextArea).ToBeFocusedAsync();
        await Expect(SignedNoteLocked).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_signed_note_is_locked_until_the_signature_is_cleared()
    {
        await OpenTheCareLog();
        await WriteAndSignTheNote("Ate a full breakfast.");

        await Note.TextArea.ClickAsync();

        await Expect(SignedNoteLocked).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Note.TextArea).Not.ToBeFocusedAsync();

        await Signed.Input.UncheckAsync();
        await Expect(SignedNoteLocked).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Note.TextArea.ClickAsync();
        await Expect(Note.TextArea).ToBeFocusedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task opening_an_empty_note_shows_what_to_write_about_until_the_caregiver_leaves_it()
    {
        await OpenTheCareLog();
        await Expect(WritingTips).ToBeHiddenAsync();

        await Note.Focus();
        await Expect(WritingTips).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Note.Blur();
        await Expect(WritingTips).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task opening_a_note_that_already_has_writing_shows_no_tips()
    {
        await OpenTheCareLog();
        await WriteAndLeaveTheNote("Ate a full breakfast.");

        await Note.Focus();

        await Expect(Note.TextArea).ToBeFocusedAsync(new() { Timeout = 5000 });
        await Expect(WritingTips).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task leaving_the_note_keeps_a_draft_and_says_how_long_it_is()
    {
        await OpenTheCareLog();
        await Expect(DraftStatus).ToHaveTextAsync("No draft kept yet.");

        await Note.FillAndBlur("Ate a full breakfast.");

        await Expect(DraftStatus).ToHaveTextAsync("Draft kept: 21 characters.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_without_a_note_asks_for_one_and_writing_one_clears_the_request()
    {
        await OpenTheCareLog();

        await ClickWhenStable(SaveToDailyLog);
        await Expect(_plan.ErrorFor(m => m.CareNote))
            .ToHaveTextAsync("Write how Margaret's day went before saving.", new() { Timeout = 5000 });

        await Note.FillAndBlur("Ate a full breakfast.");

        await Expect(_plan.ErrorFor(m => m.CareNote)).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_an_unsigned_note_asks_for_the_signature_and_signing_clears_the_request()
    {
        await OpenTheCareLog();
        await WriteAndLeaveTheNote("Ate a full breakfast.");

        await ClickWhenStable(SaveToDailyLog);
        await Expect(_plan.ErrorFor(m => m.Signed))
            .ToHaveTextAsync("Sign the note before saving it.", new() { Timeout = 5000 });

        await Signed.Check();

        await Expect(_plan.ErrorFor(m => m.Signed)).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_a_signed_note_records_it_in_the_daily_log_with_its_length()
    {
        await OpenTheCareLog();
        await WriteAndSignTheNote("Ate a full breakfast and walked the garden loop.");

        await ClickWhenStable(SaveToDailyLog);

        await Expect(Status).ToHaveTextAsync(
            "Saved Margaret Hale's day-shift note to the daily log: 48 characters.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }
}
