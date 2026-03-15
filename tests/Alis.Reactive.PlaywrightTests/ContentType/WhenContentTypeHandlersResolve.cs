namespace Alis.Reactive.PlaywrightTests.ContentType;

[TestFixture]
public class WhenContentTypeHandlersResolve : PlaywrightTestBase
{
    private const string Path = "/Sandbox/ContentType";

    /// <summary>IdGenerator type scope for ContentTypeModel.</summary>
    private const string S = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ContentTypeModel";

    [Test]
    public async Task page_loads_with_correct_title()
    {
        await NavigateTo(Path);
        await Expect(Page).ToHaveTitleAsync("Content Type — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    // ── Section 1: Flat JSON Response ──

    [Test]
    public async Task flat_json_response_resolves_message_and_count()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Fetch Flat JSON" }).ClickAsync();

        await Expect(Page.Locator("#flat-message")).ToHaveTextAsync("Hello from server", new() { Timeout = 5000 });
        await Expect(Page.Locator("#flat-count")).ToHaveTextAsync("42", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Section 2: Nested JSON Response ──

    [Test]
    public async Task nested_json_response_resolves_deep_paths()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Fetch Nested JSON" }).ClickAsync();

        await Expect(Page.Locator("#nested-name")).ToHaveTextAsync("Jane Doe", new() { Timeout = 5000 });
        await Expect(Page.Locator("#nested-email")).ToHaveTextAsync("jane@example.com", new() { Timeout = 5000 });
        await Expect(Page.Locator("#nested-total")).ToHaveTextAsync("99.5", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Section 3: HTML Partial via Into ──

    [Test]
    public async Task into_loads_partial_with_native_and_fusion_components()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Partial" }).ClickAsync();

        // Partial loaded marker appears
        await Expect(Page.Locator("#partial-loaded-marker")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#partial-loaded-marker")).ToHaveTextAsync("Partial loaded successfully");

        // Native input rendered via Html.TextBoxFor with value from model
        await Expect(Page.Locator($"#{S}__NativeValue")).ToHaveValueAsync("native-partial-value");

        // Fusion NumericTextBox rendered via Html.EJS().NumericTextBoxFor() + ej.base.append
        // SF wraps the raw input — check the container has the SF component class
        await Expect(Page.Locator("#partial-container .e-numerictextbox")).ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
