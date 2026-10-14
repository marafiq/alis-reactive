using System.Collections.Concurrent;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Validation.FallRiskScreening;

/// <summary>
/// As a nurse, I want "No" to be accepted as the answer to a required yes/no screening question
/// so that a resident who has not fallen is screened and saved, while an unanswered question
/// and an unticked attestation are still asked for, and the fall details agree with the answer.
/// </summary>
/// <remarks>
/// An "asks for" test then answers and saves: exactly one save request proves the first Save sent
/// nothing, whenever a send would have happened.
/// </remarks>
[TestFixture]
public class WhenNurseRecordsFallRiskScreening : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/FallRiskScreening";

    private PagePlan<FallRiskScreeningModel> _plan = null!;
    private ConcurrentQueue<string> _screeningsSent = null!;

    private ILocator ScreeningStatus => Page.Locator("#fall-risk-status");
    private ILocator SaveScreening => Page.Locator("#save-screening-btn");
    private NativeRadioGroupLocator FellInLast90Days => _plan.NativeRadioGroup(m => m.FellInLast90Days);
    private NativeCheckBoxLocator ObservedWalkingInPerson => _plan.NativeCheckBox(m => m.ObservedWalkingInPerson);

    // A contributing factor is the checkbox the nurse reads by its label.
    private ILocator FallFactor(string name) => Page.GetByRole(AriaRole.Checkbox, new() { Name = name, Exact = true });

    private async Task NavigateAndBoot()
    {
        _screeningsSent = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains("/Sandbox/Validation/FallRiskScreening/Save")) _screeningsSent.Enqueue(request.Url);
        };
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<FallRiskScreeningModel>.FromPage(Page);
    }

    private void AssertTheScreeningWasSentOnce()
    {
        Assert.That(_screeningsSent, Has.Count.EqualTo(1), "Only the completed screening may be sent.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task answering_no_to_a_recent_fall_saves_the_screening()
    {
        await NavigateAndBoot();
        await FellInLast90Days.Choose("No");
        await ObservedWalkingInPerson.Check();

        await ClickWhenStable(SaveScreening);

        await Expect(ScreeningStatus).ToHaveTextAsync("Screening saved for Helen Park: no fall in the last 90 days.");
        AssertTheScreeningWasSentOnce();
    }

    [Test]
    public async Task answering_yes_to_a_recent_fall_saves_the_screening()
    {
        await NavigateAndBoot();
        await FellInLast90Days.Choose("Yes");
        await ObservedWalkingInPerson.Check();

        await ClickWhenStable(SaveScreening);

        await Expect(ScreeningStatus).ToHaveTextAsync("Screening saved for Helen Park: a fall in the last 90 days.");
        AssertTheScreeningWasSentOnce();
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

        await FellInLast90Days.Choose("No");
        await ClickWhenStable(SaveScreening);
        await Expect(ScreeningStatus).ToHaveTextAsync("Screening saved for Helen Park: no fall in the last 90 days.");
        AssertTheScreeningWasSentOnce();
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

        await ObservedWalkingInPerson.Check();
        await ClickWhenStable(SaveScreening);
        await Expect(ScreeningStatus).ToHaveTextAsync("Screening saved for Helen Park: no fall in the last 90 days.");
        AssertTheScreeningWasSentOnce();
    }

    [Test]
    public async Task a_resident_who_has_not_fallen_keeps_the_fall_count_at_zero()
    {
        await NavigateAndBoot();
        await FellInLast90Days.Choose("No");
        await ObservedWalkingInPerson.Check();
        // Leaving the count may show its message at once, which moves what is below it, so the
        // count is filled last.
        await _plan.TextBox(m => m.FallsCount).FillAndBlur("2");

        await ClickWhenStable(SaveScreening);

        await Expect(_plan.ErrorFor(m => m.FallsCount))
            .ToHaveTextAsync("Leave the number of falls at 0 when the resident has not fallen.");
        await Expect(ScreeningStatus).ToHaveTextAsync("Not screened yet.");

        // 0 is what Empty() asks of a number, so a count of 0 is accepted.
        await _plan.TextBox(m => m.FallsCount).Fill("0");
        await ClickWhenStable(SaveScreening);
        await Expect(ScreeningStatus).ToHaveTextAsync("Screening saved for Helen Park: no fall in the last 90 days.");
        AssertTheScreeningWasSentOnce();
    }

    [Test]
    public async Task a_resident_who_has_not_fallen_has_no_fall_injury_to_report()
    {
        await NavigateAndBoot();
        await FellInLast90Days.Choose("No");
        await _plan.NativeCheckBox(m => m.InjuryReported).Check();
        await ObservedWalkingInPerson.Check();

        await ClickWhenStable(SaveScreening);

        await Expect(_plan.ErrorFor(m => m.InjuryReported))
            .ToHaveTextAsync("Clear the injury box when the resident has not fallen.");
        await Expect(ScreeningStatus).ToHaveTextAsync("Not screened yet.");

        // An unticked box is false, which is what Empty() asks of a yes/no.
        await _plan.NativeCheckBox(m => m.InjuryReported).Input.UncheckAsync();
        await ClickWhenStable(SaveScreening);
        await Expect(ScreeningStatus).ToHaveTextAsync("Screening saved for Helen Park: no fall in the last 90 days.");
        AssertTheScreeningWasSentOnce();
    }

    [Test]
    public async Task an_unwitnessed_fall_needs_the_time_neuro_checks_started()
    {
        await NavigateAndBoot();
        await FellInLast90Days.Choose("Yes");
        await FallFactor("Unwitnessed fall").CheckAsync();
        await ObservedWalkingInPerson.Check();

        await ClickWhenStable(SaveScreening);

        await Expect(_plan.ErrorFor(m => m.NeuroChecksStartedAt))
            .ToHaveTextAsync("Record when neuro checks started after the unwitnessed fall.");
        await Expect(ScreeningStatus).ToHaveTextAsync("Not screened yet.");

        await _plan.TextBox(m => m.NeuroChecksStartedAt).Fill("2:15 PM");
        await ClickWhenStable(SaveScreening);
        await Expect(ScreeningStatus).ToHaveTextAsync("Screening saved for Helen Park: a fall in the last 90 days.");
        AssertTheScreeningWasSentOnce();
    }

    [Test]
    public async Task a_fall_after_a_medication_change_needs_no_neuro_check_time()
    {
        await NavigateAndBoot();
        await FellInLast90Days.Choose("Yes");
        await FallFactor("Medication change").CheckAsync();
        await ObservedWalkingInPerson.Check();

        await ClickWhenStable(SaveScreening);

        // The factors hold an item, but not "Unwitnessed fall", so no neuro check time is asked for.
        await Expect(ScreeningStatus).ToHaveTextAsync("Screening saved for Helen Park: a fall in the last 90 days.");
        await Expect(_plan.ErrorFor(m => m.NeuroChecksStartedAt)).ToHaveTextAsync("");
        AssertTheScreeningWasSentOnce();
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
