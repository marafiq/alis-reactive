namespace Alis.Reactive.PlaywrightTests.HttpPipeline;

[TestFixture]
public class WhenResponseContentTypeVaries : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/ContentType";
    private const string ContentTypeModelScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ContentTypeModel";

    [Test]
    public async Task flat_json_response_extracts_message_and_count()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Fetch Flat JSON" }).ClickAsync();

        await Expect(Page.Locator("#flat-message")).ToHaveTextAsync("Hello from server", new() { Timeout = 5000 });
        await Expect(Page.Locator("#flat-count")).ToHaveTextAsync("42", new() { Timeout = 5000 });

        await Expect(Page.Locator("#flat-spinner")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task nested_json_walks_three_level_deep_path()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Fetch Nested JSON" }).ClickAsync();

        await Expect(Page.Locator("#nested-name")).ToHaveTextAsync("Jane Doe", new() { Timeout = 5000 });
        await Expect(Page.Locator("#nested-email")).ToHaveTextAsync("jane@example.com", new() { Timeout = 5000 });
        await Expect(Page.Locator("#nested-total")).ToHaveTextAsync("99.5", new() { Timeout = 5000 });

        await Expect(Page.Locator("#nested-spinner")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task html_partial_via_into_renders_native_and_fusion_components()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Partial" }).ClickAsync();

        await Expect(Page.Locator("#partial-loaded-marker")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#partial-loaded-marker")).ToHaveTextAsync("Partial loaded successfully");

        await Expect(Page.Locator($"#{ContentTypeModelScope}__NativeValue")).ToHaveValueAsync("native-partial-value");

        // The Syncfusion wrapper proves Into() used ej.base.append so ScriptManager output ran.
        await Expect(Page.Locator("#partial-container .e-numerictextbox")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Expect(Page.Locator("#partial-spinner")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task html_partial_components_are_interactive_after_injection()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Partial" }).ClickAsync();

        await Expect(Page.Locator("#partial-loaded-marker")).ToBeVisibleAsync(new() { Timeout = 5000 });

        var nativeInput = Page.Locator($"#{ContentTypeModelScope}__NativeValue");

        await Expect(nativeInput).ToHaveValueAsync("native-partial-value");

        await nativeInput.ClearAsync();
        await nativeInput.FillAsync("user-typed-value");
        await Expect(nativeInput).ToHaveValueAsync("user-typed-value");

        var isDisabled = await nativeInput.IsDisabledAsync();
        Assert.That(isDisabled, Is.False, "Native input inside injected partial must be interactive");

        AssertNoConsoleErrors();
    }
}
