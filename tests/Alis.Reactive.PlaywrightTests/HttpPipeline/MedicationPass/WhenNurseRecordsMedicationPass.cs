using System.Linq.Expressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.HttpPipeline.MedicationPass;

/// <summary>
/// As a nurse, I want each dose I tick during the medication pass to be recorded against that
/// resident, so that the record shows exactly who received their medication.
/// </summary>
[TestFixture]
public class WhenNurseRecordsMedicationPass : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/MedicationPass";
    private const int HelenPark = 1042;

    private ILocator Field(Expression<Func<MedicationPassModel, object?>> member) =>
        Page.Locator($"#{IdGenerator.For(member)}");

    private ILocator RecordPass => Page.Locator("#record-medication-pass-btn");
    private ILocator PassStatus => Page.Locator("#medication-pass-status");
    private ILocator Refills => Page.Locator("#medication-pass-refills");
    private ILocator NextPass => Page.Locator("#medication-pass-next");

    [Test]
    public async Task a_ticked_dose_is_recorded_for_that_resident()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Field(m => m.Administered[HelenPark]).CheckAsync();

        await ClickWhenStable(RecordPass);

        await Expect(PassStatus).ToHaveTextAsync("Doses recorded for Helen Park.");
        AssertNoConsoleErrors();
    }

    // The refill request runs inside the pass's success route; its own route reads the pass
    // confirmation by that response's handle, not the refill response it is nested in.
    [Test]
    public async Task the_next_pass_time_shows_once_the_refills_are_ordered()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Field(m => m.Administered[HelenPark]).CheckAsync();

        await ClickWhenStable(RecordPass);

        await Expect(Refills).ToHaveTextAsync("Refills ordered from Maple Pharmacy.");
        await Expect(NextPass).ToHaveTextAsync("Next pass at 8:00 PM.");
        await Expect(PassStatus).ToHaveTextAsync("Doses recorded for Helen Park.");
        AssertNoConsoleErrors();
    }
}
