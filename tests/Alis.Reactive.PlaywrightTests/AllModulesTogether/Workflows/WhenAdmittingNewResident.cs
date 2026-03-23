using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.AllModulesTogether.Workflows.ResidentAdmission;

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
    public async Task choosing_care_level_confirms_selection()
    {
        await NavigateAndBoot();

        await _plan.DropDownList(m => m.CareLevel).Select("Memory Care");

        await Expect(_plan.Element("care-echo"))
            .ToContainTextAsync("memory", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

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
