using Alis.Reactive.PlaywrightTests.Support.Pages;

namespace Alis.Reactive.PlaywrightTests.Validation.Contract.ResidentForm;

internal static class ResidentContractScenario
{
    internal static async Task FillRequiredFields(ResidentContractPage form)
    {
        await form.Name.FillAsync("Jane Smith");
        await form.Email.FillAsync("jane@care.com");
        await form.ConfirmEmail.FillAsync("jane@care.com");
        await form.CareLevel.SelectOptionAsync("Independent");
        await form.Street.FillAsync("123 Main St");
        await form.City.FillAsync("Springfield");
        await form.ZipCode.FillAsync("62704");
    }

    internal static async Task SetMemoryAssessment(ResidentContractPage form, string value)
    {
        await form.MemoryAssessmentScore.ClickAsync();
        await form.MemoryAssessmentScore.FillAsync(value);
        await form.MemoryAssessmentScore.PressAsync("Tab");
    }
}
