using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.TextBox;

[TestFixture]
public class WhenUsingFusionTextBox : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/TextBox";
    private static readonly string PreferredNameId = IdGenerator.For<TextBoxModel>(m => m.PreferredName);
    private static readonly string DietaryNoteId = IdGenerator.For<TextBoxModel>(m => m.DietaryNote);

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

    [Test]
    public async Task profile_opens_showing_the_name_on_file_with_a_directory_search_affordance()
    {
        await OpenProfile();

        await Expect(PreferredName.Input).ToHaveValueAsync("Margaret", new() { Timeout = 10000 });
        await Expect(DirectoryPreview).ToHaveTextAsync("Margaret");
        await Expect(PreferredName.Wrapper.Locator(".e-icons.e-search")).ToHaveCountAsync(1);

        AssertNoConsoleErrors();
    }

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

    [Test]
    public async Task starting_an_edit_focuses_the_name_field()
    {
        await OpenProfile();

        await EditNameButton.ClickAsync();

        await Expect(PreferredName.Input).ToBeFocusedAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

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

        Assert.That(body, Does.Contain("\"PreferredName\":\"Margie\""),
            "the gather pipeline must carry the preferred-name Value() source under its declared key");
        Assert.That(body, Does.Contain("\"DietaryNote\":\"Low sodium, no shellfish\""),
            "the gather pipeline must carry the dietary-note Value() source under its declared key");

        AssertNoConsoleErrors();
    }
}
