using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.InPlaceEditor;

/// <summary>
/// Proves the DOB card opens Syncfusion edit mode with visible action buttons.
/// Verified against real Syncfusion DOM (e-editable-value-wrapper → e-editable-action-buttons).
/// </summary>
[TestFixture]
public class WhenInPlaceEditorEntersEditMode : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionInPlaceEditor";

    [Test]
    public async Task clicking_the_dob_pencil_opens_edit_mode_with_save_and_cancel_buttons()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#card-dob .e-editable-value-wrapper");

        var wrapper = Page.Locator("#card-dob .e-editable-value-wrapper").First;
        await Expect(wrapper).ToBeVisibleAsync(new() { Timeout = 10000 });
        await wrapper.ClickAsync();

        var actionButtons = Page.Locator("#card-dob .e-editable-action-buttons").First;
        var saveBtn = Page.Locator("#card-dob .e-btn-save").First;
        var cancelBtn = Page.Locator("#card-dob .e-btn-cancel").First;

        await Expect(actionButtons).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(saveBtn).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(cancelBtn).ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
