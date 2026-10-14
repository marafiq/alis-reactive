using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// As the receptionist at the Maple Grove front desk, I want to sign each visitor in by name and the
/// resident they came to see, have a returning visitor's name filled in from their phone, and see the
/// badge I am about to hand over, so that the community always knows who is in the building.
/// </summary>
/// <remarks>
/// Eleanor Vance (555-0142) and Thomas Reyes (555-0178) have visited before; any other number is a first
/// visit. A text box raises change when the receptionist commits what was typed: on leaving the field, or
/// on Enter in a single-line field (HTML, the input element's change event). A value the page writes
/// raises none, so the badge reads the name the page has just filled in. Every member of the text box is
/// exercised: its value written (a returning visitor's name, the empty form after a sign-in) and read (the
/// badge, the sign-in the server answers), Focus, Change with its value, and its value committed for the
/// name check on Enter.
/// </remarks>
[TestFixture]
public class WhenTextInputChanges : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/NativeTextBox";
    private const string NotFound = "the server responded with a status of 404";

    private PagePlan<NativeTextBoxModel> _plan = null!;

    private NativeTextBoxLocator Phone => _plan.TextBox(m => m.Phone);
    private NativeTextBoxLocator VisitorName => _plan.TextBox(m => m.VisitorName);
    private NativeDropDownLocator Visiting => _plan.NativeDropDown(m => m.ResidentVisited);
    private ILocator NameError => _plan.ErrorFor(m => m.VisitorName);
    private ILocator PhoneLookup => Page.Locator("#phone-lookup");
    private ILocator Badge => Page.Locator("#badge-name");
    private ILocator SignIn => Page.Locator("#sign-in-btn");
    private ILocator Status => Page.Locator("#sign-in-status");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<NativeTextBoxModel>.FromPage(Page);
    }

    // Tabbing out of the field commits what was typed.
    private static async Task WriteAndMoveOn(NativeTextBoxLocator field, string text)
    {
        await field.Fill(text);
        await field.Input.PressAsync("Tab");
    }

    [Test]
    public async Task the_visitors_name_goes_on_the_badge_once_it_is_written()
    {
        await NavigateAndBoot();
        await Expect(Badge).ToHaveTextAsync("No name yet");

        await WriteAndMoveOn(VisitorName, "Eleanor Vance");

        await Expect(Badge).ToHaveTextAsync("Eleanor Vance");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_the_visitors_name_takes_it_off_the_badge()
    {
        await NavigateAndBoot();
        await WriteAndMoveOn(VisitorName, "Eleanor Vance");
        await Expect(Badge).ToHaveTextAsync("Eleanor Vance");

        await WriteAndMoveOn(VisitorName, "");

        await Expect(Badge).ToHaveTextAsync("No name yet");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_returning_visitors_phone_fills_in_their_name_and_their_badge()
    {
        await NavigateAndBoot();

        await WriteAndMoveOn(Phone, "555-0142");

        await Expect(PhoneLookup).ToHaveTextAsync("Welcome back: Eleanor Vance last visited Margaret Hale on 3 October.");
        await Expect(VisitorName.Input).ToHaveValueAsync("Eleanor Vance");
        await Expect(Badge).ToHaveTextAsync("Eleanor Vance");
        AssertNoConsoleErrors();
    }

    // A cleared phone is not looked up: a lookup would answer 404, a console error.
    [Test]
    public async Task clearing_the_phone_puts_back_the_returning_visitor_hint()
    {
        await NavigateAndBoot();
        await WriteAndMoveOn(Phone, "555-0142");
        await Expect(PhoneLookup).ToHaveTextAsync("Welcome back: Eleanor Vance last visited Margaret Hale on 3 October.");

        await WriteAndMoveOn(Phone, "");

        await Expect(PhoneLookup).ToHaveTextAsync("A returning visitor is found by their phone number.");
        AssertNoConsoleErrors();
    }

    // Enter commits the number without leaving the field, so only the page can move the cursor.
    [Test]
    public async Task a_first_time_visitors_phone_puts_the_cursor_in_their_name()
    {
        await NavigateAndBoot();
        await Phone.Fill("555-0199");

        await Phone.Input.PressAsync("Enter");

        await Expect(PhoneLookup).ToHaveTextAsync("No visit on file for this number. Write the visitor's name.");
        await Expect(VisitorName.Input).ToBeFocusedAsync();
        AssertNoConsoleErrorsExcept(NotFound);
    }

    [Test]
    public async Task signing_a_visitor_in_says_whom_they_went_up_to_see()
    {
        await NavigateAndBoot();
        await WriteAndMoveOn(VisitorName, "Thomas Reyes");
        await Visiting.Select("Harold Benson");

        await ClickWhenStable(SignIn);

        await Expect(Status).ToHaveTextAsync("Thomas Reyes is signed in to visit Harold Benson.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task signing_a_visitor_in_clears_the_form_and_the_badge_for_the_next_visitor()
    {
        await NavigateAndBoot();
        await WriteAndMoveOn(Phone, "555-0142");
        await Expect(VisitorName.Input).ToHaveValueAsync("Eleanor Vance");
        await Visiting.Select("Margaret Hale");

        await ClickWhenStable(SignIn);

        await Expect(Status).ToHaveTextAsync("Eleanor Vance is signed in to visit Margaret Hale.");
        await Expect(Phone.Input).ToHaveValueAsync("");
        await Expect(VisitorName.Input).ToHaveValueAsync("");
        await Expect(Visiting.Input).ToHaveValueAsync("");
        await Expect(Badge).ToHaveTextAsync("No name yet");
        AssertNoConsoleErrors();
    }

    // The click leaves the cursor on the button; the signed-in answer moves it.
    [Test]
    public async Task after_a_sign_in_the_cursor_waits_in_the_phone_for_the_next_visitor()
    {
        await NavigateAndBoot();
        await WriteAndMoveOn(VisitorName, "Thomas Reyes");
        await Visiting.Select("Harold Benson");

        await ClickWhenStable(SignIn);

        await Expect(Status).ToHaveTextAsync("Thomas Reyes is signed in to visit Harold Benson.");
        await Expect(Phone.Input).ToBeFocusedAsync();
        AssertNoConsoleErrors();
    }

    // Refused in the browser: a sign-in sent to the server would answer 400, a console error.
    [Test]
    public async Task signing_in_without_a_name_says_the_name_is_needed()
    {
        await NavigateAndBoot();
        await Visiting.Select("Margaret Hale");

        await ClickWhenStable(SignIn);

        await Expect(NameError).ToHaveTextAsync("Write the visitor's name.");
        await Expect(Status).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task writing_the_name_clears_its_refusal_without_signing_in_again()
    {
        await NavigateAndBoot();
        await ClickWhenStable(SignIn);
        await Expect(NameError).ToHaveTextAsync("Write the visitor's name.");

        await WriteAndMoveOn(VisitorName, "Eleanor Vance");

        await Expect(NameError).ToBeHiddenAsync();
        await Expect(Status).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    // Enter commits the name without leaving the field, so the check runs on the commit, not on leaving.
    [Test]
    public async Task a_name_without_a_last_name_is_refused_as_soon_as_enter_is_pressed()
    {
        await NavigateAndBoot();
        await VisitorName.Fill("Eleanor");

        await VisitorName.Input.PressAsync("Enter");

        await Expect(NameError).ToHaveTextAsync("Write the visitor's first and last name.");
        await Expect(VisitorName.Input).ToBeFocusedAsync();
        AssertNoConsoleErrors();
    }
}
