using System.Collections.Concurrent;
using System.Linq.Expressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Validation.DischargePlanning;

/// <summary>
/// As a social worker, I want the discharge plan to ask only for what the answers around it call for,
/// and to stop asking when those answers change, so that the plan I save is complete for the resident
/// it describes.
/// </summary>
/// <remarks>
/// Every form a condition can take gates one rule on this plan. Each test turns a condition on with
/// a real answer, sees the rule asked for before anything is sent, then turns the condition off or
/// satisfies the rule and sees the server save the plan. The server runs the same validator, so a
/// condition the page decided wrongly would come back as a refusal.
/// </remarks>
[TestFixture]
public class WhenSocialWorkerPlansDischarge : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/DischargePlanning";
    private const string Saved = "Discharge plan saved for Helen Park.";

    private PagePlan<DischargePlanModel> _plan = null!;
    private ConcurrentQueue<string> _plansSent = null!;

    private ILocator SavePlan => Page.Locator("#save-discharge-plan-btn");
    private ILocator PlanStatus => Page.Locator("#discharge-plan-status");
    private NativeDropDownLocator Destination => _plan.NativeDropDown(m => m.Destination);
    private NativeCheckBoxLocator GoingHome => _plan.NativeCheckBox(m => m.GoingHome);
    private NativeCheckBoxLocator LivesAlone => _plan.NativeCheckBox(m => m.LivesAlone);
    private NativeCheckBoxLocator HasPets => _plan.NativeCheckBox(m => m.HasPets);
    private NativeCheckBoxLocator NeedsTransport => _plan.NativeCheckBox(m => m.NeedsTransport);
    private NativeCheckBoxLocator PharmacistReviewed => _plan.NativeCheckBox(m => m.PharmacistReviewed);
    private NativeCheckListLocator Equipment => new(Page, IdGenerator.For<DischargePlanModel, List<string>>(m => m.Equipment));

    private async Task BeginThePlan()
    {
        _plansSent = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.EndsWith("/Sandbox/Validation/DischargePlanning/Save")) _plansSent.Enqueue(request.Url);
        };
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<DischargePlanModel>.FromPage(Page);
        await Write(m => m.ResidentName, "Helen Park");
    }

    // Leaving the field itself lets its message settle, and leaves no other field to settle under the
    // next click.
    private async Task Write(Expression<Func<DischargePlanModel, object?>> field, string text) =>
        await _plan.TextBox(field).FillAndBlur(text);

    [Test]
    public async Task a_resident_going_home_needs_a_home_address()
    {
        await BeginThePlan();
        await GoingHome.Check();

        await ClickWhenStable(SavePlan);
        await Expect(_plan.ErrorFor(m => m.HomeAddress)).ToHaveTextAsync("Enter the home address the resident goes to.");
        await Expect(PlanStatus).ToHaveTextAsync("No plan saved yet.");
        Assert.That(_plansSent, Has.Count.EqualTo(0), "The page asked for the answer, so nothing was sent.");

        await Write(m => m.HomeAddress, "12 Elm Street, Riverside");
        await ClickWhenStable(SavePlan);
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(1), "The page let the plan through, and the server accepted it.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_resident_not_going_home_has_no_home_address_to_record()
    {
        await BeginThePlan();
        await Write(m => m.HomeAddress, "12 Elm Street, Riverside");

        await ClickWhenStable(SavePlan);
        await Expect(_plan.ErrorFor(m => m.HomeAddress)).ToHaveTextAsync("Only a resident going home has a home address to record.");
        await Expect(PlanStatus).ToHaveTextAsync("No plan saved yet.");
        Assert.That(_plansSent, Has.Count.EqualTo(0), "The page asked for the answer, so nothing was sent.");

        await GoingHome.Check();
        await ClickWhenStable(SavePlan);
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(1), "The page let the plan through, and the server accepted it.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_resident_going_home_alone_needs_a_daily_check_in()
    {
        await BeginThePlan();
        await GoingHome.Check();
        await Write(m => m.HomeAddress, "12 Elm Street, Riverside");
        await LivesAlone.Check();

        await ClickWhenStable(SavePlan);
        await Expect(_plan.ErrorFor(m => m.DailyCheckInBy)).ToHaveTextAsync("Name who checks in daily on a resident living alone.");
        await Expect(PlanStatus).ToHaveTextAsync("No plan saved yet.");
        Assert.That(_plansSent, Has.Count.EqualTo(0), "The page asked for the answer, so nothing was sent.");

        // Going home, but not alone: one of the two answers is enough to drop the rule.
        await LivesAlone.Input.UncheckAsync();
        await ClickWhenStable(SavePlan);
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(1), "The page let the plan through, and the server accepted it.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_resident_going_home_with_pets_needs_a_pet_care_plan_and_pets_alone_do_not()
    {
        await BeginThePlan();
        await GoingHome.Check();
        await Write(m => m.HomeAddress, "12 Elm Street, Riverside");
        await HasPets.Check();

        await ClickWhenStable(SavePlan);
        await Expect(_plan.ErrorFor(m => m.PetCarePlan)).ToHaveTextAsync("Say who cares for the pets.");
        await Expect(PlanStatus).ToHaveTextAsync("No plan saved yet.");
        Assert.That(_plansSent, Has.Count.EqualTo(0), "The page asked for the answer, so nothing was sent.");

        // The pets stay ticked; the resident no longer goes home, so the inner condition counts for nothing.
        await GoingHome.Input.UncheckAsync();
        await Write(m => m.HomeAddress, "");
        await ClickWhenStable(SavePlan);
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(1), "The page let the plan through, and the server accepted it.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task no_follow_up_means_no_clinic_and_a_follow_up_needs_its_clinic()
    {
        await BeginThePlan();
        await Write(m => m.FollowUpClinic, "Riverside Clinic");
        await ClickWhenStable(SavePlan);
        await Expect(_plan.ErrorFor(m => m.FollowUpClinic)).ToHaveTextAsync("A clinic is named only when a follow-up is due.");
        await Expect(PlanStatus).ToHaveTextAsync("No plan saved yet.");
        Assert.That(_plansSent, Has.Count.EqualTo(0), "The page asked for the answer, so nothing was sent.");

        await Write(m => m.FollowUpDays, "14");
        await ClickWhenStable(SavePlan);
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(1), "The page let the plan through, and the server accepted it.");

        await Write(m => m.FollowUpClinic, "");
        await ClickWhenStable(SavePlan);
        await Expect(_plan.ErrorFor(m => m.FollowUpClinic)).ToHaveTextAsync("Name the clinic for the follow-up.");
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(1), "The page asked for the answer, so nothing was sent.");

        await Write(m => m.FollowUpClinic, "Riverside Clinic");
        await ClickWhenStable(SavePlan);
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(2), "The page let the plan through, and the server accepted it.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_va_clinic_needs_the_veterans_case_number()
    {
        await BeginThePlan();
        await Write(m => m.FollowUpDays, "14");
        await Write(m => m.FollowUpClinic, "VA Portland");

        await ClickWhenStable(SavePlan);
        await Expect(_plan.ErrorFor(m => m.VaCaseNumber)).ToHaveTextAsync("A VA clinic needs the veteran's case number.");
        await Expect(PlanStatus).ToHaveTextAsync("No plan saved yet.");
        Assert.That(_plansSent, Has.Count.EqualTo(0), "The page asked for the answer, so nothing was sent.");

        await Write(m => m.FollowUpClinic, "Riverside Clinic");
        await ClickWhenStable(SavePlan);
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(1), "The page let the plan through, and the server accepted it.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_caregiver_phone_needs_a_caregiver_and_a_caregiver_needs_a_phone()
    {
        await BeginThePlan();
        await Write(m => m.CaregiverPhone, "555-0100");
        await ClickWhenStable(SavePlan);
        await Expect(_plan.ErrorFor(m => m.CaregiverPhone)).ToHaveTextAsync("A caregiver phone needs a caregiver.");
        await Expect(PlanStatus).ToHaveTextAsync("No plan saved yet.");
        Assert.That(_plansSent, Has.Count.EqualTo(0), "The page asked for the answer, so nothing was sent.");

        await Write(m => m.PrimaryCaregiver, "Ana Park");
        await ClickWhenStable(SavePlan);
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(1), "The page let the plan through, and the server accepted it.");

        await Write(m => m.CaregiverPhone, "");
        await ClickWhenStable(SavePlan);
        await Expect(_plan.ErrorFor(m => m.CaregiverPhone)).ToHaveTextAsync("Enter the caregiver's phone.");
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(1), "The page asked for the answer, so nothing was sent.");

        await Write(m => m.CaregiverPhone, "555-0100");
        await ClickWhenStable(SavePlan);
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(2), "The page let the plan through, and the server accepted it.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_transfer_to_skilled_nursing_needs_the_bed_confirmation()
    {
        await BeginThePlan();
        await Destination.Select("Skilled nursing");
        await Write(m => m.TransferSummary, "Stable, walks with a walker");
        await Write(m => m.ReceivingNurse, "Dana Ortiz");

        await ClickWhenStable(SavePlan);
        await Expect(_plan.ErrorFor(m => m.BedConfirmation)).ToHaveTextAsync("Enter the bed confirmation from the nursing facility.");
        await Expect(PlanStatus).ToHaveTextAsync("No plan saved yet.");
        Assert.That(_plansSent, Has.Count.EqualTo(0), "The page asked for the answer, so nothing was sent.");

        await Write(m => m.BedConfirmation, "SNF-4471");
        await ClickWhenStable(SavePlan);
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(1), "The page let the plan through, and the server accepted it.");

        // Another destination asks for none of it.
        await Destination.Select("Hospital");
        await Write(m => m.BedConfirmation, "");
        await Write(m => m.TransferSummary, "");
        await Write(m => m.ReceivingNurse, "");
        await ClickWhenStable(SavePlan);
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(2), "The page let the plan through, and the server accepted it.");
        AssertNoConsoleErrors();
    }

    // The transfer destinations are a list the condition holds.
    [Test]
    public async Task a_transfer_to_hospice_or_skilled_nursing_needs_the_receiving_nurse_and_a_hospital_does_not()
    {
        await BeginThePlan();
        await Destination.Select("Hospice");
        await Write(m => m.TransferSummary, "Comfort care, family at the bedside");

        await ClickWhenStable(SavePlan);
        await Expect(_plan.ErrorFor(m => m.ReceivingNurse)).ToHaveTextAsync("Name the nurse receiving the resident.");
        await Expect(PlanStatus).ToHaveTextAsync("No plan saved yet.");
        Assert.That(_plansSent, Has.Count.EqualTo(0), "The page asked for the answer, so nothing was sent.");

        await Destination.Select("Hospital");
        await Write(m => m.TransferSummary, "");
        await ClickWhenStable(SavePlan);
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(1), "The page let the plan through, and the server accepted it.");
        AssertNoConsoleErrors();
    }

    // The same destinations as an array: the server asks for the summary, and so should the page.
    [Test]
    public async Task a_transfer_to_hospice_or_skilled_nursing_needs_the_nursing_summary_and_a_hospital_does_not()
    {
        await BeginThePlan();
        await Destination.Select("Hospice");
        await Write(m => m.ReceivingNurse, "Dana Ortiz");

        await ClickWhenStable(SavePlan);
        await Expect(_plan.ErrorFor(m => m.TransferSummary)).ToHaveTextAsync("A transfer needs the nursing summary.");
        await Expect(PlanStatus).ToHaveTextAsync("No plan saved yet.");
        Assert.That(_plansSent, Has.Count.EqualTo(0), "The page asked for the answer, so nothing was sent.");

        await Destination.Select("Hospital");
        await ClickWhenStable(SavePlan);
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(1), "The page let the plan through, and the server accepted it.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task only_a_resident_going_home_has_a_home_health_agency()
    {
        await BeginThePlan();
        await Destination.Select("Hospital");
        await Write(m => m.HomeHealthAgency, "Visiting Nurses");

        await ClickWhenStable(SavePlan);
        await Expect(_plan.ErrorFor(m => m.HomeHealthAgency)).ToHaveTextAsync("Only a resident going home has a home health agency.");
        await Expect(PlanStatus).ToHaveTextAsync("No plan saved yet.");
        Assert.That(_plansSent, Has.Count.EqualTo(0), "The page asked for the answer, so nothing was sent.");

        await Destination.Select("Home");
        await ClickWhenStable(SavePlan);
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(1), "The page let the plan through, and the server accepted it.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_transport_company_is_named_only_when_transport_is_needed()
    {
        await BeginThePlan();
        await Write(m => m.TransportCompany, "MedRide");
        await ClickWhenStable(SavePlan);
        await Expect(_plan.ErrorFor(m => m.TransportCompany)).ToHaveTextAsync("No transport is booked for a resident the family drives.");
        await Expect(PlanStatus).ToHaveTextAsync("No plan saved yet.");
        Assert.That(_plansSent, Has.Count.EqualTo(0), "The page asked for the answer, so nothing was sent.");

        await NeedsTransport.Check();
        await ClickWhenStable(SavePlan);
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(1), "The page let the plan through, and the server accepted it.");

        await Write(m => m.TransportCompany, "");
        await ClickWhenStable(SavePlan);
        await Expect(_plan.ErrorFor(m => m.TransportCompany)).ToHaveTextAsync("Name the transport company.");
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(1), "The page asked for the answer, so nothing was sent.");

        await Write(m => m.TransportCompany, "MedRide");
        await ClickWhenStable(SavePlan);
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(2), "The page let the plan through, and the server accepted it.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task stairs_at_home_need_a_plan_for_them()
    {
        await BeginThePlan();
        await Write(m => m.StairsAtHome, "1");

        await ClickWhenStable(SavePlan);
        await Expect(_plan.ErrorFor(m => m.StairPlan)).ToHaveTextAsync("Say how the resident manages the stairs.");
        await Expect(PlanStatus).ToHaveTextAsync("No plan saved yet.");
        Assert.That(_plansSent, Has.Count.EqualTo(0), "The page asked for the answer, so nothing was sent.");

        await Write(m => m.StairsAtHome, "0");
        await ClickWhenStable(SavePlan);
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(1), "The page let the plan through, and the server accepted it.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task repeated_falls_or_many_stairs_need_a_home_safety_visit()
    {
        await BeginThePlan();
        await Write(m => m.FallsLast90Days, "2");
        await ClickWhenStable(SavePlan);
        await Expect(_plan.ErrorFor(m => m.HomeSafetyVisit)).ToHaveTextAsync("Book a home safety visit and enter its date.");
        await Expect(PlanStatus).ToHaveTextAsync("No plan saved yet.");
        Assert.That(_plansSent, Has.Count.EqualTo(0), "The page asked for the answer, so nothing was sent.");

        await Write(m => m.FallsLast90Days, "1");
        await ClickWhenStable(SavePlan);
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(1), "The page let the plan through, and the server accepted it.");

        // Either reason is enough: three flights of stairs ask for the visit too.
        await Write(m => m.StairsAtHome, "3");
        await Write(m => m.StairPlan, "Rail on both sides, one step at a time");
        await ClickWhenStable(SavePlan);
        await Expect(_plan.ErrorFor(m => m.HomeSafetyVisit)).ToHaveTextAsync("Book a home safety visit and enter its date.");
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(1), "The page asked for the answer, so nothing was sent.");

        await Write(m => m.HomeSafetyVisit, "2026-04-01");
        await ClickWhenStable(SavePlan);
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(2), "The page let the plan through, and the server accepted it.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task warfarin_on_the_medication_list_needs_the_next_inr_check()
    {
        await BeginThePlan();
        await Write(m => m.Medications, "warfarin 5 mg daily");

        await ClickWhenStable(SavePlan);
        await Expect(_plan.ErrorFor(m => m.InrCheckDate)).ToHaveTextAsync("Warfarin needs the date of the next INR check.");
        await Expect(PlanStatus).ToHaveTextAsync("No plan saved yet.");
        Assert.That(_plansSent, Has.Count.EqualTo(0), "The page asked for the answer, so nothing was sent.");

        await Write(m => m.Medications, "lisinopril 10 mg daily");
        await ClickWhenStable(SavePlan);
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(1), "The page let the plan through, and the server accepted it.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_long_medication_list_needs_the_pharmacists_review()
    {
        const string LongList = "lisinopril 10 mg, metformin 500 mg, atorvastatin 20 mg, aspirin";
        await BeginThePlan();
        await Write(m => m.Medications, LongList);

        await ClickWhenStable(SavePlan);
        await Expect(_plan.ErrorFor(m => m.Medications)).ToHaveTextAsync("A list this long needs a pharmacist's review: tick it.");
        await Expect(PlanStatus).ToHaveTextAsync("No plan saved yet.");
        Assert.That(_plansSent, Has.Count.EqualTo(0), "The page asked for the answer, so nothing was sent.");

        await PharmacistReviewed.Check();
        await ClickWhenStable(SavePlan);
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(1), "The page let the plan through, and the server accepted it.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_dementia_diagnosis_needs_a_memory_support_plan()
    {
        await BeginThePlan();
        await Write(m => m.DiagnosisCode, "F03");

        await ClickWhenStable(SavePlan);
        await Expect(_plan.ErrorFor(m => m.MemorySupportPlan)).ToHaveTextAsync("A dementia diagnosis needs a memory support plan.");
        await Expect(PlanStatus).ToHaveTextAsync("No plan saved yet.");
        Assert.That(_plansSent, Has.Count.EqualTo(0), "The page asked for the answer, so nothing was sent.");

        await Write(m => m.DiagnosisCode, "I10");
        await ClickWhenStable(SavePlan);
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(1), "The page let the plan through, and the server accepted it.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_isolation_room_needs_who_cleared_the_resident()
    {
        await BeginThePlan();
        await Write(m => m.RoomCode, "MA-214-ISO");

        await ClickWhenStable(SavePlan);
        await Expect(_plan.ErrorFor(m => m.IsolationClearedBy)).ToHaveTextAsync("Name who cleared the resident from isolation.");
        await Expect(PlanStatus).ToHaveTextAsync("No plan saved yet.");
        Assert.That(_plansSent, Has.Count.EqualTo(0), "The page asked for the answer, so nothing was sent.");

        await Write(m => m.RoomCode, "MA-214");
        await ClickWhenStable(SavePlan);
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(1), "The page let the plan through, and the server accepted it.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task oxygen_equipment_needs_its_vendor()
    {
        await BeginThePlan();
        await Equipment.Tick("Oxygen");

        await ClickWhenStable(SavePlan);
        await Expect(_plan.ErrorFor(m => m.OxygenVendor)).ToHaveTextAsync("Name the oxygen vendor.");
        await Expect(PlanStatus).ToHaveTextAsync("No plan saved yet.");
        Assert.That(_plansSent, Has.Count.EqualTo(0), "The page asked for the answer, so nothing was sent.");

        await Equipment.Clear("Oxygen");
        await Equipment.Tick("Walker");
        await ClickWhenStable(SavePlan);
        await Expect(PlanStatus).ToHaveTextAsync(Saved);
        Assert.That(_plansSent, Has.Count.EqualTo(1), "The page let the plan through, and the server accepted it.");
        AssertNoConsoleErrors();
    }

    // The answer that gated the rule changes; leaving the field it gated rechecks it under the new answer.
    [Test]
    public async Task a_home_address_message_clears_when_the_resident_is_no_longer_going_home_and_the_field_is_left()
    {
        await BeginThePlan();
        await GoingHome.Check();
        await ClickWhenStable(SavePlan);
        await Expect(_plan.ErrorFor(m => m.HomeAddress)).ToHaveTextAsync("Enter the home address the resident goes to.");

        await GoingHome.Input.UncheckAsync();
        await _plan.TextBox(m => m.HomeAddress).Focus();
        await _plan.TextBox(m => m.HomeAddress).Blur();

        await Expect(_plan.ErrorFor(m => m.HomeAddress)).ToHaveTextAsync("");
        await Expect(PlanStatus).ToHaveTextAsync("No plan saved yet.");
        Assert.That(_plansSent, Is.Empty);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task typing_warfarin_then_leaving_the_inr_date_asks_for_it()
    {
        await BeginThePlan();
        await Write(m => m.Medications, "warfarin 5 mg daily");

        await _plan.TextBox(m => m.InrCheckDate).Focus();
        await _plan.TextBox(m => m.InrCheckDate).Blur();

        await Expect(_plan.ErrorFor(m => m.InrCheckDate)).ToHaveTextAsync("Warfarin needs the date of the next INR check.");
        await Expect(PlanStatus).ToHaveTextAsync("No plan saved yet.");
        Assert.That(_plansSent, Is.Empty, "Leaving a field sends nothing.");
        AssertNoConsoleErrors();
    }
}
