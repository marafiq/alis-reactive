using System.Text.RegularExpressions;
using Alis.Reactive.PlaywrightTests.Support.Pages;

namespace Alis.Reactive.PlaywrightTests.Validation.Contract.ResidentForm;

[TestFixture]
public class WhenResidentContractRecoversAfterCorrection : PlaywrightTestBase
{
    private ResidentContractPage Form => new(Page, () => NavigateToAndWaitForReady("/Sandbox/Validation/Contract"));

    [Test]
    public async Task fixing_previous_errors_and_resubmitting_allows_the_admission_to_save()
    {
        await Form.Open();

        await Form.Submit();
        await Expect(Form.ErrorFor("Name")).ToBeVisibleAsync();
        await Expect(Form.ErrorFor("Email")).ToBeVisibleAsync();

        await Form.Name.FillAsync("Jane Smith");
        await Form.Submit();
        await Expect(Form.ErrorFor("Name")).Not.ToBeVisibleAsync();
        await Expect(Form.ErrorFor("Email")).ToBeVisibleAsync();

        await ResidentContractScenario.FillRequiredFields(Form);
        await Form.ReasonForNoContact.FillAsync("No relatives nearby");
        await Form.Submit();

        await Expect(Form.Result).ToContainTextAsync("Admission saved", new() { Timeout = 5000 });
        await Expect(Form.ValidationSummary).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task name_and_zip_errors_clear_after_valid_input_and_blur()
    {
        await Form.Open();

        await Form.Submit();
        await Expect(Form.ErrorFor("Name")).ToBeVisibleAsync();
        await Expect(Form.Name).ToHaveClassAsync(new Regex("alis-has-error"));

        await Form.Name.FillAsync("Robert Thompson");
        await Form.Name.BlurAsync();

        await Expect(Form.ErrorFor("Name")).Not.ToBeVisibleAsync(new() { Timeout = 2000 });
        await Expect(Form.Name).Not.ToHaveClassAsync(new Regex("alis-has-error"), new() { Timeout = 2000 });

        await ResidentContractScenario.FillRequiredFields(Form);
        await Form.ZipCode.FillAsync("abc");
        await Form.Submit();
        await Expect(Form.ErrorFor("Address.ZipCode")).ToBeVisibleAsync();

        await Form.ZipCode.FillAsync("62704");
        await Form.ZipCode.BlurAsync();

        await Expect(Form.ErrorFor("Address.ZipCode")).Not.ToBeVisibleAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task fully_completed_submission_with_all_conditional_branches_succeeds()
    {
        await Form.Open();

        await Form.Name.FillAsync("Eleanor Davis");
        await Form.Email.FillAsync("eleanor@care.com");
        await Form.ConfirmEmail.FillAsync("eleanor@care.com");
        await Form.CareLevel.SelectOptionAsync("Memory Care");
        await Form.IsVeteran.CheckAsync();
        await Form.VeteranId.FillAsync("V99999");
        await Form.PhysicianName.FillAsync("Dr. Martinez");
        await ResidentContractScenario.SetMemoryAssessment(Form, "72");
        await Form.HasEmergencyContact.CheckAsync();
        await Form.EmergencyName.FillAsync("Michael Davis");
        await Form.EmergencyPhone.FillAsync("555-9876");
        await Form.Street.FillAsync("456 Oak Lane");
        await Form.City.FillAsync("Portland");
        await Form.ZipCode.FillAsync("97201");

        await Form.Submit();

        await Expect(Form.Result).ToContainTextAsync("Admission saved", new() { Timeout = 5000 });
        await Expect(Form.ErrorFor("Name")).Not.ToBeVisibleAsync();
        await Expect(Form.ErrorFor("Email")).Not.ToBeVisibleAsync();
        await Expect(Form.ErrorFor("VeteranId")).Not.ToBeVisibleAsync();
        await Expect(Form.ErrorFor("PhysicianName")).Not.ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }
}
