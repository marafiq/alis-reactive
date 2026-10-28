using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.ResidentAdmission;

namespace Alis.Reactive.PlaywrightTests.Patterns.Workflows;

/// <summary>
/// As an admissions coordinator, I want the admission form to tell me what is missing before anything is
/// sent, address the admission notice to the physician I choose, and confirm the admission in the
/// server's words, so that every resident I admit is admitted with a physician who will see them.
/// </summary>
/// <remarks>
/// The physician search lists "Dr. Alan Smith" and matches and sends the name the notice is addressed to,
/// "Alan Smith, MD". Dr. Wei Chen's panel is full this month: only the server knows it and refuses him.
/// The server's confirmation names the resident, the care level, the physician, the rate and whether the
/// resident moves in today, so a value the form failed to send reads differently. The notes are sent with
/// the admission but the confirmation does not repeat them. The rate and the switch have no reaction on
/// this page; they are read only as the server answers them. Their Change payloads are proven on their
/// own journeys: Components.Fusion.NumericTextBox.WhenNumericValueEntered and
/// Components.Fusion.Switch.WhenSwitchToggles.
/// </remarks>
[TestFixture]
public class WhenAdmittingNewResident : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Patterns/BddExperiment";
    private const string SubmitUrl = "**/Sandbox/Patterns/BddExperiment/Submit";
    private const string Refused = "the server responded with a status of 400";

    private static readonly Regex Confirmed = new(@"\btext-green-700\b");
    private static readonly Regex Warned = new(@"\btext-red-700\b");

    private PagePlan<AdmitResidentModel> _plan = null!;
    private ConcurrentQueue<string> _admissionsSent = null!;
    private SemaphoreSlim _released = null!;

    private ILocator AdmitResident => Page.Locator("#admit-resident-btn");
    private ILocator AdmissionStatus => Page.Locator("#admission-status");
    private ILocator AdmissionResident => Page.Locator("#admission-resident");
    private ILocator PhysicianNotice => Page.Locator("#physician-notice");
    private ILocator MemoryCareNote => Page.Locator("#memory-care-note");
    private AutoCompleteLocator Physician => _plan.AutoComplete(m => m.Physician);
    private NumericTextBoxLocator MonthlyRate => _plan.NumericTextBox(m => m.MonthlyRate);

    private async Task NavigateAndBoot()
    {
        _admissionsSent = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains("/Sandbox/Patterns/BddExperiment/Submit")) _admissionsSent.Enqueue(request.Url);
        };
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<AdmitResidentModel>.FromPage(Page);
    }

    // The request is held at the browser until the test releases it, then sent on unchanged, so what the
    // page shows before the answer arrives can be seen.
    private async Task HoldUntilReleased(string url)
    {
        _released = new SemaphoreSlim(0);
        await Page.RouteAsync(url, async route =>
        {
            await _released.WaitAsync(TimeSpan.FromSeconds(30));
            await route.ContinueAsync();
        });
    }

    private async Task ChoosePhysician(string search, string physician)
    {
        await Physician.Type(search);
        await Physician.SelectItem(physician);
    }

    // The three fields an admission cannot go without.
    private async Task FillTheRequiredFields(string resident, string search, string physician, string rate)
    {
        await _plan.TextBox(m => m.ResidentName).FillAndBlur(resident);
        await ChoosePhysician(search, physician);
        await MonthlyRate.FillAndBlur(rate);
    }

    [Test]
    public async Task the_form_marks_the_name_the_physician_and_the_rate_as_required()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("label", new() { HasText = "Resident name" })).ToHaveTextAsync("Resident name *");
        await Expect(Page.Locator("label", new() { HasText = "Attending physician" })).ToHaveTextAsync("Attending physician *");
        await Expect(Page.Locator("label", new() { HasText = "Monthly rate" })).ToHaveTextAsync("Monthly rate *");
        await Expect(Page.Locator("label", new() { HasText = "Care level" })).ToHaveTextAsync("Care level");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task typing_the_residents_name_names_the_admission()
    {
        await NavigateAndBoot();
        await Expect(AdmissionResident).ToHaveTextAsync("a new resident");

        await _plan.TextBox(m => m.ResidentName).FillAndBlur("Eleanor Rigby");

        await Expect(AdmissionResident).ToHaveTextAsync("Eleanor Rigby");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task searching_for_a_physician_lists_only_the_physicians_whose_name_matches()
    {
        await NavigateAndBoot();

        await Physician.Type("chen");

        await Expect(Physician.PopupItems).ToHaveTextAsync(new[] { "Dr. Wei Chen" });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_the_attending_physician_addresses_the_admission_notice_to_them()
    {
        await NavigateAndBoot();
        await Expect(PhysicianNotice).ToHaveTextAsync("the attending physician");

        await ChoosePhysician("smith", "Dr. Alan Smith");

        await Expect(PhysicianNotice).ToHaveTextAsync("Alan Smith, MD");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_memory_care_asks_for_a_cognitive_assessment()
    {
        await NavigateAndBoot();
        await Expect(MemoryCareNote).ToBeHiddenAsync();

        await _plan.DropDownList(m => m.CareLevel).Select("Memory Care");

        await Expect(MemoryCareNote).ToBeVisibleAsync();
        await Expect(MemoryCareNote).ToHaveTextAsync("Memory care admissions need a cognitive assessment on file before move-in.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_from_memory_care_to_another_level_drops_the_assessment_note()
    {
        await NavigateAndBoot();
        await _plan.DropDownList(m => m.CareLevel).Select("Memory Care");
        await Expect(MemoryCareNote).ToBeVisibleAsync();

        await _plan.DropDownList(m => m.CareLevel).Select("Assisted Living");

        await Expect(MemoryCareNote).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task admitting_an_empty_form_says_what_is_missing_and_sends_nothing()
    {
        await NavigateAndBoot();

        await ClickWhenStable(AdmitResident);

        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("Enter the resident's name.");
        await Expect(_plan.ErrorFor(m => m.Physician)).ToHaveTextAsync("Choose the attending physician.");
        await Expect(_plan.ErrorFor(m => m.MonthlyRate)).ToHaveTextAsync("Enter the monthly rate.");
        await Expect(_plan.ErrorFor(m => m.CareLevel)).ToBeHiddenAsync();
        await Expect(_plan.ErrorFor(m => m.Notes)).ToBeHiddenAsync();
        await Expect(AdmissionStatus).ToHaveTextAsync("Not admitted yet.");
        Assert.That(_admissionsSent, Is.Empty, "An admission the page can see is incomplete is not sent.");
        AssertNoConsoleErrors();
    }

    // Typing a name clears the message at once, while the coordinator is still in the field.
    [Test]
    public async Task the_name_message_clears_as_soon_as_a_name_is_typed()
    {
        await NavigateAndBoot();
        var residentName = _plan.TextBox(m => m.ResidentName);
        await ClickWhenStable(AdmitResident);
        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("Enter the resident's name.");

        await residentName.Fill("Margaret Thompson");

        await Expect(residentName.Input).ToBeFocusedAsync();
        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // Typing a rate clears the message at once, while the coordinator is still in the field.
    [Test]
    public async Task the_rate_message_clears_as_soon_as_a_rate_is_typed()
    {
        await NavigateAndBoot();
        await ClickWhenStable(AdmitResident);
        await Expect(_plan.ErrorFor(m => m.MonthlyRate)).ToHaveTextAsync("Enter the monthly rate.");

        await MonthlyRate.Fill("2500");

        await Expect(MonthlyRate.Input).ToBeFocusedAsync();
        await Expect(_plan.ErrorFor(m => m.MonthlyRate)).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_page_says_it_is_admitting_the_resident_until_the_server_answers()
    {
        await NavigateAndBoot();
        await HoldUntilReleased(SubmitUrl);
        await FillTheRequiredFields("Margaret Thompson", "smith", "Dr. Alan Smith", "4500");

        await ClickWhenStable(AdmitResident);

        await Expect(AdmissionStatus).ToHaveTextAsync("Admitting the resident...");
        _released.Release();
        await Expect(AdmissionStatus).ToHaveTextAsync(
            "Margaret Thompson is admitted under Alan Smith, MD at $4,500.00 a month, moving in on the scheduled day.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_complete_admission_is_confirmed_in_the_servers_words()
    {
        await NavigateAndBoot();
        await FillTheRequiredFields("Margaret Thompson", "smith", "Dr. Alan Smith", "4500");
        await _plan.DropDownList(m => m.CareLevel).Select("Assisted Living");
        await _plan.Switch(m => m.MovesInToday).Toggle();
        await _plan.TextBox(m => m.Notes).FillAndBlur("Prefers morning medication");

        await ClickWhenStable(AdmitResident);

        await Expect(AdmissionStatus).ToHaveTextAsync(
            "Margaret Thompson is admitted to Assisted Living under Alan Smith, MD at $4,500.00 a month, moving in today.");
        await Expect(AdmissionStatus).ToHaveClassAsync(Confirmed);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_resident_admitted_without_a_care_level_moves_in_on_the_scheduled_day()
    {
        await NavigateAndBoot();
        await FillTheRequiredFields("Harold Jenkins", "patel", "Dr. Priya Patel", "5200");

        await ClickWhenStable(AdmitResident);

        await Expect(AdmissionStatus).ToHaveTextAsync(
            "Harold Jenkins is admitted under Priya Patel, MD at $5,200.00 a month, moving in on the scheduled day.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task filling_in_what_was_missing_and_admitting_again_admits_the_resident()
    {
        await NavigateAndBoot();
        await ClickWhenStable(AdmitResident);
        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("Enter the resident's name.");

        await FillTheRequiredFields("Margaret Thompson", "smith", "Dr. Alan Smith", "4500");
        await ClickWhenStable(AdmitResident);

        await Expect(AdmissionStatus).ToHaveTextAsync(
            "Margaret Thompson is admitted under Alan Smith, MD at $4,500.00 a month, moving in on the scheduled day.");
        Assert.That(_admissionsSent, Has.Count.EqualTo(1), "Only the completed admission is sent.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_physician_whose_panel_is_full_is_refused_by_the_server_under_the_physician()
    {
        await NavigateAndBoot();
        await FillTheRequiredFields("Margaret Thompson", "chen", "Dr. Wei Chen", "4500");

        await ClickWhenStable(AdmitResident);

        await Expect(_plan.ErrorFor(m => m.Physician)).ToHaveTextAsync("Wei Chen, MD is not taking new residents this month.");
        await Expect(AdmissionStatus).ToHaveTextAsync("Not admitted: correct the marked fields.");
        await Expect(AdmissionStatus).ToHaveClassAsync(Warned);
        Assert.That(_admissionsSent, Has.Count.EqualTo(1), "Only the server knows whose panel is full.");
        AssertNoConsoleErrorsExcept(Refused);
    }

    [Test]
    public async Task choosing_another_physician_after_the_refusal_admits_the_resident()
    {
        await NavigateAndBoot();
        await FillTheRequiredFields("Margaret Thompson", "chen", "Dr. Wei Chen", "4500");
        await ClickWhenStable(AdmitResident);
        await Expect(_plan.ErrorFor(m => m.Physician)).ToHaveTextAsync("Wei Chen, MD is not taking new residents this month.");

        await Physician.Clear();
        await ChoosePhysician("smith", "Dr. Alan Smith");
        await ClickWhenStable(AdmitResident);

        await Expect(AdmissionStatus).ToHaveTextAsync(
            "Margaret Thompson is admitted under Alan Smith, MD at $4,500.00 a month, moving in on the scheduled day.");
        await Expect(AdmissionStatus).ToHaveClassAsync(Confirmed);
        await Expect(AdmissionStatus).Not.ToHaveClassAsync(Warned);
        await Expect(_plan.ErrorFor(m => m.Physician)).ToBeHiddenAsync();
        AssertNoConsoleErrorsExcept(Refused);
    }
}
