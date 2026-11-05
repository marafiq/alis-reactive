using System.Collections.Concurrent;
using System.Globalization;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.InPlaceEditor;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.InPlaceEditor;

[TestFixture]
public class WhenUsingFusionInPlaceEditor : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionInPlaceEditor";
    private const string PreferredNameTooLong = "Keep the preferred name to 30 characters so it fits her door sign.";

    private FusionInPlaceEditorLocator PreferredName => new(Page, IdGenerator.For<ResidentProfileModel>(m => m.PreferredName));
    private DatePickerLocator AdmissionDate => new(Page, IdGenerator.For<ResidentProfileModel>(m => m.AdmissionDate));
    private NativeCheckBoxLocator ProfileLocked => new(Page, IdGenerator.For<ResidentProfileModel>(m => m.ProfileLocked));
    private FusionInPlaceEditorLocator DateOfBirth => new(Page, IdGenerator.For<DateOfBirthCorrection>(m => m.Value));
    private FusionInPlaceEditorLocator CareLevel => new(Page, IdGenerator.For<CareLevelCorrection>(m => m.Value));
    private FusionInPlaceEditorLocator MonthlyRate => new(Page, IdGenerator.For<MonthlyRateCorrection>(m => m.Value));
    private NativeCheckBoxLocator CustomRate => new(Page, IdGenerator.For<MonthlyRateCorrection>(m => m.CustomRate));
    private FusionInPlaceEditorLocator LastHospitalStay => new(Page, IdGenerator.For<LastHospitalStayCorrection>(m => m.Value));
    private FusionInPlaceEditorLocator RecordNumber => new(Page, IdGenerator.For<MedicalRecordNumberCorrection>(m => m.Value));

    private ILocator ErrorFor(string componentId) => Page.Locator($"#{componentId}_error");
    private ILocator ProfileStatus => Page.Locator("#profile-status");
    private ILocator ProfileLockedNote => Page.Locator("#profile-locked-note");
    private ILocator SaveProfileReminder => Page.Locator("#save-profile-reminder");
    private ILocator ClearPreferredName => Page.Locator("#clear-preferred-name");
    private ILocator SaveProfile => Page.Locator("#save-profile");
    private ILocator DateOfBirthStatus => Page.Locator("#card-dob-status");
    private ILocator CareLevelPreview => Page.Locator("#care-level-preview");
    private ILocator CareLevelStatus => Page.Locator("#card-care-level-status");
    private ILocator MonthlyRateStatus => Page.Locator("#card-monthly-rate-status");
    private ILocator LastHospitalStayStatus => Page.Locator("#card-last-hospital-stay-status");
    private ILocator RecordNumberStatus => Page.Locator("#card-mrn-status");
    private ILocator RecordNumberInput => RecordNumber.Root.Locator("input.e-maskedtextbox").First;

    private ConcurrentQueue<string> _savesSent = null!;

    private int SavesSentTo(string action) => _savesSent.Count(url => url.EndsWith($"{Path}/{action}"));

    private async Task OpenTheProfile()
    {
        _savesSent = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.Contains(Path)) _savesSent.Enqueue(request.Url);
        };
        await NavigateToAndWaitForBoot(Path);
        await Expect(PreferredName.ShownValue).ToHaveTextAsync("Ellie", new() { Timeout = 10000 });
        await Expect(MonthlyRate.ShownValue).ToHaveTextAsync("$4,500", new() { Timeout = 10000 });
    }

    private async Task SetACustomRate(string rate)
    {
        await CustomRate.Check();
        await MonthlyRate.OpenAndType(rate);
        await MonthlyRate.Input.PressAsync("Enter");
    }

    private async Task TypeTheRecordNumber(string recordNumber)
    {
        await RecordNumber.Open();
        await RecordNumberInput.FillAsync(recordNumber);
        await RecordNumberInput.PressAsync("Enter");
    }

    private async Task ChooseMemoryCare()
    {
        await CareLevel.Open();
        await ClickWhenStable(CareLevel.ListInput);
        await CareLevel.ListItem("Memory Care").ClickAsync();
    }

    [Test]
    public async Task clearing_the_preferred_name_saves_the_profile_without_one()
    {
        await OpenTheProfile();

        await ClickWhenStable(ClearPreferredName);

        await Expect(PreferredName.ShownValue).ToHaveTextAsync("Empty", new() { Timeout = 5000 });
        await ClickWhenStable(SaveProfile);
        await Expect(ProfileStatus).ToHaveTextAsync("Saved Eleanor Vance's profile. She has no preferred name.", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_the_profile_keeps_the_preferred_name_still_being_typed()
    {
        await OpenTheProfile();
        await PreferredName.OpenAndType("Nell");

        await ClickWhenStable(SaveProfile);

        await Expect(ProfileStatus).ToHaveTextAsync("Saved Eleanor Vance's profile. Preferred name: Nell.", new() { Timeout = 10000 });
        await Expect(PreferredName.ShownValue).ToHaveTextAsync("Nell");
        await Expect(PreferredName.ActionButtons).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task pressing_enter_on_a_new_preferred_name_reminds_the_coordinator_to_save_the_profile()
    {
        await OpenTheProfile();
        await PreferredName.OpenAndType("Nell");

        await PreferredName.Input.PressAsync("Enter");

        await Expect(SaveProfileReminder).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(PreferredName.ShownValue).ToHaveTextAsync("Nell");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_the_profile_after_pressing_enter_takes_the_reminder_away()
    {
        await OpenTheProfile();
        await PreferredName.OpenAndType("Nell");
        await PreferredName.Input.PressAsync("Enter");
        await Expect(SaveProfileReminder).ToBeVisibleAsync(new() { Timeout = 5000 });

        await ClickWhenStable(SaveProfile);

        await Expect(ProfileStatus).ToHaveTextAsync("Saved Eleanor Vance's profile. Preferred name: Nell.", new() { Timeout = 10000 });
        await Expect(SaveProfileReminder).ToBeHiddenAsync();
        Assert.That(SavesSentTo("UpdateProfile"), Is.EqualTo(1));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_profile_locked_for_care_review_keeps_its_preferred_name_closed_to_edits()
    {
        await OpenTheProfile();
        await ProfileLocked.Check();

        await PreferredName.ValueArea.ClickAsync();

        await Expect(ProfileLockedNote).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(PreferredName.ActionButtons).ToHaveCountAsync(0);
        await Expect(PreferredName.ShownValue).ToHaveTextAsync("Ellie");

        await ProfileLocked.Input.UncheckAsync();
        await PreferredName.Open();
        await Expect(ProfileLockedNote).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_preferred_name_too_long_for_her_door_sign_is_refused_and_the_profile_is_not_saved()
    {
        await OpenTheProfile();
        var preferredNameId = IdGenerator.For<ResidentProfileModel>(m => m.PreferredName);
        await PreferredName.OpenAndType(new string('x', 40));

        await PreferredName.Input.PressAsync("Enter");

        await Expect(ErrorFor(preferredNameId)).ToHaveTextAsync(PreferredNameTooLong, new() { Timeout = 5000 });
        await ClickWhenStable(SaveProfile);
        await Expect(ErrorFor(preferredNameId)).ToHaveTextAsync(PreferredNameTooLong);
        await Expect(ProfileStatus).ToHaveTextAsync("No changes saved yet.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_admission_date_after_today_is_refused_and_today_is_saved()
    {
        await OpenTheProfile();
        var admissionDateId = IdGenerator.For<ResidentProfileModel>(m => m.AdmissionDate);
        var tomorrow = DateTime.Today.AddDays(1);
        var today = DateTime.Today;

        await AdmissionDate.SelectDate(tomorrow.Year, tomorrow.Month, tomorrow.Day);
        await ClickWhenStable(SaveProfile);
        await Expect(ErrorFor(admissionDateId)).ToHaveTextAsync("The admission date cannot be after today.", new() { Timeout = 5000 });

        await AdmissionDate.SelectDate(today.Year, today.Month, today.Day);
        await ClickWhenStable(SaveProfile);
        await Expect(ProfileStatus).ToHaveTextAsync("Saved Eleanor Vance's profile. Preferred name: Ellie.", new() { Timeout = 10000 });
        await Expect(ErrorFor(admissionDateId)).ToBeHiddenAsync();
        Assert.That(SavesSentTo("UpdateProfile"), Is.EqualTo(1));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task correcting_her_date_of_birth_saves_it_and_marks_it_saved()
    {
        await OpenTheProfile();
        await Expect(DateOfBirth.Root).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("alis-editor-saved"));
        await DateOfBirth.OpenAndType("3/4/1941");

        await DateOfBirth.Input.PressAsync("Enter");

        await Expect(DateOfBirthStatus).ToHaveTextAsync("Date of birth saved: March 4, 1941.", new() { Timeout = 10000 });
        await Expect(DateOfBirth.ShownValue).ToHaveTextAsync("3/4/1941");
        await Expect(DateOfBirth.Root).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("alis-editor-saved"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reopening_a_saved_date_of_birth_takes_its_saved_mark_away()
    {
        await OpenTheProfile();
        await DateOfBirth.OpenAndType("3/4/1941");
        await DateOfBirth.Input.PressAsync("Enter");
        await Expect(DateOfBirth.Root).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("alis-editor-saved"), new() { Timeout = 10000 });

        await DateOfBirth.Open();

        await Expect(DateOfBirth.Root).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("alis-editor-saved"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_date_of_birth_of_today_is_refused_before_it_is_sent()
    {
        await OpenTheProfile();
        await DateOfBirth.OpenAndType(DateTime.Today.ToString("M/d/yyyy", CultureInfo.InvariantCulture));

        await DateOfBirth.Input.PressAsync("Enter");

        await Expect(ErrorFor(IdGenerator.For<DateOfBirthCorrection>(m => m.Value)))
            .ToHaveTextAsync("A date of birth must be before today.", new() { Timeout = 5000 });
        Assert.That(SavesSentTo("UpdateDateOfBirth"), Is.Zero);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task pressing_escape_keeps_the_date_of_birth_on_file()
    {
        await OpenTheProfile();
        await DateOfBirth.OpenAndType("3/4/1941");

        await DateOfBirth.Input.PressAsync("Escape");

        await Expect(DateOfBirthStatus).ToHaveTextAsync("Kept the date of birth on file.", new() { Timeout = 5000 });
        await Expect(DateOfBirth.ShownValue).ToHaveTextAsync("6/14/1942");
        await Expect(DateOfBirth.ActionButtons).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_a_care_level_shows_what_it_costs_before_it_is_saved()
    {
        await OpenTheProfile();
        await Expect(CareLevel.ShownValue).ToHaveTextAsync("Assisted Living");

        await ChooseMemoryCare();

        await Expect(CareLevelPreview).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(CareLevelPreview).ToHaveTextAsync("From Assisted Living to Memory Care: $6,200 a month.");
        await Expect(CareLevelStatus).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_a_care_level_records_it_with_its_monthly_rate()
    {
        await OpenTheProfile();
        await ChooseMemoryCare();
        await Expect(CareLevelPreview).ToBeVisibleAsync(new() { Timeout = 5000 });

        await ClickWhenStable(CareLevel.SaveButton);

        await Expect(CareLevelStatus).ToHaveTextAsync("Care level saved: Memory Care, $6,200 a month.", new() { Timeout = 10000 });
        await Expect(CareLevel.ShownValue).ToHaveTextAsync("Memory Care");
        await Expect(CareLevelPreview).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task cancelling_a_care_level_change_keeps_her_care_level()
    {
        await OpenTheProfile();
        await ChooseMemoryCare();
        await Expect(CareLevelPreview).ToBeVisibleAsync(new() { Timeout = 5000 });

        await ClickWhenStable(CareLevel.CancelButton);

        await Expect(CareLevelStatus).ToHaveTextAsync("Care level unchanged.", new() { Timeout = 5000 });
        await Expect(CareLevelPreview).ToBeHiddenAsync();
        await Expect(CareLevel.ShownValue).ToHaveTextAsync("Assisted Living");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_monthly_rate_follows_the_care_level_until_a_custom_rate_is_set()
    {
        await OpenTheProfile();

        await MonthlyRate.ValueArea.ClickAsync(new() { Force = true });
        await Expect(MonthlyRate.ActionButtons).ToHaveCountAsync(0);

        await CustomRate.Check();
        await MonthlyRate.Open();
        await MonthlyRate.Input.PressAsync("Escape");
        await Expect(MonthlyRate.ActionButtons).ToHaveCountAsync(0, new() { Timeout = 5000 });

        await CustomRate.Input.UncheckAsync();
        await MonthlyRate.ValueArea.ClickAsync(new() { Force = true });
        await Expect(MonthlyRate.ActionButtons).ToHaveCountAsync(0);
        await Expect(MonthlyRate.ShownValue).ToHaveTextAsync("$4,500");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_custom_rate_inside_her_care_levels_band_is_saved_once_billing_accepts_it()
    {
        await OpenTheProfile();

        await SetACustomRate("4800");

        await Expect(MonthlyRateStatus).ToHaveTextAsync("Monthly rate saved: $4,800.", new() { Timeout = 10000 });
        await Expect(MonthlyRate.ShownValue).ToHaveTextAsync("$4,800");
        await Expect(MonthlyRate.ActionButtons).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_custom_rate_outside_the_band_is_refused_with_the_cursor_back_in_it()
    {
        await OpenTheProfile();

        await SetACustomRate("9000");

        await Expect(ErrorFor(IdGenerator.For<MonthlyRateCorrection>(m => m.Value)))
            .ToHaveTextAsync("Assisted Living rates run from $3,800 to $5,500 a month.", new() { Timeout = 10000 });
        await Expect(MonthlyRate.ActionButtons).ToBeVisibleAsync();
        await Expect(MonthlyRate.Input).ToBeFocusedAsync();
        await Expect(MonthlyRate.ShownValue).ToHaveTextAsync("$4,500");
        await Expect(MonthlyRateStatus).ToHaveTextAsync("");
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task a_monthly_rate_of_zero_is_refused_before_it_is_sent()
    {
        await OpenTheProfile();

        await SetACustomRate("0");

        await Expect(ErrorFor(IdGenerator.For<MonthlyRateCorrection>(m => m.Value)))
            .ToHaveTextAsync("Enter a monthly rate above $0.", new() { Timeout = 5000 });
        await Expect(MonthlyRate.ActionButtons).ToBeVisibleAsync();
        Assert.That(SavesSentTo("UpdateMonthlyRate"), Is.Zero);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_last_hospital_stay_in_the_future_is_refused_and_an_earlier_one_is_saved()
    {
        await OpenTheProfile();
        await LastHospitalStay.OpenAndType("1/15/2099 10:00 AM");
        await LastHospitalStay.Input.PressAsync("Enter");

        await Expect(ErrorFor(IdGenerator.For<LastHospitalStayCorrection>(m => m.Value)))
            .ToHaveTextAsync("A hospital stay cannot begin in the future.", new() { Timeout = 5000 });

        await LastHospitalStay.OpenAndType("3/2/2024 9:30 AM");
        await LastHospitalStay.Input.PressAsync("Enter");

        await Expect(LastHospitalStayStatus).ToHaveTextAsync("Last hospital stay saved: March 2, 2024 at 9:30 AM.", new() { Timeout = 10000 });
        Assert.That(SavesSentTo("UpdateLastHospitalStay"), Is.EqualTo(1));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_new_medical_record_number_is_saved_without_its_dash()
    {
        await OpenTheProfile();

        await TypeTheRecordNumber("EVA4217");

        await Expect(RecordNumberStatus).ToHaveTextAsync("Medical record number saved: EVA4217.", new() { Timeout = 10000 });
        await Expect(RecordNumber.ShownValue).ToHaveTextAsync("EVA-4217");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task another_residents_medical_record_number_is_refused()
    {
        await OpenTheProfile();

        await TypeTheRecordNumber("WGT4321");

        await Expect(RecordNumberStatus).ToHaveTextAsync("WGT4321 is Walter Grant's medical record number.", new() { Timeout = 10000 });
        AssertNoConsoleErrorsExcept("the server responded with a status of 409");
    }

    [Test]
    public async Task an_incomplete_medical_record_number_is_refused_before_it_is_sent()
    {
        await OpenTheProfile();

        await TypeTheRecordNumber("AB1234");

        await Expect(ErrorFor(IdGenerator.For<MedicalRecordNumberCorrection>(m => m.Value)))
            .ToHaveTextAsync("A medical record number is three letters and four digits.", new() { Timeout = 5000 });
        Assert.That(SavesSentTo("UpdateMedicalRecordNumber"), Is.Zero);
        AssertNoConsoleErrors();
    }
}
