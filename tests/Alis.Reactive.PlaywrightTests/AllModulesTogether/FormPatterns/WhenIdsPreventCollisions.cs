namespace Alis.Reactive.PlaywrightTests.AllModulesTogether.FormPatterns;

[TestFixture]
public class WhenIdsPreventCollisions : PlaywrightTestBase
{
    private const string Path = "/Sandbox/AllModulesTogether/IdGenerator";

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

    [Test]
    public async Task all_five_fields_have_unique_ids_in_json_form()
    {
        // Each field in the form — Name (text), Amount (Fusion numeric), Status (dropdown),
        // City (dropdown), PostalCode (Fusion numeric) — must have a distinct generated ID.
        // If any two collide, gather reads the wrong element.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Read the 5 generated IDs from the inspection section (source of truth)
        var inspectionRows = Page.Locator("#id-inspection .text-emerald-700");
        var amountId = (await inspectionRows.Nth(1).TextContentAsync())!.Trim();
        var statusId = (await inspectionRows.Nth(2).TextContentAsync())!.Trim();
        var cityId = (await inspectionRows.Nth(3).TextContentAsync())!.Trim();
        var postalCodeId = (await inspectionRows.Nth(4).TextContentAsync())!.Trim();

        // Also get the native text input ID from the DOM
        var nameId = await Page.Locator("#json-form input[name='Name']").GetAttributeAsync("id");

        var ids = new[] { nameId, amountId, statusId, cityId, postalCodeId };

        // All must be non-empty
        Assert.That(ids, Has.All.Not.Null.And.All.Not.Empty, "Every field must have a generated ID");

        // All must be unique
        Assert.That(ids.Distinct().Count(), Is.EqualTo(ids.Length),
            $"All 5 field IDs must be unique, got: [{string.Join(", ", ids)}]");

        // Verify each generated ID exists as an element in the json-form
        // (both forms share the same plan, so IDs appear twice — scope to json-form)
        foreach (var id in ids)
        {
            var el = Page.Locator($"#json-form #{id}");
            await Expect(el).ToBeAttachedAsync(new() { Timeout = 2000 });
        }

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task id_inspection_section_displays_all_generated_ids()
    {
        // The page has an ID inspection section that shows the IdGenerator output
        // for each property. Verify that every row contains a non-empty ID value.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var inspectionRows = Page.Locator("#id-inspection .text-emerald-700");
        var count = await inspectionRows.CountAsync();
        Assert.That(count, Is.EqualTo(5), "ID inspection section should show 5 generated IDs");

        for (var i = 0; i < count; i++)
        {
            var text = await inspectionRows.Nth(i).TextContentAsync();
            Assert.That(text, Is.Not.Null.And.Not.Empty,
                $"ID inspection row {i} should contain a non-empty generated ID");
        }

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task id_inspection_ids_match_actual_dom_element_ids()
    {
        // The ID inspection section shows what IdGenerator.For() produces. Those IDs
        // must match the actual element IDs in the form — otherwise gather resolves wrong elements.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Read the Status ID from inspection section (third row, index 2) — native dropdown
        var inspectedStatusId = await Page.Locator("#id-inspection .text-emerald-700").Nth(2).TextContentAsync();
        var actualStatusId = await Page.Locator("#json-form select[name='Status']").GetAttributeAsync("id");

        Assert.That(actualStatusId, Is.EqualTo(inspectedStatusId!.Trim()),
            "Inspected ID must match actual DOM element ID for Status");

        // Read the City ID from inspection section (fourth row, index 3) — native dropdown
        var inspectedCityId = await Page.Locator("#id-inspection .text-emerald-700").Nth(3).TextContentAsync();
        var actualCityId = await Page.Locator("#json-form select[name='Address.City']").GetAttributeAsync("id");

        Assert.That(actualCityId, Is.EqualTo(inspectedCityId!.Trim()),
            "Inspected ID must match actual DOM element ID for City");

        // Verify the Amount ID from inspection exists as an element in the DOM
        // (Fusion NumericTextBox — SF wraps the input, so we check by element ID existence)
        var inspectedAmountId = (await Page.Locator("#id-inspection .text-emerald-700").Nth(1).TextContentAsync())!.Trim();
        await Expect(Page.Locator($"#{inspectedAmountId}")).ToBeAttachedAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task nested_postal_code_has_correct_id_structure()
    {
        // PostalCode is nested under Address (same as City). Verify PostalCode ID
        // follows the same nesting pattern as City.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Read PostalCode ID from inspection section (fifth row, index 4)
        var postalId = (await Page.Locator("#id-inspection .text-emerald-700").Nth(4).TextContentAsync())!.Trim();

        Assert.That(postalId, Is.Not.Null.And.Not.Empty, "PostalCode must have a generated ID");
        Assert.That(postalId, Does.Contain("__Address_PostalCode"),
            "Nested PostalCode should use '__' delimiter with Address_PostalCode path");

        // Verify the element exists in the DOM
        await Expect(Page.Locator($"#{postalId}")).ToBeAttachedAsync(new() { Timeout = 2000 });

        // Verify it shares the same scope prefix as the City dropdown
        var cityId = await Page.Locator("#json-form select[name='Address.City']").GetAttributeAsync("id");
        var postalScope = postalId.Split("__")[0];
        var cityScope = cityId!.Split("__")[0];
        Assert.That(postalScope, Is.EqualTo(cityScope),
            "PostalCode and City must share the same model scope prefix");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task json_post_result_displays_with_success_styling()
    {
        // After successful JSON POST, the result element should show green text styling
        // indicating the server accepted the data.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var result = Page.Locator("#json-result");

        // Before submission — empty
        await Expect(result).ToHaveTextAsync("");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Submit JSON" }).ClickAsync();

        await Expect(result).ToContainTextAsync("Name=Test", new() { Timeout = 5000 });
        await Expect(result).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task form_data_post_result_displays_with_success_styling()
    {
        // After successful FormData POST, the result element should show green text styling.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var result = Page.Locator("#form-result");

        // Before submission — empty
        await Expect(result).ToHaveTextAsync("");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Submit FormData" }).ClickAsync();

        await Expect(result).ToContainTextAsync("Name=Test", new() { Timeout = 5000 });
        await Expect(result).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));

        AssertNoConsoleErrors();
    }
}
