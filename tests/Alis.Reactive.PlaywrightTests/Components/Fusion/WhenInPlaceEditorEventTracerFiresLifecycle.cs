using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

/// <summary>
/// Event tracer card: proves the 7 lifecycle events that fire without Syncfusion's UrlAdaptor
/// or ValidationRules surface their typed args-props into dedicated trace cells.
///
/// Out of scope for the sandbox demo (covered by unit tests only):
/// <list type="bullet">
///   <item><c>ActionFailure</c> requires SF's UrlAdaptor commit path, which this slice does not use.</item>
///   <item><c>Validating</c> + <c>SetErrorMessage</c> / <c>PreventDefault</c> require SF's
///   <c>ValidationRules</c>; SF's internal <c>getErrorElement</c> assumes an MVC form scaffold
///   and throws when rendered inside our <c>InputField</c> wrapper.</item>
/// </list>
/// </summary>
[TestFixture]
public class WhenInPlaceEditorEventTracerFiresLifecycle : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionInPlaceEditor";

    [Test]
    public async Task clicking_to_edit_fires_beginedit_with_inline_mode_arg()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#card-event-tracer .e-editable-value-wrapper");

        await Page.Locator("#card-event-tracer .e-editable-value-wrapper").First.ClickAsync();

        await Expect(Page.Locator("#trace-begin")).ToHaveTextAsync("Inline", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task typing_and_committing_fires_change_endedit_actionbegin_actionsuccess_and_submitclick()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#card-event-tracer .e-editable-value-wrapper");

        await Page.Locator("#card-event-tracer .e-editable-value-wrapper").First.ClickAsync();
        var inner = Page.Locator("#card-event-tracer input.e-input").First;
        await Expect(inner).ToBeVisibleAsync(new() { Timeout = 3000 });

        await inner.FillAsync("world");
        await inner.PressAsync("Enter");

        // Change carries args.Value (the new value)
        await Expect(Page.Locator("#trace-change")).ToHaveTextAsync("world", new() { Timeout = 5000 });

        // EndEdit carries args.Action ("submit" when the user confirmed)
        await Expect(Page.Locator("#trace-endedit")).ToHaveTextAsync("submit", new() { Timeout = 3000 });

        // ActionBegin + ActionSuccess both fire; ActionSuccess carries args.Value
        await Expect(Page.Locator("#trace-actionbegin")).ToHaveTextAsync("fired", new() { Timeout = 3000 });
        await Expect(Page.Locator("#trace-actionsuccess")).ToHaveTextAsync("world", new() { Timeout = 3000 });

        // SubmitClick carries args.Name (the SF event name)
        await Expect(Page.Locator("#trace-submitclick")).Not.ToHaveTextAsync("", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task cancelling_fires_cancelclick_and_endedit_with_cancel_action()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#card-event-tracer .e-editable-value-wrapper");

        await Page.Locator("#card-event-tracer .e-editable-value-wrapper").First.ClickAsync();
        var cancelBtn = Page.Locator("#card-event-tracer .e-btn-cancel").First;
        await Expect(cancelBtn).ToBeVisibleAsync(new() { Timeout = 3000 });
        await cancelBtn.ClickAsync();

        await Expect(Page.Locator("#trace-cancelclick")).Not.ToHaveTextAsync("", new() { Timeout = 3000 });
        await Expect(Page.Locator("#trace-endedit")).ToHaveTextAsync("cancel", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

}
