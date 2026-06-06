using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.InPlaceEditor;

// InPlaceEditor profile fields must use the same client validation path as other inputs.
[TestFixture]
public class WhenResidentProfileFormValidatesInPlaceEditorFields : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionInPlaceEditor";
    private const string NicknameEditorId = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ResidentProfile__Nickname";

    [Test]
    public async Task overlong_nickname_in_inplace_editor_blocks_profile_submit()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#submit-profile");

        await Page.Locator($"#{NicknameEditorId} .e-editable-value-wrapper").First.ClickAsync();
        var inner = Page.Locator($"#{NicknameEditorId} input.e-control").First;
        await Expect(inner).ToBeVisibleAsync(new() { Timeout = 5000 });
        await inner.FillAsync(new string('x', 60));
        await inner.PressAsync("Enter");

        // TODO: Replace this fixed Syncfusion commit wait with a visible editor-close or value-commit signal.
        await Page.WaitForTimeoutAsync(400);

        var postFired = false;
        _ = Page.WaitForRequestAsync(matchingRequest =>
            matchingRequest.Url.Contains("/Sandbox/Components/FusionInPlaceEditor/UpdateProfile")
            && matchingRequest.Method == "POST",
            new() { Timeout = 2500 })
            .ContinueWith(t => { postFired = !t.IsFaulted && t.Result != null; return t; });

        await Page.Locator("#submit-profile").ClickAsync();

        // TODO: Replace this fixed negative-request wait with a behavior-focused no-POST proof.
        await Page.WaitForTimeoutAsync(2800);

        Assert.That(postFired, Is.False,
            "FluentValidation must block the profile POST when Nickname exceeds MaximumLength.");

        AssertNoConsoleErrors();
    }
}
