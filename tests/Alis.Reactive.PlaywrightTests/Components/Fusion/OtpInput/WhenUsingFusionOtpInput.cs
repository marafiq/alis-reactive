using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.OtpInput;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.OtpInput;

/// <summary>
/// As a front-desk receptionist, I want to check each visitor in by the 4-digit code we texted them, fix a
/// digit I typed wrong, and give a code to a visitor who did not book, so that nobody goes past the desk
/// without being matched to a visit.
/// </summary>
/// <remarks>
/// Today's one booked code is 4821: Daniel O'Brien visiting Margaret Hale in Maple 214. A walk-in code is
/// 7305. A code the desk types is checked once its four digits are in; a code the page writes is not, and
/// neither is it hinted at as a correction. Showing a code moves focus into the fields (EJ2 focuses them
/// whenever its value changes), so the walk-in code is applied at once with DataBind and the fields are then
/// let go of; without DataBind the code lands on EJ2's next tick, after FocusOut, and leaves the last field
/// focused.
/// Not exercised: the field's form input (<c>IInputComponent.Input</c>, which Form() gathers and validation
/// reads); this page sends the code by reading Value, as the page before it did.
/// </remarks>
[TestFixture]
public class WhenUsingFusionOtpInput : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/OtpInput";
    private const string AskForTheCode = "Ask the visitor to read the code from their text message.";
    private const string DanielCheckedIn = "Daniel O'Brien is checked in to visit Margaret Hale in Maple 214.";

    private PagePlan<OtpInputModel> _plan = null!;

    private FusionOtpInputLocator VisitCode => new(Page, _plan.FindComponent(m => m.VisitCode)!.ElementId);
    private ILocator Hint => Page.Locator("#code-hint");
    private ILocator Status => Page.Locator("#visit-status");
    private ILocator IssueWalkInCode => Page.Locator("#issue-walk-in-code-btn");
    private ILocator NextVisitor => Page.Locator("#next-visitor-btn");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<OtpInputModel>.FromPage(Page);
        await Expect(VisitCode.Field(3)).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    // A code no visit matches, typed and answered.
    private async Task TypeARefusedCode()
    {
        await VisitCode.TypeCode("4812");
        await Expect(Status).ToHaveTextAsync("No visit today matches 4812.", new() { Timeout = 5000 });
    }

    // The receptionist clicks elsewhere on the page.
    private async Task ClickAwayFromTheCode() => await Status.ClickAsync();

    [Test]
    public async Task the_code_is_ready_to_type_as_soon_as_the_page_opens()
    {
        await NavigateAndBoot();

        await Expect(VisitCode.Field(0)).ToBeFocusedAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task each_digit_typed_is_named_as_it_goes_in()
    {
        await NavigateAndBoot();

        await VisitCode.TypeCode("48");

        await Expect(Hint).ToHaveTextAsync("The second digit is in.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_fourth_digit_is_named_as_it_goes_in_not_taken_for_a_correction()
    {
        await NavigateAndBoot();

        await VisitCode.TypeCode("4821");

        await Expect(Hint).ToHaveTextAsync("The fourth digit is in.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task backspace_on_the_only_digit_asks_for_the_code_again()
    {
        await NavigateAndBoot();
        await VisitCode.TypeCode("4");
        await Expect(Hint).ToHaveTextAsync("The first digit is in.", new() { Timeout = 5000 });

        await Page.Keyboard.PressAsync("Backspace");

        await Expect(Hint).ToHaveTextAsync(AskForTheCode, new() { Timeout = 5000 });
        await Expect(VisitCode.Field(0)).ToHaveValueAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_booked_code_checks_the_visitor_in()
    {
        await NavigateAndBoot();

        await VisitCode.TypeCode("4821");

        await Expect(Status).ToHaveTextAsync(DanielCheckedIn, new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // The last field keeps focus after its digit is typed; only the page lets go of it.
    [Test]
    public async Task the_code_fields_let_go_once_a_booked_code_is_typed()
    {
        await NavigateAndBoot();

        await VisitCode.TypeCode("4821");

        await Expect(Status).ToHaveTextAsync(DanielCheckedIn, new() { Timeout = 5000 });
        await Expect(VisitCode.Field(3)).Not.ToBeFocusedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_code_with_a_letter_in_it_is_refused_before_it_is_checked()
    {
        await NavigateAndBoot();

        await VisitCode.TypeCode("48B1");

        await Expect(Status).ToHaveTextAsync("Visit codes are digits only.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_code_no_visit_matches_is_refused()
    {
        await NavigateAndBoot();

        await VisitCode.TypeCode("4812");

        await Expect(Status).ToHaveTextAsync("No visit today matches 4812.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_a_digit_of_a_refused_code_offers_to_correct_that_digit()
    {
        await NavigateAndBoot();
        await TypeARefusedCode();

        await VisitCode.Field(2).ClickAsync();

        await Expect(Hint).ToHaveTextAsync("Correcting the third digit.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_away_from_a_refused_code_left_as_it_was_asks_for_the_code_again()
    {
        await NavigateAndBoot();
        await TypeARefusedCode();
        await VisitCode.Field(1).ClickAsync();
        await Expect(Hint).ToHaveTextAsync("Correcting the second digit.", new() { Timeout = 5000 });

        await ClickAwayFromTheCode();

        await Expect(Hint).ToHaveTextAsync(AskForTheCode, new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task correcting_a_digit_says_the_code_was_corrected()
    {
        await NavigateAndBoot();
        await TypeARefusedCode();

        await VisitCode.Field(3).FillAsync("3");

        await Expect(Hint).ToHaveTextAsync("Code corrected.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_second_code_no_visit_matches_asks_the_desk_to_have_the_text_read_again()
    {
        await NavigateAndBoot();
        await TypeARefusedCode();

        await VisitCode.Field(3).FillAsync("3");

        await Expect(Status).ToHaveTextAsync(
            "No visit today matches 4813 either. Ask the visitor to read the text message again.",
            new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_away_from_a_half_typed_code_says_where_the_desk_stopped()
    {
        await NavigateAndBoot();
        await VisitCode.TypeCode("48");
        await Expect(Hint).ToHaveTextAsync("The second digit is in.", new() { Timeout = 5000 });

        await ClickAwayFromTheCode();

        await Expect(Hint).ToHaveTextAsync("The code is unfinished; you stopped at the third digit.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // Once the fourth digit shows, the code has been applied and the page has let go of the fields.
    [Test]
    public async Task a_walk_in_code_is_shown_and_the_code_fields_let_go_of_it()
    {
        await NavigateAndBoot();

        await ClickWhenStable(IssueWalkInCode);

        await Expect(VisitCode.Field(0)).ToHaveValueAsync("7", new() { Timeout = 5000 });
        await Expect(VisitCode.Field(1)).ToHaveValueAsync("3");
        await Expect(VisitCode.Field(2)).ToHaveValueAsync("0");
        await Expect(VisitCode.Field(3)).ToHaveValueAsync("5");
        await Expect(VisitCode.Field(3)).Not.ToBeFocusedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_walk_in_code_is_not_checked_against_todays_visits()
    {
        await NavigateAndBoot();

        await ClickWhenStable(IssueWalkInCode);

        await Expect(VisitCode.Field(3)).ToHaveValueAsync("5", new() { Timeout = 5000 });
        await Expect(Status).ToHaveTextAsync("Walk-in code issued. It opens the front door today only.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_walk_in_code_tells_the_desk_to_read_it_to_the_visitor()
    {
        await NavigateAndBoot();

        await ClickWhenStable(IssueWalkInCode);

        await Expect(VisitCode.Field(3)).ToHaveValueAsync("5", new() { Timeout = 5000 });
        await Expect(Hint).ToHaveTextAsync("Read the code to the visitor.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task next_visitor_clears_the_code_and_the_last_check_in()
    {
        await NavigateAndBoot();
        await VisitCode.TypeCode("4821");
        await Expect(Status).ToHaveTextAsync(DanielCheckedIn, new() { Timeout = 5000 });

        await ClickWhenStable(NextVisitor);

        await Expect(VisitCode.Field(0)).ToHaveValueAsync("", new() { Timeout = 5000 });
        await Expect(VisitCode.Field(3)).ToHaveValueAsync("");
        await Expect(Status).ToHaveTextAsync("No visitor checked in yet.");
        await Expect(VisitCode.Field(0)).ToBeFocusedAsync();
        AssertNoConsoleErrors();
    }
}
