namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As the admissions coordinator, I want to correct a resident's care level, primary nurse and next care
/// review in their record, and admit the next referral by completing their admission form, so that each
/// resident's record says who cares for them and when they are reviewed, from the day they move in.
/// </summary>
/// <remarks>
/// The residents, by name: Amina Patel, Grace Bennett, Henry Liu (Assisted Living, Elena Ruiz, next review
/// October 16), Ruth Carter and Walter Grant. The nurses on staff are Nora Ellis and Malik Stone (days), Elena
/// Ruiz (evenings) and Owen Park (nights); the nurse's editor lists each with their shift and saves the name.
/// Waiting to move in, the next first: Rosa Diaz (moving in Monday, October 19, Moderate risk, 2 open tasks,
/// first review October 26), Oscar Flynn and Vera Lindqvist. A first care review falls within two weeks of
/// moving in. The server keeps nothing: every page starts from the records on file.
/// Not exercised:
/// ActionComplete's Name, Type, Action, PreviousData, RowIndex, SelectedRow and Cancel: the page sends the
/// saved row, told apart by RequestType; WhenUsingFusionGridEditing reads Action and PreviousData.
/// </remarks>
[TestFixture]
public class WhenUsingFusionGridResidentEditForm : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/ResidentEditForm";
    private const string ResidentsId = "desk-residents";
    private const string ReferralsId = "desk-referrals";

    // The grids number their columns from 1, counting the hidden ID column.
    private const int ResidentColumn = 2;
    private const int CareLevelColumn = 4;
    private const int NurseColumn = 5;
    private const int NextReviewColumn = 6;
    private const int FirstReviewColumn = 8;

    private FusionGridLocator Residents => new(Page, ResidentsId);
    private FusionGridLocator Referrals => new(Page, ReferralsId);
    private ILocator Status => Page.Locator("#desk-status");
    private ILocator RowBeingCorrected => Page.Locator($"#{ResidentsId} .e-editedrow");
    private ILocator ResidentBeingCorrected => Page.Locator($"#{ResidentsId}residentName");
    private ILocator CareLevelEditor => RowBeingCorrected.Locator("select[name='careLevel']");
    private ILocator NurseEditor => RowBeingCorrected.Locator("select[name='primaryNurse']");
    private ILocator NextReviewEditor => RowBeingCorrected.Locator("input[name='nextReview']");
    private ILocator CorrectRecord => Page.Locator("#correct-record");
    private ILocator SaveCorrection => Page.Locator("#save-correction");
    private ILocator CancelCorrection => Page.Locator("#cancel-correction");
    private ILocator AdmitNextReferral => Page.Locator("#admit-next-referral");
    private ILocator AdmissionForm => Page.Locator($"#{ReferralsId}_dialogEdit_wrapper");
    private ILocator AdmissionField(string name) => AdmissionForm.Locator($"[name='{name}']");
    private ILocator AdmissionLabel(string text) => AdmissionForm.GetByText(text, new() { Exact = true });
    private ILocator AdmissionButton(string text) => AdmissionForm.Locator(".e-footer-content button", new() { HasText = text });

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Residents.Rows).ToHaveCountAsync(5, new() { Timeout = 10000 });
        await Expect(Referrals.Rows).ToHaveCountAsync(3, new() { Timeout = 10000 });
    }

    // A click on Henry's name selects his row; Correct Record opens the selected row's editors.
    private async Task CorrectHenrysRecord()
    {
        await Residents.Cell("Henry Liu", ResidentColumn).ClickAsync();
        await ClickWhenStable(CorrectRecord);
        await Expect(NextReviewEditor).ToHaveValueAsync("2026-10-16", new() { Timeout = 10000 });
    }

    private async Task AdmitTheNextReferral()
    {
        await ClickWhenStable(AdmitNextReferral);
        await Expect(AdmissionField("residentName")).ToHaveValueAsync("Rosa Diaz", new() { Timeout = 10000 });
    }

    [Test]
    public async Task the_desk_opens_on_the_residents_records_and_the_referrals_waiting_to_move_in()
    {
        await NavigateAndBoot();

        await Expect(Residents.Rows.First).ToContainTextAsync("Amina Patel");
        await Expect(Residents.Cell("Henry Liu", NurseColumn)).ToHaveTextAsync("Elena Ruiz");
        await Expect(Residents.Cell("Henry Liu", NextReviewColumn)).ToHaveTextAsync("2026-10-16");
        await Expect(Referrals.Rows.First).ToContainTextAsync("Rosa Diaz");
        await Expect(Referrals.Rows.Last).ToContainTextAsync("Vera Lindqvist");
        await Expect(Status).ToHaveTextAsync("Nothing saved yet today.");
        AssertNoConsoleErrors();
    }

    // Lists that opened on their first option would change Henry's care level and nurse when left untouched.
    [Test]
    public async Task correcting_a_record_opens_its_editors_on_what_the_record_holds()
    {
        await NavigateAndBoot();

        await CorrectHenrysRecord();

        await Expect(RowBeingCorrected).ToHaveCountAsync(1);
        await Expect(ResidentBeingCorrected).ToHaveValueAsync("Henry Liu");
        await Expect(CareLevelEditor).ToHaveValueAsync("Assisted Living");
        await Expect(NurseEditor).ToHaveValueAsync("Elena Ruiz");
        await Expect(NurseEditor.Locator("option:checked")).ToHaveTextAsync("Elena Ruiz, evenings");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_corrected_record_is_saved_with_its_new_care_level_nurse_and_review_day()
    {
        await NavigateAndBoot();
        await CorrectHenrysRecord();
        await CareLevelEditor.SelectOptionAsync("Memory Care");
        await NurseEditor.SelectOptionAsync("Owen Park, nights");
        await NextReviewEditor.FillAsync("2026-10-20");

        await ClickWhenStable(SaveCorrection);

        await Expect(Status).ToHaveTextAsync(
            "Henry Liu's record now reads Memory Care with Owen Park, next care review Tuesday, October 20.", new() { Timeout = 10000 });
        await Expect(RowBeingCorrected).ToHaveCountAsync(0);
        await Expect(Residents.Cell("Henry Liu", CareLevelColumn)).ToHaveTextAsync("Memory Care");
        await Expect(Residents.Cell("Henry Liu", NurseColumn)).ToHaveTextAsync("Owen Park");
        await Expect(Residents.Cell("Henry Liu", NextReviewColumn)).ToHaveTextAsync("2026-10-20");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task cancelling_a_correction_keeps_the_record_as_it_was()
    {
        await NavigateAndBoot();
        await CorrectHenrysRecord();
        await CareLevelEditor.SelectOptionAsync("Memory Care");

        await ClickWhenStable(CancelCorrection);

        await Expect(RowBeingCorrected).ToHaveCountAsync(0, new() { Timeout = 10000 });
        await Expect(Residents.Cell("Henry Liu", CareLevelColumn)).ToHaveTextAsync("Assisted Living");
        await Expect(Status).ToHaveTextAsync("Nothing saved yet today.");
        AssertNoConsoleErrors();
    }

    // Rosa Diaz moves in first, so the form opens on her referral; her risk list opens on her moderate risk.
    [Test]
    public async Task admitting_the_next_referral_opens_the_admission_form_on_that_referral()
    {
        await NavigateAndBoot();

        await AdmitTheNextReferral();

        await Expect(AdmissionLabel("Resident")).ToBeVisibleAsync();
        await Expect(AdmissionLabel("Risk")).ToBeVisibleAsync();
        await Expect(AdmissionLabel("Open care tasks")).ToBeVisibleAsync();
        await Expect(AdmissionLabel("First care review")).ToBeVisibleAsync();
        await Expect(AdmissionField("riskLevel")).ToHaveValueAsync("Moderate");
        await Expect(AdmissionField("openTasks")).ToHaveValueAsync("2");
        await Expect(AdmissionField("firstReview")).ToHaveValueAsync("2026-10-26");
        AssertNoConsoleErrors();
    }

    // The coordinator writes Rosa's full name as it is on her residency agreement.
    [Test]
    public async Task an_admission_completed_on_the_form_admits_the_resident()
    {
        await NavigateAndBoot();
        await AdmitTheNextReferral();
        await AdmissionField("residentName").FillAsync("Rosa Maria Diaz");
        await AdmissionField("riskLevel").SelectOptionAsync("High");
        await AdmissionField("openTasks").FillAsync("3");
        await AdmissionField("firstReview").FillAsync("2026-10-27");

        await ClickWhenStable(AdmissionButton("Save"));

        await Expect(Status).ToHaveTextAsync(
            "Welcome, Rosa Maria Diaz: admitted at High risk with 3 open care tasks, first care review Tuesday, October 27.",
            new() { Timeout = 10000 });
        await Expect(AdmissionForm).ToBeHiddenAsync();
        await Expect(Residents.Rows.Nth(3)).ToContainTextAsync("Rosa Maria Diaz");
        await Expect(Residents.Cell("Rosa Maria Diaz", NextReviewColumn)).ToHaveTextAsync("2026-10-27");
        await Expect(Referrals.Rows).ToHaveCountAsync(2);
        await Expect(Referrals.Rows.First).ToContainTextAsync("Oscar Flynn");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_admission_whose_first_review_falls_too_late_is_refused_and_the_referral_keeps_waiting()
    {
        await NavigateAndBoot();
        await AdmitTheNextReferral();
        await AdmissionField("firstReview").FillAsync("2026-11-10");

        await ClickWhenStable(AdmissionButton("Save"));

        await Expect(Status).ToHaveTextAsync(
            "Rosa Diaz's first care review must fall between moving in on Monday, October 19 and Monday, November 2.",
            new() { Timeout = 10000 });
        await Expect(Referrals.Cell("Rosa Diaz", FirstReviewColumn)).ToHaveTextAsync("2026-10-26");
        await Expect(Referrals.Rows).ToHaveCountAsync(3);
        await Expect(Residents.Row("Rosa Diaz")).ToHaveCountAsync(0);
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }
}
