namespace Alis.Reactive.PlaywrightTests.Components.Fusion.InPlaceEditor;

// Control-panel buttons exercise InPlaceEditor operations through page-visible behavior.
[TestFixture]
public class WhenInPlaceEditorControlPanelExercisesApi : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionInPlaceEditor";

    [Test]
    public async Task domready_value_read_echoes_current_value_and_triggers_conditional_greeting()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#card-control-panel");

        await Expect(Page.Locator("#control-echo")).ToHaveTextAsync("Hello", new() { Timeout = 5000 });
        await Expect(Page.Locator("#control-greeting")).ToBeVisibleAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task set_value_button_writes_hello_to_editor_value()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#ctrl-btn-set-hello");

        await Page.Locator("#ctrl-btn-set-null").ClickAsync();
        await Page.Locator("#ctrl-btn-set-hello").ClickAsync();

        // Syncfusion reflects the property write in the collapsed display.
        await Expect(Page.Locator("#card-control-panel .e-editable-value").First)
            .ToHaveTextAsync("Hello", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task disable_button_adds_sf_disabled_class_and_enable_removes_it()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#ctrl-btn-disable");

        var editor = Page.Locator("#Alis_Reactive_SandboxApp_Areas_Sandbox_Models_InPlaceEditorControlPanelModel__Value");

        await Page.Locator("#ctrl-btn-disable").ClickAsync();
        await Expect(editor).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("e-disable"), new() { Timeout = 3000 });

        await Page.Locator("#ctrl-btn-enable").ClickAsync();
        await Expect(editor).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("e-disable"), new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }
}
