using System.Collections.Concurrent;
using System.Linq.Expressions;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Validation.MoveInReview;

/// <summary>
/// As a move-in coordinator, I want the review form to tell me what is missing as I work, under the
/// same rules the server applies, so that a review I save is one the community accepts.
/// </summary>
/// <remarks>
/// The rules are written once in FluentValidation. The page checks every rule it can; a rule whose
/// condition only the server can decide (a returning resident's previous room) is answered by the
/// server, in the page's validation summary. The page sends every field it renders and, beside the
/// pharmacy phone, the pharmacy network the community contracts.
/// </remarks>
[TestFixture]
public class WhenCoordinatorReviewsMoveIn : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/MoveInReview";
    private const string Saved = "Move-in review saved for Margaret Hale.";
    private const string SavedWithPharmacy = Saved + " Prescriptions go to Riverside Health at 555-0100.";

    private PagePlan<MoveInReviewModel> _plan = null!;
    private ConcurrentQueue<string> _reviewsSent = null!;

    private ILocator SaveReview => Page.Locator("#save-review-btn");
    private ILocator ReviewStatus => Page.Locator("#review-status");
    private ILocator ValidationSummary => Page.Locator("[data-reactive-validation-summary]");

    private async Task NavigateAndBoot()
    {
        _reviewsSent = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains("/Sandbox/Validation/MoveInReview/Save")) _reviewsSent.Enqueue(request.Url);
        };
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<MoveInReviewModel>.FromPage(Page);
    }

    // Leaving the field lets its message settle, so the next click lands where it aims.
    private async Task Write(Expression<Func<MoveInReviewModel, object?>> field, string text)
    {
        await _plan.TextBox(field).Fill(text);
        await Page.Keyboard.PressAsync("Tab");
    }

    [Test]
    public async Task an_empty_review_asks_for_the_resident_and_the_score_before_anything_is_sent()
    {
        await NavigateAndBoot();

        await ClickWhenStable(SaveReview);

        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("'Resident Name' must not be empty.");
        await Expect(_plan.ErrorFor(m => m.FallRiskScore)).ToHaveTextAsync("Score the resident's fall risk.");
        await Expect(ReviewStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_reviewsSent, Is.Empty, "A review the page can see is incomplete is not sent.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_name_too_long_for_the_wristband_is_refused_by_the_server_in_its_own_words()
    {
        const string LongName = "Margaret Elizabeth Hale-Worthington";
        await NavigateAndBoot();
        await Write(m => m.ResidentName, LongName);
        await Write(m => m.FallRiskScore, "2");

        await ClickWhenStable(SaveReview);

        await Expect(_plan.ErrorFor(m => m.ResidentName))
            .ToHaveTextAsync($"{LongName} is longer than the 30 characters a wristband prints.");
        Assert.That(_reviewsSent, Has.Count.EqualTo(1), "A message written from the model is the server's to say.");
        await Write(m => m.ResidentName, "Margaret Hale");
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(Saved);
        Assert.That(_reviewsSent, Has.Count.EqualTo(2));
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task a_name_the_wristband_cannot_print_is_refused_with_every_reason_the_server_gives()
    {
        const string NameWithDigits = "Margaret Elizabeth Hale-Worthington 2nd";
        await NavigateAndBoot();
        await Write(m => m.ResidentName, NameWithDigits);
        await Write(m => m.FallRiskScore, "2");

        await ClickWhenStable(SaveReview);

        // The server refuses the name twice, too long and not letters only, and the page shows both reasons.
        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync(
            $"{NameWithDigits} is longer than the 30 characters a wristband prints. " +
            "A wristband prints letters only: remove the digits from the name.");
        Assert.That(_reviewsSent, Has.Count.EqualTo(1), "Both checks are the server's.");
        await Write(m => m.ResidentName, "Margaret Hale");
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(Saved);
        Assert.That(_reviewsSent, Has.Count.EqualTo(2));
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task a_fall_risk_score_outside_zero_to_ten_is_refused()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Margaret Hale");
        await Write(m => m.FallRiskScore, "11");

        await ClickWhenStable(SaveReview);

        await Expect(_plan.ErrorFor(m => m.FallRiskScore)).ToHaveTextAsync("Fall risk is scored 0 to 10.");
        await Write(m => m.FallRiskScore, "4");
        await Expect(_plan.ErrorFor(m => m.FallRiskScore)).ToBeHiddenAsync();
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(Saved);
        Assert.That(_reviewsSent, Has.Count.EqualTo(1), "Only the corrected review is sent.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_high_fall_risk_needs_a_mobility_plan()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Margaret Hale");
        await Write(m => m.FallRiskScore, "9");

        await ClickWhenStable(SaveReview);

        await Expect(_plan.ErrorFor(m => m.MobilityNotes)).ToHaveTextAsync("Describe the mobility plan for a high fall risk.");
        Assert.That(_reviewsSent, Is.Empty);
        await Write(m => m.MobilityNotes, "Walker at all times, two-person assist on stairs");
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(Saved);
        Assert.That(_reviewsSent, Has.Count.EqualTo(1));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_low_fall_risk_keeps_the_mobility_plan_short()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Margaret Hale");
        await Write(m => m.FallRiskScore, "3");
        await Write(m => m.MobilityNotes, "Walker at all times, two-person assist on stairs");

        await ClickWhenStable(SaveReview);

        await Expect(_plan.ErrorFor(m => m.MobilityNotes)).ToHaveTextAsync("Keep mobility notes under 40 characters.");
        Assert.That(_reviewsSent, Is.Empty);
        await Write(m => m.MobilityNotes, "Walker at all times");
        await Expect(_plan.ErrorFor(m => m.MobilityNotes)).ToBeHiddenAsync();
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(Saved);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_moderate_fall_risk_of_five_needs_a_reassessment_plan()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Margaret Hale");
        await Write(m => m.FallRiskScore, "5");

        await ClickWhenStable(SaveReview);

        await Expect(_plan.ErrorFor(m => m.ReassessmentPlan)).ToHaveTextAsync("A moderate fall risk needs a reassessment plan.");
        Assert.That(_reviewsSent, Is.Empty);
        await Write(m => m.ReassessmentPlan, "Reassess in 30 days with physical therapy");
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(Saved);
        Assert.That(_reviewsSent, Has.Count.EqualTo(1));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_fall_risk_of_seven_needs_a_reassessment_plan_and_eight_does_not()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Margaret Hale");
        await Write(m => m.FallRiskScore, "7");

        await ClickWhenStable(SaveReview);

        // Seven is the top of the moderate band, 5 to 7 inclusive.
        await Expect(_plan.ErrorFor(m => m.ReassessmentPlan)).ToHaveTextAsync("A moderate fall risk needs a reassessment plan.");
        Assert.That(_reviewsSent, Is.Empty);
        await Write(m => m.FallRiskScore, "8");
        await Write(m => m.MobilityNotes, "Walker at all times");
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(Saved);
        await Expect(_plan.ErrorFor(m => m.ReassessmentPlan)).ToHaveTextAsync("");
        Assert.That(_reviewsSent, Has.Count.EqualTo(1));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_long_mobility_plan_is_sent_and_the_server_asks_for_a_therapist_to_review_it()
    {
        const string LongPlan = "Walker at all times, two-person assist on stairs, gait belt for transfers";
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Margaret Hale");
        await Write(m => m.FallRiskScore, "9");
        await Write(m => m.MobilityNotes, LongPlan);

        await ClickWhenStable(SaveReview);

        // A condition on the plan's length has no browser form, so only the server asks, in the summary.
        await Expect(ValidationSummary).ToHaveTextAsync("A mobility plan this long needs a therapist to review it.");
        await Expect(_plan.ErrorFor(m => m.TherapistReviewer)).ToHaveTextAsync("");
        await Expect(ReviewStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_reviewsSent, Has.Count.EqualTo(1), "The page cannot decide this rule, so the server is asked.");

        await Write(m => m.TherapistReviewer, "Dana Ortiz, PT");
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(Saved);
        await Expect(ValidationSummary).ToBeHiddenAsync();
        Assert.That(_reviewsSent, Has.Count.EqualTo(2));
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task a_resident_in_a_wheelchair_needs_an_evacuation_assistant()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Margaret Hale");
        await Write(m => m.FallRiskScore, "2");
        await _plan.NativeCheckBox(m => m.UsesWheelchair).Check();

        await ClickWhenStable(SaveReview);

        await Expect(_plan.ErrorFor(m => m.EvacuationAssist)).ToHaveTextAsync("Name who helps the resident out in an evacuation.");
        Assert.That(_reviewsSent, Is.Empty);
        await Write(m => m.EvacuationAssist, "Night shift aide");
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(Saved);
        Assert.That(_reviewsSent, Has.Count.EqualTo(1));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_resident_on_oxygen_needs_an_evacuation_assistant()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Margaret Hale");
        await Write(m => m.FallRiskScore, "2");
        await _plan.NativeCheckBox(m => m.UsesOxygen).Check();

        await ClickWhenStable(SaveReview);

        // Either need is enough; a resident with neither is saved without an assistant in every other test.
        await Expect(_plan.ErrorFor(m => m.EvacuationAssist)).ToHaveTextAsync("Name who helps the resident out in an evacuation.");
        Assert.That(_reviewsSent, Is.Empty);
        await Write(m => m.EvacuationAssist, "Night shift aide");
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(Saved);
        Assert.That(_reviewsSent, Has.Count.EqualTo(1));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_room_on_the_memory_care_wing_needs_the_familys_secured_unit_consent()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Margaret Hale");
        await Write(m => m.FallRiskScore, "2");
        await Write(m => m.AssignedRoom, "MC-104");

        await ClickWhenStable(SaveReview);

        await Expect(_plan.ErrorFor(m => m.SecuredUnitConsentBy))
            .ToHaveTextAsync("Name the family member who consented to the secured unit.");
        Assert.That(_reviewsSent, Is.Empty);
        await Write(m => m.SecuredUnitConsentBy, "Ana Hale");
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(Saved);
        Assert.That(_reviewsSent, Has.Count.EqualTo(1), "A room on another wing, like MA-214, needs no consent.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_temporary_wristband_needs_the_reason_it_was_issued()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Margaret Hale");
        await Write(m => m.FallRiskScore, "2");
        await Write(m => m.WristbandCode, "WB-912345");

        await ClickWhenStable(SaveReview);

        await Expect(_plan.ErrorFor(m => m.TemporaryBandReason)).ToHaveTextAsync("Say why the resident wears a temporary wristband.");
        Assert.That(_reviewsSent, Is.Empty);
        await Write(m => m.TemporaryBandReason, "Band printer down at admission");
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(Saved);
        Assert.That(_reviewsSent, Has.Count.EqualTo(1), "A permanent band, like WB-104233, needs no reason.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_family_portal_link_must_be_a_web_address_and_may_be_left_blank()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Margaret Hale");
        await Write(m => m.FallRiskScore, "2");
        await Write(m => m.FamilyPortalLink, "ftp://portal");

        await ClickWhenStable(SaveReview);

        await Expect(_plan.ErrorFor(m => m.FamilyPortalLink)).ToHaveTextAsync("The family portal link must be a web address.");
        Assert.That(_reviewsSent, Is.Empty);
        await Write(m => m.FamilyPortalLink, "");
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(Saved);
        Assert.That(_reviewsSent, Has.Count.EqualTo(1));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_wristband_code_is_checked_the_way_the_communitys_printer_reads_it()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Margaret Hale");
        await Write(m => m.FallRiskScore, "2");
        await Write(m => m.WristbandCode, "104233");

        await ClickWhenStable(SaveReview);

        await Expect(_plan.ErrorFor(m => m.WristbandCode))
            .ToHaveTextAsync("Enter the wristband code as WB- and six digits, like WB-104233.");
        Assert.That(_reviewsSent, Is.Empty);
        await Write(m => m.WristbandCode, "WB-104233");
        await Expect(_plan.ErrorFor(m => m.WristbandCode)).ToBeHiddenAsync();
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(Saved);
        Assert.That(_reviewsSent, Has.Count.EqualTo(1));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_assigned_room_reads_the_same_in_any_case()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Margaret Hale");
        await Write(m => m.FallRiskScore, "2");
        await Write(m => m.AssignedRoom, "maple 214");

        await ClickWhenStable(SaveReview);

        await Expect(_plan.ErrorFor(m => m.AssignedRoom)).ToHaveTextAsync("Enter the room as wing and number, like MA-214.");
        Assert.That(_reviewsSent, Is.Empty);
        await Write(m => m.AssignedRoom, "ma-214");
        await Expect(_plan.ErrorFor(m => m.AssignedRoom)).ToBeHiddenAsync();
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(Saved);
        Assert.That(_reviewsSent, Has.Count.EqualTo(1), "The page and the server both ignore case.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_pharmacy_phone_is_asked_for_in_the_house_format()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Margaret Hale");
        await Write(m => m.FallRiskScore, "2");
        await Write(m => m.Pharmacy.Phone, "5550100");
        await Write(m => m.Pharmacy.Name, "Riverside Pharmacy");

        await ClickWhenStable(SaveReview);

        await Expect(_plan.ErrorFor(m => m.Pharmacy.Phone)).ToHaveTextAsync("Enter the pharmacy phone as 555-0100.");
        Assert.That(_reviewsSent, Is.Empty, "The nested object's rule runs in the page.");
        await Write(m => m.Pharmacy.Phone, "555-0100");
        await Expect(_plan.ErrorFor(m => m.Pharmacy.Phone)).ToBeHiddenAsync();
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(SavedWithPharmacy);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_pharmacy_phone_reaches_the_server_beside_the_communitys_pharmacy_network()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Margaret Hale");
        await Write(m => m.FallRiskScore, "2");
        await Write(m => m.Pharmacy.Phone, "555-0100");
        await Write(m => m.Pharmacy.Name, "Riverside Pharmacy");

        await ClickWhenStable(SaveReview);

        await Expect(ReviewStatus).ToHaveTextAsync(SavedWithPharmacy);
        Assert.That(_reviewsSent, Has.Count.EqualTo(1));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_pharmacy_phone_needs_the_name_of_the_pharmacy_it_reaches()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Margaret Hale");
        await Write(m => m.FallRiskScore, "2");
        await Write(m => m.Pharmacy.Phone, "555-0100");

        await ClickWhenStable(SaveReview);

        // The rule applies unless the phone is empty; the phone is given, so the pharmacy is asked for.
        await Expect(_plan.ErrorFor(m => m.Pharmacy.Name)).ToHaveTextAsync("Name the pharmacy for this phone number.");
        Assert.That(_reviewsSent, Is.Empty);
        await Write(m => m.Pharmacy.Name, "Riverside Pharmacy");
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(SavedWithPharmacy);
        Assert.That(_reviewsSent, Has.Count.EqualTo(1), "A review with no pharmacy phone needs no pharmacy name; the other saves prove it.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_forwarding_zip_is_checked_the_way_the_server_checks_it()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Margaret Hale");
        await Write(m => m.FallRiskScore, "2");
        await Write(m => m.ForwardingZip, "0213");

        await ClickWhenStable(SaveReview);

        // The field has no rule the page runs, so the server's answer shows in the page's summary.
        await Expect(ValidationSummary).ToContainTextAsync("Enter the forwarding ZIP as five digits.");
        Assert.That(_reviewsSent, Has.Count.EqualTo(1), "The pattern ends in .NET's \\z, so the server checks it.");
        await Write(m => m.ForwardingZip, "02139");
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(Saved);
        await Expect(ValidationSummary).ToBeHiddenAsync();
        Assert.That(_reviewsSent, Has.Count.EqualTo(2), "Five digits pass on the server, as the page let them through.");
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task a_rule_for_the_care_plan_summary_another_page_edits_does_not_hold_up_the_review()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Margaret Hale");
        await Write(m => m.FallRiskScore, "2");

        await ClickWhenStable(SaveReview);

        await Expect(ReviewStatus).ToHaveTextAsync(Saved);
        await Expect(ValidationSummary).ToBeHiddenAsync();
        Assert.That(_reviewsSent, Has.Count.EqualTo(1), "This page sends no care plan summary, which the length rule lets pass.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_server_asks_for_a_returning_residents_previous_room()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Margaret Hale");
        await Write(m => m.FallRiskScore, "2");
        await _plan.NativeCheckBox(m => m.IsReturning).Check();

        await ClickWhenStable(SaveReview);

        await Expect(ValidationSummary).ToBeVisibleAsync();
        await Expect(ValidationSummary).ToContainTextAsync("A returning resident's previous room is required.");
        await Expect(ReviewStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_reviewsSent, Has.Count.EqualTo(1), "The page cannot decide this rule, so the server is asked.");

        await Write(m => m.PreviousRoom, "Maple 214");
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(Saved);
        await Expect(ValidationSummary).ToBeHiddenAsync();
        Assert.That(_reviewsSent, Has.Count.EqualTo(2));
        AssertNoConsoleErrorsExcept("400");
    }
}
