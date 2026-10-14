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
/// 1083), a key the desk presses raises it with IsInteracted true.
/// Digits are typed after a click on the field's label: a click on the input itself makes EJ2 move the caret to
/// the first blank 110 ms later (mask-base.js:350-369), which would scatter digits typed in that window.
/// </remarks>
[TestFixture]
public class WhenMaskedInputEntered : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/InputMask";

    private PagePlan<InputMaskModel> _plan = null!;

    private ILocator FamilyPhone => _plan.InputMask(m => m.FamilyPhone).Input;
    private ILocator EmergencyPhone => _plan.InputMask(m => m.EmergencyPhone).Input;
    private ILocator SamePerson => _plan.NativeCheckBox(m => m.EmergencyIsFamily).Input;
    private ILocator Copied => Page.Locator("#emergency-copied");
    private ILocator SaveNumbers => Page.Locator("#save-phones-btn");
    private ILocator Status => Page.Locator("#phones-status");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<InputMaskModel>.FromPage(Page);
        await Expect(FamilyPhone).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    private async Task TypeDigits(ILocator field, string digits)
    {
        var id = await field.GetAttributeAsync("id");
        await Page.Locator($"label[for='{id}']").ClickAsync();
        await field.PressSequentiallyAsync(digits, new() { Delay = 20 });
    }

    [Test]
    public async Task a_number_is_refused_while_its_ten_digits_are_not_all_in()
    {
        await NavigateAndBoot();

        await TypeDigits(FamilyPhone, "617555");

        await Expect(FamilyPhone).ToHaveValueAsync("(617) 555-____");
        await Expect(_plan.ErrorFor(m => m.FamilyPhone))
            .ToHaveTextAsync("Enter all ten digits of the family member's phone number.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task typing_the_last_digits_clears_the_refusal()
    {
        await NavigateAndBoot();
        await TypeDigits(FamilyPhone, "617555");
        await Expect(_plan.ErrorFor(m => m.FamilyPhone)).ToBeVisibleAsync(new() { Timeout = 5000 });

        await FamilyPhone.PressSequentiallyAsync("0142", new() { Delay = 20 });

        await Expect(FamilyPhone).ToHaveValueAsync("(617) 555-0142");
        await Expect(_plan.ErrorFor(m => m.FamilyPhone)).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_with_an_unfinished_family_phone_is_refused()
    {
        await NavigateAndBoot();
        await TypeDigits(FamilyPhone, "617555");
        await TypeDigits(EmergencyPhone, "6175550199");

        await ClickWhenStable(SaveNumbers);

        await Expect(_plan.ErrorFor(m => m.FamilyPhone))
            .ToHaveTextAsync("Enter all ten digits of the family member's phone number.", new() { Timeout = 5000 });
        await Expect(Status).ToHaveTextAsync("Not saved yet.");
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
        await Expect(_plan.ErrorFor(m => m.FamilyPhone)).ToBeHiddenAsync();
        await Expect(Status).ToHaveTextAsync("Not saved yet.");
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

        await Expect(EmergencyPhone).ToHaveValueAsync("(617) 555-0142", new() { Timeout = 5000 });
        await Expect(Copied).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_copied_number_is_saved_as_the_emergency_contacts()
    {
        await NavigateAndBoot();
        await TypeDigits(FamilyPhone, "6175550142");
        await SamePerson.CheckAsync();
        await Expect(EmergencyPhone).ToHaveValueAsync("(617) 555-0142", new() { Timeout = 5000 });

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

        var id = await EmergencyPhone.GetAttributeAsync("id");
        await Page.Locator($"label[for='{id}']").ClickAsync();
        await Page.Keyboard.PressAsync("ControlOrMeta+a");
        await Page.Keyboard.PressAsync("Backspace");
        await EmergencyPhone.PressSequentiallyAsync("6175550199", new() { Delay = 20 });

        await Expect(EmergencyPhone).ToHaveValueAsync("(617) 555-0199");
        await Expect(Copied).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task another_emergency_contact_clears_the_copied_number_and_puts_the_cursor_there()
    {
        await NavigateAndBoot();
        await TypeDigits(FamilyPhone, "6175550142");
        await SamePerson.CheckAsync();
        await Expect(EmergencyPhone).ToHaveValueAsync("(617) 555-0142", new() { Timeout = 5000 });

        await SamePerson.UncheckAsync();

        await Expect(EmergencyPhone).ToBeFocusedAsync(new() { Timeout = 5000 });
        await Expect(EmergencyPhone).ToHaveValueAsync("(___) ___-____");
        await Expect(Copied).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }
}
