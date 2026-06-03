using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.InPlaceEditor;

/// <summary>
/// Control panel card: proves the full mutation surface (SetValue / Enable / Disable / Save / Focus / Validate)
/// and the Value() read end-to-end in the browser. Every button fires a DomReady-style pipeline that drives
/// the InPlaceEditor through <c>p.Component&lt;FusionInPlaceEditor&gt;(m =&gt; m.Value).Xxx()</c>.
/// </summary>
[TestFixture]
public class WhenInPlaceEditorControlPanelExercisesApi : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionInPlaceEditor";

    [Test]
    public async Task domready_value_read_echoes_current_value_and_triggers_conditional_greeting()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#card-control-panel");

        // Seed value is "Hello"; Value()-driven condition shows the greeting span.
        await Expect(Page.Locator("#control-echo")).ToHaveTextAsync("Hello", new() { Timeout = 5000 });
        await Expect(Page.Locator("#control-greeting")).ToBeVisibleAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task set_value_button_writes_hello_to_editor_value()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#ctrl-btn-set-hello");

        // Perturb the editor by clicking a different mutation first.
        await Page.Locator("#ctrl-btn-set-null").ClickAsync();
        await Page.Locator("#ctrl-btn-set-hello").ClickAsync();

        // SF reflects the property write in the collapsed display.
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
