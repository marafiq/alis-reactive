using Alis.Reactive.PlaywrightTests.Support.Pages;

namespace Alis.Reactive.PlaywrightTests.Validation.Contract.ResidentForm;

[TestFixture]
public class WhenCareLevelChangesContractValidation : PlaywrightTestBase
{
    private ResidentContractPage Form => new(Page, () => NavigateToAndWaitForReady("/Sandbox/Validation/Contract"));

    [Test]
    public async Task assisted_living_requires_physician_name()
    {
        await Form.Open();

        await ResidentContractScenario.FillRequiredFields(Form);
        await Form.CareLevel.SelectOptionAsync("Assisted Living");

        await Form.Submit();

        await Expect(Form.ErrorFor("PhysicianName")).ToContainTextAsync("required");
        await Expect(Form.ValidationSummary).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task memory_care_requires_physician_and_assessment()
    {
        await Form.Open();

        await ResidentContractScenario.FillRequiredFields(Form);
        await Form.CareLevel.SelectOptionAsync("Memory Care");

        await Form.Submit();

        await Expect(Form.ErrorFor("PhysicianName")).ToContainTextAsync("required");
        await Expect(Form.ErrorFor("MemoryAssessmentScore")).ToContainTextAsync("required");
        await Expect(Form.ValidationSummary).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task switching_away_from_memory_care_removes_the_assessment_requirement()
    {
        await Form.Open();

        await ResidentContractScenario.FillRequiredFields(Form);
        await Form.CareLevel.SelectOptionAsync("Memory Care");
        await Form.PhysicianName.FillAsync("Dr. Smith");

        await Form.Submit();
        await Expect(Form.ErrorFor("MemoryAssessmentScore")).ToBeVisibleAsync();

        await Form.CareLevel.SelectOptionAsync("Assisted Living");
        await Form.Submit();

        await Expect(Form.ErrorFor("MemoryAssessmentScore")).Not.ToBeVisibleAsync();

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task memory_care_submission_succeeds_after_filling_the_fusion_numeric_assessment()
    {
        await Form.Open();

        await ResidentContractScenario.FillRequiredFields(Form);
        await Form.CareLevel.SelectOptionAsync("Memory Care");
        await Form.PhysicianName.FillAsync("Dr. Smith");
        await Form.ReasonForNoContact.FillAsync("No relatives");

        await Form.Submit();
        await Expect(Form.ErrorFor("MemoryAssessmentScore")).ToContainTextAsync("required");

        await ResidentContractScenario.SetMemoryAssessment(Form, "85");
        await Form.Submit();

        await Expect(Form.ErrorFor("MemoryAssessmentScore")).Not.ToBeVisibleAsync();
        await Expect(Form.Result).ToContainTextAsync("Admission saved", new() { Timeout = 5000 });
        await Expect(Form.ValidationSummary).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }
}
