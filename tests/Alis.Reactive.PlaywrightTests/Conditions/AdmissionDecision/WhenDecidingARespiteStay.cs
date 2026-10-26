using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.AdmissionDecision;

namespace Alis.Reactive.PlaywrightTests.Conditions.AdmissionDecision;

/// <summary>
/// As the admissions director, I want an applicant's stay decided from the rules as they combine, by the
/// first reason that fits, and only after the waiting list or I have answered where an answer is needed,
/// so that no stay is decided on half the facts or on a question nobody asked.
/// </summary>
/// <remarks>
/// Margaret Hale's facts sit on an intake sheet the sandbox wrote itself (BroughtComponents/IntakeSheet):
/// 62, a veteran, assisted living, second on the waiting list. The page combines them with the director's
/// approval and the bed offered: chains of and and or, a group inside each, a negated group, a group two
/// deep, and a ladder whose first three reasons all fit a 95-year-old with approval. The waiting list
/// answers a moment after it is asked and the sheet counts each time it is asked, so the count shows
/// when a term that waits was reached and when an earlier term settled the matter without it: inside an
/// or, inside a negated group and as a reason after the first. The director is asked the same three
/// ways through the page's confirmation, and may decline. The letter asks the sheet for its summary and
/// the list for its answer once each, though it shows both twice.
/// </remarks>
[TestFixture]
public class WhenDecidingARespiteStay : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/AdmissionDecision";

    private PagePlan<AdmissionDecisionModel> _plan = null!;

    private ILocator Sheet => Page.Locator($"#{AdmissionDecisionModel.SheetId}");
    private ILocator WaitlistChecks => Sheet.Locator(".is-waitlist-checks");
    private ILocator SummariesWritten => Sheet.Locator(".is-summaries");
    private NativeCheckBoxLocator Approved => _plan.NativeCheckBox(m => m.DirectorApproved);
    private NativeDropDownLocator Bed => _plan.NativeDropDown(m => m.Bed);
    private ILocator ConfirmDialog => Page.Locator("#alisConfirmDialog");

    private ILocator Decide => Page.Locator("#decide");
    private ILocator AdmitTopThree => Page.Locator("#decide-top-three");
    private ILocator AssignNearTheTop => Page.Locator("#decide-list-depth");
    private ILocator DecideAskingTheList => Page.Locator("#decide-ladder-list");
    private ILocator AdmitAskingIfUnapproved => Page.Locator("#decide-ask");
    private ILocator AssignTheBed => Page.Locator("#assign-bed");
    private ILocator DecideProvisional => Page.Locator("#decide-provisional");
    private ILocator WriteTheLetter => Page.Locator("#write-letter");

    private ILocator Field(string label) => Sheet.GetByLabel(label, new() { Exact = true });

    private ILocator Line(string id) => Page.Locator($"#{id}");

    private async Task OpenTheDecision()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<AdmissionDecisionModel>.FromPage(Page);
        await Expect(WaitlistChecks).ToHaveTextAsync("Waitlist checks: 0");
    }

    private async Task Type(string label, string text)
    {
        var field = Field(label);
        await field.FillAsync(text);
        await field.BlurAsync();
    }

    private async Task Choose(string label, string option) =>
        await Field(label).SelectOptionAsync(new SelectOptionValue { Label = option });

    private async Task Answer(string button) =>
        await ClickWhenStable(ConfirmDialog.GetByRole(AriaRole.Button, new() { Name = button }));

    // ── how the rules combine ───────────────────────────────────────────────

    [Test]
    public async Task of_age_with_a_bed_outside_memory_care_is_approved_and_without_a_bed_or_in_memory_care_is_not()
    {
        await OpenTheDecision();

        await Decide.ClickAsync();
        await Expect(Line("decision-and")).ToHaveTextAsync("Not yet approved.");

        await Bed.Select(Beds.Garden);
        await Decide.ClickAsync();
        await Expect(Line("decision-and")).ToHaveTextAsync("Approved: of age, a bed is free, and the Garden wing can care for her.");

        await Choose("Care level", "Memory care");
        await Decide.ClickAsync();
        await Expect(Line("decision-and")).ToHaveTextAsync("Not yet approved.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task approval_or_first_on_the_list_admits_now_and_second_without_approval_waits()
    {
        await OpenTheDecision();

        await Decide.ClickAsync();
        await Expect(Line("decision-or")).ToHaveTextAsync("Wait: neither approved nor first on the list.");

        await Type("Waiting list number", "1");
        await Decide.ClickAsync();
        await Expect(Line("decision-or")).ToHaveTextAsync("Admit now: approved, or first on the list.");

        await Type("Waiting list number", "");
        await Approved.Check();
        await Decide.ClickAsync();
        await Expect(Line("decision-or")).ToHaveTextAsync("Admit now: approved, or first on the list.");
        AssertNoConsoleErrors();
    }

    // A group inside an and: veteran, and (90 or over, or memory care).
    [Test]
    public async Task a_veteran_of_ninety_or_in_memory_care_has_priority_needs_and_a_civilian_does_not()
    {
        await OpenTheDecision();

        await Decide.ClickAsync();
        await Expect(Line("decision-group-in-and")).ToHaveTextAsync("No veteran priority.");

        await Type("Age", "90");
        await Decide.ClickAsync();
        await Expect(Line("decision-group-in-and")).ToHaveTextAsync("A veteran with priority needs.");

        await Type("Age", "62");
        await Choose("Care level", "Memory care");
        await Decide.ClickAsync();
        await Expect(Line("decision-group-in-and")).ToHaveTextAsync("A veteran with priority needs.");

        await Field("Veteran").UncheckAsync();
        await Decide.ClickAsync();
        await Expect(Line("decision-group-in-and")).ToHaveTextAsync("No veteran priority.");
        AssertNoConsoleErrors();
    }

    // A group inside an or: approved, or (of age and a bed).
    [Test]
    public async Task approval_or_an_applicant_of_age_with_a_bed_clears_the_admission()
    {
        await OpenTheDecision();

        await Decide.ClickAsync();
        await Expect(Line("decision-group-in-or")).ToHaveTextAsync("Not cleared.");

        await Bed.Select(Beds.Garden);
        await Decide.ClickAsync();
        await Expect(Line("decision-group-in-or")).ToHaveTextAsync("Cleared to admit.");

        await Type("Age", "61");
        await Decide.ClickAsync();
        await Expect(Line("decision-group-in-or")).ToHaveTextAsync("Not cleared.");

        await Approved.Check();
        await Decide.ClickAsync();
        await Expect(Line("decision-group-in-or")).ToHaveTextAsync("Cleared to admit.");
        AssertNoConsoleErrors();
    }

    // A negated group: a bed, and not (memory care and the Garden bed).
    [Test]
    public async Task a_garden_bed_suits_every_level_but_memory_care_and_a_maple_bed_suits_memory_care()
    {
        await OpenTheDecision();

        await Bed.Select(Beds.Garden);
        await Decide.ClickAsync();
        await Expect(Line("decision-not-group")).ToHaveTextAsync("The bed suits her care level.");

        await Choose("Care level", "Memory care");
        await Decide.ClickAsync();
        await Expect(Line("decision-not-group")).ToHaveTextAsync("The bed does not suit her care level.");

        await Bed.Select(Beds.Maple);
        await Decide.ClickAsync();
        await Expect(Line("decision-not-group")).ToHaveTextAsync("The bed suits her care level.");

        await Bed.Select("-- No bed --");
        await Decide.ClickAsync();
        await Expect(Line("decision-not-group")).ToHaveTextAsync("The bed does not suit her care level.");
        AssertNoConsoleErrors();
    }

    // A group two deep: of age, and (approved, or (a bed and a place in the top three)).
    [Test]
    public async Task placement_needs_eligibility_and_either_approval_or_a_bed_with_a_place_near_the_top_of_the_list()
    {
        await OpenTheDecision();

        await Decide.ClickAsync();
        await Expect(Line("decision-nested")).ToHaveTextAsync("Cannot be placed yet.");

        await Bed.Select(Beds.Garden);
        await Decide.ClickAsync();
        await Expect(Line("decision-nested")).ToHaveTextAsync("Can be placed: approved, or a bed with a place near the top of the list.");

        await Type("Waiting list number", "4");
        await Decide.ClickAsync();
        await Expect(Line("decision-nested")).ToHaveTextAsync("Cannot be placed yet.");

        await Approved.Check();
        await Decide.ClickAsync();
        await Expect(Line("decision-nested")).ToHaveTextAsync("Can be placed: approved, or a bed with a place near the top of the list.");

        await Type("Age", "61");
        await Decide.ClickAsync();
        await Expect(Line("decision-nested")).ToHaveTextAsync("Cannot be placed yet.");
        AssertNoConsoleErrors();
    }

    // Not on one comparison inside a group: a Garden bed, and not memory care.
    [Test]
    public async Task a_garden_bed_is_for_a_resident_outside_memory_care()
    {
        await OpenTheDecision();

        await Bed.Select(Beds.Garden);
        await Decide.ClickAsync();
        await Expect(Line("decision-garden")).ToHaveTextAsync("A Garden bed for a resident the Garden wing can care for.");

        await Choose("Care level", "Memory care");
        await Decide.ClickAsync();
        await Expect(Line("decision-garden")).ToHaveTextAsync("Not a Garden bed, or needs the secured wing.");

        await Choose("Care level", "Assisted living");
        await Bed.Select(Beds.Maple);
        await Decide.ClickAsync();
        await Expect(Line("decision-garden")).ToHaveTextAsync("Not a Garden bed, or needs the secured wing.");
        AssertNoConsoleErrors();
    }

    // ── the first reason that fits ──────────────────────────────────────────

    [Test]
    public async Task the_first_reason_that_fits_decides_ninety_and_over_before_approval_before_eligibility()
    {
        await OpenTheDecision();
        await Approved.Check();

        await Type("Age", "95");
        await Decide.ClickAsync();
        await Expect(Line("decision-reason")).ToHaveTextAsync("Priority: 90 and over.");

        await Type("Age", "70");
        await Decide.ClickAsync();
        await Expect(Line("decision-reason")).ToHaveTextAsync("Approved by the director.");

        await Approved.Input.UncheckAsync();
        await Decide.ClickAsync();
        await Expect(Line("decision-reason")).ToHaveTextAsync("Eligible: awaiting the director.");

        await Type("Age", "61");
        await Decide.ClickAsync();
        await Expect(Line("decision-reason")).ToHaveTextAsync("Under age.");
        AssertNoConsoleErrors();
    }

    // ── the waiting list: a term that waits ─────────────────────────────────

    [Test]
    public async Task an_approved_applicant_is_admitted_without_asking_the_waiting_list()
    {
        await OpenTheDecision();
        await Approved.Check();

        await AdmitTopThree.ClickAsync();

        await Expect(Line("waitlist-or")).ToHaveTextAsync("Admit now: approved, or in the top three.");
        await Expect(WaitlistChecks).ToHaveTextAsync("Waitlist checks: 0");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task without_approval_the_top_three_on_the_list_are_admitted_once_the_list_answers_and_fourth_waits()
    {
        await OpenTheDecision();
        await Type("Waiting list number", "3");

        await AdmitTopThree.ClickAsync();
        await Expect(Line("waitlist-or")).ToHaveTextAsync("Admit now: approved, or in the top three.", new() { Timeout = 5000 });
        await Expect(WaitlistChecks).ToHaveTextAsync("Waitlist checks: 1");

        await Type("Waiting list number", "4");
        await AdmitTopThree.ClickAsync();
        await Expect(Line("waitlist-or")).ToHaveTextAsync("Wait for the list.", new() { Timeout = 5000 });
        await Expect(WaitlistChecks).ToHaveTextAsync("Waitlist checks: 2");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_bed_with_a_place_within_the_first_ten_assigns_it_and_the_eleventh_place_is_far_down()
    {
        await OpenTheDecision();
        await Bed.Select(Beds.Garden);
        await Type("Waiting list number", "10");

        await AssignNearTheTop.ClickAsync();
        await Expect(Line("waitlist-not")).ToHaveTextAsync("A bed, and not far down the list.", new() { Timeout = 5000 });
        await Expect(WaitlistChecks).ToHaveTextAsync("Waitlist checks: 1");

        await Type("Waiting list number", "11");
        await AssignNearTheTop.ClickAsync();
        await Expect(Line("waitlist-not")).ToHaveTextAsync("No bed, or far down the list.", new() { Timeout = 5000 });
        await Expect(WaitlistChecks).ToHaveTextAsync("Waitlist checks: 2");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task without_a_bed_the_list_is_not_asked_how_far_down_she_is()
    {
        await OpenTheDecision();

        await AssignNearTheTop.ClickAsync();

        await Expect(Line("waitlist-not")).ToHaveTextAsync("No bed, or far down the list.");
        await Expect(WaitlistChecks).ToHaveTextAsync("Waitlist checks: 0");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_decision_asks_the_list_only_when_approval_fails_and_waits_for_its_answer()
    {
        await OpenTheDecision();
        await Approved.Check();

        await DecideAskingTheList.ClickAsync();
        await Expect(Line("waitlist-ladder")).ToHaveTextAsync("Approved: admit.");
        await Expect(WaitlistChecks).ToHaveTextAsync("Waitlist checks: 0");

        await Approved.Input.UncheckAsync();
        await Type("Waiting list number", "3");
        await DecideAskingTheList.ClickAsync();
        await Expect(Line("waitlist-ladder")).ToHaveTextAsync("Top three on the list: admit soon.", new() { Timeout = 5000 });
        await Expect(WaitlistChecks).ToHaveTextAsync("Waitlist checks: 1");

        await Type("Waiting list number", "4");
        await DecideAskingTheList.ClickAsync();
        await Expect(Line("waitlist-ladder")).ToHaveTextAsync("Wait for the list.", new() { Timeout = 5000 });
        await Expect(WaitlistChecks).ToHaveTextAsync("Waitlist checks: 2");
        AssertNoConsoleErrors();
    }

    // The list has not answered: no number is not in the top three.
    [Test]
    public async Task with_no_answer_from_the_list_the_decision_waits()
    {
        await OpenTheDecision();
        await Type("Waiting list number", "");

        await DecideAskingTheList.ClickAsync();

        await Expect(Line("waitlist-ladder")).ToHaveTextAsync("Wait for the list.", new() { Timeout = 5000 });
        await Expect(WaitlistChecks).ToHaveTextAsync("Waitlist checks: 1");
        AssertNoConsoleErrors();
    }

    // ── asking the director ─────────────────────────────────────────────────

    [Test]
    public async Task without_approval_the_director_is_asked_and_yes_admits()
    {
        await OpenTheDecision();

        await AdmitAskingIfUnapproved.ClickAsync();
        await Expect(ConfirmDialog).ToContainTextAsync("Admit without the director's approval?");
        await Expect(Line("ask-or")).ToHaveTextAsync("");

        await Answer("OK");

        await Expect(Line("ask-or")).ToHaveTextAsync("Admitted.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task declining_to_admit_without_approval_leaves_the_applicant_not_admitted()
    {
        await OpenTheDecision();
        await AdmitAskingIfUnapproved.ClickAsync();
        await Expect(ConfirmDialog).ToContainTextAsync("Admit without the director's approval?");

        await Answer("Cancel");

        await Expect(Line("ask-or")).ToHaveTextAsync("Not admitted.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_approved_applicant_is_admitted_without_a_question()
    {
        await OpenTheDecision();
        await Approved.Check();

        await AdmitAskingIfUnapproved.ClickAsync();

        await Expect(Line("ask-or")).ToHaveTextAsync("Admitted.");
        await Expect(ConfirmDialog).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // Declining the question makes the negated group hold: the bed is Margaret's.
    [Test]
    public async Task declining_to_hold_the_bed_for_someone_else_assigns_it_to_margaret()
    {
        await OpenTheDecision();
        await Bed.Select(Beds.Garden);

        await AssignTheBed.ClickAsync();
        await Expect(ConfirmDialog).ToContainTextAsync("Hold this bed for someone else instead?");
        await Answer("Cancel");

        await Expect(Line("ask-not")).ToHaveTextAsync("Bed assigned to Margaret.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task holding_the_bed_for_someone_else_leaves_margaret_without_it()
    {
        await OpenTheDecision();
        await Bed.Select(Beds.Garden);

        await AssignTheBed.ClickAsync();
        await Expect(ConfirmDialog).ToContainTextAsync("Hold this bed for someone else instead?");
        await Answer("OK");

        await Expect(Line("ask-not")).ToHaveTextAsync("Bed held for someone else, or no bed.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task with_no_bed_nobody_is_asked_about_holding_one()
    {
        await OpenTheDecision();

        await AssignTheBed.ClickAsync();

        await Expect(Line("ask-not")).ToHaveTextAsync("Bed held for someone else, or no bed.");
        await Expect(ConfirmDialog).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task without_approval_but_with_a_bed_a_provisional_admission_is_offered_and_accepted()
    {
        await OpenTheDecision();
        await Bed.Select(Beds.Garden);

        await DecideProvisional.ClickAsync();
        await Expect(ConfirmDialog).ToContainTextAsync("Admit on a provisional basis?");
        await Answer("OK");

        await Expect(Line("ask-ladder")).ToHaveTextAsync("Admitted provisionally.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task turning_down_the_provisional_admission_declines_the_stay_for_now()
    {
        await OpenTheDecision();
        await Bed.Select(Beds.Garden);

        await DecideProvisional.ClickAsync();
        await Expect(ConfirmDialog).ToContainTextAsync("Admit on a provisional basis?");
        await Answer("Cancel");

        await Expect(Line("ask-ladder")).ToHaveTextAsync("Declined for now.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_approved_applicant_is_admitted_without_the_provisional_question()
    {
        await OpenTheDecision();
        await Approved.Check();
        await Bed.Select(Beds.Garden);

        await DecideProvisional.ClickAsync();

        await Expect(Line("ask-ladder")).ToHaveTextAsync("Approved: admit.");
        await Expect(ConfirmDialog).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task without_a_bed_no_provisional_admission_is_offered()
    {
        await OpenTheDecision();

        await DecideProvisional.ClickAsync();

        await Expect(Line("ask-ladder")).ToHaveTextAsync("Declined for now.");
        await Expect(ConfirmDialog).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // ── the letter: asked once, shown twice ─────────────────────────────────

    [Test]
    public async Task the_summary_is_written_once_though_the_letter_shows_it_twice()
    {
        await OpenTheDecision();
        await Expect(SummariesWritten).ToHaveTextAsync("Summaries written: 0");

        await WriteTheLetter.ClickAsync();

        await Expect(Line("summary-line")).ToHaveTextAsync("Margaret Hale, 62, assisted, moving in 2026-07-01");
        await Expect(Line("letter-opening")).ToHaveTextAsync("Margaret Hale, 62, assisted, moving in 2026-07-01");
        await Expect(SummariesWritten).ToHaveTextAsync("Summaries written: 1");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_list_is_asked_once_though_the_letter_uses_its_answer_twice()
    {
        await OpenTheDecision();

        await WriteTheLetter.ClickAsync();

        await Expect(Line("letter-near")).ToHaveTextAsync("Near the top of the list.", new() { Timeout = 5000 });
        await Expect(Line("letter-position")).ToHaveTextAsync("2");
        await Expect(WaitlistChecks).ToHaveTextAsync("Waitlist checks: 1");

        await Type("Waiting list number", "7");
        await WriteTheLetter.ClickAsync();
        await Expect(Line("letter-near")).ToHaveTextAsync("Further down the list.", new() { Timeout = 5000 });
        await Expect(Line("letter-position")).ToHaveTextAsync("7");
        await Expect(WaitlistChecks).ToHaveTextAsync("Waitlist checks: 2");
        AssertNoConsoleErrors();
    }
}
