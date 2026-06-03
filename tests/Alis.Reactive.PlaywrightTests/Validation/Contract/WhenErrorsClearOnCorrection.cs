using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.Validation.Contract;

[TestFixture]
public class WhenErrorsClearOnCorrection : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Patterns/ComponentGather";
    private const string R = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ComponentGatherModel__";

    private ILocator SubmitBtn => Page.Locator("#submit-json-btn");
    private ILocator Input(string suffix) => Page.Locator($"#{R}{suffix}");
    private ILocator ErrorFor(string suffix) => Page.Locator($"#{R}{suffix}_error");

    private async Task TriggerResidentNameRequiredError()
    {
        await Input("ResidentName").ClearAsync();
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("ResidentName")).ToContainTextAsync("required");
    }

    private async Task FillResidentNameAndExpectErrorCleared()
    {
        await Input("ResidentName").FillAsync("Margaret");
        await Expect(ErrorFor("ResidentName")).ToBeHiddenAsync();
    }

    private async Task ClearResidentNameAndBlur()
    {
        await Input("ResidentName").ClearAsync();
        await Input("ResidentName").BlurAsync();
    }

    [Test]
    public async Task error_clears_when_user_types_valid_value()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await TriggerResidentNameRequiredError();
        await Expect(Input("ResidentName")).ToHaveClassAsync(new Regex("alis-has-error"));

        await FillResidentNameAndExpectErrorCleared();
        await Expect(Input("ResidentName")).Not.ToHaveClassAsync(new Regex("alis-has-error"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task error_reappears_when_user_empties_previously_corrected_field()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await TriggerResidentNameRequiredError();
        await FillResidentNameAndExpectErrorCleared();
        await ClearResidentNameAndBlur();
        await Expect(ErrorFor("ResidentName")).ToContainTextAsync("required", new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }
}
