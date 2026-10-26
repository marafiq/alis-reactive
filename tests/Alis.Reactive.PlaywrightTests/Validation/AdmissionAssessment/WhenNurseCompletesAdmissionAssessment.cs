using System.Collections.Concurrent;
using System.Linq.Expressions;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Validation.AdmissionAssessment;

/// <summary>
/// As an admissions nurse, I want every answer on the assessment checked where the chart draws the
/// line, as I write it, so that an assessment the page accepts is one the chart accepts.
/// </summary>
/// <remarks>
/// Save checks the page's rules first and sends nothing it can refuse. Print for the chart sends the
/// assessment as it is, and the server, running the same validator, refuses it in the same words
/// beside the same field. So a refusal is proven by both deciders, the chart first, so that the
/// message beside the field is the server's; an accepted boundary value is proven by the save the
/// server confirms.
/// </remarks>
[TestFixture]
public class WhenNurseCompletesAdmissionAssessment : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/AdmissionAssessment";
    private const string Saved = "Assessment saved for Helen Park.";
    private const string Printed = "Assessment printed for Helen Park.";
    private const string ChartRefused = "The chart refused the assessment.";
    private const string ServerRefusal = "the server responded with a status of 400";

    private PagePlan<AdmissionAssessmentModel> _plan = null!;
    private ConcurrentQueue<string> _savesSent = null!;
    private ConcurrentQueue<string> _printsSent = null!;

    private ILocator SaveAssessment => Page.Locator("#save-assessment-btn");
    private ILocator PrintForChart => Page.Locator("#print-assessment-btn");
    private ILocator AssessmentStatus => Page.Locator("#assessment-status");
    private ILocator PrintStatus => Page.Locator("#print-status");
    private NativeDropDownLocator CareLevel => _plan.NativeDropDown(m => m.CareLevel);
    private NativeDropDownLocator Residency => _plan.NativeDropDown(m => m.Residency);
    private NativeDropDownLocator MobilityAid => _plan.NativeDropDown(m => m.MobilityAid);
    private NativeRadioGroupLocator FellInLastYear => _plan.NativeRadioGroup(m => m.FellInLastYear);
    private NativeCheckBoxLocator ConsentOnFile => _plan.NativeCheckBox(m => m.ConsentOnFile);
    private NativeCheckBoxLocator PhotoConsent => _plan.NativeCheckBox(m => m.PhotoConsent);

    private async Task NavigateAndBoot()
    {
        _savesSent = new ConcurrentQueue<string>();
        _printsSent = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.EndsWith("/Sandbox/Validation/AdmissionAssessment/Save")) _savesSent.Enqueue(request.Url);
            if (request.Url.EndsWith("/Sandbox/Validation/AdmissionAssessment/Print")) _printsSent.Enqueue(request.Url);
        };
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<AdmissionAssessmentModel>.FromPage(Page);
    }

    // Leaving the field lets its message settle, so the next click lands where it aims.
    private async Task Write(Expression<Func<AdmissionAssessmentModel, object?>> field, string text)
    {
        await _plan.TextBox(field).Fill(text);
        await Page.Keyboard.PressAsync("Tab");
    }

    // Every answer the chart needs, valid. The photo consent is ticked and one fall recorded because
    // the page asks for them, though the chart takes an unticked box as a no and no falls as zero.
    private async Task CompleteTheAssessmentLeavingTheFallQuestion()
    {
        await Write(m => m.ResidentName, "Helen Park");
        await Write(m => m.ConsentWord, "AGREE");
        await Write(m => m.SignaturesCollected, "2");
        await Write(m => m.MoveInDate, "2025-03-01");
        await Write(m => m.FallsLastYear, "1");
        await CareLevel.Select("Assisted living");
        await Residency.Select("Permanent");
        await MobilityAid.Select("Walker");
        await ConsentOnFile.Check();
        await PhotoConsent.Check();
    }

    private async Task CompleteTheAssessment()
    {
        await CompleteTheAssessmentLeavingTheFallQuestion();
        await FellInLastYear.Choose("No");
    }

    // The chart refuses the assessment as it is, in its own words beside the field; then the page
    // refuses the same value before anything is sent.
    private async Task AssertRefusedByTheChartAndThePage(Expression<Func<AdmissionAssessmentModel, object?>> field, string message)
    {
        var printsBefore = _printsSent.Count;
        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(field)).ToHaveTextAsync(message);
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(printsBefore + 1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(field)).ToHaveTextAsync(message);
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");
    }

    private async Task AssertSaved(int savesSoFar, string confirmation = Saved)
    {
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(confirmation);
        Assert.That(_savesSent, Has.Count.EqualTo(savesSoFar), "The page let the value through, and the server accepted it.");
    }

    // ── text ──────────────────────────────────────────────────────────────────

    [Test]
    public async Task leaving_the_residents_name_blank_asks_for_it()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.ResidentName, "");

        await AssertRefusedByTheChartAndThePage(m => m.ResidentName, "Enter the resident's name.");

        await Write(m => m.ResidentName, "Helen Park");
        await AssertSaved(1);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_one_letter_name_is_refused_and_a_two_letter_name_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.ResidentName, "H");

        await AssertRefusedByTheChartAndThePage(m => m.ResidentName, "The resident's name is 2 to 40 characters.");

        await Write(m => m.ResidentName, "Al");
        await AssertSaved(1, "Assessment saved for Al.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_forty_one_letter_name_is_refused_and_a_forty_letter_name_is_saved()
    {
        var fortyLetters = new string('H', 40);
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.ResidentName, fortyLetters + "H");

        await AssertRefusedByTheChartAndThePage(m => m.ResidentName, "The resident's name is 2 to 40 characters.");

        await Write(m => m.ResidentName, fortyLetters);
        await AssertSaved(1, $"Assessment saved for {fortyLetters}.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_two_letter_preferred_name_is_refused_and_a_three_letter_one_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.PreferredName, "Le");

        await AssertRefusedByTheChartAndThePage(m => m.PreferredName, "A preferred name is at least 3 characters.");

        await Write(m => m.PreferredName, "Len");
        await AssertSaved(1);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_thirteen_character_wristband_label_is_refused_and_twelve_characters_are_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.WristbandLabel, "PARK, HELEN M");

        await AssertRefusedByTheChartAndThePage(m => m.WristbandLabel, "A wristband label is at most 12 characters.");

        await Write(m => m.WristbandLabel, "PARK, HELEN ");
        await AssertSaved(1);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_room_code_of_five_or_seven_characters_is_refused_and_six_are_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.RoomCode, "MA-21");
        await AssertRefusedByTheChartAndThePage(m => m.RoomCode, "Enter the room as wing and number, like MA-214.");

        await Write(m => m.RoomCode, "MA-2145");
        await AssertRefusedByTheChartAndThePage(m => m.RoomCode, "Enter the room as wing and number, like MA-214.");

        await Write(m => m.RoomCode, "MA-214");
        await AssertSaved(1);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_family_email_with_nothing_on_one_side_of_the_at_is_refused_and_name_at_domain_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.FamilyEmail, "ana@");
        await AssertRefusedByTheChartAndThePage(m => m.FamilyEmail, "Enter the family email as name@domain.");

        await Write(m => m.FamilyEmail, "@park.example");
        await AssertRefusedByTheChartAndThePage(m => m.FamilyEmail, "Enter the family email as name@domain.");

        await Write(m => m.FamilyEmail, "ana@park.example");
        await AssertSaved(1);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_family_email_with_two_at_signs_is_refused()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.FamilyEmail, "ana@park@example");

        await AssertRefusedByTheChartAndThePage(m => m.FamilyEmail, "Enter the family email as name@domain.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_portal_link_that_is_not_a_web_address_is_refused_and_an_https_link_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.PortalLink, "ftp://family.example/hale");

        await AssertRefusedByTheChartAndThePage(m => m.PortalLink, "The family portal link must be a web address.");

        await Write(m => m.PortalLink, "https://family.example/hale");
        await AssertSaved(1);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    // The server reads the whole address; a space in the host is no address.
    [Test]
    public async Task a_portal_link_with_a_space_in_it_is_refused_by_the_chart()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.PortalLink, "http://family example/hale");

        await ClickWhenStable(PrintForChart);

        await Expect(_plan.ErrorFor(m => m.PortalLink)).ToHaveTextAsync("The family portal link must be a web address.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1));
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_portal_link_with_a_space_in_it_is_refused_before_it_is_sent()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.PortalLink, "http://family example/hale");

        await ClickWhenStable(SaveAssessment);

        await Expect(_plan.ErrorFor(m => m.PortalLink)).ToHaveTextAsync("The family portal link must be a web address.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_consent_word_in_lower_case_is_refused_and_agree_in_capitals_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.ConsentWord, "agree");

        await AssertRefusedByTheChartAndThePage(m => m.ConsentWord, "Type AGREE to confirm the resident's consent.");

        await Write(m => m.ConsentWord, "AGREE");
        await AssertSaved(1);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    // ── numbers ───────────────────────────────────────────────────────────────

    [Test]
    public async Task one_signature_or_three_are_refused_and_two_are_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.SignaturesCollected, "1");
        await AssertRefusedByTheChartAndThePage(m => m.SignaturesCollected, "Two signatures are collected: the resident's and a witness's.");

        await Write(m => m.SignaturesCollected, "3");
        await AssertRefusedByTheChartAndThePage(m => m.SignaturesCollected, "Two signatures are collected: the resident's and a witness's.");

        await Write(m => m.SignaturesCollected, "2");
        await AssertSaved(1);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task an_age_of_fifty_four_is_refused_and_fifty_five_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.Age, "54");

        await AssertRefusedByTheChartAndThePage(m => m.Age, "Residents are 55 or older.");

        await Write(m => m.Age, "55");
        await AssertSaved(1);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task an_age_of_one_hundred_twenty_is_refused_and_one_hundred_nineteen_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.Age, "120");

        await AssertRefusedByTheChartAndThePage(m => m.Age, "Check the age: 120 or more cannot be right.");

        await Write(m => m.Age, "119");
        await AssertSaved(1);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_weight_of_twenty_kilograms_is_refused_and_twenty_one_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.WeightKg, "20");

        await AssertRefusedByTheChartAndThePage(m => m.WeightKg, "Weight is recorded in kilograms, above 20.");

        await Write(m => m.WeightKg, "21");
        await AssertSaved(1);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_weight_of_two_hundred_fifty_one_kilograms_is_refused_and_two_hundred_fifty_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.WeightKg, "251");

        await AssertRefusedByTheChartAndThePage(m => m.WeightKg, "Weight is recorded in kilograms, up to 250.");

        await Write(m => m.WeightKg, "250");
        await AssertSaved(1);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_pain_score_below_zero_or_above_ten_is_refused_and_zero_and_ten_are_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.PainScore, "-1");
        await AssertRefusedByTheChartAndThePage(m => m.PainScore, "Pain is scored 0 to 10.");

        await Write(m => m.PainScore, "11");
        await AssertRefusedByTheChartAndThePage(m => m.PainScore, "Pain is scored 0 to 10.");

        await Write(m => m.PainScore, "0");
        await AssertSaved(1);
        await Write(m => m.PainScore, "10");
        await AssertSaved(2);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    // ── dates ─────────────────────────────────────────────────────────────────

    [Test]
    public async Task leaving_the_move_in_day_blank_asks_for_it()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.MoveInDate, "");

        await AssertRefusedByTheChartAndThePage(m => m.MoveInDate, "Enter the move-in day.");

        await Write(m => m.MoveInDate, "2025-03-01");
        await AssertSaved(1);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_move_in_day_in_2023_is_refused_and_january_1_2024_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.MoveInDate, "2023-12-31");

        await AssertRefusedByTheChartAndThePage(m => m.MoveInDate, "Move-in days start at January 1, 2024.");

        await Write(m => m.MoveInDate, "2024-01-01");
        await AssertSaved(1);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task an_assessment_dated_outside_2024_to_2026_is_refused_and_its_first_and_last_days_are_saved()
    {
        const string Outside = "Date the assessment between January 1, 2024 and December 31, 2026.";
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.AssessmentDate, "2023-12-31");
        await AssertRefusedByTheChartAndThePage(m => m.AssessmentDate, Outside);

        await Write(m => m.AssessmentDate, "2027-01-01");
        await AssertRefusedByTheChartAndThePage(m => m.AssessmentDate, Outside);

        await Write(m => m.AssessmentDate, "2024-01-01");
        await AssertSaved(1);
        await Write(m => m.AssessmentDate, "2026-12-31");
        await AssertSaved(2);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_next_review_on_the_assessment_day_is_refused_and_the_day_after_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.AssessmentDate, "2025-03-01");
        await Write(m => m.NextReviewDate, "2025-03-01");

        await AssertRefusedByTheChartAndThePage(m => m.NextReviewDate, "The next review comes after the assessment day.");

        await Write(m => m.NextReviewDate, "2025-03-02");
        await AssertSaved(1);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    // A review is after the assessment day only when there is one: a missing day fails the comparison.
    [Test]
    public async Task a_next_review_without_an_assessment_day_is_refused()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.NextReviewDate, "2025-03-02");

        await AssertRefusedByTheChartAndThePage(m => m.NextReviewDate, "The next review comes after the assessment day.");

        await Write(m => m.AssessmentDate, "2025-03-01");
        await AssertSaved(1);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_next_review_in_2028_is_refused_and_december_31_2027_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.AssessmentDate, "2025-03-01");
        await Write(m => m.NextReviewDate, "2028-01-01");

        await AssertRefusedByTheChartAndThePage(m => m.NextReviewDate, "Reviews are planned up to December 31, 2027.");

        await Write(m => m.NextReviewDate, "2027-12-31");
        await AssertSaved(1);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    // ── choices and yes/no answers ────────────────────────────────────────────

    [Test]
    public async Task leaving_the_care_level_on_its_placeholder_asks_for_it()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await CareLevel.Input.SelectOptionAsync(new SelectOptionValue { Value = "" });

        await AssertRefusedByTheChartAndThePage(m => m.CareLevel, "Choose a care level.");

        await CareLevel.Select("Memory care");
        await AssertSaved(1);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_mobility_aid_left_not_assessed_is_refused_and_a_cane_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await MobilityAid.Select("Not assessed");

        await AssertRefusedByTheChartAndThePage(m => m.MobilityAid, "Assess the mobility aid.");

        await MobilityAid.Select("Cane");
        await AssertSaved(1);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task leaving_the_fall_question_unanswered_asks_for_an_answer()
    {
        await NavigateAndBoot();
        await CompleteTheAssessmentLeavingTheFallQuestion();

        await AssertRefusedByTheChartAndThePage(m => m.FellInLastYear, "Answer whether the resident fell in the last year.");

        await FellInLastYear.Choose("Yes");
        await AssertSaved(1);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task an_unticked_consent_box_is_refused_and_a_ticked_one_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await ConsentOnFile.Input.UncheckAsync();

        await AssertRefusedByTheChartAndThePage(m => m.ConsentOnFile, "Confirm the signed consent is on file.");

        await ConsentOnFile.Check();
        await AssertSaved(1);
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    // The residency's default, not set, is the type's default: the chart reads it as no answer.
    [Test]
    public async Task a_residency_left_not_set_is_refused_by_the_chart()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Residency.Select("Not set");

        await ClickWhenStable(PrintForChart);

        await Expect(_plan.ErrorFor(m => m.Residency)).ToHaveTextAsync("Say whether the stay is permanent or respite.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1));
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_residency_left_not_set_is_refused_before_it_is_sent()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Residency.Select("Not set");

        await ClickWhenStable(SaveAssessment);

        await Expect(_plan.ErrorFor(m => m.Residency)).ToHaveTextAsync("Say whether the stay is permanent or respite.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");
        AssertNoConsoleErrors();
    }

    // NotNull on a yes/no box: an unticked box is a no, and a no is an answer. The page's own check
    // disagrees and says so beside the box as the box is unticked (see the test after this one).
    [Test]
    public async Task an_unticked_photo_consent_box_is_printed_as_a_no()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await PhotoConsent.Input.UncheckAsync();

        await ClickWhenStable(PrintForChart);

        await Expect(PrintStatus).ToHaveTextAsync(Printed);
        Assert.That(_printsSent, Has.Count.EqualTo(1));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_unticked_photo_consent_box_is_saved_as_a_no()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await PhotoConsent.Input.UncheckAsync();

        await AssertSaved(1);
        await Expect(_plan.ErrorFor(m => m.PhotoConsent)).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    // NotNull on a count: no falls is zero, and zero is an answer. The page's own check disagrees and
    // says so beside the count as the field is left (see the test after this one).
    [Test]
    public async Task no_falls_last_year_is_printed_as_zero()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.FallsLastYear, "0");

        await ClickWhenStable(PrintForChart);

        await Expect(PrintStatus).ToHaveTextAsync(Printed);
        Assert.That(_printsSent, Has.Count.EqualTo(1));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task no_falls_last_year_is_saved_as_zero()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.FallsLastYear, "0");

        await AssertSaved(1);
        await Expect(_plan.ErrorFor(m => m.FallsLastYear)).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    // ── as the nurse writes ───────────────────────────────────────────────────

    [Test]
    public async Task typing_into_the_name_clears_its_message_before_the_field_is_left()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.ResidentName, "");
        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("Enter the resident's name.");

        await _plan.TextBox(m => m.ResidentName).Input.PressSequentiallyAsync("H");

        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("");
        await Expect(_plan.TextBox(m => m.ResidentName).Input).ToBeFocusedAsync();
        Assert.That(_savesSent, Is.Empty);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task leaving_the_name_with_one_letter_asks_for_two_without_saving()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();

        await _plan.TextBox(m => m.ResidentName).Fill("H");
        await Page.Keyboard.PressAsync("Tab");

        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("The resident's name is 2 to 40 characters.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "Leaving a field sends nothing.");
        AssertNoConsoleErrors();
    }

    // A choice is committed as it is made: the message follows the choice without the field being left.
    [Test]
    public async Task choosing_a_care_level_clears_its_message_at_once_and_unchoosing_it_asks_again()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await CareLevel.Input.SelectOptionAsync(new SelectOptionValue { Value = "" });
        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.CareLevel)).ToHaveTextAsync("Choose a care level.");

        await CareLevel.Select("Memory care");
        await Expect(_plan.ErrorFor(m => m.CareLevel)).ToHaveTextAsync("");

        await CareLevel.Input.SelectOptionAsync(new SelectOptionValue { Value = "" });
        await Expect(_plan.ErrorFor(m => m.CareLevel)).ToHaveTextAsync("Choose a care level.");
        Assert.That(_savesSent, Is.Empty);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task ticking_the_consent_box_clears_its_message_at_once()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await ConsentOnFile.Input.UncheckAsync();
        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.ConsentOnFile)).ToHaveTextAsync("Confirm the signed consent is on file.");

        await ConsentOnFile.Check();

        await Expect(_plan.ErrorFor(m => m.ConsentOnFile)).ToHaveTextAsync("");
        Assert.That(_savesSent, Is.Empty);
        AssertNoConsoleErrors();
    }
}
