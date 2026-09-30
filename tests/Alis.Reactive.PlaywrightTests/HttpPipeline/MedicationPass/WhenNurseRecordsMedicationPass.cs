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
}
