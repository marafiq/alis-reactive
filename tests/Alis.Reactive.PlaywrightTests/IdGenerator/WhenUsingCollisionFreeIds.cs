namespace Alis.Reactive.PlaywrightTests.IdGenerator;

[TestFixture]
public class WhenUsingCollisionFreeIds : PlaywrightTestBase
{
    private const string Path = "/Sandbox/IdGenerator";

    [Test]
    public async Task page_loads_with_unique_ids()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page).ToHaveTitleAsync("IdGenerator Test — Alis.Reactive Sandbox");

        // TypeScope is displayed — should contain the model class name with underscores
        var scopeText = await Page.Locator("#id-prefix").TextContentAsync();
        Assert.That(scopeText, Does.Contain("IdGeneratorModel"));
        Assert.That(scopeText, Does.Not.Contain("."), "Scope should use underscores, not dots");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task unique_ids_exist_in_dom()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var scope = await Page.Locator("#id-prefix").TextContentAsync();

        // Elements with the full-name scope prefix exist in the form
        var scopedElements = Page.Locator($"#json-form [id^='{scope}__']");
        var count = await scopedElements.CountAsync();
        Assert.That(count, Is.GreaterThan(0), "Form should contain elements with full-name scope prefix");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task json_post_receives_correct_field_values()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Submit JSON" }).ClickAsync();

        var result = Page.Locator("#json-result");
        await Expect(result).ToContainTextAsync("Name=Test", new() { Timeout = 5000 });
        await Expect(result).ToContainTextAsync("Amount=42.5", new() { Timeout = 5000 });
        await Expect(result).ToContainTextAsync("Status=active", new() { Timeout = 5000 });
        await Expect(result).ToContainTextAsync("City=Seattle", new() { Timeout = 5000 });
        await Expect(result).ToContainTextAsync("Zip=98101", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task form_data_post_receives_correct_field_values()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Submit FormData" }).ClickAsync();

        var result = Page.Locator("#form-result");
        await Expect(result).ToContainTextAsync("Name=Test", new() { Timeout = 5000 });
        await Expect(result).ToContainTextAsync("Amount=42.5", new() { Timeout = 5000 });
        await Expect(result).ToContainTextAsync("Status=active", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task name_attributes_preserved_for_model_binding()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // name attributes are NOT prefixed — they match the model property path
        var statusSelect = Page.Locator("#json-form select[name='Status']");
        await Expect(statusSelect).ToBeAttachedAsync(new() { Timeout = 5000 });

        var citySelect = Page.Locator("#json-form select[name='Address.City']");
        await Expect(citySelect).ToBeAttachedAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task id_contains_double_underscore_delimiter()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // The native dropdown for Status should have id containing "__Status"
        var statusSelect = Page.Locator("#json-form select[name='Status']");
        var id = await statusSelect.GetAttributeAsync("id");
        Assert.That(id, Does.Contain("__Status"), "ID should contain '__' delimiter before property path");

        // The native dropdown for Address.City should have id containing "__Address_City"
        var citySelect = Page.Locator("#json-form select[name='Address.City']");
        var cityId = await citySelect.GetAttributeAsync("id");
        Assert.That(cityId, Does.Contain("__Address_City"), "Nested ID should use '__' delimiter and underscores for nesting");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task json_post_round_trips_exact_field_values_to_server()
    {
        // Fill specific user-chosen values in form fields, POST as JSON, server echoes back
        // exact values. This proves the full gather -> serialize -> deserialize pipeline
        // preserves field names and values even when the user changes them from defaults.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Change Name from default "Test" to "CustomName"
        var nameInput = Page.Locator("#json-form input[name='Name']");
        await nameInput.ClearAsync();
        await nameInput.FillAsync("CustomName");

        // Change Status dropdown from "active" to "inactive"
        var statusSelect = Page.Locator("#json-form select[name='Status']");
        await statusSelect.SelectOptionAsync("inactive");

        // Change City dropdown from "Seattle" to "Denver"
        var citySelect = Page.Locator("#json-form select[name='Address.City']");
        await citySelect.SelectOptionAsync("Denver");

        // Submit JSON and verify the server echoes back the exact user-entered values
        await Page.GetByRole(AriaRole.Button, new() { Name = "Submit JSON" }).ClickAsync();

        var result = Page.Locator("#json-result");
        await Expect(result).ToContainTextAsync("Name=CustomName", new() { Timeout = 5000 });
        await Expect(result).ToContainTextAsync("Status=inactive", new() { Timeout = 5000 });
        await Expect(result).ToContainTextAsync("City=Denver", new() { Timeout = 5000 });

        // Amount and PostalCode keep their model defaults (SF NumericTextBox — not changed)
        await Expect(result).ToContainTextAsync("Amount=42.5", new() { Timeout = 5000 });
        await Expect(result).ToContainTextAsync("Zip=98101", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task form_data_post_round_trips_exact_field_values_to_server()
    {
        // Same as JSON round-trip but with application/x-www-form-urlencoded content type.
        // Proves both content types resolve to correct model binding names through the
        // gather -> serialize pipeline.
        //
        // NOTE: Both forms on this page share the same ReactivePlan<IdGeneratorModel>, so
        // gather resolves component values by ID via document.getElementById — which returns
        // the first matching element (in #json-form). We fill the json-form inputs because
        // that is where gather reads from.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Change Name from default "Test" to "FormUser" (in json-form — first ID match)
        var nameInput = Page.Locator("#json-form input[name='Name']");
        await nameInput.ClearAsync();
        await nameInput.FillAsync("FormUser");

        // Change Status dropdown from "active" to "pending" (in json-form — first ID match)
        var statusSelect = Page.Locator("#json-form select[name='Status']");
        await statusSelect.SelectOptionAsync("pending");

        // Submit FormData and verify the server echoes back the exact user-entered values
        await Page.GetByRole(AriaRole.Button, new() { Name = "Submit FormData" }).ClickAsync();

        var result = Page.Locator("#form-result");
        await Expect(result).ToContainTextAsync("Name=FormUser", new() { Timeout = 5000 });
        await Expect(result).ToContainTextAsync("Status=pending", new() { Timeout = 5000 });

        // Amount keeps its model default (SF NumericTextBox — not changed)
        await Expect(result).ToContainTextAsync("Amount=42.5", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task nested_property_generates_different_id_than_flat_property()
    {
        // Verify that Address.City (nested) produces a structurally different ID than Status (flat).
        // Nested properties contain an extra underscore segment for each nesting level.
        // This catches IdGenerator regressions where nesting flattening changes.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Get the flat property ID (Status — no nesting)
        var statusSelect = Page.Locator("#json-form select[name='Status']");
        var statusId = await statusSelect.GetAttributeAsync("id");

        // Get the nested property ID (Address.City — one level of nesting)
        var citySelect = Page.Locator("#json-form select[name='Address.City']");
        var cityId = await citySelect.GetAttributeAsync("id");

        // Both IDs must exist
        Assert.That(statusId, Is.Not.Null.And.Not.Empty, "Status element must have an ID");
        Assert.That(cityId, Is.Not.Null.And.Not.Empty, "City element must have an ID");

        // Split on "__" — both share the same scope prefix (index 0)
        var statusParts = statusId!.Split("__");
        var cityParts = cityId!.Split("__");

        Assert.That(statusParts.Length, Is.EqualTo(2), "Flat property ID must have exactly one '__' delimiter");
        Assert.That(cityParts.Length, Is.EqualTo(2), "Nested property ID must have exactly one '__' delimiter");

        // Same scope prefix (both from IdGeneratorModel)
        Assert.That(cityParts[0], Is.EqualTo(statusParts[0]),
            "Both properties belong to the same model, so scope prefix must match");

        // Different property path segments — Status is flat, Address_City has nesting underscore
        Assert.That(statusParts[1], Is.EqualTo("Status"),
            "Flat property path should be just the property name");
        Assert.That(cityParts[1], Is.EqualTo("Address_City"),
            "Nested property path should use underscore for nesting (Address_City, not AddressCity)");

        // The nested ID is longer because it contains the parent segment
        Assert.That(cityParts[1].Length, Is.GreaterThan(statusParts[1].Length),
            "Nested property path must be longer than flat property path");

        AssertNoConsoleErrors();
    }
}
