using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.TextBox;

[TestFixture]
public class WhenUsingFusionTextBox : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/TextBox";
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_TextBoxModel";
    private const string ResidentNameId = Scope + "__ResidentName";
    private const string AutoBlurNoteId = Scope + "__AutoBlurNote";

    private FusionTextBoxLocator ResidentName => new(Page, ResidentNameId);
    private FusionTextBoxLocator AutoBlurNote => new(Page, AutoBlurNoteId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#value-echo");
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionTextBox — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_contains_typed_textbox_members()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();

        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""));
        Assert.That(planJson, Does.Contain("\"set\""));
        Assert.That(planJson, Does.Contain("\"dataBind\""));
        Assert.That(planJson, Does.Contain("\"addAppendIcon\""));
        Assert.That(planJson, Does.Contain("\"input\""));
        Assert.That(planJson, Does.Contain("\"change\""));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_sets_visible_value_and_reads_value_source()
    {
        await NavigateAndBoot();

        await Expect(ResidentName.Input).ToHaveValueAsync("Amina Patel", new() { Timeout = 5000 });
        await Expect(Page.Locator("#value-echo")).ToHaveTextAsync("Amina Patel", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_adds_append_icon()
    {
        await NavigateAndBoot();

        await Expect(ResidentName.Wrapper.Locator(".e-icons.e-search")).ToHaveCountAsync(1);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task input_event_exposes_value_and_previous_value()
    {
        await NavigateAndBoot();

        await ResidentName.Fill("Nora Lee");

        await Expect(Page.Locator("#input-value")).ToHaveTextAsync("Nora Lee", new() { Timeout = 5000 });
        await Expect(Page.Locator("#input-previous")).ToHaveTextAsync("Amina Patel", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_exposes_value_previous_value_and_interaction_state()
    {
        await NavigateAndBoot();

        await ResidentName.FillAndBlur("Nora Lee");

        await Expect(Page.Locator("#change-value")).ToHaveTextAsync("Nora Lee", new() { Timeout = 5000 });
        await Expect(Page.Locator("#change-previous")).ToHaveTextAsync("Amina Patel", new() { Timeout = 5000 });
        await Expect(Page.Locator("#change-interacted")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Page.Locator("#args-condition")).ToHaveTextAsync("name entered", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task focusin_method_focuses_textbox_and_focus_event_reads_value()
    {
        await NavigateAndBoot();

        await Page.Locator("#focus-in-btn").ClickAsync();

        await Expect(ResidentName.Input).ToBeFocusedAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#focus-state")).ToHaveTextAsync("focused", new() { Timeout = 5000 });
        await Expect(Page.Locator("#focus-value")).ToHaveTextAsync("Amina Patel", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task blur_event_reads_value()
    {
        await NavigateAndBoot();

        await ResidentName.Focus();
        await ResidentName.Blur();

        await Expect(Page.Locator("#focus-state")).ToHaveTextAsync("blurred", new() { Timeout = 5000 });
        await Expect(Page.Locator("#blur-value")).ToHaveTextAsync("Amina Patel", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task focusout_method_removes_focus_without_button_click_stealing_focus()
    {
        await NavigateAndBoot();

        await AutoBlurNote.Focus();

        await Expect(Page.Locator("#focusout-method-state")).ToHaveTextAsync("focusout called", new() { Timeout = 5000 });
        await Expect(Page.Locator("#autoblur-state")).ToHaveTextAsync("blurred", new() { Timeout = 5000 });
        await Expect(AutoBlurNote.Input).Not.ToBeFocusedAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_reads_current_value()
    {
        await NavigateAndBoot();

        await Page.Locator("#check-name-btn").ClickAsync();
        await Expect(Page.Locator("#name-warning")).ToHaveTextAsync("resident name set", new() { Timeout = 5000 });

        await ResidentName.Clear();
        await Page.Locator("#check-name-btn").ClickAsync();
        await Expect(Page.Locator("#name-warning")).ToHaveTextAsync("resident name required", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
