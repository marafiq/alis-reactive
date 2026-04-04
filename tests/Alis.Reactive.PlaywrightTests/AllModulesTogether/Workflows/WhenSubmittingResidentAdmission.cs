using System.Text.RegularExpressions;
using Alis.Reactive.PlaywrightTests.Support.Controls;

namespace Alis.Reactive.PlaywrightTests.AllModulesTogether.Workflows;

[TestFixture]
public class WhenSubmittingResidentAdmission : PlaywrightTestBase
{
    private const string Path = "/Sandbox/AllModulesTogether/ResidentAdmission";
    private ResidentAdmissionPage _page = null!;

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#submit-btn");
        _page = new ResidentAdmissionPage(Page);
    }

    private ILocator SubmitBtn => Page.Locator("#submit-btn");
    private ILocator Status => _page.Status;

    [Test]
    public async Task searching_physician_by_name_shows_matching_results_to_pick_from()
    {
        await NavigateAndBoot();

        await _page.Physician.Type("smith");
        await Expect(_page.Physician.PopupItems.First).ToBeVisibleAsync(new() { Timeout = 5000 });
        await _page.Physician.SelectItem("Dr. Smith");

        await Expect(_page.PhysicianEcho)
            .ToContainTextAsync("smith", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task searching_physician_filters_to_matching_names_only()
    {
        await NavigateAndBoot();

        await _page.Physician.Type("chen");
        await Expect(_page.Physician.PopupItem("Dr. Chen")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(_page.Physician.PopupItem("Dr. Smith")).Not.ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_care_level_confirms_selection()
    {
        await NavigateAndBoot();

        await _page.CareLevel.Select("Memory Care");

        await Expect(_page.CareEcho)
            .ToContainTextAsync("memory", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task typing_resident_name_echoes_to_the_feedback_panel()
    {
        await NavigateAndBoot();

        await _page.ResidentName.FillAndBlur("Eleanor Rigby");

        await Expect(_page.NameEcho)
            .ToContainTextAsync("Eleanor Rigby", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task entering_monthly_rate_echoes_to_the_feedback_panel()
    {
        await NavigateAndBoot();

        await _page.MonthlyRate.FillAndBlur("3200");

        await Expect(_page.RateEcho)
            .ToContainTextAsync("3200", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task toggling_active_switch_echoes_to_the_feedback_panel()
    {
        await NavigateAndBoot();

        await _page.IsActive.Toggle();

        await Expect(_page.ActiveEcho)
            .ToContainTextAsync("true", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task typing_notes_echoes_to_the_feedback_panel()
    {
        await NavigateAndBoot();

        await _page.Notes.FillAndBlur("Allergic to penicillin");

        await Expect(_page.NotesEcho)
            .ToContainTextAsync("Allergic to penicillin", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task submitting_without_required_fields_tells_admin_what_is_missing()
    {
        await NavigateAndBoot();

        await SubmitBtn.ClickAsync();

        await Expect(_page.ResidentNameError)
            .ToContainTextAsync("Resident name is required", new() { Timeout = 5000 });
        await Expect(_page.PhysicianError)
            .ToContainTextAsync("Physician is required", new() { Timeout = 5000 });
        await Expect(_page.MonthlyRateError)
            .ToContainTextAsync("Monthly rate is required", new() { Timeout = 5000 });
        await Expect(Status).ToHaveTextAsync("Ready");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task optional_fields_do_not_show_errors_on_empty_submit()
    {
        await NavigateAndBoot();

        await SubmitBtn.ClickAsync();

        await Expect(_page.ResidentNameError)
            .ToContainTextAsync("required", new() { Timeout = 5000 });

        await Expect(_page.CareLevelError).ToBeHiddenAsync();
        await Expect(_page.NotesError).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task status_remains_ready_when_validation_blocks_submission()
    {
        await NavigateAndBoot();

        await Expect(Status).ToHaveTextAsync("Ready");

        await SubmitBtn.ClickAsync();

        await Expect(_page.ResidentNameError)
            .ToContainTextAsync("required", new() { Timeout = 5000 });
        await Expect(Status).ToHaveTextAsync("Ready");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task resident_name_error_clears_when_admin_types_a_name()
    {
        await NavigateAndBoot();

        await SubmitBtn.ClickAsync();
        await Expect(_page.ResidentNameError)
            .ToContainTextAsync("required", new() { Timeout = 5000 });

        await _page.ResidentName.FillAndBlur("Margaret Thompson");

        await Expect(_page.ResidentNameError)
            .ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task monthly_rate_error_clears_when_admin_enters_a_rate()
    {
        await NavigateAndBoot();

        await SubmitBtn.ClickAsync();
        await Expect(_page.MonthlyRateError)
            .ToContainTextAsync("required", new() { Timeout = 5000 });

        await _page.MonthlyRate.FillAndBlur("2500");

        await Expect(_page.MonthlyRateError)
            .ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task fixing_missing_fields_and_resubmitting_admits_the_resident()
    {
        await NavigateAndBoot();

        await SubmitBtn.ClickAsync();
        await Expect(_page.ResidentNameError)
            .ToContainTextAsync("required", new() { Timeout = 5000 });

        await _page.ResidentName.FillAndBlur("Margaret Thompson");
        await _page.Physician.Type("smith");
        await _page.Physician.SelectItem("Dr. Smith");
        await _page.MonthlyRate.FillAndBlur("4500");

        await SubmitBtn.ClickAsync();
        await Expect(Status).ToContainTextAsync("Resident admitted", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task successful_admission_shows_green_confirmation()
    {
        await NavigateAndBoot();

        await _page.ResidentName.FillAndBlur("Dorothy Williams");
        await _page.Physician.Type("jones");
        await _page.Physician.SelectItem("Dr. Jones");
        await _page.MonthlyRate.FillAndBlur("3800");

        await SubmitBtn.ClickAsync();

        await Expect(Status).ToContainTextAsync("Resident admitted", new() { Timeout = 5000 });
        await Expect(Status).ToHaveClassAsync(new Regex("text-green-600"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task admission_without_optional_fields_still_succeeds()
    {
        await NavigateAndBoot();

        await _page.ResidentName.FillAndBlur("Harold Jenkins");
        await _page.Physician.Type("patel");
        await _page.Physician.SelectItem("Dr. Patel");
        await _page.MonthlyRate.FillAndBlur("5200");

        await SubmitBtn.ClickAsync();

        await Expect(Status).ToContainTextAsync("Resident admitted", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task complete_admission_posts_all_resident_data_to_server()
    {
        await NavigateAndBoot();

        await _page.ResidentName.FillAndBlur("Margaret Thompson");
        await _page.Physician.Type("smith");
        await _page.Physician.SelectItem("Dr. Smith");
        await _page.CareLevel.Select("Assisted Living");
        await _page.MonthlyRate.FillAndBlur("4500");
        await _page.IsActive.Toggle();
        await _page.Notes.FillAndBlur("Prefers morning medication");

        await Expect(_page.NameEcho)
            .ToContainTextAsync("Margaret Thompson", new() { Timeout = 5000 });
        await Expect(_page.PhysicianEcho)
            .ToContainTextAsync("smith", new() { Timeout = 5000 });
        await Expect(_page.RateEcho)
            .ToContainTextAsync("4500", new() { Timeout = 5000 });
        await Expect(_page.NotesEcho)
            .ToContainTextAsync("morning medication", new() { Timeout = 5000 });

        await SubmitBtn.ClickAsync();
        await Expect(Status).ToContainTextAsync("Resident admitted", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    private sealed class ResidentAdmissionPage
    {
        private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ResidentAdmissionModel__";
        private readonly IPage _page;

        public ResidentAdmissionPage(IPage page)
        {
            _page = page;
        }

        public NativeTextBoxLocator ResidentName => new(_page, Scope + "ResidentName");
        public AutoCompleteLocator Physician => new(_page, Scope + "Physician");
        public DropDownListLocator CareLevel => new(_page, Scope + "CareLevel");
        public NumericTextBoxLocator MonthlyRate => new(_page, Scope + "MonthlyRate");
        public SwitchLocator IsActive => new(_page, Scope + "IsActive");
        public NativeTextBoxLocator Notes => new(_page, Scope + "Notes");

        public ILocator Status => _page.Locator("#submit-status");
        public ILocator NameEcho => _page.Locator("#name-echo");
        public ILocator PhysicianEcho => _page.Locator("#physician-echo");
        public ILocator CareEcho => _page.Locator("#care-echo");
        public ILocator RateEcho => _page.Locator("#rate-echo");
        public ILocator ActiveEcho => _page.Locator("#active-echo");
        public ILocator NotesEcho => _page.Locator("#notes-echo");

        public ILocator ResidentNameError => _page.Locator("span[data-valmsg-for='ResidentName']");
        public ILocator PhysicianError => _page.Locator("span[data-valmsg-for='Physician']");
        public ILocator MonthlyRateError => _page.Locator("span[data-valmsg-for='MonthlyRate']");
        public ILocator CareLevelError => _page.Locator("span[data-valmsg-for='CareLevel']");
        public ILocator NotesError => _page.Locator("span[data-valmsg-for='Notes']");
    }
}
