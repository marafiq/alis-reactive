namespace Alis.Reactive.PlaywrightTests.CoreBehaviors;

// DomReady dispatch payloads must flow into typed CustomEvent paths and rendered text.
[TestFixture]
public class WhenPayloadFlowsBetweenEvents : PlaywrightTestBase
{
    private const string PayloadPath = "/Sandbox/CoreBehaviors/Payload";
    private const string JavaScriptMaxSafeIntegerText = "9007199254740991";

    [Test]
    public async Task int_value_survives_serialization_and_displays_correctly()
    {
        await NavigateTo(PayloadPath);
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#int-value")).ToHaveTextAsync("42");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task long_value_at_javascript_max_safe_integer_preserves_full_precision()
    {
        await NavigateTo(PayloadPath);
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#long-value")).ToHaveTextAsync(JavaScriptMaxSafeIntegerText);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task double_value_preserves_decimal_places()
    {
        await NavigateTo(PayloadPath);
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#double-value")).ToHaveTextAsync("3.14159");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task string_value_preserves_spaces()
    {
        await NavigateTo(PayloadPath);
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#string-value")).ToHaveTextAsync("hello world");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task bool_value_uses_javascript_string_casing()
    {
        await NavigateTo(PayloadPath);
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#bool-value")).ToHaveTextAsync("true");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task nested_three_level_path_resolves_street_city_zip()
    {
        // The PascalCase expression path must resolve against camelCase payload JSON.
        await NavigateTo(PayloadPath);
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#address-street")).ToHaveTextAsync("123 Main St");
        await Expect(Page.Locator("#address-city")).ToHaveTextAsync("Seattle");
        await Expect(Page.Locator("#address-zip")).ToHaveTextAsync("98101");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task all_properties_resolved_shows_success_status()
    {
        // The status SetText reaction runs after every source binding. If any SetText throws on a bad path,
        // the sequence aborts before this element turns green.
        await NavigateTo(PayloadPath);
        await WaitForTraceMessage("booted", 5000);

        var status = Page.Locator("#payload-status");
        await Expect(status).ToHaveTextAsync("All payload properties resolved successfully");
        await Expect(status).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));
        await Expect(status).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("font-semibold"));

        var statusClasses = await status.GetAttributeAsync("class") ?? "";
        Assert.That(statusClasses, Does.Not.Contain("text-text-muted"),
            "RemoveClass('text-text-muted') must have removed the initial styling");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task all_primitive_types_display_without_type_coercion_errors()
    {
        await NavigateTo(PayloadPath);
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#int-value")).ToHaveTextAsync("42");
        await Expect(Page.Locator("#long-value")).ToHaveTextAsync(JavaScriptMaxSafeIntegerText);
        await Expect(Page.Locator("#double-value")).ToHaveTextAsync("3.14159");
        await Expect(Page.Locator("#string-value")).ToHaveTextAsync("hello world");
        await Expect(Page.Locator("#bool-value")).ToHaveTextAsync("true");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task status_element_has_correct_css_classes_after_all_resolved()
    {
        await NavigateTo(PayloadPath);
        await WaitForTraceMessage("booted", 5000);

        var status = Page.Locator("#payload-status");
        var statusClasses = await status.GetAttributeAsync("class") ?? "";

        Assert.That(statusClasses, Does.Contain("text-green-600"),
            "AddClass('text-green-600') must have applied — success color");
        Assert.That(statusClasses, Does.Contain("font-semibold"),
            "AddClass('font-semibold') must have applied — emphasis styling");
        Assert.That(statusClasses, Does.Not.Contain("text-text-muted"),
            "RemoveClass('text-text-muted') must have removed the initial muted class — " +
            "proves the remove+add+add class update chain executed in correct order");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task success_status_has_green_and_semibold_without_muted()
    {
        await NavigateTo(PayloadPath);
        await WaitForTraceMessage("booted", 5000);

        var status = Page.Locator("#payload-status");
        var statusClasses = await status.GetAttributeAsync("class") ?? "";

        Assert.Multiple(() =>
        {
            Assert.That(statusClasses, Does.Contain("text-green-600"),
                "AddClass('text-green-600') must have applied — success color after payload resolution");
            Assert.That(statusClasses, Does.Contain("font-semibold"),
                "AddClass('font-semibold') must have applied — emphasis after payload resolution");
            Assert.That(statusClasses, Does.Not.Contain("text-text-muted"),
                "RemoveClass('text-text-muted') must have stripped the initial muted class — " +
                "stale class would cause conflicting green+muted styles");
        });

        AssertNoConsoleErrors();
    }
}
