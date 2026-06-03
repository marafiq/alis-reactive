namespace Alis.Reactive.PlaywrightTests.CoreBehaviors;

/// <summary>
/// Proves C# payload values survive the plan path:
/// expression -> plan JSON source binding -> runtime resolution -> DOM text.
/// The payload is dispatched on dom-ready and consumed by a typed CustomEvent,
/// so failures usually point at expression path casing, JSON shape, or runtime value walking.
/// </summary>
[TestFixture]
public class WhenPayloadFlowsBetweenEvents : PlaywrightTestBase
{
    private const string Path = "/Sandbox/CoreBehaviors/Payload";

    [Test]
    public async Task int_value_survives_serialization_and_displays_correctly()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#int-value")).ToHaveTextAsync("42");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task long_value_preserves_full_precision()
    {
        // 9007199254740991 is Number.MAX_SAFE_INTEGER: the largest integer JS can represent exactly.
        // If the JSON number or runtime text conversion loses precision, this displays the wrong value.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#long-value")).ToHaveTextAsync("9007199254740991");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task double_value_preserves_decimal_places()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#double-value")).ToHaveTextAsync("3.14159");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task string_value_passes_through_unchanged()
    {
        // The space in "hello world" catches accidental trimming or separator changes.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#string-value")).ToHaveTextAsync("hello world");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task bool_value_displays_as_string()
    {
        // Runtime text conversion should follow JavaScript casing: String(true) -> "true".
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#bool-value")).ToHaveTextAsync("true");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task nested_three_level_path_resolves_street_city_zip()
    {
        // All three Address properties prove the same PascalCase -> camelCase structured path
        // resolution at different leaves. Depth failures break these while flat properties still pass.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#address-street")).ToHaveTextAsync("123 Main St");
        await Expect(Page.Locator("#address-city")).ToHaveTextAsync("Seattle");
        await Expect(Page.Locator("#address-zip")).ToHaveTextAsync("98101");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task all_properties_resolved_shows_success_status()
    {
        // The status mutation runs after every source binding. If any SetText throws on a bad path,
        // the sequence aborts before this element turns green.
        await NavigateTo(Path);
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
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#int-value")).ToHaveTextAsync("42");
        await Expect(Page.Locator("#long-value")).ToHaveTextAsync("9007199254740991");
        await Expect(Page.Locator("#double-value")).ToHaveTextAsync("3.14159");
        await Expect(Page.Locator("#string-value")).ToHaveTextAsync("hello world");
        await Expect(Page.Locator("#bool-value")).ToHaveTextAsync("true");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task status_element_has_correct_css_classes_after_all_resolved()
    {
        // Final class state proves the remove/add/add mutation sequence did not skip a step.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var status = Page.Locator("#payload-status");
        var statusClasses = await status.GetAttributeAsync("class") ?? "";

        Assert.That(statusClasses, Does.Contain("text-green-600"),
            "AddClass('text-green-600') must have applied — success color");
        Assert.That(statusClasses, Does.Contain("font-semibold"),
            "AddClass('font-semibold') must have applied — emphasis styling");
        Assert.That(statusClasses, Does.Not.Contain("text-text-muted"),
            "RemoveClass('text-text-muted') must have removed the initial muted class — " +
            "proves the remove+add+add mutation chain executed in correct order");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task success_status_has_green_and_semibold_without_muted()
    {
        // Keep the complete final class invariant in one assertion block.
        await NavigateTo(Path);
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
