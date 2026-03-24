namespace Alis.Reactive.PlaywrightTests.HttpPipeline;

/// <summary>
/// Proves that the framework's response handling pipeline works for all supported content types:
///   1. Flat JSON — OnSuccess&lt;T&gt; with shallow property walking
///   2. Nested JSON — OnSuccess&lt;T&gt; with 3-level deep dot-path walking
///   3. HTML partial — Into() injects server-rendered HTML, native inputs work, SF components initialize
///
/// Each test clicks a button (triggering dispatch -> HTTP GET -> response handler) and verifies
/// that the EXACT server values arrive in the DOM. This proves the full pipeline:
///   C# anonymous object -> JSON serialization -> fetch -> response body walking -> DOM mutation
/// </summary>
[TestFixture]
public class WhenResponseContentTypeVaries : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/ContentType";

    /// <summary>IdGenerator type scope for ContentTypeModel.</summary>
    private const string S = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ContentTypeModel";

    [Test]
    public async Task flat_json_response_extracts_message_and_count()
    {
        // Server returns: { message: "Hello from server", count: 42 }
        // Plan walks: responseBody.message -> #flat-message, responseBody.count -> #flat-count
        //
        // Exact value matching — not "contains text". If the server response shape changes
        // or the ExpressionPathHelper generates wrong paths for FlatResponse properties,
        // the exact match will fail.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Fetch Flat JSON" }).ClickAsync();

        await Expect(Page.Locator("#flat-message")).ToHaveTextAsync("Hello from server", new() { Timeout = 5000 });
        await Expect(Page.Locator("#flat-count")).ToHaveTextAsync("42", new() { Timeout = 5000 });

        // Spinner must be hidden after success handler completes
        await Expect(Page.Locator("#flat-spinner")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task nested_json_walks_three_level_deep_path()
    {
        // Server returns: { data: { user: { name: "Jane Doe", email: "jane@example.com" }, total: 99.5 } }
        // Plan walks:
        //   responseBody.data.user.name  -> #nested-name   (3 levels: data -> user -> name)
        //   responseBody.data.user.email -> #nested-email   (3 levels: data -> user -> email)
        //   responseBody.data.total      -> #nested-total   (2 levels: data -> total, decimal precision)
        //
        // If walk() fails at depth > 2, name and email will be empty but total might still work.
        // If decimal 99.5 gets mangled (e.g., "99.50" or "100"), the exact match catches it.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Fetch Nested JSON" }).ClickAsync();

        await Expect(Page.Locator("#nested-name")).ToHaveTextAsync("Jane Doe", new() { Timeout = 5000 });
        await Expect(Page.Locator("#nested-email")).ToHaveTextAsync("jane@example.com", new() { Timeout = 5000 });
        await Expect(Page.Locator("#nested-total")).ToHaveTextAsync("99.5", new() { Timeout = 5000 });

        // Spinner must be hidden after success handler completes
        await Expect(Page.Locator("#nested-spinner")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task html_partial_via_into_renders_native_and_fusion_components()
    {
        // Into("partial-container") fetches HTML from /Sandbox/HttpPipeline/ContentType/Partial and injects it
        // using ej.base.append (which executes ScriptManager output to initialize SF components).
        //
        // The partial contains:
        //   1. Native <input> via Html.NativeTextBoxFor() — must have value "native-partial-value"
        //   2. Syncfusion NumericTextBox via Html.NumericTextBoxFor() — must initialize (has .e-numerictextbox class)
        //   3. Marker element #partial-loaded-marker — proves the HTML was actually injected
        //
        // If Into() uses innerHTML instead of ej.base.append, the SF component won't initialize
        // (no .e-numerictextbox class). If the partial model binding breaks, the native input
        // will have an empty value.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Partial" }).ClickAsync();

        // Marker proves HTML was injected into the container
        await Expect(Page.Locator("#partial-loaded-marker")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#partial-loaded-marker")).ToHaveTextAsync("Partial loaded successfully");

        // Native input rendered via Html.NativeTextBoxFor — value comes from server model
        await Expect(Page.Locator($"#{S}__NativeValue")).ToHaveValueAsync("native-partial-value");

        // Syncfusion NumericTextBox initialized via ej.base.append — the SF wrapper class proves
        // the component constructor ran (not just raw HTML injection)
        await Expect(Page.Locator("#partial-container .e-numerictextbox")).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Spinner must be hidden after success handler completes
        await Expect(Page.Locator("#partial-spinner")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task html_partial_components_are_interactive_after_injection()
    {
        // After Into() injects the partial, verify the native input inside is interactive —
        // not just dead HTML. Type a new value into the NativeTextBoxFor input and verify
        // the DOM value changes. This proves Into() with ej.base.append correctly initializes
        // the injected components so they respond to user interaction.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Load the partial via Into()
        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Partial" }).ClickAsync();

        // Wait for partial injection — marker element proves HTML arrived
        await Expect(Page.Locator("#partial-loaded-marker")).ToBeVisibleAsync(new() { Timeout = 5000 });

        // The native input rendered by NativeTextBoxFor has the IdGenerator-based ID
        var nativeInput = Page.Locator($"#{S}__NativeValue");

        // Verify the input has its server-rendered default value
        await Expect(nativeInput).ToHaveValueAsync("native-partial-value");

        // Clear and type a new value — proves the input is interactive, not inert HTML
        await nativeInput.ClearAsync();
        await nativeInput.FillAsync("user-typed-value");
        await Expect(nativeInput).ToHaveValueAsync("user-typed-value");

        // Verify the input is editable (not disabled/readonly)
        var isDisabled = await nativeInput.IsDisabledAsync();
        Assert.That(isDisabled, Is.False, "Native input inside injected partial must be interactive");

        AssertNoConsoleErrors();
    }
}
