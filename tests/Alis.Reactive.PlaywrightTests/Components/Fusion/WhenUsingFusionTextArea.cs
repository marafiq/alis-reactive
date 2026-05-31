using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

[TestFixture]
public class WhenUsingFusionTextArea : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/TextArea";
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_TextAreaModel";
    private const string CareNoteId = Scope + "__CareNote";
    private const string AutoBlurNoteId = Scope + "__AutoBlurNote";

    private FusionTextAreaLocator CareNote => new(Page, CareNoteId);
    private FusionTextAreaLocator AutoBlurNote => new(Page, AutoBlurNoteId);

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#value-echo");
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("FusionTextArea — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_contains_typed_textarea_members()
    {
        await NavigateAndBoot();
        var planJson = await Page.Locator("#plan-json").TextContentAsync();

        Assert.That(planJson, Does.Contain("\"vendor\": \"fusion\""));
        Assert.That(planJson, Does.Contain("\"set\""));
        Assert.That(planJson, Does.Contain("\"dataBind\""));
        Assert.That(planJson, Does.Contain("\"input\""));
        Assert.That(planJson, Does.Contain("\"change\""));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_sets_visible_value_and_reads_value_source()
    {
        await NavigateAndBoot();

        await Expect(CareNote.TextArea).ToHaveValueAsync("Resident prefers morning medication rounds.", new() { Timeout = 5000 });
        await Expect(Page.Locator("#value-echo")).ToHaveTextAsync("Resident prefers morning medication rounds.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task input_event_exposes_value_and_previous_value()
    {
        await NavigateAndBoot();

        await CareNote.Fill("Hydration check completed.");

        await Expect(Page.Locator("#input-value")).ToHaveTextAsync("Hydration check completed.", new() { Timeout = 5000 });
        await Expect(Page.Locator("#input-previous")).ToHaveTextAsync("Resident prefers morning medication rounds.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_exposes_value_previous_value_and_interaction_state()
    {
        await NavigateAndBoot();

        await CareNote.FillAndBlur("Hydration check completed.");

        await Expect(Page.Locator("#change-value")).ToHaveTextAsync("Hydration check completed.", new() { Timeout = 5000 });
        await Expect(Page.Locator("#change-previous")).ToHaveTextAsync("Resident prefers morning medication rounds.", new() { Timeout = 5000 });
        await Expect(Page.Locator("#change-interacted")).ToHaveTextAsync("true", new() { Timeout = 5000 });
        await Expect(Page.Locator("#args-condition")).ToHaveTextAsync("note entered", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task focusin_method_focuses_textarea_and_focus_event_reads_value()
    {
        await NavigateAndBoot();

        await Page.Locator("#focus-in-btn").ClickAsync();

        await Expect(CareNote.TextArea).ToBeFocusedAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#focus-state")).ToHaveTextAsync("focused", new() { Timeout = 5000 });
        await Expect(Page.Locator("#focus-value")).ToHaveTextAsync("Resident prefers morning medication rounds.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task blur_event_reads_value()
    {
        await NavigateAndBoot();

        await CareNote.Focus();
        await CareNote.Blur();

        await Expect(Page.Locator("#focus-state")).ToHaveTextAsync("blurred", new() { Timeout = 5000 });
        await Expect(Page.Locator("#blur-value")).ToHaveTextAsync("Resident prefers morning medication rounds.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task focusout_method_removes_focus_without_button_click_stealing_focus()
    {
        await NavigateAndBoot();

        await AutoBlurNote.Focus();

        await Expect(Page.Locator("#focusout-method-state")).ToHaveTextAsync("focusout called", new() { Timeout = 5000 });
        await Expect(Page.Locator("#autoblur-state")).ToHaveTextAsync("blurred", new() { Timeout = 5000 });
        await Expect(AutoBlurNote.TextArea).Not.ToBeFocusedAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_reads_current_value()
    {
        await NavigateAndBoot();

        await Page.Locator("#check-note-btn").ClickAsync();
        await Expect(Page.Locator("#note-warning")).ToHaveTextAsync("care note set", new() { Timeout = 5000 });

        await CareNote.Clear();
        await Page.Locator("#check-note-btn").ClickAsync();
        await Expect(Page.Locator("#note-warning")).ToHaveTextAsync("care note required", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
