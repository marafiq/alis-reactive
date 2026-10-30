using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.RegularExpressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Validation.IntakeSteps;

/// <summary>
/// As an admissions coordinator, I want to take a new resident's intake one step at a time in one
/// form, so that each step asks only about what it shows and finishing never trips over a step I
/// already completed.
/// </summary>
/// <remarks>
/// Each step is a partial the server returns into the form. A step checks its fields against the
/// whole form, so the form must forget a step's rules when the next step replaces it. The care
/// step's mobility questions arrive once a care level is chosen; until then Finish says what they
/// still need. The server checks the whole intake with the same rules the steps check. The nurse
/// review, a page action, checks the care needs only where the care step renders them.
/// </remarks>
[TestFixture]
public class WhenCoordinatorTakesIntakeInSteps : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/IntakeSteps";
    private const string NoOpenRoom =
        "Memory care has no open room this month. Choose another level or add the resident to the waitlist.";
    private const string FamilyPhoneFormat = "Enter the family phone as 555-0100.";
    private const string MobilityAidMissing = "Choose the resident's mobility aid.";

    private PagePlan<IntakeStepsModel> _plan = null!;
    private ConcurrentQueue<string> _residentStepsSent = null!;
    private ConcurrentQueue<string> _finishesSent = null!;
    private ConcurrentQueue<string> _nurseReviewsSent = null!;

    // The intake form and the nurse's care needs are two forms, each with a summary of its own.
    private ILocator IntakeSummary => Page.Locator("#intake-form_validation_summary");
    private ILocator VisibleSummaries => Page.Locator("[data-reactive-validation-summary]:visible");
    private ILocator StepHeading => Page.Locator("#intake-step h2");
    private ILocator ResidentName => Input(m => m.ResidentName);
    private ILocator FamilyPhone => Input(m => m.FamilyPhone);
    private ILocator CareLevel => Input(m => m.CareLevel);
    private ILocator MobilityAid => Input(m => m.MobilityAid);
    private ILocator ContinueToCare => Page.Locator("#continue-to-care-btn");
    private ILocator FinishIntake => Page.Locator("#finish-intake-btn");
    private ILocator IntakeStatus => Page.Locator("#intake-status");
    private ILocator AskNurseToReview => Page.Locator("#ask-nurse-review-btn");
    private ILocator NurseReviewStatus => Page.Locator("#nurse-review-status");

    // The steps arrive after boot, so their inputs are found by the id the model member gives them.
    private ILocator Input(Expression<Func<IntakeStepsModel, object?>> member) =>
        Page.Locator($"#{IdGenerator.For(member)}");

    private async Task NavigateAndBoot()
    {
        _residentStepsSent = new ConcurrentQueue<string>();
        _finishesSent = new ConcurrentQueue<string>();
        _nurseReviewsSent = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.EndsWith("/Sandbox/Validation/IntakeSteps/Resident")) _residentStepsSent.Enqueue(request.Url);
            if (request.Url.Contains("/Sandbox/Validation/IntakeSteps/Finish")) _finishesSent.Enqueue(request.Url);
            if (request.Url.Contains("/Sandbox/Validation/IntakeSteps/NurseReview")) _nurseReviewsSent.Enqueue(request.Url);
        };
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<IntakeStepsModel>.FromPage(Page);
        await Expect(StepHeading).ToHaveTextAsync("Step 1 of 2: Resident", new() { Timeout = 5000 });
    }

    private async Task CompleteResidentStep(string name)
    {
        await ResidentName.FillAsync(name);
        await FamilyPhone.FillAsync("555-0100");
        await Page.Keyboard.PressAsync("Tab");
        await ClickWhenStable(ContinueToCare);
        await Expect(StepHeading).ToHaveTextAsync($"Step 2 of 2: Care needs for {name}", new() { Timeout = 5000 });
    }

    // Choosing a level asks the server for its mobility questions; the aid is answered once they arrive.
    private async Task ChooseCareLevelAndAid(string careLevel, string aid)
    {
        await CareLevel.SelectOptionAsync(careLevel);
        await Expect(Page.Locator("#mobility-questions h3")).ToHaveTextAsync($"Mobility for {careLevel}", new() { Timeout = 5000 });
        await MobilityAid.SelectOptionAsync(aid);
    }

    [Test]
    public async Task the_resident_step_refuses_to_continue_without_a_name()
    {
        await NavigateAndBoot();

        await ClickWhenStable(ContinueToCare);

        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("Enter the resident's name.");
        await Expect(StepHeading).ToHaveTextAsync("Step 1 of 2: Resident");
        await Expect(CareLevel).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task finishing_the_care_step_saves_the_intake_without_rechecking_the_resident_step()
    {
        await NavigateAndBoot();
        await CompleteResidentStep("Jane Smith");
        await Expect(ResidentName).ToHaveCountAsync(0);
        await ChooseCareLevelAndAid("Assisted living", "Walker");

        var finish = await Page.RunAndWaitForRequestAsync(
            () => ClickWhenStable(FinishIntake),
            request => request.Method == "POST" && request.Url.Contains("/IntakeSteps/Finish"));

        // The resident step left the page; its answers travel beside the care step's inputs.
        using (var body = JsonDocument.Parse(finish.PostData ?? "{}"))
        {
            Assert.That(body.RootElement.GetProperty("ResidentName").GetString(), Is.EqualTo("Jane Smith"), finish.PostData);
            Assert.That(body.RootElement.GetProperty("FamilyPhone").GetString(), Is.EqualTo("555-0100"), finish.PostData);
            Assert.That(body.RootElement.GetProperty("CareLevel").GetString(), Is.EqualTo("Assisted living"), finish.PostData);
            Assert.That(body.RootElement.GetProperty("MobilityAid").GetString(), Is.EqualTo("Walker"), finish.PostData);
        }
        await Expect(IntakeStatus).ToHaveTextAsync("Assisted living intake saved for Jane Smith.", new() { Timeout = 5000 });
        await Expect(VisibleSummaries).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task finishing_without_a_care_level_asks_for_it_and_for_the_mobility_questions_it_opens()
    {
        await NavigateAndBoot();
        await CompleteResidentStep("Jane Smith");

        await ClickWhenStable(FinishIntake);

        await Expect(_plan.ErrorFor(m => m.CareLevel)).ToHaveTextAsync("Choose a care level.");
        // The mobility questions are not on the page yet, so what they need is said in the summary.
        await Expect(IntakeSummary).ToHaveTextAsync(MobilityAidMissing);
        await Expect(IntakeStatus).ToHaveTextAsync("No intake saved yet.");
        Assert.That(_finishesSent, Is.Empty, "A care step without a care level may not be sent.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task finishing_while_the_mobility_questions_are_still_loading_sends_nothing_and_says_what_they_need()
    {
        var questionsReleased = new SemaphoreSlim(0);
        await NavigateAndBoot();
        await CompleteResidentStep("Jane Smith");
        await Page.RouteAsync("**/Sandbox/Validation/IntakeSteps/MobilityQuestions**", async route =>
        {
            await questionsReleased.WaitAsync(TimeSpan.FromSeconds(30));
            await route.ContinueAsync();
        });
        await CareLevel.SelectOptionAsync("Assisted living");

        await ClickWhenStable(FinishIntake);

        await Expect(IntakeSummary).ToHaveTextAsync(MobilityAidMissing);
        await Expect(_plan.ErrorFor(m => m.CareLevel)).ToHaveTextAsync("");
        Assert.That(_finishesSent, Is.Empty, "An intake whose mobility questions have not arrived is not sent.");

        questionsReleased.Release();
        await Expect(Page.Locator("#mobility-questions h3")).ToHaveTextAsync("Mobility for Assisted living", new() { Timeout = 5000 });
        await MobilityAid.SelectOptionAsync("Cane");
        await ClickWhenStable(FinishIntake);
        await Expect(IntakeStatus).ToHaveTextAsync("Assisted living intake saved for Jane Smith.", new() { Timeout = 5000 });
        await Expect(VisibleSummaries).ToHaveCountAsync(0);
        Assert.That(_finishesSent, Has.Count.EqualTo(1), "Only the complete intake is sent.");
        AssertNoConsoleErrors();
    }

    // The review checks the care needs where the care step renders them. Before that step there is no
    // such place on the page: nothing is checked, so nothing is asked for, and nothing is sent.
    [Test]
    public async Task asking_the_nurse_to_review_care_needs_before_the_care_step_sends_nothing()
    {
        await NavigateAndBoot();

        await ClickWhenStable(AskNurseToReview);

        await Expect(NurseReviewStatus).ToHaveTextAsync("Not sent to the nurse yet.");
        await Expect(VisibleSummaries).ToHaveCountAsync(0);
        await Expect(StepHeading).ToHaveTextAsync("Step 1 of 2: Resident");

        await CompleteResidentStep("Jane Smith");
        await ChooseCareLevelAndAid("Assisted living", "Walker");
        await ClickWhenStable(AskNurseToReview);
        await Expect(NurseReviewStatus).ToHaveTextAsync(
            "Assisted living care needs sent to the nurse for review.", new() { Timeout = 5000 });
        Assert.That(_nurseReviewsSent, Has.Count.EqualTo(1), "The review asked for before the care step was not sent.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_care_level_with_no_open_room_is_refused_beside_the_care_level()
    {
        await NavigateAndBoot();
        await CompleteResidentStep("Jane Smith");
        await ChooseCareLevelAndAid(IntakeCareValidator.FullCareLevel, "Walker");

        await ClickWhenStable(FinishIntake);

        await Expect(_plan.ErrorFor(m => m.CareLevel)).ToHaveTextAsync(NoOpenRoom, new() { Timeout = 5000 });
        await Expect(CareLevel).ToHaveClassAsync(new Regex(@"\balis-has-error\b"));
        await Expect(VisibleSummaries).ToHaveCountAsync(0);
        await Expect(IntakeStatus).ToHaveTextAsync("No intake saved yet.");

        await ChooseCareLevelAndAid("Assisted living", "Walker");
        await ClickWhenStable(FinishIntake);
        await Expect(IntakeStatus).ToHaveTextAsync("Assisted living intake saved for Jane Smith.", new() { Timeout = 5000 });
        await Expect(_plan.ErrorFor(m => m.CareLevel)).ToBeHiddenAsync();
        await Expect(CareLevel).Not.ToHaveClassAsync(new Regex(@"\balis-has-error\b"));
        Assert.That(_finishesSent, Has.Count.EqualTo(2), "Each Finish asks the server once.");
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task the_resident_step_refuses_a_family_phone_out_of_house_format_without_sending_it()
    {
        await NavigateAndBoot();
        await ResidentName.FillAsync("Jane Smith");
        await FamilyPhone.FillAsync("5550100");
        await Page.Keyboard.PressAsync("Tab");

        await ClickWhenStable(ContinueToCare);

        await Expect(_plan.ErrorFor(m => m.FamilyPhone)).ToHaveTextAsync(FamilyPhoneFormat);
        await Expect(StepHeading).ToHaveTextAsync("Step 1 of 2: Resident");
        Assert.That(_residentStepsSent, Is.Empty, "The resident step sends no phone it can see is wrong.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_family_phone_out_of_house_format_on_the_care_step_is_refused_by_the_server_with_the_same_message()
    {
        await NavigateAndBoot();
        await CompleteResidentStep("Jane Smith");
        await ChooseCareLevelAndAid("Assisted living", "Walker");
        await FamilyPhone.FillAsync("5550100");
        await Page.Keyboard.PressAsync("Tab");

        await ClickWhenStable(FinishIntake);

        // The care step does not check the phone in the browser, so the server answers with the
        // resident step's rule, in the same words, beside the phone the care step renders.
        await Expect(_plan.ErrorFor(m => m.FamilyPhone)).ToHaveTextAsync(FamilyPhoneFormat, new() { Timeout = 5000 });
        await Expect(VisibleSummaries).ToHaveCountAsync(0);
        await Expect(IntakeStatus).ToHaveTextAsync("No intake saved yet.");
        Assert.That(_finishesSent, Has.Count.EqualTo(1), "The care step leaves the family phone to the server.");

        await FamilyPhone.FillAsync("555-0100");
        await Page.Keyboard.PressAsync("Tab");
        await ClickWhenStable(FinishIntake);
        await Expect(IntakeStatus).ToHaveTextAsync("Assisted living intake saved for Jane Smith.", new() { Timeout = 5000 });
        await Expect(VisibleSummaries).ToHaveCountAsync(0);
        Assert.That(_finishesSent, Has.Count.EqualTo(2));
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }
}
