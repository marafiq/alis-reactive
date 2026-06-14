using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.TextArea;

// Journey: a caregiver documents a resident's Daily Care Log before the end of their shift.
// The log opens with last shift's note carried over. The caregiver edits it (a live preview,
// then a committed record showing what changed and that they edited it by hand), can pull back
// last shift's note, focus the field to keep editing and close it when done, then saves the
// note to the resident's daily log.
[TestFixture]
public class WhenUsingFusionTextArea : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/TextArea";
    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_TextAreaModel";
    private const string CareNoteId = GeneratedTypeScope + "__CareNote";

    private const string LastShiftNote =
        "Resident ate a full breakfast and walked the garden loop with assistance.";

    private FusionTextAreaLocator CareNote => new(Page, CareNoteId);

    private ILocator Presence => Page.Locator("#note-presence");
    private ILocator CommittedNote => Page.Locator("#note-committed");
    private ILocator CommitStatus => Page.Locator("#note-commit-status");
    private ILocator NoteInField => Page.Locator("#note-restored");
    private ILocator LivePreview => Page.Locator("#note-draft");
    private ILocator BeforeKeystroke => Page.Locator("#note-draft-previous");
    private ILocator WillReplace => Page.Locator("#note-replaced");
    private ILocator FocusSnapshot => Page.Locator("#note-focus-snapshot");
    private ILocator BlurSnapshot => Page.Locator("#note-blur-snapshot");
    private ILocator SaveGuard => Page.Locator("#note-save-guard");
    private ILocator Confirmation => Page.Locator("#note-confirmation");
    private ILocator ResumeButton => Page.Locator("#resume-editing");
    private ILocator CloseButton => Page.Locator("#close-note");
    private ILocator RestoreButton => Page.Locator("#restore-note");
    private ILocator SaveButton => Page.Locator("#save-note");

    private async Task OpenCareLog()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(CareNote.TextArea).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    // RENDERS — the FusionTextArea builder renders the textarea bound to last shift's note carried
    // onto the model.
    [Test]
    public async Task care_log_opens_showing_the_note_carried_over_from_last_shift()
    {
        await OpenCareLog();

        await Expect(CareNote.TextArea).ToHaveValueAsync(LastShiftNote, new() { Timeout = 10000 });
        await Expect(CommittedNote).ToHaveTextAsync(LastShiftNote);

        AssertNoConsoleErrors();
    }

    // INTERACTS — editing the note fires the Input event through the .Reactive wiring; the
    // FusionTextAreaInputArgs payload carries the new Value into the live preview and the
    // PreviousValue into the "before this keystroke" line.
    [Test]
    public async Task editing_the_note_updates_the_live_preview_and_remembers_what_it_was()
    {
        await OpenCareLog();

        await CareNote.Fill("Hydration encouraged at lunch.");

        await Expect(LivePreview).ToHaveTextAsync("Hydration encouraged at lunch.", new() { Timeout = 10000 });
        await Expect(BeforeKeystroke).ToHaveTextAsync(LastShiftNote);

        AssertNoConsoleErrors();
    }

    // Committing an edit proves FusionTextAreaChangeArgs delivers Value (the committed note),
    // PreviousValue (what it replaced), and IsInteracted=true routing the "edited by you" branch
    // when the caregiver typed the change themselves.
    [Test]
    public async Task finishing_an_edit_records_the_committed_note_and_what_it_replaced()
    {
        await OpenCareLog();

        await CareNote.FillAndBlur("Hydration encouraged at lunch.");

        await Expect(CommittedNote).ToHaveTextAsync("Hydration encouraged at lunch.", new() { Timeout = 10000 });
        await Expect(WillReplace).ToHaveTextAsync(LastShiftNote);
        await Expect(CommitStatus).ToHaveTextAsync("Edited by you this shift — ready to save.");

        AssertNoConsoleErrors();
    }

    // SetValue writes last shift's note back onto the field and the Value() source reads it back into
    // the "note in the field now" line; the programmatic write fires Changed with IsInteracted=false,
    // routing the "filled, not edited by hand" branch — the distinction from a hand-typed edit.
    [Test]
    public async Task using_last_shifts_note_fills_it_back_and_marks_it_not_edited_by_hand()
    {
        await OpenCareLog();

        await CareNote.FillAndBlur("Draft to be discarded.");
        await Expect(CommitStatus).ToHaveTextAsync("Edited by you this shift — ready to save.", new() { Timeout = 10000 });

        await RestoreButton.ClickAsync();

        await Expect(CareNote.TextArea).ToHaveValueAsync(LastShiftNote, new() { Timeout = 10000 });
        await Expect(NoteInField).ToHaveTextAsync(LastShiftNote);
        await Expect(CommitStatus).ToHaveTextAsync("Filled from last shift's note, not edited by hand.");

        AssertNoConsoleErrors();
    }

    // FocusIn moves focus into the note when the caregiver chooses to keep editing.
    [Test]
    public async Task resuming_editing_focuses_the_note()
    {
        await OpenCareLog();

        await ResumeButton.ClickAsync();

        await Expect(CareNote.TextArea).ToBeFocusedAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    // FocusOut removes focus from the note when the caregiver marks documentation done.
    [Test]
    public async Task closing_the_note_moves_focus_off_it()
    {
        await OpenCareLog();

        await ResumeButton.ClickAsync();
        await Expect(CareNote.TextArea).ToBeFocusedAsync(new() { Timeout = 10000 });

        await CloseButton.ClickAsync();

        await Expect(CareNote.TextArea).Not.ToBeFocusedAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    // INTERACTS (focus) — opening the note fires the Focus event; FusionTextAreaFocusArgs carries the
    // Value already on file, so the caregiver sees the note they are about to edit and the status
    // confirms they are editing now.
    [Test]
    public async Task opening_the_note_shows_it_is_being_edited_now_with_the_note_on_file()
    {
        await OpenCareLog();

        await CareNote.Focus();

        await Expect(Presence).ToHaveTextAsync("You are editing this note now.", new() { Timeout = 10000 });
        await Expect(FocusSnapshot).ToHaveTextAsync(LastShiftNote);

        AssertNoConsoleErrors();
    }

    // Leaving the note fires the Blur event; FusionTextAreaBlurArgs carries the Value held in the
    // field, which the log confirms as autosave-pending so the caregiver knows the change is kept.
    [Test]
    public async Task leaving_the_note_holds_the_change_for_autosave()
    {
        await OpenCareLog();

        await CareNote.Focus();
        await CareNote.Blur();

        await Expect(Presence).ToHaveTextAsync("Autosave pending — your changes are held.", new() { Timeout = 10000 });
        await Expect(BlurSnapshot).ToHaveTextAsync(LastShiftNote);

        AssertNoConsoleErrors();
    }

    // The Value() source feeds the save guard: with an empty note, the When(...).IsEmpty() condition
    // takes the Then branch and the caregiver is asked to write a note instead of a save being sent.
    [Test]
    public async Task saving_an_empty_note_asks_the_caregiver_to_write_one_first()
    {
        await OpenCareLog();

        await CareNote.Clear();
        await SaveButton.ClickAsync();

        await Expect(SaveGuard)
            .ToHaveTextAsync("Please write a care note before saving.", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    // SUBMITS — the Value() source feeds the gather body; the server confirmation the caregiver sees
    // reflects the saved note.
    [Test]
    public async Task saving_the_note_confirms_it_was_recorded()
    {
        await OpenCareLog();

        await CareNote.FillAndBlur("Hydration encouraged at lunch.");
        await SaveButton.ClickAsync();

        await Expect(Confirmation)
            .ToHaveTextAsync("Saved to the resident's daily log: “Hydration encouraged at lunch.”",
                new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    // GATHERS — the framework gather pipeline carries the Value() source into the POST body under the
    // declared key. (Framework gather test: asserts request.PostData.)
    [Test]
    public async Task saving_posts_the_care_note_to_the_server()
    {
        await OpenCareLog();

        await CareNote.FillAndBlur("Hydration encouraged at lunch.");

        var requestTask = Page.WaitForRequestAsync(request =>
            request.Url.Contains("/Sandbox/Components/TextArea/Echo") && request.Method == "POST",
            new() { Timeout = 10000 });

        await SaveButton.ClickAsync();

        var request = await requestTask;
        var body = request.PostData ?? "";

        Assert.That(body, Does.Contain("\"careNote\":\"Hydration encouraged at lunch.\""),
            "the gather pipeline must carry the care-note Value() source under its declared key");

        AssertNoConsoleErrors();
    }
}
