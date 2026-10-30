using System.Text.RegularExpressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.SplitButton;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.SplitButton;

/// <summary>
/// As a care coordinator, I want to write a care note, sign it, and save it to the resident's chart, telling her
/// family or the nurse in the same step when it matters, so that a family hears about a fall the day it happens.
/// </summary>
/// <remarks>
/// The note is for Margaret Hale in Maple 214; her daughter Susan Hale gets the family's texts. Save stays
/// locked until the coordinator signs the note; signing unlocks it and puts the keyboard on it. A disabled
/// button takes no focus, so the unlock is applied with DataBind before FocusIn; without DataBind the button
/// is still disabled when FocusIn runs and never gets the keyboard. A new note about a fall (Save still reads
/// Save note) is never saved quietly: Save opens its menu on the family's option instead.
/// Not exercised: reading Disabled and CssClass back (no step here needs the button's own state or classes;
/// the writes below prove the same JavaScript members); Toggle closing an open menu (every gesture that
/// reaches the page while the menu is open closes it on its own, a click outside or on an item); RemoveItems
/// by text (the same EJ2 removeItems as by ID, differing only in the field it compares; exercised by text on
/// the DropDownButton page); Select's Item.Disabled (a disabled item cannot be clicked or reached by the arrow
/// keys, so every Select the coordinator can raise carries false and nothing on the page could differ).
/// </remarks>
[TestFixture]
public class WhenUsingFusionSplitButton : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionSplitButton";
    private const string OnTheChart = "Saved to Margaret Hale's chart.";
    private const string WhatHappened = "Helped Margaret shower and dress before breakfast.";

    private PagePlan<FusionSplitButtonModel> _plan = null!;

    private FusionSplitButtonLocator Save => new(Page, "care-note-save");
    private ILocator Note => Page.Locator($"#{_plan.FindComponent(m => m.Note)!.ElementId}");
    private ILocator Status => Page.Locator("#care-note-status");
    private ILocator LastAction => Page.Locator("#care-note-last-action-line");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<FusionSplitButtonModel>.FromPage(Page);
        await Expect(Save.Primary).ToHaveTextAsync("Save note", new() { Timeout = 5000 });
    }

    private async Task Sign()
    {
        await _plan.NativeCheckBox(m => m.Signed).Check();
        await Expect(Save.Primary).ToBeEnabledAsync(new() { Timeout = 5000 });
    }

    private async Task WriteAndSign(string whatHappened)
    {
        await Note.FillAsync(whatHappened);
        await Sign();
    }

    [Test]
    public async Task save_is_locked_until_the_note_is_signed()
    {
        await NavigateAndBoot();

        await Expect(Save.Primary).ToBeDisabledAsync();
        await Expect(Save.Secondary).ToBeDisabledAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task signing_the_note_unlocks_save_with_the_keyboard_on_it()
    {
        await NavigateAndBoot();

        await _plan.NativeCheckBox(m => m.Signed).Check();

        await Expect(Save.Primary).ToBeEnabledAsync(new() { Timeout = 5000 });
        await Expect(Save.Primary).ToBeFocusedAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task taking_the_signature_back_locks_save_again()
    {
        await NavigateAndBoot();
        await Sign();

        await _plan.NativeCheckBox(m => m.Signed).Input.UncheckAsync();

        await Expect(Save.Primary).ToBeDisabledAsync(new() { Timeout = 5000 });
        await Expect(Save.Secondary).ToBeDisabledAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_a_signed_note_puts_it_on_the_chart()
    {
        await NavigateAndBoot();
        await WriteAndSign(WhatHappened);

        await ClickWhenStable(Save.Primary);

        await Expect(Status).ToHaveTextAsync(OnTheChart, new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_saved_note_is_saved_again_as_changes()
    {
        await NavigateAndBoot();
        await WriteAndSign(WhatHappened);

        await ClickWhenStable(Save.Primary);

        await Expect(Save.Primary).ToHaveTextAsync("Save changes", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_empty_note_is_not_saved()
    {
        await NavigateAndBoot();
        await Sign();

        await ClickWhenStable(Save.Primary);

        await Expect(Status).ToHaveTextAsync("Write what happened before saving.", new() { Timeout = 5000 });
        await Expect(Save.Primary).ToHaveTextAsync("Save note");
        AssertNoConsoleErrors();
    }

    // Save itself never opens the menu; only the page does.
    [Test]
    public async Task saving_a_fall_opens_the_save_menu_on_the_familys_option()
    {
        await NavigateAndBoot();
        await _plan.NativeDropDown(m => m.Kind).Select("Fall");
        await WriteAndSign("Margaret slipped getting out of bed at 2 PM. No injury.");

        await ClickWhenStable(Save.Primary);

        await Expect(Status).ToHaveTextAsync("A fall is shared with the family. Choose Save and notify family.", new() { Timeout = 5000 });
        await Expect(Save.Item("Save and notify family")).ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_note_about_a_fall_marks_the_save_button()
    {
        await NavigateAndBoot();

        await _plan.NativeDropDown(m => m.Kind).Select("Fall");

        await Expect(Save.Primary).ToHaveClassAsync(new Regex(@"\be-warning\b"), new() { Timeout = 5000 });
        await Expect(Save.Primary).Not.ToHaveClassAsync(new Regex(@"\be-primary\b"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task save_and_notify_family_says_the_family_was_texted()
    {
        await NavigateAndBoot();
        await WriteAndSign(WhatHappened);

        await Save.Choose("Save and notify family");

        await Expect(Status).ToHaveTextAsync(OnTheChart + " Susan Hale, her daughter, was sent a text.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_last_action_names_the_save_chosen_from_the_menu()
    {
        await NavigateAndBoot();
        await WriteAndSign(WhatHappened);

        await Save.Choose("Save and flag for the nurse");

        await Expect(Status).ToHaveTextAsync(OnTheChart + " Nurse Okafor will review it.", new() { Timeout = 5000 });
        await Expect(LastAction).ToHaveTextAsync("Last action: Save and flag for the nurse");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task once_the_family_is_notified_the_menu_no_longer_offers_it()
    {
        await NavigateAndBoot();
        await WriteAndSign(WhatHappened);
        await Save.Choose("Save and notify family");
        await Expect(Status).ToHaveTextAsync(OnTheChart + " Susan Hale, her daughter, was sent a text.", new() { Timeout = 5000 });

        await ClickWhenStable(Save.Secondary);

        await Expect(Save.Item("Save and flag for the nurse")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Save.Item("Save and notify family")).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }
}
