using System.Collections.Concurrent;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.InputMask;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.InputMask;

/// <summary>
/// As the front desk, I want to enter Margaret Hale's family phone numbers in the shape the desk dials them and
/// have a number refused until all ten digits are in, so that the community can always reach her family.
/// </summary>
/// <remarks>
/// Both fields mask "(000) 000-0000"; their value is the ten digits alone. The browser checks the rules of
/// <c>FamilyPhoneNumbersValidator</c> each time a field commits (the mask commits on every key, mask-base.js:734)
/// and on Save; the server checks them again. Ticking "Daniel is also her emergency contact" copies his number
/// into the emergency field; a copy raises the field's Change with IsInteracted false (mask-base.js:741-761,
/// 1083), a key the desk presses raises it with IsInteracted true. The desk reaches a field by its label and
/// types over what it holds (<see cref="InputMaskLocator.Type"/>): typing from wherever the caret was left
/// loses every key when Chromium restores it after the mask's last blank on a label click.
/// A refused Save sends nothing: each refusal test checks no save request left the page.
/// </remarks>
[TestFixture]
public class WhenMaskedInputEntered : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/InputMask";

    private PagePlan<InputMaskModel> _plan = null!;

    private InputMaskLocator FamilyPhone => _plan.InputMask(m => m.FamilyPhone);
    private InputMaskLocator EmergencyPhone => _plan.InputMask(m => m.EmergencyPhone);
    private ILocator SamePerson => _plan.NativeCheckBox(m => m.EmergencyIsFamily).Input;
    private ILocator Copied => Page.Locator("#emergency-copied");
    private ILocator SaveNumbers => Page.Locator("#save-phones-btn");
    private ILocator Status => Page.Locator("#phones-status");

    private ConcurrentQueue<string> _savesSent = null!;

    private async Task NavigateAndBoot()
    {
        _savesSent = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains("/Sandbox/Components/InputMask/Save")) _savesSent.Enqueue(request.Url);
        };
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<InputMaskModel>.FromPage(Page);
        await Expect(FamilyPhone.Input).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    private async Task TypeDigits(InputMaskLocator field, string digits)
    {
        var id = await field.Input.GetAttributeAsync("id");
        await Page.Locator($"label[for='{id}']").ClickAsync();
        await field.Type(digits);
    }

    [Test]
    public async Task a_number_is_refused_while_its_ten_digits_are_not_all_in()
    {
        await NavigateAndBoot();

        await TypeDigits(FamilyPhone, "617555");

        await Expect(FamilyPhone.Input).ToHaveValueAsync("(617) 555-____");
        await Expect(_plan.ErrorFor(m => m.FamilyPhone))
            .ToHaveTextAsync("Enter all ten digits of the family member's phone number.", new() { Timeout = 5000 });
        await Expect(_plan.ErrorFor(m => m.FamilyPhone)).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task typing_the_last_digits_clears_the_refusal()
    {
        await NavigateAndBoot();
        await TypeDigits(FamilyPhone, "617555");
        await Expect(_plan.ErrorFor(m => m.FamilyPhone)).ToBeVisibleAsync(new() { Timeout = 5000 });

        await FamilyPhone.Input.PressSequentiallyAsync("0142", new() { Delay = 20 });

        await Expect(FamilyPhone.Input).ToHaveValueAsync("(617) 555-0142");
        await Expect(_plan.ErrorFor(m => m.FamilyPhone)).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // The family phone's message is already on screen as it is typed; the emergency phone's, left empty, comes
    // only from the Save, so it shows the Save was handled before the requests are counted.
    [Test]
    public async Task saving_with_an_unfinished_family_phone_is_refused()
    {
        await NavigateAndBoot();
        await TypeDigits(FamilyPhone, "617555");

        await ClickWhenStable(SaveNumbers);

        await Expect(_plan.ErrorFor(m => m.EmergencyPhone))
            .ToHaveTextAsync("Enter the emergency contact's phone number.", new() { Timeout = 5000 });
        await Expect(_plan.ErrorFor(m => m.EmergencyPhone)).ToBeVisibleAsync();
        await Expect(_plan.ErrorFor(m => m.FamilyPhone))
            .ToHaveTextAsync("Enter all ten digits of the family member's phone number.");
        await Expect(_plan.ErrorFor(m => m.FamilyPhone)).ToBeVisibleAsync();
        await Expect(Status).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "Numbers the page can see are unfinished are not sent.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_without_the_emergency_phone_asks_for_it()
    {
        await NavigateAndBoot();
        await TypeDigits(FamilyPhone, "6175550142");

        await ClickWhenStable(SaveNumbers);

        await Expect(_plan.ErrorFor(m => m.EmergencyPhone))
            .ToHaveTextAsync("Enter the emergency contact's phone number.", new() { Timeout = 5000 });
        await Expect(_plan.ErrorFor(m => m.EmergencyPhone)).ToBeVisibleAsync();
        await Expect(_plan.ErrorFor(m => m.FamilyPhone)).ToBeHiddenAsync();
        await Expect(Status).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "Numbers the page can see are missing are not sent.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task both_numbers_complete_are_saved_as_the_desk_dials_them()
    {
        await NavigateAndBoot();
        await TypeDigits(FamilyPhone, "6175550142");
        await TypeDigits(EmergencyPhone, "6175550199");

        await ClickWhenStable(SaveNumbers);

        await Expect(Status).ToHaveTextAsync(
            "Saved Margaret Hale's family phone, (617) 555-0142, and her emergency contact's, (617) 555-0199.",
            new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_same_person_copies_the_family_phone_into_the_emergency_phone()
    {
        await NavigateAndBoot();
        await TypeDigits(FamilyPhone, "6175550142");

        await SamePerson.CheckAsync();

        await Expect(EmergencyPhone.Input).ToHaveValueAsync("(617) 555-0142", new() { Timeout = 5000 });
        await Expect(Copied).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_copied_number_is_saved_as_the_emergency_contacts()
    {
        await NavigateAndBoot();
        await TypeDigits(FamilyPhone, "6175550142");
        await SamePerson.CheckAsync();
        await Expect(EmergencyPhone.Input).ToHaveValueAsync("(617) 555-0142", new() { Timeout = 5000 });

        await ClickWhenStable(SaveNumbers);

        await Expect(Status).ToHaveTextAsync(
            "Saved Margaret Hale's family phone, (617) 555-0142, and her emergency contact's, (617) 555-0142.",
            new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task typing_over_the_copied_number_no_longer_says_it_was_copied()
    {
        await NavigateAndBoot();
        await TypeDigits(FamilyPhone, "6175550142");
        await SamePerson.CheckAsync();
        await Expect(Copied).ToBeVisibleAsync(new() { Timeout = 5000 });

        await TypeDigits(EmergencyPhone, "6175550199");

        await Expect(EmergencyPhone.Input).ToHaveValueAsync("(617) 555-0199");
        await Expect(Copied).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task another_emergency_contact_clears_the_copied_number_and_puts_the_cursor_there()
    {
        await NavigateAndBoot();
        await TypeDigits(FamilyPhone, "6175550142");
        await SamePerson.CheckAsync();
        await Expect(EmergencyPhone.Input).ToHaveValueAsync("(617) 555-0142", new() { Timeout = 5000 });

        await SamePerson.UncheckAsync();

        await Expect(EmergencyPhone.Input).ToBeFocusedAsync(new() { Timeout = 5000 });
        await Expect(EmergencyPhone.Input).ToHaveValueAsync("(___) ___-____");
        await Expect(Copied).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }
}
