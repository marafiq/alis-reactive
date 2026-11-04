using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.MedicationRound;

namespace Alis.Reactive.PlaywrightTests.Conditions.MedicationRound;

[TestFixture]
public class WhenCheckingDosesOnTheMedicationRound : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/MedicationRound";

    private static readonly Dictionary<string, string> RoomOf = new()
    {
        ["Margaret Hale"] = "214",
        ["Walter Brooks"] = "216",
        ["Evelyn Hart"] = "218",
        ["Robert Miles"] = "220",
        ["Rose Delgado"] = "222",
        ["Harold Kim"] = "224",
        ["Agnes Moore"] = "226",
        ["Ida Bell"] = "228",
        ["Frank Ortiz"] = "230",
        ["Clara Nguyen"] = "232",
        ["Henry Walsh"] = "234",
    };

    private PagePlan<MedicationRoundModel> _plan = null!;

    private ILocator CheckButtonFor(string resident) => Page.Locator($"#check-dose-{RoomOf[resident]}");
    private ILocator DoseResident => _plan.Element("dose-resident");
    private ILocator DoseMedication => _plan.Element("dose-medication");
    private ILocator DoseChecks => _plan.Element("dose-checks");
    private ILocator Whereabouts => _plan.Element("check-whereabouts");
    private ILocator PulseReading => _plan.Element("pulse-reading");
    private ILocator Pulse => _plan.Element("check-pulse");
    private ILocator TemperatureReading => _plan.Element("temperature-reading");
    private ILocator Temperature => _plan.Element("check-temperature");
    private ILocator LastGiven => _plan.Element("check-last-dose");
    private ILocator Order => _plan.Element("check-order");
    private ILocator Route => _plan.Element("check-route");
    private ILocator Allergies => _plan.Element("check-allergies");
    private ILocator Status => _plan.Element("check-status");
    private ILocator PainScoreLine => _plan.Element("check-pain");
    private ILocator PainScore => _plan.Element("pain-score");
    private ILocator Verdict => _plan.Element("dose-verdict");
    private ILocator ChargeNurseAlert => _plan.Element("charge-nurse-alert");
    private ILocator LabelCheck => _plan.Element("label-check");
    private ILocator ControlledNote => _plan.Element("controlled-note");
    private ILocator WitnessCheck => _plan.Element("witness-check");
    private ILocator HoldReasonCheck => _plan.Element("hold-reason-check");
    private ILocator RecordAsGiven => Page.Locator("#record-dose-btn");
    private ILocator RecordStatus => _plan.Element("record-status");
    private ILocator ConfirmDialog => Page.Locator("#alisConfirmDialog");

    private NativeTextBoxLocator PharmacyLabel => _plan.TextBox(m => m.LabelScan);
    private NativeTextBoxLocator Witness => _plan.TextBox(m => m.WitnessEmail);
    private NativeTextBoxLocator HoldReason => _plan.TextBox(m => m.HoldReason);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<MedicationRoundModel>.FromPage(Page);
        await Expect(CheckButtonFor("Margaret Hale")).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    private async Task CheckTheDoseOf(string resident)
    {
        await ClickWhenStable(CheckButtonFor(resident));
        await Expect(DoseResident).ToHaveTextAsync(resident);
    }

    [Test]
    public async Task opening_a_dose_names_the_resident_and_the_medication_and_shows_its_checks()
    {
        await NavigateAndBoot();
        await Expect(DoseResident).ToHaveTextAsync("Choose a dose to check.");
        await Expect(DoseChecks).ToBeHiddenAsync();

        await CheckTheDoseOf("Margaret Hale");

        await Expect(DoseMedication).ToHaveTextAsync("Metoprolol 25 mg tablet");
        await Expect(DoseChecks).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_due_dose_with_nothing_to_hold_it_for_is_clear_to_give()
    {
        await NavigateAndBoot();

        await CheckTheDoseOf("Margaret Hale");
        await Expect(Whereabouts).ToHaveTextAsync("On the wing.");
        await Expect(Verdict).ToHaveTextAsync("Clear to give.");

        await CheckTheDoseOf("Evelyn Hart");
        await Expect(Verdict).ToHaveTextAsync("Clear to give.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_resident_away_from_the_wing_has_the_dose_held()
    {
        await NavigateAndBoot();

        await CheckTheDoseOf("Ida Bell");

        await Expect(Whereabouts).ToHaveTextAsync("Away from the wing. Come back to this dose.");
        await Expect(Verdict).ToHaveTextAsync("Hold this dose.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_pulse_of_exactly_60_or_exactly_100_is_within_range()
    {
        await NavigateAndBoot();

        await CheckTheDoseOf("Margaret Hale");
        await Expect(PulseReading).ToHaveTextAsync("60");
        await Expect(Pulse).ToHaveTextAsync("within 60 to 100.");

        await CheckTheDoseOf("Evelyn Hart");
        await Expect(PulseReading).ToHaveTextAsync("100");
        await Expect(Pulse).ToHaveTextAsync("within 60 to 100.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_pulse_of_59_or_101_is_outside_the_range()
    {
        await NavigateAndBoot();

        await CheckTheDoseOf("Walter Brooks");
        await Expect(PulseReading).ToHaveTextAsync("59");
        await Expect(Pulse).ToHaveTextAsync("outside 60 to 100.");

        await CheckTheDoseOf("Robert Miles");
        await Expect(PulseReading).ToHaveTextAsync("101");
        await Expect(Pulse).ToHaveTextAsync("outside 60 to 100.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_pulse_outside_the_range_holds_the_dose()
    {
        await NavigateAndBoot();

        await CheckTheDoseOf("Walter Brooks");
        await Expect(Verdict).ToHaveTextAsync("Hold this dose.");

        await CheckTheDoseOf("Robert Miles");
        await Expect(Verdict).ToHaveTextAsync("Hold this dose.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task once_the_physician_says_give_a_dose_held_for_its_pulse_is_clear_to_give()
    {
        await NavigateAndBoot();
        await CheckTheDoseOf("Walter Brooks");
        await Expect(Verdict).ToHaveTextAsync("Hold this dose.");

        await _plan.NativeCheckBox(m => m.PhysicianSaysGive).Check();
        await CheckTheDoseOf("Walter Brooks");

        await Expect(Verdict).ToHaveTextAsync("Clear to give.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_physician_saying_give_does_not_clear_a_dose_held_for_another_reason()
    {
        await NavigateAndBoot();
        await CheckTheDoseOf("Walter Brooks");
        await _plan.NativeCheckBox(m => m.PhysicianSaysGive).Check();

        await CheckTheDoseOf("Ida Bell");

        await Expect(Verdict).ToHaveTextAsync("Hold this dose.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_dose_on_hold_or_not_available_from_the_pharmacy_is_held()
    {
        await NavigateAndBoot();

        await CheckTheDoseOf("Frank Ortiz");
        await Expect(Verdict).ToHaveTextAsync("Hold this dose.");

        await CheckTheDoseOf("Clara Nguyen");
        await Expect(Verdict).ToHaveTextAsync("Hold this dose.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_status_reads_due_now_for_a_due_dose_and_names_any_other_status()
    {
        await NavigateAndBoot();

        await CheckTheDoseOf("Margaret Hale");
        await Expect(Status).ToHaveTextAsync("Due now");

        await CheckTheDoseOf("Frank Ortiz");
        await Expect(Status).ToHaveTextAsync("On hold");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_dose_given_less_than_four_hours_ago_is_held()
    {
        await NavigateAndBoot();

        await CheckTheDoseOf("Rose Delgado");

        await Expect(LastGiven).ToHaveTextAsync("Less than 4 hours ago. Do not repeat it yet.");
        await Expect(Verdict).ToHaveTextAsync("Hold this dose.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task four_hours_or_more_since_the_last_dose_is_long_enough()
    {
        await NavigateAndBoot();

        await CheckTheDoseOf("Harold Kim");
        await Expect(LastGiven).ToHaveTextAsync("4 or more hours ago.");

        await CheckTheDoseOf("Margaret Hale");
        await Expect(LastGiven).ToHaveTextAsync("4 or more hours ago.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_temperature_over_100_4_is_out_of_range_and_exactly_100_4_or_just_under_is_normal()
    {
        await NavigateAndBoot();

        await CheckTheDoseOf("Walter Brooks");
        await Expect(TemperatureReading).ToHaveTextAsync("100.4");
        await Expect(Temperature).ToHaveTextAsync("normal.");

        await CheckTheDoseOf("Evelyn Hart");
        await Expect(TemperatureReading).ToHaveTextAsync("100.5");
        await Expect(Temperature).ToHaveTextAsync("out of range. Take it again.");

        await CheckTheDoseOf("Robert Miles");
        await Expect(TemperatureReading).ToHaveTextAsync("100.3");
        await Expect(Temperature).ToHaveTextAsync("normal.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_temperature_under_96_is_out_of_range_and_exactly_96_is_normal()
    {
        await NavigateAndBoot();

        await CheckTheDoseOf("Clara Nguyen");
        await Expect(TemperatureReading).ToHaveTextAsync("95.9");
        await Expect(Temperature).ToHaveTextAsync("out of range. Take it again.");

        await CheckTheDoseOf("Henry Walsh");
        await Expect(TemperatureReading).ToHaveTextAsync("96");
        await Expect(Temperature).ToHaveTextAsync("normal.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_dose_with_no_order_on_file_is_held_and_sends_the_aide_to_the_pharmacy()
    {
        await NavigateAndBoot();

        await CheckTheDoseOf("Harold Kim");

        await Expect(Order).ToHaveTextAsync("None on file. Call the pharmacy.");
        await Expect(Verdict).ToHaveTextAsync("Hold this dose.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_signed_order_that_runs_past_today_is_current()
    {
        await NavigateAndBoot();

        await CheckTheDoseOf("Margaret Hale");
        await Expect(Order).ToHaveTextAsync("Signed by the physician and current.");

        await CheckTheDoseOf("Walter Brooks");
        await Expect(Order).ToHaveTextAsync("Signed by the physician and current.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_verbal_order_says_it_is_not_yet_signed()
    {
        await NavigateAndBoot();

        await CheckTheDoseOf("Rose Delgado");

        await Expect(Order).ToHaveTextAsync("Verbal order, not yet signed by the physician.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_order_that_ends_today_or_ended_before_has_ended_and_holds_the_dose()
    {
        await NavigateAndBoot();

        await CheckTheDoseOf("Henry Walsh");
        await Expect(Order).ToHaveTextAsync("Ended. Call the physician.");
        await Expect(Verdict).ToHaveTextAsync("Hold this dose.");

        await CheckTheDoseOf("Agnes Moore");
        await Expect(Order).ToHaveTextAsync("Ended. Call the physician.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_route_reads_by_mouth_only_when_the_order_says_so()
    {
        await NavigateAndBoot();

        await CheckTheDoseOf("Margaret Hale");
        await Expect(Route).ToHaveTextAsync("By mouth, with a full glass of water.");

        await CheckTheDoseOf("Robert Miles");
        await Expect(Route).ToHaveTextAsync("Check the route on the order.");

        await CheckTheDoseOf("Rose Delgado");
        await Expect(Route).ToHaveTextAsync("Check the route on the order.");

        await CheckTheDoseOf("Harold Kim");
        await Expect(Route).ToHaveTextAsync("Check the route on the order.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task recorded_allergies_are_shown_and_a_blank_or_missing_record_reads_none_on_file()
    {
        await NavigateAndBoot();

        await CheckTheDoseOf("Margaret Hale");
        await Expect(Allergies).ToHaveTextAsync("Penicillin");

        await CheckTheDoseOf("Walter Brooks");
        await Expect(Allergies).ToHaveTextAsync("None on file.");

        await CheckTheDoseOf("Evelyn Hart");
        await Expect(Allergies).ToHaveTextAsync("None on file.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_pain_medication_without_a_pain_score_asks_for_one_first()
    {
        await NavigateAndBoot();

        await CheckTheDoseOf("Rose Delgado");

        await Expect(PainScoreLine).ToBeVisibleAsync();
        await Expect(PainScore).ToHaveTextAsync("not asked yet. Ask the resident before you give it.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_pain_score_of_zero_counts_as_asked()
    {
        await NavigateAndBoot();

        await CheckTheDoseOf("Harold Kim");

        await Expect(PainScoreLine).ToBeVisibleAsync();
        await Expect(PainScore).ToHaveTextAsync("0");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_pain_score_shows_only_for_pain_medications()
    {
        await NavigateAndBoot();

        await CheckTheDoseOf("Margaret Hale");
        await Expect(PainScoreLine).ToBeHiddenAsync();

        await CheckTheDoseOf("Rose Delgado");
        await Expect(PainScoreLine).ToBeVisibleAsync();

        await CheckTheDoseOf("Margaret Hale");
        await Expect(PainScoreLine).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_fever_raises_the_charge_nurse_alert()
    {
        await NavigateAndBoot();
        await Expect(ChargeNurseAlert).ToBeHiddenAsync();

        await CheckTheDoseOf("Evelyn Hart");

        await Expect(ChargeNurseAlert).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_time_critical_dose_missed_because_the_resident_is_away_raises_the_charge_nurse_alert()
    {
        await NavigateAndBoot();

        await CheckTheDoseOf("Agnes Moore");

        await Expect(Whereabouts).ToHaveTextAsync("Away from the wing. Come back to this dose.");
        await Expect(ChargeNurseAlert).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_away_resident_with_a_routine_dose_or_a_time_critical_dose_given_on_the_wing_raises_no_alert()
    {
        await NavigateAndBoot();

        await CheckTheDoseOf("Ida Bell");
        await Expect(ChargeNurseAlert).ToBeHiddenAsync();

        await CheckTheDoseOf("Robert Miles");
        await Expect(ChargeNurseAlert).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_temperature_of_exactly_100_4_raises_no_alert()
    {
        await NavigateAndBoot();

        await CheckTheDoseOf("Walter Brooks");

        await Expect(ChargeNurseAlert).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_charge_nurse_alert_stays_up_for_the_rest_of_the_round()
    {
        await NavigateAndBoot();
        await CheckTheDoseOf("Evelyn Hart");
        await Expect(ChargeNurseAlert).ToBeVisibleAsync();

        await CheckTheDoseOf("Margaret Hale");

        await Expect(ChargeNurseAlert).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_pharmacy_label_that_reads_cleanly_is_accepted()
    {
        await NavigateAndBoot();

        await PharmacyLabel.FillAndBlur("MGP-104233");

        await Expect(LabelCheck).ToHaveTextAsync("Label read.");
        await Expect(ControlledNote).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_controlled_medication_label_reads_and_asks_for_a_witness()
    {
        await NavigateAndBoot();

        await PharmacyLabel.FillAndBlur("MGP-C2-104233");

        await Expect(LabelCheck).ToHaveTextAsync("Label read.");
        await Expect(ControlledNote).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task rescanning_the_plain_label_after_a_controlled_one_takes_the_witness_note_away()
    {
        await NavigateAndBoot();
        await PharmacyLabel.FillAndBlur("MGP-C2-104233");
        await Expect(ControlledNote).ToBeVisibleAsync();

        await PharmacyLabel.FillAndBlur("MGP-104233");

        await Expect(LabelCheck).ToHaveTextAsync("Label read.");
        await Expect(ControlledNote).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_pharmacy_label_missing_a_digit_is_scanned_again()
    {
        await NavigateAndBoot();

        await PharmacyLabel.FillAndBlur("MGP-10423");

        await Expect(LabelCheck).ToHaveTextAsync("The label did not read cleanly. Scan it again.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_sticker_that_only_carries_the_pharmacy_code_is_not_a_pharmacy_label()
    {
        await NavigateAndBoot();

        await PharmacyLabel.FillAndBlur("LOT-MGP-104233");

        await Expect(LabelCheck).ToHaveTextAsync("This is not a Maple Grove Pharmacy label. Scan the pharmacy label.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_witness_must_sign_in_with_a_maple_grove_address()
    {
        await NavigateAndBoot();

        await Witness.FillAndBlur("j.okafor@maplegrove.org");
        await Expect(WitnessCheck).ToHaveTextAsync("Witness on staff.");

        await Witness.FillAndBlur("j.okafor@maplegrove.org.uk");
        await Expect(WitnessCheck).ToHaveTextAsync("The witness must sign in with a Maple Grove address.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_hold_reason_needs_at_least_ten_characters()
    {
        await NavigateAndBoot();

        await HoldReason.FillAndBlur("Nauseous.");
        await Expect(HoldReasonCheck).ToHaveTextAsync("Say a little more: at least 10 characters.");

        await HoldReason.FillAndBlur("Vomited x2");
        await Expect(HoldReasonCheck).ToHaveTextAsync("Reason noted.");

        await HoldReason.FillAndBlur("Vomited twice after breakfast");
        await Expect(HoldReasonCheck).ToHaveTextAsync("Reason noted.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_the_hold_reason_asks_for_one()
    {
        await NavigateAndBoot();
        await HoldReason.FillAndBlur("Vomited x2");
        await Expect(HoldReasonCheck).ToHaveTextAsync("Reason noted.");

        await HoldReason.FillAndBlur("");

        await Expect(HoldReasonCheck).ToHaveTextAsync("Say why the dose is held.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task recording_a_dose_waits_for_the_aide_to_confirm_it()
    {
        await NavigateAndBoot();

        await ClickWhenStable(RecordAsGiven);
        await Expect(ConfirmDialog).ToContainTextAsync("Record this dose as given?");
        await Expect(RecordStatus).ToHaveTextAsync("");
        await ClickWhenStable(ConfirmDialog.GetByRole(AriaRole.Button, new() { Name = "OK" }));

        await Expect(RecordStatus).ToHaveTextAsync("Recorded as given.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task cancelling_the_question_leaves_the_dose_not_recorded()
    {
        await NavigateAndBoot();
        await ClickWhenStable(RecordAsGiven);
        await Expect(ConfirmDialog).ToContainTextAsync("Record this dose as given?");

        await ClickWhenStable(ConfirmDialog.GetByRole(AriaRole.Button, new() { Name = "Cancel" }));

        await Expect(RecordStatus).ToHaveTextAsync("Not recorded. The dose is still due.");
        AssertNoConsoleErrors();
    }
}
