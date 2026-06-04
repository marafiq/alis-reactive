using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.InPlaceEditor;

/// <summary>
/// Visual commit feedback: `p.Component&lt;FusionInPlaceEditor&gt;(m =&gt; m.X).AddClass("alis-editor-saved")`
/// on ActionSuccess + `.RemoveClass(...)` on BeginEdit toggles a CSS-driven green check mark on
/// the editor's outer wrapper. Syncfusion preserves the wrapper's classList across edit/close cycles,
/// so the class survives until the user re-enters edit mode.
/// </summary>
[TestFixture]
public class WhenInPlaceEditorShowsSavedIndicator : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionInPlaceEditor";

    [Test]
    public async Task committing_dob_adds_alis_editor_saved_class_on_editor_wrapper()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#card-dob .e-editable-value-wrapper");

        var editor = Page.Locator("#Alis_Reactive_SandboxApp_Areas_Sandbox_Models_DateOfBirthQuickEdit__Value");
        await Expect(editor).Not.ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("alis-editor-saved"), new() { Timeout = 2000 });

        await Page.Locator("#card-dob .e-editable-value-wrapper").First.ClickAsync();
        var inner = Page.Locator("#card-dob input.e-control").First;
        await Expect(inner).ToBeVisibleAsync(new() { Timeout = 5000 });

        await inner.FillAsync("5/15/2000");
        await inner.PressAsync("Enter");

        await Expect(editor).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("alis-editor-saved"), new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task re_entering_edit_mode_removes_alis_editor_saved_class()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#card-dob .e-editable-value-wrapper");

        await Page.Locator("#card-dob .e-editable-value-wrapper").First.ClickAsync();
        var inner = Page.Locator("#card-dob input.e-control").First;
        await Expect(inner).ToBeVisibleAsync(new() { Timeout = 5000 });
        await inner.FillAsync("5/15/2000");
        await inner.PressAsync("Enter");

        var editor = Page.Locator("#Alis_Reactive_SandboxApp_Areas_Sandbox_Models_DateOfBirthQuickEdit__Value");
        await Expect(editor).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("alis-editor-saved"), new() { Timeout = 5000 });

        await Page.Locator("#card-dob .e-editable-value-wrapper").First.ClickAsync();

        await Expect(editor).Not.ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("alis-editor-saved"), new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }
}
