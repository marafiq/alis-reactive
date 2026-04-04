using Alis.Reactive.PlaywrightTests.Support.Pages;

namespace Alis.Reactive.PlaywrightTests.Validation.Contract.ResidentForm;

[TestFixture]
public class WhenResidentContractComparesFields : PlaywrightTestBase
{
    private ResidentContractPage Form => new(Page, () => NavigateToAndWaitForReady("/Sandbox/Validation/Contract"));

    [Test]
    public async Task confirm_email_must_match_email()
    {
        await Form.Open();

        await ResidentContractScenario.FillRequiredFields(Form);
        await Form.Email.FillAsync("resident@care.com");
        await Form.ConfirmEmail.FillAsync("wrong@email.com");

        await Form.Submit();

        await Expect(Form.ErrorFor("ConfirmEmail")).ToContainTextAsync("must match");
        await Expect(Form.ValidationSummary).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task confirm_email_error_clears_after_correction_and_blur()
    {
        await Form.Open();

        await ResidentContractScenario.FillRequiredFields(Form);
        await Form.Email.FillAsync("test@care.com");
        await Form.ConfirmEmail.FillAsync("wrong@care.com");

        await Form.Submit();
        await Expect(Form.ErrorFor("ConfirmEmail")).ToContainTextAsync("must match");

        await Form.ConfirmEmail.FillAsync("test@care.com");
        await Form.ConfirmEmail.BlurAsync();

        await Expect(Form.ErrorFor("ConfirmEmail")).Not.ToBeVisibleAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }
}
