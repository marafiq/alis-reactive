using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.TextBox;

// Journey: a care coordinator updates a resident's profile card — the preferred name the
// resident goes by (shown to staff on the directory) and a dietary note kitchen staff read.
// The card opens with the name on file, the coordinator edits it (live preview, then a
// committed record showing what changed and who changed it), can pull in the legal name,
// focus the field to edit and blur it when done, review and update the dietary note, then
// save the profile to the resident record.
[TestFixture]
public class WhenUsingFusionTextBox : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/TextBox";
    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_TextBoxModel";
    private const string PreferredNameId = GeneratedTypeScope + "__PreferredName";
    private const string DietaryNoteId = GeneratedTypeScope + "__DietaryNote";

    private FusionTextBoxLocator PreferredName => new(Page, PreferredNameId);
    private FusionTextBoxLocator DietaryNote => new(Page, DietaryNoteId);

    private ILocator DirectoryPreview => Page.Locator("#name-preview");
    private ILocator NamePreviewHint => Page.Locator("#name-preview-hint");
    private ILocator NameReplacing => Page.Locator("#name-replacing");
    private ILocator LastSavedName => Page.Locator("#name-committed");
    private ILocator ChangedFrom => Page.Locator("#name-changed-from");
    private ILocator EditSource => Page.Locator("#name-edit-source");
    private ILocator DietaryGuidance => Page.Locator("#dietary-guidance");
    private ILocator DietaryOnOpen => Page.Locator("#dietary-on-open");
    private ILocator DietaryCaptured => Page.Locator("#dietary-captured");
    private ILocator SaveStatus => Page.Locator("#save-status");
    private ILocator SaveConfirmation => Page.Locator("#save-confirmation");
    private ILocator UseLegalNameButton => Page.Locator("#use-legal-name");
    private ILocator EditNameButton => Page.Locator("#start-editing-name");
    private ILocator DoneEditingButton => Page.Locator("#done-editing-name");
    private ILocator SaveButton => Page.Locator("#save-profile");

    private async Task OpenProfile()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(PreferredName.Input).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    // RENDERS — the FusionTextBox builder renders the field bound to the name on file, and the
    // DomReady AddAppendIcon adds the directory-search affordance inside the input.
    [Test]
    public async Task profile_opens_showing_the_name_on_file_with_a_directory_search_affordance()
    {
        await OpenProfile();

        await Expect(PreferredName.Input).ToHaveValueAsync("Margaret", new() { Timeout = 10000 });
        await Expect(DirectoryPreview).ToHaveTextAsync("Margaret");
        await Expect(PreferredName.Wrapper.Locator(".e-icons.e-search")).ToHaveCountAsync(1);

        AssertNoConsoleErrors();
    }

    // INTERACTS — typing into the field fires the Input event through the .Reactive wiring; the
    // FusionTextBoxInputArgs payload carries the new Value into the live directory preview and the
    // PreviousValue into the "replacing the name on record" line.
    [Test]
    public async Task typing_a_new_preferred_name_updates_the_directory_preview()
    {
        await OpenProfile();

        await PreferredName.Fill("Margie");

        await Expect(DirectoryPreview).ToHaveTextAsync("Margie", new() { Timeout = 10000 });
        await Expect(NameReplacing).ToHaveTextAsync("Margaret");
        await Expect(NamePreviewHint)
            .ToHaveTextAsync("This is how staff will see the resident on the directory.");

        AssertNoConsoleErrors();
    }

    // Committing an edit proves FusionTextBoxChangeArgs delivers Value (the saved name),
    // PreviousValue (what it changed from), and IsInteracted=true routing the "edited by you"
    // branch when the coordinator typed the change themselves.
    [Test]
    public async Task finishing_a_name_edit_records_the_saved_name_and_what_it_changed_from()
    {
        await OpenProfile();

        await PreferredName.FillAndBlur("Margie");

        await Expect(LastSavedName).ToHaveTextAsync("Margie", new() { Timeout = 10000 });
        await Expect(ChangedFrom).ToHaveTextAsync("Margaret");
        await Expect(EditSource).ToHaveTextAsync("Edited by you.");

        AssertNoConsoleErrors();
    }

    // SetValue writes the legal name onto the field and the Value() source reads it back into the
    // preview; the programmatic write fires Changed with IsInteracted=false, routing the
    // "filled from the record" branch — the distinction from a hand-typed edit.
    [Test]
    public async Task filling_the_legal_name_records_it_as_filled_from_the_record_not_a_manual_edit()
    {
        await OpenProfile();

        await UseLegalNameButton.ClickAsync();

        await Expect(PreferredName.Input).ToHaveValueAsync("Margaret Whitfield", new() { Timeout = 10000 });
        await Expect(DirectoryPreview).ToHaveTextAsync("Margaret Whitfield");
        await Expect(EditSource).ToHaveTextAsync("Filled from the resident's record.");

        AssertNoConsoleErrors();
    }

    // FocusIn moves focus into the name field when the coordinator chooses to edit it.
    [Test]
    public async Task starting_an_edit_focuses_the_name_field()
    {
        await OpenProfile();

        await EditNameButton.ClickAsync();

        await Expect(PreferredName.Input).ToBeFocusedAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    // FocusOut removes focus from the name field when the coordinator marks the edit done.
    [Test]
    public async Task marking_the_edit_done_moves_focus_off_the_name_field()
    {
        await OpenProfile();

        await EditNameButton.ClickAsync();
        await Expect(PreferredName.Input).ToBeFocusedAsync(new() { Timeout = 10000 });

        await DoneEditingButton.ClickAsync();

        await Expect(PreferredName.Input).Not.ToBeFocusedAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    // INTERACTS (dietary) — focusing the dietary field fires the Focus event; FusionTextBoxFocusArgs
    // carries the Value already on file so the coordinator sees the existing note as they start.
    [Test]
    public async Task opening_the_dietary_field_shows_the_note_already_on_file()
    {
        await OpenProfile();

        await DietaryNote.Focus();

        await Expect(DietaryGuidance)
            .ToHaveTextAsync("Kitchen staff read this exactly as written. Be specific.", new() { Timeout = 10000 });
        await Expect(DietaryOnOpen).ToHaveTextAsync("No shellfish");

        AssertNoConsoleErrors();
    }

    // Leaving the dietary field fires the Blur event; FusionTextBoxBlurArgs carries the updated
    // Value, which the card confirms as the captured note for kitchen staff.
    [Test]
    public async Task leaving_the_dietary_field_captures_the_updated_note()
    {
        await OpenProfile();

        await DietaryNote.FillAndBlur("Low sodium, no shellfish");

        await Expect(DietaryGuidance)
            .ToHaveTextAsync("Dietary note captured for kitchen staff.", new() { Timeout = 10000 });
        await Expect(DietaryCaptured).ToHaveTextAsync("Low sodium, no shellfish");

        AssertNoConsoleErrors();
    }

    // The Value() source feeds the save guard: with no name, the When(...).NotEmpty() condition takes
    // the Else branch and the coordinator is asked for a name instead of a save being attempted.
    [Test]
    public async Task saving_a_profile_without_a_name_asks_for_one_first()
    {
        await OpenProfile();

        await PreferredName.Clear();
        await SaveButton.ClickAsync();

        await Expect(SaveStatus)
            .ToHaveTextAsync("Add a preferred name before saving the profile.", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    // SUBMITS — the Value() source feeds the gather body; the server confirmation the coordinator
    // sees reflects the saved preferred name.
    [Test]
    public async Task saving_the_profile_confirms_it_with_the_residents_name()
    {
        await OpenProfile();

        await PreferredName.FillAndBlur("Margie");
        await SaveButton.ClickAsync();

        await Expect(SaveConfirmation)
            .ToHaveTextAsync("Saved. Margie's profile is up to date.", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    // GATHERS — the framework gather pipeline carries both Value() sources into the POST body under
    // their declared keys. (Framework gather test: asserts request.PostData.)
    [Test]
    public async Task saving_posts_the_preferred_name_and_dietary_note_to_the_server()
    {
        await OpenProfile();

        await PreferredName.FillAndBlur("Margie");
        await DietaryNote.FillAndBlur("Low sodium, no shellfish");

        var requestTask = Page.WaitForRequestAsync(request =>
            request.Url.Contains("/Sandbox/Components/TextBox/Save") && request.Method == "POST",
            new() { Timeout = 10000 });

        await SaveButton.ClickAsync();

        var request = await requestTask;
        var body = request.PostData ?? "";

        Assert.That(body, Does.Contain("\"preferredName\":\"Margie\""),
            "the gather pipeline must carry the preferred-name Value() source under its declared key");
        Assert.That(body, Does.Contain("\"dietaryNote\":\"Low sodium, no shellfish\""),
            "the gather pipeline must carry the dietary-note Value() source under its declared key");

        AssertNoConsoleErrors();
    }
}
