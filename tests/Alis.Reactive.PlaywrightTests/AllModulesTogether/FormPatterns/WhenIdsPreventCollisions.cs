namespace Alis.Reactive.PlaywrightTests.AllModulesTogether.FormPatterns;

[TestFixture]
public class WhenIdsPreventCollisions : PlaywrightTestBase
{
    private const string Path = "/Sandbox/AllModulesTogether/IdGenerator";

    [Test]
    public async Task form_fields_are_selectable_by_generated_ids()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // If IDs contained dots, querySelector would fail (dot = class selector)
        // Verify the framework generates IDs that work with standard DOM APIs
        var nameField = Page.Locator("#json-form input[name='Name']");
        await Expect(nameField).ToBeAttachedAsync(new() { Timeout = 5000 });
        var id = await nameField.GetAttributeAsync("id");
        Assert.That(id, Is.Not.Null.And.Not.Empty, "Field must have a generated ID");
        // Verify the generated ID is usable in a selector (no dots that break querySelector)
        // Scope to #json-form since both forms on this page share the same generated IDs
        var foundById = Page.Locator($"#json-form #{id}");
        await Expect(foundById).ToBeAttachedAsync(new() { Timeout = 5000 });
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
