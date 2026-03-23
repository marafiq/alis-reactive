using System.Text.RegularExpressions;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.AllModulesTogether.Workflows;

/// <summary>
/// As a facility administrator
/// I want to admit a new resident
/// So that their care plan is documented in the system
/// </summary>
[TestFixture]
public class WhenAdmittingNewResident : PlaywrightTestBase
{
    private const string Path = "/Sandbox/AllModulesTogether/BddExperiment";
    private PagePlan<BddExperimentModel> _plan = null!;

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<BddExperimentModel>.FromPage(Page);
    }

    private ILocator SubmitBtn => Page.Locator("#submit-btn");
    private ILocator Status => _plan.Element("submit-status");

    // ─── Physician Search ───

    [Test]
    public async Task searching_physician_by_name_shows_matching_results_to_pick_from()
    {
        await NavigateAndBoot();
        var physician = _plan.AutoComplete(m => m.Physician);

        await physician.Type("smith");
        await Expect(physician.PopupItems.First).ToBeVisibleAsync(new() { Timeout = 5000 });
        await physician.SelectItem("Dr. Smith");

        await Expect(_plan.Element("physician-echo"))
            .ToContainTextAsync("smith", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task searching_physician_filters_to_matching_names_only()
    {
        await NavigateAndBoot();
        var physician = _plan.AutoComplete(m => m.Physician);

        await physician.Type("chen");
        await Expect(physician.PopupItem("Dr. Chen")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(physician.PopupItem("Dr. Smith")).Not.ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    // ─── Care Level Selection ───

    [Test]
    public async Task choosing_care_level_confirms_selection()
    {
        await NavigateAndBoot();

        await _plan.DropDownList(m => m.CareLevel).Select("Memory Care");

        await Expect(_plan.Element("care-echo"))
            .ToContainTextAsync("memory", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ─── Echo Panel — Live Feedback ───

    [Test]
    public async Task typing_resident_name_echoes_to_the_feedback_panel()
    {
        await NavigateAndBoot();

        await _plan.TextBox(m => m.ResidentName).FillAndBlur("Eleanor Rigby");

        await Expect(_plan.Element("name-echo"))
            .ToContainTextAsync("Eleanor Rigby", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task entering_monthly_rate_echoes_to_the_feedback_panel()
    {
        await NavigateAndBoot();

        await _plan.NumericTextBox(m => m.MonthlyRate).FillAndBlur("3200");

        await Expect(_plan.Element("rate-echo"))
            .ToContainTextAsync("3200", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task toggling_active_switch_echoes_to_the_feedback_panel()
    {
        await NavigateAndBoot();

        await _plan.Switch(m => m.IsActive).Toggle();

        await Expect(_plan.Element("active-echo"))
            .ToContainTextAsync("true", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task typing_notes_echoes_to_the_feedback_panel()
    {
        await NavigateAndBoot();

        await _plan.TextBox(m => m.Notes).FillAndBlur("Allergic to penicillin");

        await Expect(_plan.Element("notes-echo"))
            .ToContainTextAsync("Allergic to penicillin", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ─── Validation — Required Fields ───

    [Test]
    public async Task submitting_without_required_fields_tells_admin_what_is_missing()
    {
        await NavigateAndBoot();

        await SubmitBtn.ClickAsync();

        await Expect(_plan.ErrorFor(m => m.ResidentName))
            .ToContainTextAsync("Resident name is required", new() { Timeout = 5000 });
        await Expect(_plan.ErrorFor(m => m.Physician))
            .ToContainTextAsync("Physician is required", new() { Timeout = 5000 });
        await Expect(_plan.ErrorFor(m => m.MonthlyRate))
            .ToContainTextAsync("Monthly rate is required", new() { Timeout = 5000 });
        await Expect(Status).ToHaveTextAsync("Ready");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task optional_fields_do_not_show_errors_on_empty_submit()
    {
        await NavigateAndBoot();

        await SubmitBtn.ClickAsync();

        // Wait for required errors to appear first (proves validation ran)
        await Expect(_plan.ErrorFor(m => m.ResidentName))
            .ToContainTextAsync("required", new() { Timeout = 5000 });

        // CareLevel and Notes are optional — no error messages
        await Expect(_plan.ErrorFor(m => m.CareLevel)).ToBeHiddenAsync();
        await Expect(_plan.ErrorFor(m => m.Notes)).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task status_remains_ready_when_validation_blocks_submission()
    {
        await NavigateAndBoot();

        await Expect(Status).ToHaveTextAsync("Ready");

        await SubmitBtn.ClickAsync();

        // Validation blocks the HTTP post — status should NOT change to "Saving..."
        // and should NOT change to "Resident admitted"
        await Expect(_plan.ErrorFor(m => m.ResidentName))
            .ToContainTextAsync("required", new() { Timeout = 5000 });
        await Expect(Status).ToHaveTextAsync("Ready");
        AssertNoConsoleErrors();
    }

    // ─── Validation — Live Clear ───

    [Test]
    public async Task resident_name_error_clears_when_admin_types_a_name()
    {
        await NavigateAndBoot();

        await SubmitBtn.ClickAsync();
        await Expect(_plan.ErrorFor(m => m.ResidentName))
            .ToContainTextAsync("required", new() { Timeout = 5000 });

        await _plan.TextBox(m => m.ResidentName).FillAndBlur("Margaret Thompson");

        await Expect(_plan.ErrorFor(m => m.ResidentName))
            .ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task monthly_rate_error_clears_when_admin_enters_a_rate()
    {
        await NavigateAndBoot();

        await SubmitBtn.ClickAsync();
        await Expect(_plan.ErrorFor(m => m.MonthlyRate))
            .ToContainTextAsync("required", new() { Timeout = 5000 });

        await _plan.NumericTextBox(m => m.MonthlyRate).FillAndBlur("2500");

        await Expect(_plan.ErrorFor(m => m.MonthlyRate))
            .ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ─── Fix and Resubmit ───

    [Test]
    public async Task fixing_missing_fields_and_resubmitting_admits_the_resident()
    {
        await NavigateAndBoot();

        await SubmitBtn.ClickAsync();
        await Expect(_plan.ErrorFor(m => m.ResidentName))
            .ToContainTextAsync("required", new() { Timeout = 5000 });

        await _plan.TextBox(m => m.ResidentName).FillAndBlur("Margaret Thompson");
        var physician = _plan.AutoComplete(m => m.Physician);
        await physician.Type("smith");
        await physician.SelectItem("Dr. Smith");
        await _plan.NumericTextBox(m => m.MonthlyRate).FillAndBlur("4500");

        await SubmitBtn.ClickAsync();
        await Expect(Status).ToContainTextAsync("Resident admitted", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ─── Successful Admission ───

    [Test]
    public async Task successful_admission_shows_green_confirmation()
    {
        await NavigateAndBoot();

        await _plan.TextBox(m => m.ResidentName).FillAndBlur("Dorothy Williams");
        var physician = _plan.AutoComplete(m => m.Physician);
        await physician.Type("jones");
        await physician.SelectItem("Dr. Jones");
        await _plan.NumericTextBox(m => m.MonthlyRate).FillAndBlur("3800");

        await SubmitBtn.ClickAsync();

        await Expect(Status).ToContainTextAsync("Resident admitted", new() { Timeout = 5000 });
        await Expect(Status).ToHaveClassAsync(new Regex("text-green-600"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task admission_without_optional_fields_still_succeeds()
    {
        await NavigateAndBoot();

        // Fill only the three required fields — skip CareLevel, IsActive, Notes
        await _plan.TextBox(m => m.ResidentName).FillAndBlur("Harold Jenkins");
        var physician = _plan.AutoComplete(m => m.Physician);
        await physician.Type("patel");
        await physician.SelectItem("Dr. Patel");
        await _plan.NumericTextBox(m => m.MonthlyRate).FillAndBlur("5200");

        await SubmitBtn.ClickAsync();

        await Expect(Status).ToContainTextAsync("Resident admitted", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task complete_admission_posts_all_resident_data_to_server()
    {
        await NavigateAndBoot();

        await _plan.TextBox(m => m.ResidentName).FillAndBlur("Margaret Thompson");
        var physician = _plan.AutoComplete(m => m.Physician);
        await physician.Type("smith");
        await physician.SelectItem("Dr. Smith");
        await _plan.DropDownList(m => m.CareLevel).Select("Assisted Living");
        await _plan.NumericTextBox(m => m.MonthlyRate).FillAndBlur("4500");
        await _plan.Switch(m => m.IsActive).Toggle();
        await _plan.TextBox(m => m.Notes).FillAndBlur("Prefers morning medication");

        var request = await Page.RunAndWaitForRequestAsync(
            async () => await SubmitBtn.ClickAsync(),
            "**/Sandbox/AllModulesTogether/BddExperiment/Submit");

        var body = request.PostData ?? "";
        Assert.That(body, Does.Contain("Margaret Thompson"), "Resident name");
        Assert.That(body, Does.Contain("4500"), "Monthly rate");
        Assert.That(body, Does.Contain("assisted"), "Care level");
        Assert.That(body, Does.Contain("morning medication"), "Notes");

        await Expect(Status).ToContainTextAsync("Resident admitted", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
