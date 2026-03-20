using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.ValidationContract;

/// <summary>
/// Live-clear behavior: after submit shows validation errors,
/// typing into a field should clear the error. But if the user
/// clears the field again, the error should re-appear on the next keystroke
/// (re-evaluate, not just clear).
/// </summary>
[TestFixture]
public class WhenLiveClearingValidationErrors : PlaywrightTestBase
{
    private const string Path = "/Sandbox/ComponentGather";
    private const string R = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ComponentGatherModel__";

    private ILocator SubmitBtn => Page.Locator("#submit-json-btn");
    private ILocator Input(string suffix) => Page.Locator($"#{R}{suffix}");
    private ILocator ErrorFor(string suffix) => Page.Locator($"#{R}{suffix}_error");

    [Test]
    public async Task error_clears_when_user_types_valid_value()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Clear resident name and submit → error shows
        await Input("ResidentName").ClearAsync();
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("ResidentName")).ToContainTextAsync("required");
        await Expect(Input("ResidentName")).ToHaveClassAsync(new Regex("alis-has-error"));

        // Type valid name → error should clear
        await Input("ResidentName").FillAsync("Margaret");
        await Expect(ErrorFor("ResidentName")).ToBeHiddenAsync();
        await Expect(Input("ResidentName")).Not.ToHaveClassAsync(new Regex("alis-has-error"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task error_reappears_when_user_clears_field_after_live_clear()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Step 1: Clear name, submit → error shows
        await Input("ResidentName").ClearAsync();
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("ResidentName")).ToContainTextAsync("required");

        // Step 2: Type valid name → error clears (live-clear)
        await Input("ResidentName").FillAsync("Margaret");
        await Expect(ErrorFor("ResidentName")).ToBeHiddenAsync();

        // Step 3: Clear the field and blur → error re-appears (re-validate on blur)
        await Input("ResidentName").ClearAsync();
        await Input("ResidentName").BlurAsync();

        await Expect(ErrorFor("ResidentName")).ToContainTextAsync("required", new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }
}
