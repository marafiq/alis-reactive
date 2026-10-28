using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Stepper;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Stepper;

/// <summary>
/// As a move-in coordinator, I want a checklist that walks me through a resident's move-in in order and
/// will not let me move the resident in before the room is signed off, so that no resident moves into a
/// room nobody has inspected.
/// </summary>
/// <remarks>
/// The checklist is Paperwork, Room ready, Moved in. The page says where the checklist is and what to do
/// there; moving to Moved in waits for the signed room inspection. The admission steps below it are a
/// stepper that opens its steps in order and marks Intake done (optional) and Review in need of attention.
/// RefreshProgressbar is not exercised: it redraws the bar after the stepper's container changes size,
/// which no page here does. StepChanging's PreviousStep and IsInteracted are the fields StepChanged
/// carries, read here from StepChanged.
/// </remarks>
[TestFixture]
public class WhenUsingFusionStepper : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionStepper";

    private PagePlan<FusionStepperModel> _plan = null!;

    private FusionStepperLocator Checklist => new(Page, "move-in-steps");
    private FusionStepperLocator AdmissionSteps => new(Page, "admission-steps");
    private ILocator Back => Page.Locator("#checklist-back-btn");
    private ILocator Next => Page.Locator("#checklist-next-btn");
    private ILocator ReviewPaperwork => Page.Locator("#checklist-review-paperwork-btn");
    private ILocator StartOver => Page.Locator("#checklist-start-over-btn");
    private ILocator Status => Page.Locator("#checklist-status");
    private ILocator Instruction => Page.Locator("#checklist-instruction");
    private ILocator Warning => Page.Locator("#checklist-warning");
    private ILocator Done => Page.Locator("#checklist-done");
    private ILocator Clicked => Page.Locator("#checklist-clicked");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<FusionStepperModel>.FromPage(Page);
        await Expect(Checklist.OpenStep).ToContainTextAsync("Paperwork", new() { Timeout = 5000 });
    }

    private async Task SignTheRoomInspection() => await _plan.NativeCheckBox(m => m.RoomInspectionSigned).Check();

    // Room ready, with the inspection signed, then on to Moved in.
    private async Task MoveTheResidentIn()
    {
        await ClickWhenStable(Next);
        await Expect(Checklist.OpenStep).ToContainTextAsync("Room ready", new() { Timeout = 5000 });
        await SignTheRoomInspection();
        await ClickWhenStable(Next);
        await Expect(Checklist.OpenStep).ToContainTextAsync("Moved in", new() { Timeout = 5000 });
    }

    [Test]
    public async Task next_moves_the_checklist_on_to_the_room_and_says_what_to_do_there()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Next);

        await Expect(Checklist.OpenStep).ToContainTextAsync("Room ready", new() { Timeout = 5000 });
        await Expect(Status).ToHaveTextAsync("Moved on to Room ready.");
        await Expect(Instruction).ToHaveTextAsync("Walk the room with maintenance and sign the inspection.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task back_returns_to_the_paperwork()
    {
        await NavigateAndBoot();
        await ClickWhenStable(Next);
        await Expect(Checklist.OpenStep).ToContainTextAsync("Room ready", new() { Timeout = 5000 });

        await ClickWhenStable(Back);

        await Expect(Checklist.OpenStep).ToContainTextAsync("Paperwork", new() { Timeout = 5000 });
        await Expect(Status).ToHaveTextAsync("Back to Paperwork.");
        await Expect(Instruction).ToHaveTextAsync("Collect the signed residency agreement and the physician's report.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_resident_is_not_moved_in_before_the_room_inspection_is_signed()
    {
        await NavigateAndBoot();
        await ClickWhenStable(Next);
        await Expect(Checklist.OpenStep).ToContainTextAsync("Room ready", new() { Timeout = 5000 });

        await ClickWhenStable(Next);

        await Expect(Warning).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Checklist.OpenStep).ToContainTextAsync("Room ready");
        await Expect(Status).ToHaveTextAsync("Moved on to Room ready.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task once_the_inspection_is_signed_next_moves_the_resident_in()
    {
        await NavigateAndBoot();

        await MoveTheResidentIn();

        await Expect(Status).ToHaveTextAsync("Moved on to Moved in.");
        await Expect(Instruction).ToHaveTextAsync("Welcome the resident and hand over the room key.");
        await Expect(Warning).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task next_on_the_last_step_says_the_checklist_is_complete()
    {
        await NavigateAndBoot();
        await MoveTheResidentIn();
        await Expect(Done).ToBeHiddenAsync();

        await ClickWhenStable(Next);

        await Expect(Done).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Checklist.OpenStep).ToContainTextAsync("Moved in");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_a_step_opens_it_and_notes_where_the_coordinator_was()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Checklist.StepLabel("Room ready"));

        await Expect(Checklist.OpenStep).ToContainTextAsync("Room ready", new() { Timeout = 5000 });
        await Expect(Status).ToHaveTextAsync("Opened Room ready.");
        await Expect(Clicked).ToHaveTextAsync("Clicked Room ready while on Paperwork.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_moved_in_before_the_inspection_is_signed_keeps_the_room_step_open()
    {
        await NavigateAndBoot();
        await ClickWhenStable(Next);
        await Expect(Checklist.OpenStep).ToContainTextAsync("Room ready", new() { Timeout = 5000 });

        await ClickWhenStable(Checklist.StepLabel("Moved in"));

        await Expect(Warning).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Clicked).ToHaveTextAsync("Clicked Moved in while on Room ready.");
        await Expect(Checklist.OpenStep).ToContainTextAsync("Room ready");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reviewing_the_paperwork_opens_its_step_from_anywhere_in_the_checklist()
    {
        await NavigateAndBoot();
        await MoveTheResidentIn();

        await ClickWhenStable(ReviewPaperwork);

        await Expect(Checklist.OpenStep).ToContainTextAsync("Paperwork", new() { Timeout = 5000 });
        await Expect(Status).ToHaveTextAsync("Opened Paperwork.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task starting_over_returns_to_the_paperwork_and_clears_the_signed_inspection()
    {
        await NavigateAndBoot();
        await MoveTheResidentIn();

        await ClickWhenStable(StartOver);

        await Expect(Checklist.OpenStep).ToContainTextAsync("Paperwork", new() { Timeout = 5000 });
        await Expect(Status).ToHaveTextAsync("Back to Paperwork.");
        await Expect(_plan.NativeCheckBox(m => m.RoomInspectionSigned).Input).Not.ToBeCheckedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task admission_steps_show_which_step_is_done_which_needs_attention_and_which_is_optional()
    {
        await NavigateAndBoot();

        await Expect(AdmissionSteps.Step("Intake")).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("e-step-valid"));
        await Expect(AdmissionSteps.Step("Review")).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("e-step-error"));
        await Expect(AdmissionSteps.Step("Complete")).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("e-step-valid|e-step-error"));
        await Expect(AdmissionSteps.Root.Locator(".e-step-label-optional")).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_admission_step_opens_only_after_the_one_before_it()
    {
        await NavigateAndBoot();

        await AdmissionSteps.Step("Complete").ClickAsync(new() { Force = true });
        await Expect(AdmissionSteps.OpenStep).ToContainTextAsync("Intake");
        await Expect(Page.Locator("#admission-step-open")).ToHaveTextAsync("Intake");

        await AdmissionSteps.Step("Review").ClickAsync(new() { Force = true });
        await Expect(AdmissionSteps.OpenStep).ToContainTextAsync("Review", new() { Timeout = 5000 });
        await Expect(Page.Locator("#admission-step-open")).ToHaveTextAsync("Review");

        await AdmissionSteps.Step("Complete").ClickAsync(new() { Force = true });
        await Expect(AdmissionSteps.OpenStep).ToContainTextAsync("Complete", new() { Timeout = 5000 });
        await Expect(Page.Locator("#admission-step-open")).ToHaveTextAsync("Complete");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task hovering_the_next_admission_step_names_it()
    {
        await NavigateAndBoot();

        // A linear stepper takes the pointer only on the steps beside the open one (EJ2's e-linear rule).
        await AdmissionSteps.StepLabel("Review").HoverAsync();

        await Expect(AdmissionSteps.Tooltip).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(AdmissionSteps.Tooltip).ToContainTextAsync("Review");
        AssertNoConsoleErrors();
    }
}
