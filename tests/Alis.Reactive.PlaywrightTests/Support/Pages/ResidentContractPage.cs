using System;
using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Support.Pages;

internal sealed class ResidentContractPage
{
    private const string Prefix = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ResidentModel__";

    private readonly IPage _page;
    private readonly Func<Task> _open;

    internal ResidentContractPage(IPage page, Func<Task> open)
    {
        _page = page;
        _open = open;
    }

    internal Task Open() => _open();

    internal ILocator SubmitButton => _page.Locator("#submit-btn");
    internal ILocator Result => _page.Locator("#result");
    internal ILocator ValidationSummary => _page.Locator("[data-reactive-validation-summary]");

    internal ILocator Name => Field("Name");
    internal ILocator Email => Field("Email");
    internal ILocator ConfirmEmail => Field("ConfirmEmail");
    internal ILocator CareLevel => Field("CareLevel");
    internal ILocator IsVeteran => Field("IsVeteran");
    internal ILocator VeteranId => Field("VeteranId");
    internal ILocator MemoryAssessmentScore => Field("MemoryAssessmentScore");
    internal ILocator PhysicianName => Field("PhysicianName");
    internal ILocator HasEmergencyContact => Field("HasEmergencyContact");
    internal ILocator EmergencyName => Field("EmergencyName");
    internal ILocator EmergencyPhone => Field("EmergencyPhone");
    internal ILocator ReasonForNoContact => Field("ReasonForNoContact");
    internal ILocator Street => Field("Address_Street");
    internal ILocator City => Field("Address_City");
    internal ILocator ZipCode => Field("Address_ZipCode");

    internal ILocator Field(string suffix) => _page.Locator($"#{Prefix}{suffix}");

    internal ILocator ErrorFor(string fieldName) =>
        _page.Locator($"#resident-form span[data-valmsg-for='{fieldName}']");

    internal async Task Submit()
    {
        await SubmitButton.ClickAsync();
    }
}
