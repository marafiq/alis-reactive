namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// Exercises NativeHiddenField API end-to-end in the browser:
/// hidden inputs with seeded values, property reads (Value as source),
/// and POST gather (IncludeAll picks up hidden fields).
///
/// Page under test: /Sandbox/Components/NativeHiddenField
/// </summary>
[TestFixture]
public class WhenHiddenStateIsPersisted : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/NativeHiddenField";
    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_NativeHiddenFieldModel__";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
    }

    // -- Page loads --

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("NativeHiddenField — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    // -- Section 1: Hidden inputs have seeded values --

    [Test]
    public async Task hidden_resident_id_has_seeded_value()
    {
        await NavigateAndBoot();

        var input = Page.Locator($"#{Scope}ResidentId");
        await Expect(input).ToHaveValueAsync("RES-1042");
        await Expect(input).ToHaveAttributeAsync("type", "hidden");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task hidden_form_token_has_seeded_value()
    {
        await NavigateAndBoot();

        var input = Page.Locator($"#{Scope}FormToken");
        await Expect(input).ToHaveValueAsync("abc123");
        await Expect(input).ToHaveAttributeAsync("type", "hidden");
        AssertNoConsoleErrors();
    }

    // -- Section 2: Property Read -- DomReady reads hidden value into echo --

    [Test]
    public async Task domready_reads_resident_id_into_echo()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#resident-id-echo");
        await Expect(echo).ToHaveTextAsync("RES-1042", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_reads_form_token_into_echo()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#form-token-echo");
        await Expect(echo).ToHaveTextAsync("abc123", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    // -- Section 3: POST gather includes hidden values --

    [Test]
    public async Task post_gather_includes_hidden_fields()
    {
        await NavigateAndBoot();

        // Fill in the visible resident name field
        var nameInput = Page.Locator($"#{Scope}ResidentName");
        await nameInput.FillAsync("Margaret Thompson");

        // Click submit button
        await Page.Locator("#submit-btn").ClickAsync();

        // Verify server echoes back hidden field values
        await Expect(Page.Locator("#echo-resident-id")).ToHaveTextAsync("RES-1042", new() { Timeout = 5000 });
        await Expect(Page.Locator("#echo-form-token")).ToHaveTextAsync("abc123");
        await Expect(Page.Locator("#echo-resident-name")).ToHaveTextAsync("Margaret Thompson");
        await Expect(Page.Locator("#echo-field-count")).ToHaveTextAsync("3");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task post_gather_sends_hidden_fields_even_without_visible_input()
    {
        await NavigateAndBoot();

        // Don't fill in resident name — just click submit
        await Page.Locator("#submit-btn").ClickAsync();

        // Hidden fields should still be gathered
        await Expect(Page.Locator("#echo-resident-id")).ToHaveTextAsync("RES-1042", new() { Timeout = 5000 });
        await Expect(Page.Locator("#echo-form-token")).ToHaveTextAsync("abc123");
        // field count is 2 (ResidentId + FormToken, but not ResidentName which is empty)
        await Expect(Page.Locator("#echo-field-count")).ToHaveTextAsync("2");
        AssertNoConsoleErrors();
    }
}
