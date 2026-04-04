using Alis.Reactive.PlaywrightTests.Support.Pages;

namespace Alis.Reactive.PlaywrightTests.Validation.Contract.ResidentForm;

[TestFixture]
public class WhenResidentFlagsChangeContractValidation : PlaywrightTestBase
{
    private ResidentContractPage Form => new(Page, () => NavigateToAndWaitForReady("/Sandbox/Validation/Contract"));

    [Test]
    public async Task veteran_id_becomes_required_when_resident_is_marked_veteran()
    {
        await Form.Open();

        await ResidentContractScenario.FillRequiredFields(Form);
        await Form.IsVeteran.CheckAsync();

        await Form.Submit();

        await Expect(Form.ErrorFor("VeteranId")).ToContainTextAsync("required");
        await Expect(Form.ValidationSummary).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task toggling_veteran_status_turns_the_requirement_on_and_back_off()
    {
        await Form.Open();

        await ResidentContractScenario.FillRequiredFields(Form);

        await Form.Submit();
        await Expect(Form.ErrorFor("VeteranId")).Not.ToBeVisibleAsync();

        await Form.IsVeteran.CheckAsync();
        await Form.Submit();
        await Expect(Form.ErrorFor("VeteranId")).ToContainTextAsync("required");

        await Form.IsVeteran.UncheckAsync();
        await Form.Submit();
        await Expect(Form.ErrorFor("VeteranId")).Not.ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reason_for_no_contact_is_required_until_emergency_contact_is_provided()
    {
        await Form.Open();

        await ResidentContractScenario.FillRequiredFields(Form);

        await Form.Submit();
        await Expect(Form.ErrorFor("ReasonForNoContact")).ToContainTextAsync("required");
        await Expect(Form.ErrorFor("EmergencyName")).Not.ToBeVisibleAsync();

        await Form.HasEmergencyContact.CheckAsync();
        await Form.Submit();
        await Expect(Form.ErrorFor("EmergencyName")).ToBeVisibleAsync();
        await Expect(Form.ErrorFor("ReasonForNoContact")).Not.ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }
}
