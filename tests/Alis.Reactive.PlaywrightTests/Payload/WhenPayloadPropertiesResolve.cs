namespace Alis.Reactive.PlaywrightTests.Payload;

/// <summary>
/// Proves that C# types survive the full serialization path:
///   C# expression -> ExpressionPathHelper -> plan JSON source binding -> runtime resolve() -> DOM text
///
/// Each test targets a specific C# type (int, long, double, string, bool) or a nested dot-path.
/// If ExpressionPathHelper changes its casing/path logic, or resolver.ts changes its walk logic,
/// or System.Text.Json changes its serialization, one or more of these tests will catch it.
///
/// The payload is dispatched via dom-ready and consumed via CustomEvent<PayloadShowcaseModel>.
/// No HTTP calls — pure plan-driven data flow.
/// </summary>
[TestFixture]
public class WhenPayloadPropertiesResolve : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Payload";

    [Test]
    public async Task int_value_survives_serialization_and_displays_correctly()
    {
        // C# int 42 -> JSON number 42 -> resolveToString -> DOM "42"
        // If System.Text.Json wraps ints in quotes or resolver.ts coerces differently, this fails.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#int-value")).ToHaveTextAsync("42");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task long_value_preserves_full_precision()
    {
        // 9007199254740991 is Number.MAX_SAFE_INTEGER — the largest integer JS can represent exactly.
        // If C# serializes as a JSON string instead of number, or JS loses precision via parseFloat,
        // the displayed value will be wrong. This is the boundary case for C#-to-JS numeric fidelity.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#long-value")).ToHaveTextAsync("9007199254740991");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task double_value_preserves_decimal_places()
    {
        // C# double 3.14159 -> JSON 3.14159 -> DOM "3.14159"
        // Floating point formatting must preserve all significant digits.
        // If serialization truncates or rounds, the exact string won't match.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#double-value")).ToHaveTextAsync("3.14159");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task string_value_passes_through_unchanged()
    {
        // "hello world" must arrive in the DOM exactly — no encoding, no trimming, no escaping.
        // The space character is the simplest case that breaks if someone URL-encodes or HTML-encodes
        // the value during serialization.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#string-value")).ToHaveTextAsync("hello world");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task bool_value_displays_as_string()
    {
        // C# bool true -> JSON true -> resolveToString -> DOM "true"
        // If resolveToString returns "True" (C# casing) or "1" or empty string, this fails.
        // JavaScript's String(true) produces "true" — that's the expected canonical form.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#bool-value")).ToHaveTextAsync("true");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task nested_three_level_path_resolves_street_city_zip()
    {
        // C# expression x => x.Address.Street -> ExpressionPathHelper -> "evt.address.street"
        // Runtime: walk(ctx, "evt.address.street") -> "123 Main St"
        //
        // All three Address properties test the SAME dot-path walking code at different leaf nodes.
        // If ExpressionPathHelper breaks PascalCase -> camelCase conversion, ALL three fail.
        // If walk() breaks at depth > 1, the nested paths fail but flat paths still pass.
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
        // After ALL SetText mutations complete, the reaction chain also mutates #payload-status:
        //   RemoveClass("text-text-muted") + AddClass("text-green-600") + AddClass("font-semibold") + SetText(...)
        //
        // This is the aggregate indicator — if ANY preceding SetText failed to execute (e.g., because
        // resolve() threw on a bad path), the sequential reaction would abort and status would stay gray.
        // Green status = every single source binding in the reaction resolved without error.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var status = Page.Locator("#payload-status");
        await Expect(status).ToHaveTextAsync("All payload properties resolved successfully");
        await Expect(status).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));
        await Expect(status).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("font-semibold"));

        // The initial muted class must have been removed — proves RemoveClass worked
        var classAttr = await status.GetAttributeAsync("class") ?? "";
        Assert.That(classAttr, Does.Not.Contain("text-text-muted"),
            "RemoveClass('text-text-muted') must have removed the initial styling");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task all_primitive_types_display_without_type_coercion_errors()
    {
        // Single pass across all 5 primitive types: int, long, double, string, bool.
        // Each type exercises a different serialization path in System.Text.Json and a
        // different coercion path in resolveToString(). If any type breaks, the assertion
        // identifies WHICH one failed — not just that "something" failed.
        // This proves the entire resolver pipeline handles type diversity end-to-end.
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
        // The reaction chain on #payload-status is a 3-mutation sequence:
        //   1. RemoveClass("text-text-muted")  — removes initial gray styling
        //   2. AddClass("text-green-600")      — adds success color
        //   3. AddClass("font-semibold")       — adds emphasis
        //
        // This test verifies all three mutations executed correctly as a unit.
        // If RemoveClass fails: muted class remains (conflicting styles).
        // If either AddClass fails: visual cue is incomplete.
        // All three together prove the multi-class mutation chain works correctly.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var status = Page.Locator("#payload-status");
        var classAttr = await status.GetAttributeAsync("class") ?? "";

        Assert.That(classAttr, Does.Contain("text-green-600"),
            "AddClass('text-green-600') must have applied — success color");
        Assert.That(classAttr, Does.Contain("font-semibold"),
            "AddClass('font-semibold') must have applied — emphasis styling");
        Assert.That(classAttr, Does.Not.Contain("text-text-muted"),
            "RemoveClass('text-text-muted') must have removed the initial muted class — " +
            "proves the remove+add+add mutation chain executed in correct order");

        AssertNoConsoleErrors();
    }
}
