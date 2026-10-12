using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Text.Json;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Validation.IntakeSteps;

/// <summary>
/// As an admissions coordinator, I want to take a new resident's intake one step at a time in one
/// form, so that each step asks only about what it shows and finishing never trips over a step I
/// already completed.
/// </summary>
/// <remarks>
/// Each step is a partial the server returns into the form. A step checks its fields against the
/// whole form, so the form must forget a step's rules when the next step replaces it.
/// </remarks>
[TestFixture]
public class WhenCoordinatorTakesIntakeInSteps : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/IntakeSteps";
    private const string NoOpenRoom =
        "Memory care has no open room this month. Choose another level or add the resident to the waitlist.";

    private PagePlan<IntakeStepsModel> _plan = null!;
    private ConcurrentQueue<string> _finishesSent = null!;

    private ILocator ValidationSummary => Page.Locator("[data-reactive-validation-summary]");
    private ILocator StepHeading => Page.Locator("#intake-step h2");
    private ILocator ResidentName => Input(m => m.ResidentName);
    private ILocator FamilyPhone => Input(m => m.FamilyPhone);
    private ILocator CareLevel => Input(m => m.CareLevel);
    private ILocator ContinueToCare => Page.Locator("#continue-to-care-btn");
    private ILocator FinishIntake => Page.Locator("#finish-intake-btn");
    private ILocator IntakeStatus => Page.Locator("#intake-status");

    // The steps arrive after boot, so their inputs are found by the id the model member gives them.
    private ILocator Input(Expression<Func<IntakeStepsModel, object?>> member) =>
        Page.Locator($"#{IdGenerator.For(member)}");

    private async Task NavigateAndBoot()
    {
        _finishesSent = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains("/Sandbox/Validation/IntakeSteps/Finish")) _finishesSent.Enqueue(request.Url);
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
        await CareLevel.SelectOptionAsync("Assisted living");

        var finish = await Page.RunAndWaitForRequestAsync(
            () => ClickWhenStable(FinishIntake),
            request => request.Method == "POST" && request.Url.Contains("/IntakeSteps/Finish"));

        // The resident step left the page; its answers travel beside the care step's inputs.
        using (var body = JsonDocument.Parse(finish.PostData ?? "{}"))
        {
            Assert.That(body.RootElement.GetProperty("ResidentName").GetString(), Is.EqualTo("Jane Smith"), finish.PostData);
            Assert.That(body.RootElement.GetProperty("FamilyPhone").GetString(), Is.EqualTo("555-0100"), finish.PostData);
            Assert.That(body.RootElement.GetProperty("CareLevel").GetString(), Is.EqualTo("Assisted living"), finish.PostData);
        }
        await Expect(IntakeStatus).ToHaveTextAsync("Assisted living intake saved for Jane Smith.", new() { Timeout = 5000 });
        await Expect(ValidationSummary).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task finishing_without_a_care_level_is_asked_for_before_anything_is_sent()
    {
        await NavigateAndBoot();
        await CompleteResidentStep("Jane Smith");

        await ClickWhenStable(FinishIntake);

        await Expect(_plan.ErrorFor(m => m.CareLevel)).ToHaveTextAsync("Choose a care level.");
        await Expect(ValidationSummary).ToBeHiddenAsync();
        await Expect(IntakeStatus).ToHaveTextAsync("No intake saved yet.");
        Assert.That(_finishesSent, Is.Empty, "A care step without a care level may not be sent.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_care_level_with_no_open_room_is_refused_beside_the_care_level()
    {
        await NavigateAndBoot();
        await CompleteResidentStep("Jane Smith");
        await CareLevel.SelectOptionAsync(IntakeCareValidator.FullCareLevel);

        await ClickWhenStable(FinishIntake);

        await Expect(_plan.ErrorFor(m => m.CareLevel)).ToHaveTextAsync(NoOpenRoom, new() { Timeout = 5000 });
        await Expect(ValidationSummary).ToBeHiddenAsync();
        await Expect(IntakeStatus).ToHaveTextAsync("No intake saved yet.");

        await CareLevel.SelectOptionAsync("Assisted living");
        await ClickWhenStable(FinishIntake);
        await Expect(IntakeStatus).ToHaveTextAsync("Assisted living intake saved for Jane Smith.", new() { Timeout = 5000 });
        await Expect(_plan.ErrorFor(m => m.CareLevel)).ToBeHiddenAsync();
        Assert.That(_finishesSent, Has.Count.EqualTo(2), "Each Finish asks the server once.");
        AssertNoConsoleErrorsExcept("400");
    }
}
