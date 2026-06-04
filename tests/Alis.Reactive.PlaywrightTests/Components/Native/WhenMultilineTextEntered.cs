namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// Exercises NativeTextArea SetValue, Value reads, Changed-event conditions,
/// and component-read conditions.
/// </summary>
[TestFixture]
public class WhenMultilineTextEntered : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/NativeTextArea";
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_NativeTextAreaModel__";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("NativeTextArea — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_sets_initial_care_notes()
    {
        await NavigateAndBoot();

        var textarea = Page.Locator($"#{Scope}CareNotes");
        await Expect(textarea).ToHaveValueAsync("Resident stable. Vitals within normal range.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task value_echoed_from_component_read()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#value-echo");
        await Expect(echo).ToHaveTextAsync("Resident stable. Vitals within normal range.", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_with_condition_shows_incident_status()
    {
        await NavigateAndBoot();

        var textarea = Page.Locator($"#{Scope}IncidentDescription");

        await textarea.FillAsync("Resident fell in hallway at 2pm.");
        // FillAsync dispatches input, but this test needs the change event.
        await textarea.BlurAsync();

        var status = Page.Locator("#incident-status");
        await Expect(status).ToHaveTextAsync("incident logged", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changed_event_with_condition_shows_warning_when_cleared()
    {
        await NavigateAndBoot();

        var textarea = Page.Locator($"#{Scope}IncidentDescription");

        await textarea.FillAsync("Resident fell in hallway at 2pm.");
        await textarea.BlurAsync();
        await textarea.ClearAsync();
        await textarea.BlurAsync();

        var status = Page.Locator("#incident-status");
        await Expect(status).ToHaveTextAsync("no incident", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_warns_when_empty()
    {
        await NavigateAndBoot();

        var textarea = Page.Locator($"#{Scope}CareNotes");
        await textarea.ClearAsync();

        await Page.Locator("#check-notes-btn").ClickAsync();

        var warning = Page.Locator("#notes-warning");
        await Expect(warning).ToHaveTextAsync("notes required", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task component_value_condition_confirms_when_filled()
    {
        await NavigateAndBoot();

        // DomReady already set the value under test.
        await Page.Locator("#check-notes-btn").ClickAsync();

        var warning = Page.Locator("#notes-warning");
        await Expect(warning).ToHaveTextAsync("notes recorded", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }
}
