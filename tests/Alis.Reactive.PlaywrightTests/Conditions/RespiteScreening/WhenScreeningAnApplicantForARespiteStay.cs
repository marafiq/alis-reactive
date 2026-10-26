using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.RespiteScreening;

namespace Alis.Reactive.PlaywrightTests.Conditions.RespiteScreening;

/// <summary>
/// As the admissions coordinator, I want an applicant's intake sheet checked against the community's
/// rules as I fill it in, fact by fact, and held against the Garden wing's policy, so that a stay is
/// never promised on a fact that fails a rule.
/// </summary>
/// <remarks>
/// The intake sheet is a widget the sandbox wrote itself (Scripts/intake-sheet.ts, BroughtComponents/
/// IntakeSheet); it holds each fact as its own kind: text, a number, a date, yes or no, an option, a
/// number with cents, and each of those or nothing for the facts not yet asked, plus a contact object
/// and a list of allergies. Every rule on the page reads one fact and says what it means, and every
/// test edits that fact to each side of the rule's edge: the rule's line must change with the edit, so
/// an operator one step off (greater for at least, a dropped cent, a date a day out, a missing fact
/// read as zero, a case-folded text, a list read as text) names the wrong side. Margaret Hale opens the
/// sheet on the edge of most rules: 62, moving in on 1 July 2026, a veteran, assisted living at $5,000,
/// one pet, with no room preference, previous stay, flu shot or diet recorded. The wing's policy is
/// asked for on each check and every fact is compared with the wing's, read when the check runs.
/// Not exercised: an order over yes/no or an option (Gt on a bool compiles but says nothing a
/// coordinator would write), and In over yes/no, which is Eq.
/// </remarks>
[TestFixture]
public class WhenScreeningAnApplicantForARespiteStay : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/RespiteScreening";

    private PagePlan<RespiteScreeningModel> _plan = null!;

    private ILocator Sheet => Page.Locator($"#{RespiteScreeningModel.SheetId}");
    private ILocator CheckWing => Page.Locator("#check-wing");
    private NativeCheckListLocator Services => new(Page, _plan.FindComponent(m => m.Services)!.ElementId);

    private ILocator Field(string label) => Sheet.GetByLabel(label, new() { Exact = true });

    private ILocator Line(string id) => Page.Locator($"#{id}");

    private async Task OpenTheSheet()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<RespiteScreeningModel>.FromPage(Page);
        await Expect(Line("name-waiting")).ToHaveTextAsync("On the waiting list.", new() { Timeout = 5000 });
    }

    // Typing into the sheet commits on blur, the way the sheet listens; a date commits as it is filled.
    private async Task Type(string label, string text)
    {
        var field = Field(label);
        await field.FillAsync(text);
        await field.BlurAsync();
    }

    private async Task Choose(string label, string option) =>
        await Field(label).SelectOptionAsync(new SelectOptionValue { Label = option });

    private async Task CheckTheWing()
    {
        await CheckWing.ClickAsync();
    }

    // ── the applicant's name: text ──────────────────────────────────────────

    [Test]
    public async Task the_applicant_on_the_waiting_list_is_known_and_one_letter_off_is_not()
    {
        await OpenTheSheet();

        await Type("Applicant", "Margaret Hal");
        await Expect(Line("name-waiting")).ToHaveTextAsync("Not on the waiting list.");

        await Type("Applicant", "Margaret Hale");
        await Expect(Line("name-waiting")).ToHaveTextAsync("On the waiting list.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_different_name_is_not_the_applicant_the_wing_expected()
    {
        await OpenTheSheet();
        await Expect(Line("name-expected")).ToHaveTextAsync("The applicant the wing expected.");

        await Type("Applicant", "Harold Benson");

        await Expect(Line("name-expected")).ToHaveTextAsync("Not the applicant the wing expected.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task margaret_hale_and_harold_benson_are_returning_residents_and_anyone_else_is_a_first_stay()
    {
        await OpenTheSheet();
        await Expect(Line("name-returning")).ToHaveTextAsync("A returning resident: pull the old chart.");

        await Type("Applicant", "Harold Benson");
        await Expect(Line("name-returning")).ToHaveTextAsync("A returning resident: pull the old chart.");

        await Type("Applicant", "Rose Delgado");
        await Expect(Line("name-returning")).ToHaveTextAsync("A first stay with us.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_name_on_the_do_not_admit_list_is_barred()
    {
        await OpenTheSheet();
        await Expect(Line("name-barred")).ToHaveTextAsync("Clear to admit.");

        await Type("Applicant", "Walter Brooks");

        await Expect(Line("name-barred")).ToHaveTextAsync("Barred: speak to the director.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_blank_name_asks_for_one_and_a_single_letter_is_a_name()
    {
        await OpenTheSheet();

        await Type("Applicant", "");
        await Expect(Line("name-recorded")).ToHaveTextAsync("Record the applicant's name.");

        await Type("Applicant", "M");
        await Expect(Line("name-recorded")).ToHaveTextAsync("Name recorded.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_blank_name_is_blank_and_a_filled_in_one_is_not()
    {
        await OpenTheSheet();
        await Expect(Line("name-blank")).ToHaveTextAsync("The name is filled in.");

        await Type("Applicant", "");

        await Expect(Line("name-blank")).ToHaveTextAsync("The name is blank.");
        AssertNoConsoleErrors();
    }

    // A blank name is still a name field on the sheet: blank is not missing.
    [Test]
    public async Task a_blank_name_is_still_on_the_sheet()
    {
        await OpenTheSheet();

        await Type("Applicant", "");

        await Expect(Line("name-blank")).ToHaveTextAsync("The name is blank.");
        await Expect(Line("name-present")).ToHaveTextAsync("Name field present.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_hale_is_of_the_hale_family_and_a_hal_is_not()
    {
        await OpenTheSheet();
        await Expect(Line("name-hale")).ToHaveTextAsync("Of the Hale family.");

        await Type("Applicant", "Margaret Hal");

        await Expect(Line("name-hale")).ToHaveTextAsync("Not a Hale.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_doctor_is_addressed_as_doctor_only_with_the_title_capitalised()
    {
        await OpenTheSheet();

        await Type("Applicant", "Dr Margaret Hale");
        await Expect(Line("name-doctor")).ToHaveTextAsync("Address as Doctor.");

        await Type("Applicant", "dr Margaret Hale");
        await Expect(Line("name-doctor")).ToHaveTextAsync("Address by name.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_junior_suffix_asks_to_check_the_fathers_record_and_a_dotted_one_is_no_suffix()
    {
        await OpenTheSheet();

        await Type("Applicant", "Harold Benson Jr");
        await Expect(Line("name-junior")).ToHaveTextAsync("A junior: check it is not the father's record.");

        await Type("Applicant", "Harold Benson Jr.");
        await Expect(Line("name-junior")).ToHaveTextAsync("No suffix.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_capitalised_first_and_last_name_is_a_full_name_and_a_lower_case_or_single_name_is_not()
    {
        await OpenTheSheet();
        await Expect(Line("name-full")).ToHaveTextAsync("First and last name.");

        await Type("Applicant", "Margaret hale");
        await Expect(Line("name-full")).ToHaveTextAsync("Give first and last name.");

        await Type("Applicant", "Margaret");
        await Expect(Line("name-full")).ToHaveTextAsync("Give first and last name.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_name_of_twelve_characters_is_long_enough_and_eleven_is_not()
    {
        await OpenTheSheet();

        await Type("Applicant", "Margaret Hal");
        await Expect(Line("name-length")).ToHaveTextAsync("A full name (12 or more characters).");

        await Type("Applicant", "Margaret Ha");
        await Expect(Line("name-length")).ToHaveTextAsync("Too short for a full name.");
        AssertNoConsoleErrors();
    }

    // ── age: a number ───────────────────────────────────────────────────────

    [Test]
    public async Task a_centenarian_is_exactly_one_hundred()
    {
        await OpenTheSheet();

        await Type("Age", "100");
        await Expect(Line("age-centenarian")).ToHaveTextAsync("A centenarian: the director visits.");

        await Type("Age", "101");
        await Expect(Line("age-centenarian")).ToHaveTextAsync("Not a centenarian.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_age_of_zero_asks_for_the_age_and_one_is_an_age()
    {
        await OpenTheSheet();

        await Type("Age", "0");
        await Expect(Line("age-given")).ToHaveTextAsync("Give the applicant's age.");

        await Type("Age", "1");
        await Expect(Line("age-given")).ToHaveTextAsync("Age given.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sixty_two_is_old_enough_for_respite_care_and_sixty_one_is_not()
    {
        await OpenTheSheet();
        await Expect(Line("age-respite")).ToHaveTextAsync("Old enough for respite care (62 and over).");

        await Type("Age", "61");

        await Expect(Line("age-respite")).ToHaveTextAsync("Under 62: needs the director's approval.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task ninety_needs_a_wellness_visit_and_eighty_nine_does_not()
    {
        await OpenTheSheet();

        await Type("Age", "90");
        await Expect(Line("age-wellness")).ToHaveTextAsync("Over 89: a wellness visit before move-in.");

        await Type("Age", "89");
        await Expect(Line("age-wellness")).ToHaveTextAsync("No wellness visit needed.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sixty_four_is_under_medicare_age_and_sixty_five_is_not()
    {
        await OpenTheSheet();

        await Type("Age", "64");
        await Expect(Line("age-medicare")).ToHaveTextAsync("Under 65: check Medicare eligibility.");

        await Type("Age", "65");
        await Expect(Line("age-medicare")).ToHaveTextAsync("65 or over: Medicare age.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task fifty_five_is_a_young_applicant_and_fifty_six_is_not()
    {
        await OpenTheSheet();

        await Type("Age", "55");
        await Expect(Line("age-young")).ToHaveTextAsync("55 or under: note why a stay is sought.");

        await Type("Age", "56");
        await Expect(Line("age-young")).ToHaveTextAsync("Over 55.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_usual_range_runs_from_sixty_two_to_eighty_nine_inclusive()
    {
        await OpenTheSheet();
        await Expect(Line("age-usual")).ToHaveTextAsync("In the usual range for a stay.");

        await Type("Age", "61");
        await Expect(Line("age-usual")).ToHaveTextAsync("Outside the usual range.");

        await Type("Age", "89");
        await Expect(Line("age-usual")).ToHaveTextAsync("In the usual range for a stay.");

        await Type("Age", "90");
        await Expect(Line("age-usual")).ToHaveTextAsync("Outside the usual range.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_milestone_birthday_is_one_of_the_listed_ages()
    {
        await OpenTheSheet();

        await Type("Age", "65");
        await Expect(Line("age-milestone")).ToHaveTextAsync("A milestone birthday this year: plan a celebration.");

        await Type("Age", "66");
        await Expect(Line("age-milestone")).ToHaveTextAsync("No milestone birthday.");
        AssertNoConsoleErrors();
    }

    // ── the monthly rate: a number with cents ───────────────────────────────

    [Test]
    public async Task a_rate_one_cent_off_the_standard_is_not_the_standard_rate()
    {
        await OpenTheSheet();
        await Expect(Line("rate-standard")).ToHaveTextAsync("The standard rate.");

        await Type("Monthly rate", "5000.01");

        await Expect(Line("rate-standard")).ToHaveTextAsync("Not the standard rate: note why.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_rate_not_on_the_published_list_is_a_custom_rate()
    {
        await OpenTheSheet();
        await Expect(Line("rate-published")).ToHaveTextAsync("A published rate.");

        await Type("Monthly rate", "4999.99");

        await Expect(Line("rate-published")).ToHaveTextAsync("A custom rate: the director signs it.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task five_thousand_is_within_the_budget_and_a_cent_more_is_over_it()
    {
        await OpenTheSheet();
        await Expect(Line("rate-budget")).ToHaveTextAsync("Within the family's $5,000 budget.");

        await Type("Monthly rate", "5000.01");

        await Expect(Line("rate-budget")).ToHaveTextAsync("Over the family's budget.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_published_range_runs_from_4500_to_5500_to_the_cent()
    {
        await OpenTheSheet();

        await Type("Monthly rate", "4499.99");
        await Expect(Line("rate-range")).ToHaveTextAsync("Outside the published range.");

        await Type("Monthly rate", "4500.00");
        await Expect(Line("rate-range")).ToHaveTextAsync("Within the published range.");

        await Type("Monthly rate", "5500.00");
        await Expect(Line("rate-range")).ToHaveTextAsync("Within the published range.");

        await Type("Monthly rate", "5500.01");
        await Expect(Line("rate-range")).ToHaveTextAsync("Outside the published range.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_cent_over_5500_is_a_premium_rate_and_5500_itself_is_not()
    {
        await OpenTheSheet();

        await Type("Monthly rate", "5500.00");
        await Expect(Line("rate-premium")).ToHaveTextAsync("A standard suite rate.");

        await Type("Monthly rate", "5500.01");
        await Expect(Line("rate-premium")).ToHaveTextAsync("A premium suite rate.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task four_thousand_five_hundred_is_the_minimum_rate_and_a_cent_under_is_below_it()
    {
        await OpenTheSheet();

        await Type("Monthly rate", "4500.00");
        await Expect(Line("rate-minimum")).ToHaveTextAsync("At or above the minimum rate.");

        await Type("Monthly rate", "4499.99");
        await Expect(Line("rate-minimum")).ToHaveTextAsync("Below the minimum rate.");
        AssertNoConsoleErrors();
    }

    // ── the move-in date: a date ────────────────────────────────────────────

    [Test]
    public async Task moving_in_on_1_july_is_opening_day_and_2_july_is_not()
    {
        await OpenTheSheet();
        await Expect(Line("move-in-opening")).ToHaveTextAsync("Moving in on opening day.");

        await Type("Move-in date", "2026-07-02");

        await Expect(Line("move-in-opening")).ToHaveTextAsync("Not on opening day.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_move_in_on_independence_day_is_refused()
    {
        await OpenTheSheet();
        await Expect(Line("move-in-holiday")).ToHaveTextAsync("Not a holiday move-in.");

        await Type("Move-in date", "2026-07-04");

        await Expect(Line("move-in-holiday")).ToHaveTextAsync("Independence Day: no move-ins.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_move_in_on_1_july_is_after_the_wing_opens_and_30_june_is_before()
    {
        await OpenTheSheet();
        await Expect(Line("move-in-open")).ToHaveTextAsync("After the wing opens on 1 July.");

        await Type("Move-in date", "2026-06-30");

        await Expect(Line("move-in-open")).ToHaveTextAsync("Before the wing opens.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sixteen_july_is_after_the_inspection_and_fifteen_july_is_not()
    {
        await OpenTheSheet();

        await Type("Move-in date", "2026-07-16");
        await Expect(Line("move-in-after-inspection")).ToHaveTextAsync("After the mid-July inspection.");

        await Type("Move-in date", "2026-07-15");
        await Expect(Line("move-in-after-inspection")).ToHaveTextAsync("Not after the inspection.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task fourteen_july_is_before_the_inspection_and_fifteen_july_is_not()
    {
        await OpenTheSheet();

        await Type("Move-in date", "2026-07-14");
        await Expect(Line("move-in-before-inspection")).ToHaveTextAsync("Before the mid-July inspection.");

        await Type("Move-in date", "2026-07-15");
        await Expect(Line("move-in-before-inspection")).ToHaveTextAsync("Not before the inspection.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task thirty_one_july_is_by_the_end_of_july_and_1_august_is_not()
    {
        await OpenTheSheet();

        await Type("Move-in date", "2026-07-31");
        await Expect(Line("move-in-month-end")).ToHaveTextAsync("By the end of July.");

        await Type("Move-in date", "2026-08-01");
        await Expect(Line("move-in-month-end")).ToHaveTextAsync("August or later.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_july_move_in_runs_from_1_july_to_31_july_inclusive()
    {
        await OpenTheSheet();
        await Expect(Line("move-in-july")).ToHaveTextAsync("A July move-in.");

        await Type("Move-in date", "2026-06-30");
        await Expect(Line("move-in-july")).ToHaveTextAsync("Not a July move-in.");

        await Type("Move-in date", "2026-07-31");
        await Expect(Line("move-in-july")).ToHaveTextAsync("A July move-in.");

        await Type("Move-in date", "2026-08-01");
        await Expect(Line("move-in-july")).ToHaveTextAsync("Not a July move-in.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_orientation_monday_is_one_of_the_listed_mondays()
    {
        await OpenTheSheet();

        await Type("Move-in date", "2026-07-13");
        await Expect(Line("move-in-orientation")).ToHaveTextAsync("An orientation Monday.");

        await Type("Move-in date", "2026-07-14");
        await Expect(Line("move-in-orientation")).ToHaveTextAsync("Not an orientation day.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_holiday_weekend_is_3_and_4_july_and_5_july_is_clear_of_it()
    {
        await OpenTheSheet();

        await Type("Move-in date", "2026-07-03");
        await Expect(Line("move-in-holiday-weekend")).ToHaveTextAsync("On the holiday weekend.");

        await Type("Move-in date", "2026-07-05");
        await Expect(Line("move-in-holiday-weekend")).ToHaveTextAsync("Clear of the holiday weekend.");
        AssertNoConsoleErrors();
    }

    // ── veteran: yes or no ──────────────────────────────────────────────────

    [Test]
    public async Task a_veteran_is_told_to_the_va_liaison_and_a_civilian_is_not()
    {
        await OpenTheSheet();
        await Expect(Line("veteran-va")).ToHaveTextAsync("A veteran: the VA liaison is told.");

        await Field("Veteran").UncheckAsync();

        await Expect(Line("veteran-va")).ToHaveTextAsync("Not a veteran.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_veteran_is_billed_through_the_va_and_anyone_else_as_standard()
    {
        await OpenTheSheet();
        await Expect(Line("veteran-billing")).ToHaveTextAsync("VA billing.");

        await Field("Veteran").UncheckAsync();

        await Expect(Line("veteran-billing")).ToHaveTextAsync("Standard billing.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_applicant_who_did_not_serve_is_a_civilian()
    {
        await OpenTheSheet();
        await Expect(Line("veteran-civilian")).ToHaveTextAsync("Served.");

        await Field("Veteran").UncheckAsync();
        await Expect(Line("veteran-civilian")).ToHaveTextAsync("A civilian.");

        await Field("Veteran").CheckAsync();
        await Expect(Line("veteran-civilian")).ToHaveTextAsync("Served.");
        AssertNoConsoleErrors();
    }

    // ── care level: an option ───────────────────────────────────────────────

    [Test]
    public async Task memory_care_is_the_secured_wing_and_assisted_living_is_not()
    {
        await OpenTheSheet();

        await Choose("Care level", "Memory care");
        await Expect(Line("care-secured")).ToHaveTextAsync("Memory care: the secured wing.");

        await Choose("Care level", "Assisted living");
        await Expect(Line("care-secured")).ToHaveTextAsync("Not the secured wing.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task independent_living_has_no_care_staff_and_every_other_level_does()
    {
        await OpenTheSheet();
        await Expect(Line("care-staff")).ToHaveTextAsync("Care staff assigned.");

        await Choose("Care level", "Independent living");

        await Expect(Line("care-staff")).ToHaveTextAsync("Independent living: no care staff.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task assisted_living_and_memory_care_need_a_care_plan_and_independent_living_does_not()
    {
        await OpenTheSheet();
        await Expect(Line("care-plan")).ToHaveTextAsync("A care plan is written.");

        await Choose("Care level", "Memory care");
        await Expect(Line("care-plan")).ToHaveTextAsync("A care plan is written.");

        await Choose("Care level", "Independent living");
        await Expect(Line("care-plan")).ToHaveTextAsync("No care plan needed.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_garden_wing_takes_any_level_but_memory_care()
    {
        await OpenTheSheet();
        await Expect(Line("care-garden")).ToHaveTextAsync("Eligible for the Garden wing.");

        await Choose("Care level", "Memory care");

        await Expect(Line("care-garden")).ToHaveTextAsync("The Garden wing is not secured.");
        AssertNoConsoleErrors();
    }

    // ── room preference: text, or nothing ───────────────────────────────────

    [Test]
    public async Task no_room_preference_is_recorded_until_one_is_typed()
    {
        await OpenTheSheet();
        await Expect(Line("room-preference")).ToHaveTextAsync("No room preference recorded.");

        await Type("Room preference", "Garden view");

        await Expect(Line("room-preference")).ToHaveTextAsync("Room preference recorded.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_preference_typed_is_noted_and_cleared_is_none()
    {
        await OpenTheSheet();
        await Expect(Line("room-noted")).ToHaveTextAsync("No preference.");

        await Type("Room preference", "Garden view");
        await Expect(Line("room-noted")).ToHaveTextAsync("Preference noted.");

        await Type("Room preference", "");
        await Expect(Line("room-noted")).ToHaveTextAsync("No preference.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_garden_view_room_is_the_garden_side_and_a_differently_cased_one_is_not()
    {
        await OpenTheSheet();

        await Type("Room preference", "Garden view");
        await Expect(Line("room-garden")).ToHaveTextAsync("Prefers the garden side.");

        await Type("Room preference", "Garden View");
        await Expect(Line("room-garden")).ToHaveTextAsync("No garden-side request.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_room_ending_in_view_asks_for_a_view_and_no_preference_asks_for_none()
    {
        await OpenTheSheet();

        await Type("Room preference", "Courtyard view");
        await Expect(Line("room-view")).ToHaveTextAsync("Asked for a view.");

        await Type("Room preference", "");
        await Expect(Line("room-view")).ToHaveTextAsync("No view asked for.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task any_room_will_do_until_a_room_is_named()
    {
        await OpenTheSheet();
        await Expect(Line("room-any")).ToHaveTextAsync("Any room will do.");

        await Type("Room preference", "Room 214");

        await Expect(Line("room-any")).ToHaveTextAsync("A particular room.");
        AssertNoConsoleErrors();
    }

    // ── pets: a number, or nothing ──────────────────────────────────────────

    [Test]
    public async Task pets_read_as_not_asked_when_the_count_is_cleared_and_as_asked_at_zero()
    {
        await OpenTheSheet();
        await Expect(Line("pets-unknown")).ToHaveTextAsync("Pets: asked.");

        await Type("Pets", "");
        await Expect(Line("pets-unknown")).ToHaveTextAsync("Pets: not asked yet.");

        await Type("Pets", "0");
        await Expect(Line("pets-unknown")).ToHaveTextAsync("Pets: asked.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_pet_count_is_recorded_and_a_cleared_one_asks_again()
    {
        await OpenTheSheet();
        await Expect(Line("pets-known")).ToHaveTextAsync("Pets recorded.");

        await Type("Pets", "");

        await Expect(Line("pets-known")).ToHaveTextAsync("Ask about pets.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task zero_pets_is_no_pets_and_an_unasked_count_is_not_zero()
    {
        await OpenTheSheet();

        await Type("Pets", "0");
        await Expect(Line("pets-none")).ToHaveTextAsync("No pets.");

        await Type("Pets", "");
        await Expect(Line("pets-none")).ToHaveTextAsync("Pets, or not asked.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task one_pet_comes_along_and_zero_or_an_unasked_count_does_not()
    {
        await OpenTheSheet();
        await Expect(Line("pets-brings")).ToHaveTextAsync("Brings a pet.");

        await Type("Pets", "0");
        await Expect(Line("pets-brings")).ToHaveTextAsync("No pet to settle in.");

        await Type("Pets", "");
        await Expect(Line("pets-brings")).ToHaveTextAsync("No pet to settle in.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task one_pet_is_within_the_limit_and_two_or_an_unasked_count_is_not()
    {
        await OpenTheSheet();
        await Expect(Line("pets-limit")).ToHaveTextAsync("Within the one-pet limit.");

        await Type("Pets", "2");
        await Expect(Line("pets-limit")).ToHaveTextAsync("Over the limit, or not asked.");

        await Type("Pets", "");
        await Expect(Line("pets-limit")).ToHaveTextAsync("Over the limit, or not asked.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task one_or_two_pets_is_a_pair_and_three_or_an_unasked_count_is_not()
    {
        await OpenTheSheet();
        await Expect(Line("pets-pair")).ToHaveTextAsync("One or two pets.");

        await Type("Pets", "2");
        await Expect(Line("pets-pair")).ToHaveTextAsync("One or two pets.");

        await Type("Pets", "3");
        await Expect(Line("pets-pair")).ToHaveTextAsync("Not one or two pets.");

        await Type("Pets", "");
        await Expect(Line("pets-pair")).ToHaveTextAsync("Not one or two pets.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task no_pet_or_an_unasked_count_has_nothing_to_settle_and_one_pet_does()
    {
        await OpenTheSheet();
        await Expect(Line("pets-settle")).ToHaveTextAsync("A pet comes along.");

        await Type("Pets", "0");
        await Expect(Line("pets-settle")).ToHaveTextAsync("No pet to settle, or not asked.");

        await Type("Pets", "");
        await Expect(Line("pets-settle")).ToHaveTextAsync("No pet to settle, or not asked.");
        AssertNoConsoleErrors();
    }

    // ── the previous stay: a date, or nothing ───────────────────────────────

    [Test]
    public async Task a_first_stay_has_no_previous_stay_until_one_is_dated()
    {
        await OpenTheSheet();
        await Expect(Line("prior-first-stay")).ToHaveTextAsync("A first stay with us.");

        await Type("Previous stay ended", "2025-12-31");

        await Expect(Line("prior-first-stay")).ToHaveTextAsync("A returning resident.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_dated_previous_stay_pulls_the_previous_chart()
    {
        await OpenTheSheet();
        await Expect(Line("prior-chart")).ToHaveTextAsync("No previous chart.");

        await Type("Previous stay ended", "2025-12-31");

        await Expect(Line("prior-chart")).ToHaveTextAsync("Pull the previous chart.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_previous_stay_ending_on_31_december_2025_left_on_the_last_day_of_the_year()
    {
        await OpenTheSheet();
        await Expect(Line("prior-year-end")).ToHaveTextAsync("Did not leave on the last day of 2025.");

        await Type("Previous stay ended", "2025-12-31");
        await Expect(Line("prior-year-end")).ToHaveTextAsync("Left on the last day of 2025.");

        await Type("Previous stay ended", "2025-12-30");
        await Expect(Line("prior-year-end")).ToHaveTextAsync("Did not leave on the last day of 2025.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_previous_stay_ending_this_year_counts_and_one_ending_last_year_or_never_does_not()
    {
        await OpenTheSheet();
        await Expect(Line("prior-this-year")).ToHaveTextAsync("Not this year, or never.");

        await Type("Previous stay ended", "2026-01-01");
        await Expect(Line("prior-this-year")).ToHaveTextAsync("Stayed with us this year.");

        await Type("Previous stay ended", "2025-12-31");
        await Expect(Line("prior-this-year")).ToHaveTextAsync("Not this year, or never.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_stay_that_ended_more_than_a_year_ago_asks_for_a_reassessment()
    {
        await OpenTheSheet();
        await Expect(Line("prior-reassess")).ToHaveTextAsync("Recent enough, or no previous stay.");

        await Type("Previous stay ended", "2025-06-30");
        await Expect(Line("prior-reassess")).ToHaveTextAsync("More than a year since the last stay: reassess.");

        await Type("Previous stay ended", "2025-07-01");
        await Expect(Line("prior-reassess")).ToHaveTextAsync("Recent enough, or no previous stay.");
        AssertNoConsoleErrors();
    }

    // ── the flu shot: yes or no, or nothing ─────────────────────────────────

    [Test]
    public async Task the_flu_shot_is_not_asked_until_answered()
    {
        await OpenTheSheet();
        await Expect(Line("flu-not-asked")).ToHaveTextAsync("Flu shot: not asked.");

        await Choose("Flu shot", "Yes");

        await Expect(Line("flu-not-asked")).ToHaveTextAsync("Flu shot: asked.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_answered_flu_shot_is_recorded_and_not_asked_is_unanswered()
    {
        await OpenTheSheet();
        await Expect(Line("flu-recorded")).ToHaveTextAsync("Flu shot unanswered.");

        await Choose("Flu shot", "No");

        await Expect(Line("flu-recorded")).ToHaveTextAsync("Flu shot answered.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_flu_shot_answered_yes_is_given_and_no_or_not_asked_is_not_on_record()
    {
        await OpenTheSheet();
        await Expect(Line("flu-given")).ToHaveTextAsync("No flu shot on record.");

        await Choose("Flu shot", "Yes");
        await Expect(Line("flu-given")).ToHaveTextAsync("Flu shot given.");

        await Choose("Flu shot", "No");
        await Expect(Line("flu-given")).ToHaveTextAsync("No flu shot on record.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_flu_shot_answered_no_is_declined_and_not_asked_is_not_declined()
    {
        await OpenTheSheet();
        await Expect(Line("flu-declined")).ToHaveTextAsync("Not declined.");

        await Choose("Flu shot", "No");

        await Expect(Line("flu-declined")).ToHaveTextAsync("Flu shot declined: offer it at move-in.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_flu_shot_is_offered_unless_it_was_given()
    {
        await OpenTheSheet();
        await Expect(Line("flu-offer")).ToHaveTextAsync("Offer the flu shot.");

        await Choose("Flu shot", "No");
        await Expect(Line("flu-offer")).ToHaveTextAsync("Offer the flu shot.");

        await Choose("Flu shot", "Yes");
        await Expect(Line("flu-offer")).ToHaveTextAsync("Flu shot done.");
        AssertNoConsoleErrors();
    }

    // ── the diet: an option, or nothing ─────────────────────────────────────

    [Test]
    public async Task no_diet_is_recorded_until_one_is_chosen()
    {
        await OpenTheSheet();
        await Expect(Line("diet-none")).ToHaveTextAsync("No diet recorded.");

        await Choose("Diet", "Regular");

        await Expect(Line("diet-none")).ToHaveTextAsync("Diet recorded.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_chosen_diet_is_known_and_none_recorded_is_unknown()
    {
        await OpenTheSheet();
        await Expect(Line("diet-known")).ToHaveTextAsync("Diet unknown.");

        await Choose("Diet", "Pureed");

        await Expect(Line("diet-known")).ToHaveTextAsync("Diet known.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_diabetic_diet_tells_the_kitchen_and_a_regular_one_does_not()
    {
        await OpenTheSheet();

        await Choose("Diet", "Diabetic");
        await Expect(Line("diet-diabetic")).ToHaveTextAsync("Diabetic diet: tell the kitchen.");

        await Choose("Diet", "Regular");
        await Expect(Line("diet-diabetic")).ToHaveTextAsync("Not a diabetic diet.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task diabetic_and_pureed_diets_are_special_and_a_regular_or_unrecorded_one_is_not()
    {
        await OpenTheSheet();
        await Expect(Line("diet-special")).ToHaveTextAsync("No special diet.");

        await Choose("Diet", "Diabetic");
        await Expect(Line("diet-special")).ToHaveTextAsync("A special diet.");

        await Choose("Diet", "Pureed");
        await Expect(Line("diet-special")).ToHaveTextAsync("A special diet.");

        await Choose("Diet", "Regular");
        await Expect(Line("diet-special")).ToHaveTextAsync("No special diet.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task only_a_pureed_diet_is_pureed_textures_and_a_diet_not_recorded_is_regular_textures()
    {
        await OpenTheSheet();
        await Expect(Line("diet-textures")).ToHaveTextAsync("Regular textures.");

        await Choose("Diet", "Pureed");
        await Expect(Line("diet-textures")).ToHaveTextAsync("Pureed textures.");

        await Choose("Diet", "Regular");
        await Expect(Line("diet-textures")).ToHaveTextAsync("Regular textures.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_kitchen_has_nothing_to_prepare_until_a_diet_is_recorded()
    {
        await OpenTheSheet();
        await Expect(Line("diet-kitchen")).ToHaveTextAsync("Nothing for the kitchen yet.");

        await Choose("Diet", "Regular");

        await Expect(Line("diet-kitchen")).ToHaveTextAsync("The kitchen has a diet to prepare.");
        AssertNoConsoleErrors();
    }

    // ── allergies and services: lists ───────────────────────────────────────

    [Test]
    public async Task allergies_listed_are_recorded_and_a_cleared_list_records_none()
    {
        await OpenTheSheet();
        await Expect(Line("allergies-none")).ToHaveTextAsync("Allergies recorded.");

        await Type("Allergies", "");

        await Expect(Line("allergies-none")).ToHaveTextAsync("No allergies recorded.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_penicillin_allergy_flags_the_pharmacy_and_a_latex_one_alone_does_not()
    {
        await OpenTheSheet();
        await Expect(Line("allergies-penicillin")).ToHaveTextAsync("Penicillin allergy: flag the pharmacy.");

        await Type("Allergies", "Latex");

        await Expect(Line("allergies-penicillin")).ToHaveTextAsync("No penicillin allergy.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_latex_allergy_anywhere_on_the_list_tells_housekeeping()
    {
        await OpenTheSheet();
        await Expect(Line("allergies-latex")).ToHaveTextAsync("No latex allergy.");

        await Type("Allergies", "Penicillin, Latex");

        await Expect(Line("allergies-latex")).ToHaveTextAsync("Latex allergy: tell housekeeping.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_codeine_allergy_tells_the_physician_and_penicillin_alone_does_not()
    {
        await OpenTheSheet();

        await Type("Allergies", "Codeine");
        await Expect(Line("allergies-codeine")).ToHaveTextAsync("Codeine allergy: tell the physician.");

        await Type("Allergies", "Penicillin");
        await Expect(Line("allergies-codeine")).ToHaveTextAsync("No codeine allergy.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task allergies_are_on_file_until_the_list_is_cleared()
    {
        await OpenTheSheet();

        await Type("Allergies", "Penicillin, Latex");
        await Expect(Line("allergies-any")).ToHaveTextAsync("Allergies on file.");

        await Type("Allergies", "");
        await Expect(Line("allergies-any")).ToHaveTextAsync("No allergies on file.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task ticking_transport_arranges_it_and_clearing_it_does_not()
    {
        await OpenTheSheet();

        await Services.Tick(RespiteServices.Transport);
        await Expect(Line("services-transport")).ToHaveTextAsync("Transport arranged.");

        await Services.Clear(RespiteServices.Transport);
        await Expect(Line("services-transport")).ToHaveTextAsync("No transport.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task ticking_laundry_arranges_it_and_meals_alone_does_not()
    {
        await OpenTheSheet();

        await Services.Tick(RespiteServices.Laundry);
        await Expect(Line("services-laundry")).ToHaveTextAsync("Laundry arranged.");

        await Services.Clear(RespiteServices.Laundry);
        await Services.Tick(RespiteServices.Meals);
        await Expect(Line("services-laundry")).ToHaveTextAsync("No laundry.");
        AssertNoConsoleErrors();
    }

    // ── what the edit itself says ───────────────────────────────────────────

    [Test]
    public async Task editing_the_age_is_the_last_edit_and_editing_the_name_is_not()
    {
        await OpenTheSheet();

        await Type("Age", "63");
        await Expect(Line("edited-age")).ToHaveTextAsync("Age just edited.");

        await Type("Applicant", "Margaret Hale Jr");
        await Expect(Line("edited-age")).ToHaveTextAsync("Another fact edited.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_family_number_starting_555_is_local_and_one_with_an_area_code_is_not()
    {
        await OpenTheSheet();

        await Type("Contact phone", "555-0199");
        await Expect(Line("contact-local")).ToHaveTextAsync("A local family number.");

        await Type("Contact phone", "212-555-0199");
        await Expect(Line("contact-local")).ToHaveTextAsync("A long-distance family number.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_daughter_or_son_is_a_child_contact_and_a_niece_is_not()
    {
        await OpenTheSheet();

        await Type("Contact relation", "Son");
        await Expect(Line("contact-child")).ToHaveTextAsync("A child is the contact.");

        await Type("Contact relation", "Niece");
        await Expect(Line("contact-child")).ToHaveTextAsync("The contact is not a child.");
        AssertNoConsoleErrors();
    }

    // ── against the Garden wing: each fact compared with the wing's, read when the check runs ──

    [Test]
    public async Task the_wing_expects_margaret_hale_and_no_one_else()
    {
        await OpenTheSheet();

        await CheckTheWing();
        await Expect(Line("policy-expected")).ToHaveTextAsync("The applicant the wing expects.", new() { Timeout = 5000 });

        await Type("Applicant", "Harold Benson");
        await CheckTheWing();
        await Expect(Line("policy-expected")).ToHaveTextAsync("Not the applicant the wing expects.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_wings_minimum_age_is_met_at_62_and_not_at_61()
    {
        await OpenTheSheet();

        await CheckTheWing();
        await Expect(Line("policy-min-age")).ToHaveTextAsync("Meets the wing's minimum age.", new() { Timeout = 5000 });

        await Type("Age", "61");
        await CheckTheWing();
        await Expect(Line("policy-min-age")).ToHaveTextAsync("Under the wing's minimum age.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sixty_two_is_exactly_the_wings_minimum_age_and_sixty_three_is_not()
    {
        await OpenTheSheet();

        await CheckTheWing();
        await Expect(Line("policy-exact-age")).ToHaveTextAsync("Exactly the minimum age.", new() { Timeout = 5000 });

        await Type("Age", "63");
        await CheckTheWing();
        await Expect(Line("policy-exact-age")).ToHaveTextAsync("Not exactly the minimum age.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_quote_of_5000_is_the_wings_rate_and_a_cent_more_is_not()
    {
        await OpenTheSheet();

        await CheckTheWing();
        await Expect(Line("policy-rate")).ToHaveTextAsync("Quoted at the wing's rate.", new() { Timeout = 5000 });

        await Type("Monthly rate", "5000.01");
        await CheckTheWing();
        await Expect(Line("policy-rate")).ToHaveTextAsync("Quoted off the wing's rate.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_cent_over_the_wings_rate_is_over_it_and_the_rate_itself_is_not()
    {
        await OpenTheSheet();

        await CheckTheWing();
        await Expect(Line("policy-rate-over")).ToHaveTextAsync("Not over the wing's rate.", new() { Timeout = 5000 });

        await Type("Monthly rate", "5000.01");
        await CheckTheWing();
        await Expect(Line("policy-rate-over")).ToHaveTextAsync("Over the wing's rate.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_move_in_from_the_wings_opening_day_is_once_it_is_open_and_the_day_before_is_not()
    {
        await OpenTheSheet();

        await CheckTheWing();
        await Expect(Line("policy-opens")).ToHaveTextAsync("Moves in once the wing is open.", new() { Timeout = 5000 });

        await Type("Move-in date", "2026-06-30");
        await CheckTheWing();
        await Expect(Line("policy-opens")).ToHaveTextAsync("Moves in before the wing opens.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task only_the_wings_opening_day_is_its_opening_day()
    {
        await OpenTheSheet();

        await CheckTheWing();
        await Expect(Line("policy-opening-day")).ToHaveTextAsync("Moves in on the wing's opening day.", new() { Timeout = 5000 });

        await Type("Move-in date", "2026-07-02");
        await CheckTheWing();
        await Expect(Line("policy-opening-day")).ToHaveTextAsync("Not on the wing's opening day.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_veteran_matches_the_va_record_and_a_civilian_does_not()
    {
        await OpenTheSheet();

        await CheckTheWing();
        await Expect(Line("policy-veteran")).ToHaveTextAsync("Veteran status matches the VA record.", new() { Timeout = 5000 });

        await Field("Veteran").UncheckAsync();
        await CheckTheWing();
        await Expect(Line("policy-veteran")).ToHaveTextAsync("Veteran status differs from the VA record.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task assisted_living_fits_the_wing_and_memory_care_does_not()
    {
        await OpenTheSheet();

        await CheckTheWing();
        await Expect(Line("policy-care")).ToHaveTextAsync("The wing's care level fits.", new() { Timeout = 5000 });

        await Choose("Care level", "Memory care");
        await CheckTheWing();
        await Expect(Line("policy-care")).ToHaveTextAsync("The wing offers a different care level.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task no_preference_or_another_room_differs_from_the_room_offered_and_the_garden_view_is_it()
    {
        await OpenTheSheet();

        await CheckTheWing();
        await Expect(Line("policy-room")).ToHaveTextAsync("Prefers a different room than the one offered.", new() { Timeout = 5000 });

        await Type("Room preference", "Garden view");
        await CheckTheWing();
        await Expect(Line("policy-room")).ToHaveTextAsync("Happy with the room offered.", new() { Timeout = 5000 });

        await Type("Room preference", "Room 214");
        await CheckTheWing();
        await Expect(Line("policy-room")).ToHaveTextAsync("Prefers a different room than the one offered.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task one_pet_is_the_wings_limit_and_two_or_an_unasked_count_is_not()
    {
        await OpenTheSheet();

        await CheckTheWing();
        await Expect(Line("policy-pets")).ToHaveTextAsync("At the wing's pet limit.", new() { Timeout = 5000 });

        await Type("Pets", "2");
        await CheckTheWing();
        await Expect(Line("policy-pets")).ToHaveTextAsync("Not at the pet limit.", new() { Timeout = 5000 });

        await Type("Pets", "");
        await CheckTheWing();
        await Expect(Line("policy-pets")).ToHaveTextAsync("Not at the pet limit.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task two_pets_is_over_the_wings_limit_and_one_or_an_unasked_count_is_not()
    {
        await OpenTheSheet();

        await CheckTheWing();
        await Expect(Line("policy-pets-over")).ToHaveTextAsync("Within the pet limit, or not asked.", new() { Timeout = 5000 });

        await Type("Pets", "2");
        await CheckTheWing();
        await Expect(Line("policy-pets-over")).ToHaveTextAsync("Over the wing's pet limit.", new() { Timeout = 5000 });

        await Type("Pets", "");
        await CheckTheWing();
        await Expect(Line("policy-pets-over")).ToHaveTextAsync("Within the pet limit, or not asked.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task no_previous_stay_matches_a_record_of_none_and_a_dated_one_differs()
    {
        await OpenTheSheet();

        await CheckTheWing();
        await Expect(Line("policy-prior")).ToHaveTextAsync("The previous stay matches our record.", new() { Timeout = 5000 });

        await Type("Previous stay ended", "2025-12-31");
        await CheckTheWing();
        await Expect(Line("policy-prior")).ToHaveTextAsync("The previous stay differs from our record.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_clinic_recorded_a_flu_shot_so_only_yes_matches_its_record()
    {
        await OpenTheSheet();

        await CheckTheWing();
        await Expect(Line("policy-flu")).ToHaveTextAsync("Flu shot differs from the clinic's record.", new() { Timeout = 5000 });

        await Choose("Flu shot", "Yes");
        await CheckTheWing();
        await Expect(Line("policy-flu")).ToHaveTextAsync("Flu shot matches the clinic's record.", new() { Timeout = 5000 });

        await Choose("Flu shot", "No");
        await CheckTheWing();
        await Expect(Line("policy-flu")).ToHaveTextAsync("Flu shot differs from the clinic's record.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_dietitian_recorded_a_diabetic_diet_so_only_diabetic_matches_her_record()
    {
        await OpenTheSheet();

        await CheckTheWing();
        await Expect(Line("policy-diet")).ToHaveTextAsync("Diet differs from the dietitian's record.", new() { Timeout = 5000 });

        await Choose("Diet", "Diabetic");
        await CheckTheWing();
        await Expect(Line("policy-diet")).ToHaveTextAsync("Diet matches the dietitian's record.", new() { Timeout = 5000 });

        await Choose("Diet", "Pureed");
        await CheckTheWing();
        await Expect(Line("policy-diet")).ToHaveTextAsync("Diet differs from the dietitian's record.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
