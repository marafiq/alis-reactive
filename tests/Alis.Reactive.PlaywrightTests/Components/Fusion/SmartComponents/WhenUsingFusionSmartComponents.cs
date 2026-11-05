using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.SmartComponents;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.SmartComponents;

[TestFixture]
public class WhenUsingFusionSmartComponents : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/SmartComponents";
    private const string Template = "Referral reviewed with the family. ";
    private const string LakesideLetter =
        "Lakeside Rehabilitation Center\n"
        + "Patient: Ruth Carter\n"
        + "Referring physician: Dr. Priya Shah\n"
        + "Level of care: Assisted Living\n"
        + "Reason for referral: Needs help with bathing and medications after a stroke.\n";

    private PagePlan<SmartComponentsModel> _plan = null!;

    private ILocator Resident => _plan.TextBox(m => m.ResidentName).Input;
    private ILocator ReferredBy => _plan.TextBox(m => m.ReferredBy).Input;
    private ILocator CareLevel => _plan.NativeDropDown(m => m.CareLevel).Input;
    private ILocator Reason => _plan.TextBox(m => m.Reason).Input;
    private ILocator Note => _plan.TextBox(m => m.IntakeNote).Input;
    private ILocator Letter => Page.Locator("#custom-clipboard");
    private ILocator FillInFromTheLetter => Page.Locator("#referral-smart-paste");
    private ILocator StartFromTheTemplate => Page.Locator("#start-from-template-btn");
    private ILocator SaveReferral => Page.Locator("#save-referral-btn");
    private ILocator NoteStatus => Page.Locator("#note-status");
    private ILocator IntakeStatus => Page.Locator("#intake-status");
    private ILocator SuggestionAdded => Page.Locator("#suggestion-added");
    private ILocator DoseLeftOut => Page.Locator("#dose-left-out");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<SmartComponentsModel>.FromPage(Page);
        await Expect(FillInFromTheLetter).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("e-control"), new() { Timeout = 10000 });
    }

    private async Task OpenMargaretHalesReferral()
    {
        await _plan.NativeDropDown(m => m.IncomingReferral).Select("St. Mary's Hospital: Margaret Hale");
        await Expect(Resident).ToHaveValueAsync("Margaret Hale", new() { Timeout = 10000 });
    }

    private async Task WriteInTheNote(string text)
    {
        await Note.ClickAsync();
        await Note.PressSequentiallyAsync(text, new() { Delay = 20 });
    }

    [Test]
    public async Task opening_a_referral_from_the_inbox_fills_in_the_form_from_its_letter()
    {
        await NavigateAndBoot();

        await _plan.NativeDropDown(m => m.IncomingReferral).Select("St. Mary's Hospital: Margaret Hale");

        await Expect(Resident).ToHaveValueAsync("Margaret Hale", new() { Timeout = 10000 });
        await Expect(ReferredBy).ToHaveValueAsync("Dr. Lena Ortiz");
        await Expect(CareLevel).ToHaveValueAsync("Memory Care");
        await Expect(Reason).ToHaveValueAsync("Confusion at night since her hip surgery; she needs a secure unit.");
        await Expect(Letter).ToHaveValueAsync(new System.Text.RegularExpressions.Regex("^St\\. Mary's Hospital, Discharge Planning"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_letter_pasted_by_hand_fills_in_the_form()
    {
        await NavigateAndBoot();
        await Letter.FillAsync(LakesideLetter);

        await ClickWhenStable(FillInFromTheLetter);

        await Expect(Resident).ToHaveValueAsync("Ruth Carter", new() { Timeout = 10000 });
        await Expect(ReferredBy).ToHaveValueAsync("Dr. Priya Shah");
        await Expect(CareLevel).ToHaveValueAsync("Assisted Living");
        await Expect(Reason).ToHaveValueAsync("Needs help with bathing and medications after a stroke.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_before_the_form_is_filled_in_points_to_fill_in_from_the_letter()
    {
        await NavigateAndBoot();

        await ClickWhenStable(SaveReferral);

        await Expect(IntakeStatus).ToHaveTextAsync("Fill in the referral from its letter before saving.", new() { Timeout = 5000 });
        await Expect(FillInFromTheLetter).ToBeFocusedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_the_referral_files_it_with_the_intake_note_and_locks_fill_in_from_the_letter()
    {
        await NavigateAndBoot();
        await OpenMargaretHalesReferral();
        await WriteInTheNote("Daughter visits on Sundays.");

        await ClickWhenStable(SaveReferral);

        await Expect(IntakeStatus).ToHaveTextAsync(
            "Saved Margaret Hale's referral for Memory Care with a 4-word intake note.", new() { Timeout = 5000 });
        await Expect(FillInFromTheLetter).ToBeDisabledAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_a_saved_referral_again_says_it_is_already_saved()
    {
        await NavigateAndBoot();
        await OpenMargaretHalesReferral();
        await ClickWhenStable(SaveReferral);
        await Expect(IntakeStatus).ToHaveTextAsync(
            "Saved Margaret Hale's referral for Memory Care with no intake note.", new() { Timeout = 5000 });

        await ClickWhenStable(SaveReferral);

        await Expect(IntakeStatus).ToHaveTextAsync("This referral is already saved.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task starting_from_the_template_puts_the_cursor_after_it()
    {
        await NavigateAndBoot();

        await ClickWhenStable(StartFromTheTemplate);

        await Expect(Note).ToHaveValueAsync(Template, new() { Timeout = 5000 });
        await Expect(Note).ToBeFocusedAsync();
        await Expect(NoteStatus).ToHaveTextAsync("Started from the template.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task writing_a_note_from_scratch_says_it_was_written()
    {
        await NavigateAndBoot();
        await WriteInTheNote("Daughter visits on Sundays.");

        await Page.Keyboard.PressAsync("Tab");

        await Expect(NoteStatus).ToHaveTextAsync("Intake note written.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task adding_to_the_template_says_the_note_was_updated()
    {
        await NavigateAndBoot();
        await ClickWhenStable(StartFromTheTemplate);
        await Expect(Note).ToBeFocusedAsync(new() { Timeout = 5000 });
        await Note.PressSequentiallyAsync("Daughter visits on Sundays.", new() { Delay = 20 });

        await Page.Keyboard.PressAsync("Tab");

        await Expect(Note).ToHaveValueAsync(Template + "Daughter visits on Sundays.");
        await Expect(NoteStatus).ToHaveTextAsync("Intake note updated.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task erasing_the_note_says_it_was_cleared()
    {
        await NavigateAndBoot();
        await WriteInTheNote("Daughter visits on Sundays.");
        await Page.Keyboard.PressAsync("Tab");
        await Expect(NoteStatus).ToHaveTextAsync("Intake note written.", new() { Timeout = 5000 });

        await Note.ClickAsync();
        await Page.Keyboard.PressAsync("ControlOrMeta+a");
        await Page.Keyboard.PressAsync("Backspace");
        await Page.Keyboard.PressAsync("Tab");

        await Expect(NoteStatus).ToHaveTextAsync("Intake note cleared.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task tab_adds_the_suggested_phrase_to_the_note()
    {
        await NavigateAndBoot();
        await WriteInTheNote("Family");
        await Expect(Note).ToHaveValueAsync("Family tour scheduled for Thursday ", new() { Timeout = 10000 });

        await Note.PressAsync("Tab");

        await Expect(SuggestionAdded).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#suggestion-added-text")).ToHaveTextAsync("tour scheduled for Thursday");
        await Expect(Note).ToHaveValueAsync("Family tour scheduled for Thursday ");
        await Expect(Note).ToBeFocusedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_suggested_dose_is_left_out_of_the_note()
    {
        await NavigateAndBoot();

        await WriteInTheNote("Started on metformin");

        await Expect(DoseLeftOut).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Note).ToHaveValueAsync("Started on metformin");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_suggested_phrase_after_a_left_out_dose_takes_the_dose_warning_down()
    {
        await NavigateAndBoot();
        await WriteInTheNote("Started on metformin");
        await Expect(DoseLeftOut).ToBeVisibleAsync(new() { Timeout = 10000 });

        await Note.PressSequentiallyAsync(", spoke with the family", new() { Delay = 20 });

        await Expect(Note).ToHaveValueAsync("Started on metformin, spoke with the family tour scheduled for Thursday ", new() { Timeout = 10000 });
        await Expect(DoseLeftOut).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }
}
