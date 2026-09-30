using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Validation.FallRiskScreening;

/// <summary>
/// As a nurse, I want "No" to be accepted as the answer to a required yes/no screening question
/// so that a resident who has not fallen is screened and saved, while an unanswered question
/// and an unticked attestation are still asked for.
/// </summary>
[TestFixture]
public class WhenNurseRecordsFallRiskScreening : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/FallRiskScreening";

    private PagePlan<FallRiskScreeningModel> _plan = null!;

    private ILocator ScreeningStatus => Page.Locator("#fall-risk-status");
    private ILocator SaveScreening => Page.Locator("#save-screening-btn");
    private NativeRadioGroupLocator FellInLast90Days => _plan.NativeRadioGroup(m => m.FellInLast90Days);
    private NativeCheckBoxLocator ObservedWalkingInPerson => _plan.NativeCheckBox(m => m.ObservedWalkingInPerson);

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<FallRiskScreeningModel>.FromPage(Page);
    }

    [Test]
    public async Task answering_no_to_a_recent_fall_saves_the_screening()
    {
        await NavigateAndBoot();
        await FellInLast90Days.Choose("No");
        await ObservedWalkingInPerson.Check();

        await ClickWhenStable(SaveScreening);

        await Expect(ScreeningStatus).ToHaveTextAsync("Screening saved for Helen Park: no fall in the last 90 days.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task answering_yes_to_a_recent_fall_saves_the_screening()
    {
        await NavigateAndBoot();
        await FellInLast90Days.Choose("Yes");
        await ObservedWalkingInPerson.Check();

        await ClickWhenStable(SaveScreening);

        await Expect(ScreeningStatus).ToHaveTextAsync("Screening saved for Helen Park: a fall in the last 90 days.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task leaving_the_fall_question_unanswered_asks_for_an_answer()
    {
        await NavigateAndBoot();
        await ObservedWalkingInPerson.Check();

        await ClickWhenStable(SaveScreening);

        await Expect(_plan.ErrorFor(m => m.FellInLast90Days))
            .ToHaveTextAsync("Answer whether the resident fell in the last 90 days.");
        await Expect(_plan.ErrorFor(m => m.ObservedWalkingInPerson)).ToHaveTextAsync("");
        await Expect(ScreeningStatus).ToHaveTextAsync("Not screened yet.");
        // The server's NotEmpty answers with the same message in a 400, which the browser logs as a
        // console error before the message can show; none here means the page asked, not the server.
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_without_the_in_person_observation_asks_for_it()
    {
        await NavigateAndBoot();
        await FellInLast90Days.Choose("No");

        await ClickWhenStable(SaveScreening);

        await Expect(_plan.ErrorFor(m => m.ObservedWalkingInPerson))
            .ToHaveTextAsync("Confirm you observed the resident walking in person.");
        await Expect(_plan.ErrorFor(m => m.FellInLast90Days)).ToHaveTextAsync("");
        await Expect(ScreeningStatus).ToHaveTextAsync("Not screened yet.");
        // As above: no console error means the page asked, not the server.
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task answering_no_after_being_asked_clears_the_prompt()
    {
        await NavigateAndBoot();
        await ObservedWalkingInPerson.Check();
        await ClickWhenStable(SaveScreening);
        await Expect(_plan.ErrorFor(m => m.FellInLast90Days))
            .ToHaveTextAsync("Answer whether the resident fell in the last 90 days.");

        await FellInLast90Days.Choose("No");

        await Expect(_plan.ErrorFor(m => m.FellInLast90Days)).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }
}
