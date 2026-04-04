using System.Text.RegularExpressions;
using Alis.Reactive.PlaywrightTests.Support.Pages;

namespace Alis.Reactive.PlaywrightTests.Validation.Contract.ResidentForm;

[TestFixture]
public class WhenBasicResidentContractRulesRun : PlaywrightTestBase
{
    private ResidentContractPage Form => new(Page, () => NavigateToAndWaitForReady("/Sandbox/Validation/Contract"));

    [Test]
    public async Task empty_submission_shows_required_errors_inline_and_blocks_save()
    {
        await Form.Open();

        await Form.Submit();

        await Expect(Form.ErrorFor("Name")).ToContainTextAsync("'Name' is required");
        await Expect(Form.ErrorFor("Email")).ToContainTextAsync("'Email' is required");
        await Expect(Form.ErrorFor("CareLevel")).ToContainTextAsync("required");
        await Expect(Form.Name).ToHaveClassAsync(new Regex("alis-has-error"));
        await Expect(Form.Email).ToHaveClassAsync(new Regex("alis-has-error"));
        await Expect(Form.ValidationSummary).ToBeHiddenAsync();
        await Expect(Form.Result).ToHaveTextAsync("");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task short_name_shows_minimum_length_error()
    {
        await Form.Open();

        await ResidentContractScenario.FillRequiredFields(Form);
        await Form.Name.FillAsync("A");

        await Form.Submit();

        await Expect(Form.ErrorFor("Name")).ToContainTextAsync("minimum length");
        await Expect(Form.ValidationSummary).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task invalid_email_and_zipcode_show_format_errors()
    {
        await Form.Open();

        await ResidentContractScenario.FillRequiredFields(Form);
        await Form.Email.FillAsync("not-an-email");
        await Form.ZipCode.FillAsync("abc");

        await Form.Submit();

        await Expect(Form.ErrorFor("Email")).ToContainTextAsync("valid email");
        await Expect(Form.ErrorFor("Address.ZipCode")).ToContainTextAsync("5 digits");
        await Expect(Form.Email).ToHaveClassAsync(new Regex("alis-has-error"));
        await Expect(Form.ZipCode).ToHaveClassAsync(new Regex("alis-has-error"));
        await Expect(Form.ValidationSummary).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }
}
