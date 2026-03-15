namespace Alis.Reactive.PlaywrightTests.Validation;

[TestFixture]
public class WhenValidatingFormFields : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation";
    private const string S = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ValidationShowcaseModel__";

    [Test]
    public async Task Page_loads_with_no_errors()
    {
        await NavigateTo(Path);
        await Expect(Page).ToHaveTitleAsync("Validation — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    // ── Section 1: Client-Side Rule Types ────────────────────

    [Test]
    public async Task Required_field_shows_error()
    {
        await NavigateTo(Path);

        // Click "Validate All" with empty Name field
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate All" }).ClickAsync();

        // Name is required — error should appear (framework .Validate() handles registration + validation)
        var nameError = Page.Locator("#all-rules-form [data-valmsg-for='AllRules.Name']");
        await Expect(nameError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(nameError).ToContainTextAsync("required");

        // With framework pattern, validation failure aborts the POST — result stays at default
        var result = Page.Locator("#all-rules-result");
        await Expect(result).ToContainTextAsync("Click to validate");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Email_field_shows_error()
    {
        await NavigateTo(Path);

        // Fill Name (passes required) but put invalid email
        await Page.FillAsync($"#{S}AllRules_Name", "Test User");
        await Page.FillAsync($"#{S}AllRules_Email", "not-an-email");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate All" }).ClickAsync();

        var emailError = Page.Locator("#all-rules-form [data-valmsg-for='AllRules.Email']");
        await Expect(emailError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(emailError).ToContainTextAsync("email");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Regex_field_shows_error()
    {
        await NavigateTo(Path);

        // Fill required fields, put invalid phone
        await Page.FillAsync($"#{S}AllRules_Name", "Test User");
        await Page.FillAsync($"#{S}AllRules_Email", "test@example.com");
        await Page.FillAsync($"#{S}AllRules_Phone", "123");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate All" }).ClickAsync();

        var phoneError = Page.Locator("#all-rules-form [data-valmsg-for='AllRules.Phone']");
        await Expect(phoneError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(phoneError).ToContainTextAsync("123-456-7890");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Range_field_shows_error()
    {
        await NavigateTo(Path);

        // Fill required fields, put age out of range
        await Page.FillAsync($"#{S}AllRules_Name", "Test User");
        await Page.FillAsync($"#{S}AllRules_Email", "test@example.com");
        await Page.FillAsync($"#{S}AllRules_Age", "200");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate All" }).ClickAsync();

        var ageError = Page.Locator("#all-rules-form [data-valmsg-for='AllRules.Age']");
        await Expect(ageError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(ageError).ToContainTextAsync("between");

        AssertNoConsoleErrors();
    }

    // ── Section 2: Server-Side 400 Errors ────────────────────

    [Test]
    public async Task Server_errors_display_at_fields()
    {
        await NavigateTo(Path);

        // Click "Save (Server Validate)" with empty fields → 400
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save (Server Validate)" }).ClickAsync();

        // Server result should indicate errors
        var result = Page.Locator("#server-result");
        await Expect(result).ToContainTextAsync("validation errors", new() { Timeout = 5000 });

        // Error spans at fields should be visible
        AssertNoConsoleErrorsExcept("400");
    }

    // ── Section 3: Nested Properties ─────────────────────────

    [Test]
    public async Task Nested_field_errors_display()
    {
        await NavigateTo(Path);

        // Click save with empty nested address fields → 400
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save Address (Server)" }).ClickAsync();

        var result = Page.Locator("#nested-result");
        await Expect(result).ToContainTextAsync("errors", new() { Timeout = 5000 });

        // Server returns errors with dotted names like "Nested.Address.Street"
        AssertNoConsoleErrorsExcept("400");
    }

    // ── Section 4: Conditional Rules ─────────────────────────

    [Test]
    public async Task Conditional_rule_applies_when_met()
    {
        await NavigateTo(Path);

        // Check "Is Employed" then validate with empty JobTitle
        await Page.CheckAsync($"#{S}Conditional_IsEmployed");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate Conditional" }).ClickAsync();

        var jobError = Page.Locator("#conditional-form [data-valmsg-for='Conditional.JobTitle']");
        await Expect(jobError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(jobError).ToContainTextAsync("required");

        // With framework pattern, validation failure aborts the POST — result stays at default
        var result = Page.Locator("#conditional-result");
        await Expect(result).ToContainTextAsync("Click to validate");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Model_bound_checkbox_uses_generated_id_and_mvc_name()
    {
        await NavigateTo(Path);

        var checkbox = Page.Locator($"#{S}Conditional_IsEmployed");
        await Expect(checkbox).ToHaveAttributeAsync("name", "Conditional.IsEmployed");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Conditional_rule_skips_when_not_met()
    {
        await NavigateTo(Path);

        // Leave "Is Employed" unchecked — JobTitle should not be required
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate Conditional" }).ClickAsync();

        var jobError = Page.Locator("#conditional-form [data-valmsg-for='Conditional.JobTitle']");
        await Expect(jobError).ToBeHiddenAsync(new() { Timeout = 3000 });

        var result = Page.Locator("#conditional-result");
        await Expect(result).ToContainTextAsync("passed");

        AssertNoConsoleErrors();
    }

    // ── Section 5: Live Clearing ─────────────────────────────

    [Test]
    public async Task Typing_clears_field_error()
    {
        await NavigateTo(Path);

        // Trigger errors first
        await Page.GetByRole(AriaRole.Button, new() { Name = "Show Errors First" }).ClickAsync();

        // Name error should be visible
        var nameError = Page.Locator("#live-form [data-valmsg-for='Live.Name']");
        await Expect(nameError).ToBeVisibleAsync(new() { Timeout = 3000 });

        // Type into the field — error should clear
        await Page.FillAsync($"#{S}Live_Name", "John");
        await Expect(nameError).ToBeHiddenAsync(new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Section 6: Combined Client + Server ──────────────────

    [Test]
    public async Task Client_validation_blocks_submit()
    {
        await NavigateTo(Path);

        // Click combined save with empty fields — client validation should block the POST
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate + Save" }).ClickAsync();

        // Client-side errors should show (Combined.Name required)
        var nameError = Page.Locator("#combined-form [data-valmsg-for='Combined.Name']");
        await Expect(nameError).ToBeVisibleAsync(new() { Timeout = 3000 });

        // The spinner should NOT have appeared (request was blocked)
        var spinner = Page.Locator("#combined-spinner");
        await Expect(spinner).ToBeHiddenAsync();

        // Result should not have changed from default (no server response)
        var result = Page.Locator("#combined-result");
        await Expect(result).ToContainTextAsync("Not submitted yet");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Combined_flow_passes_client_then_hits_server()
    {
        await NavigateTo(Path);

        // Fill valid data for client-side rules
        await Page.FillAsync($"#{S}Combined_Name", "Valid Name");
        await Page.FillAsync($"#{S}Combined_Email", "valid@example.com");

        // Click Validate + Save — client passes, server validates remaining rules
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate + Save" }).ClickAsync();

        // Should reach the server and get a response (success or error)
        var result = Page.Locator("#combined-result");
        await Expect(result).Not.ToContainTextAsync("Not submitted yet", new() { Timeout = 5000 });

        AssertNoConsoleErrorsExcept("400");
    }

    // ── Section 7: Hidden Fields — Validate with Hidden Sections ───

    [Test]
    public async Task Hidden_fields_skipped_during_validation()
    {
        await NavigateTo(Path);

        // Extra fields are hidden by default (display:none).
        // Fill only the visible Name field and validate.
        await Page.FillAsync($"#{S}Hidden_Name", "Visible User");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate Hidden Form" }).ClickAsync();

        // Should pass — hidden Phone/Salary are skipped
        var result = Page.Locator("#hidden-result");
        await Expect(result).ToContainTextAsync("passed", new() { Timeout = 3000 });

        // Hidden field error spans should NOT be visible
        await Expect(Page.Locator("#hidden-fields-form [data-valmsg-for='Hidden.Phone']")).ToBeHiddenAsync();
        await Expect(Page.Locator("#hidden-fields-form [data-valmsg-for='Hidden.Salary']")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Revealed_fields_validate_after_toggle()
    {
        await NavigateTo(Path);

        // Fill Name, check toggle to reveal extra fields
        await Page.FillAsync($"#{S}Hidden_Name", "Visible User");
        await Page.CheckAsync($"#{S}Hidden_ShowExtras");

        // Put invalid data in revealed fields
        await Page.FillAsync($"#{S}Hidden_Phone", "bad");
        await Page.FillAsync($"#{S}Hidden_Salary", "999999");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate Hidden Form" }).ClickAsync();

        // Now that fields are visible, they should be validated
        var phoneError = Page.Locator("#hidden-fields-form [data-valmsg-for='Hidden.Phone']");
        await Expect(phoneError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(phoneError).ToContainTextAsync("123-456-7890");

        var salaryError = Page.Locator("#hidden-fields-form [data-valmsg-for='Hidden.Salary']");
        await Expect(salaryError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(salaryError).ToContainTextAsync("500,000");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Hidden_fields_submit_to_server_successfully()
    {
        await NavigateTo(Path);

        // Fill visible Name, leave extra fields hidden, submit to server
        await Page.FillAsync($"#{S}Hidden_Name", "Server User");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Submit Profile" }).ClickAsync();

        var result = Page.Locator("#hidden-result");
        await Expect(result).ToContainTextAsync("Profile saved", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Hidden_fields_client_blocks_empty_name()
    {
        await NavigateTo(Path);

        // Leave Name empty, click Submit — client .Validate(hiddenDesc) catches it
        await Page.GetByRole(AriaRole.Button, new() { Name = "Submit Profile" }).ClickAsync();

        // Client-side required error shows at the field (POST is aborted)
        var nameError = Page.Locator("#hidden-fields-form [data-valmsg-for='Hidden.Name']");
        await Expect(nameError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(nameError).ToContainTextAsync("required");

        // Result stays at default — no server request fired
        var result = Page.Locator("#hidden-result");
        await Expect(result).ToContainTextAsync("Click to validate or submit");

        AssertNoConsoleErrors();
    }

    // ── Section 8: Fake DB Validation (ProblemDetails 400) ─────

    [Test]
    public async Task Db_validation_rejects_duplicate_email()
    {
        await NavigateTo(Path);

        // Fill valid data but use "taken" in email to trigger DB uniqueness check
        await Page.FillAsync($"#{S}Db_Name", "Normal User");
        await Page.FillAsync($"#{S}Db_Email", "taken@test.com");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save with DB Check" }).ClickAsync();

        // Server DB check returns 400 with email error
        var result = Page.Locator("#db-result");
        await Expect(result).ToContainTextAsync("Database validation failed", new() { Timeout = 5000 });

        // Email error should display at the field
        var emailError = Page.Locator("#db-form [data-valmsg-for='Db.Email']");
        await Expect(emailError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(emailError).ToContainTextAsync("already registered");

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task Db_validation_rejects_reserved_username()
    {
        await NavigateTo(Path);

        // Use "admin" to trigger reserved username check
        await Page.FillAsync($"#{S}Db_Name", "admin");
        await Page.FillAsync($"#{S}Db_Email", "admin@test.com");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save with DB Check" }).ClickAsync();

        var result = Page.Locator("#db-result");
        await Expect(result).ToContainTextAsync("Database validation failed", new() { Timeout = 5000 });

        var nameError = Page.Locator("#db-form [data-valmsg-for='Db.Name']");
        await Expect(nameError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(nameError).ToContainTextAsync("reserved");

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task Db_validation_passes_with_clean_data()
    {
        await NavigateTo(Path);

        // Valid data that passes both FluentValidation and DB checks
        await Page.FillAsync($"#{S}Db_Name", "John Smith");
        await Page.FillAsync($"#{S}Db_Email", "john@example.com");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save with DB Check" }).ClickAsync();

        var result = Page.Locator("#db-result");
        await Expect(result).ToContainTextAsync("Saved to database successfully", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Db_validation_client_blocks_empty_fields()
    {
        await NavigateTo(Path);

        // Empty fields — client .Validate(dbDesc) catches required Name/Email before POST
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save with DB Check" }).ClickAsync();

        // Client-side required error at the field (POST aborted, no server request)
        var nameError = Page.Locator("#db-form [data-valmsg-for='Db.Name']");
        await Expect(nameError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(nameError).ToContainTextAsync("required");

        // Result stays at default — no server request fired
        var result = Page.Locator("#db-result");
        await Expect(result).ToContainTextAsync("Not submitted yet");

        AssertNoConsoleErrors();
    }

    // ── Section 9: Partial View + Client/Server Validation ────

    [Test]
    public async Task Partial_view_loads_address_form()
    {
        await NavigateTo(Path);

        // Click "Load Address Form" → GET partial view
        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Address Form" }).ClickAsync();

        // Partial status should update
        var status = Page.Locator("#partial-status");
        await Expect(status).ToContainTextAsync("loaded", new() { Timeout = 5000 });

        // Address fields should now exist in DOM
        await Expect(Page.Locator($"#{S}Nested_Address_Street")).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(Page.Locator($"#{S}Nested_Address_City")).ToBeVisibleAsync();
        await Expect(Page.Locator($"#{S}Nested_Address_ZipCode")).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Partial_client_validation_blocks_submit_with_empty_fields()
    {
        await NavigateTo(Path);

        // Load the partial first
        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Address Form" }).ClickAsync();
        await Expect(Page.Locator($"#{S}Nested_Address_Street")).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Click "Save Address" with empty fields — client-side validation blocks the POST
        await Page.Locator("#save-partial-btn").ClickAsync();

        // Error spans in the partial should be visible (client-side validation fired)
        var streetError = Page.Locator("#partial-form [data-valmsg-for='Nested.Address.Street']");
        await Expect(streetError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(streetError).ToContainTextAsync("required");

        var cityError = Page.Locator("#partial-form [data-valmsg-for='Nested.Address.City']");
        await Expect(cityError).ToBeVisibleAsync();
        await Expect(cityError).ToContainTextAsync("required");

        // POST never fired — result text stays at initial value
        var result = Page.Locator("#partial-save-result");
        await Expect(result).ToContainTextAsync("Not submitted yet");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Partial_client_validation_clears_on_input()
    {
        await NavigateTo(Path);

        // Load partial, trigger validation errors
        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Address Form" }).ClickAsync();
        await Expect(Page.Locator($"#{S}Nested_Address_Street")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.Locator("#save-partial-btn").ClickAsync();

        var streetError = Page.Locator("#partial-form [data-valmsg-for='Nested.Address.Street']");
        await Expect(streetError).ToBeVisibleAsync(new() { Timeout = 3000 });

        // Type into the field — live clearing should hide the error
        await Page.FillAsync($"#{S}Nested_Address_Street", "123 Main St");
        await Expect(streetError).ToBeHiddenAsync(new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Partial_valid_data_saves_successfully()
    {
        await NavigateTo(Path);

        // Load the partial
        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Address Form" }).ClickAsync();
        await Expect(Page.Locator($"#{S}Nested_Address_Street")).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Fill valid address data
        await Page.FillAsync($"#{S}Nested_Address_Street", "123 Main St");
        await Page.FillAsync($"#{S}Nested_Address_City", "Springfield");
        await Page.FillAsync($"#{S}Nested_Address_ZipCode", "62701");

        // Click "Save Address" → client passes → POST fires → 200
        await Page.Locator("#save-partial-btn").ClickAsync();

        var result = Page.Locator("#partial-save-result");
        await Expect(result).ToContainTextAsync("saved successfully", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Section 10: Delivery Note Partial (Same TModel) ─────

    [Test]
    public async Task Delivery_partial_loads_form()
    {
        await NavigateTo(Path);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Delivery Note Form" }).ClickAsync();

        var status = Page.Locator("#delivery-status");
        await Expect(status).ToContainTextAsync("loaded", new() { Timeout = 5000 });

        // Delivery fields should exist in DOM
        await Expect(Page.Locator($"#{S}Nested_Delivery_Instructions")).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(Page.Locator($"#{S}Nested_Delivery_ContactPhone")).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Delivery_partial_client_validation_blocks_submit()
    {
        await NavigateTo(Path);

        // Load the partial
        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Delivery Note Form" }).ClickAsync();
        await Expect(Page.Locator($"#{S}Nested_Delivery_Instructions")).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Click save with empty fields — client validation blocks POST
        await Page.Locator("#save-delivery-btn").ClickAsync();

        var instrError = Page.Locator("#delivery-form [data-valmsg-for='Nested.Delivery.Instructions']");
        await Expect(instrError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(instrError).ToContainTextAsync("required");

        // POST never fired
        var result = Page.Locator("#delivery-result");
        await Expect(result).ToContainTextAsync("Not submitted yet");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Delivery_partial_server_validation_on_bad_phone()
    {
        await NavigateTo(Path);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Delivery Note Form" }).ClickAsync();
        await Expect(Page.Locator($"#{S}Nested_Delivery_Instructions")).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Fill instructions (passes required) but bad phone (fails regex on server)
        await Page.FillAsync($"#{S}Nested_Delivery_Instructions", "Leave at door");
        await Page.FillAsync($"#{S}Nested_Delivery_ContactPhone", "bad");

        await Page.Locator("#save-delivery-btn").ClickAsync();

        // Client validation passes (phone regex is a client rule too), so check if it catches it
        // Phone regex rule: ^\d{3}-\d{3}-\d{4}$ — "bad" should fail client-side
        var phoneError = Page.Locator("#delivery-form [data-valmsg-for='Nested.Delivery.ContactPhone']");
        await Expect(phoneError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(phoneError).ToContainTextAsync("Phone must match");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Delivery_partial_valid_data_saves()
    {
        await NavigateTo(Path);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Delivery Note Form" }).ClickAsync();
        await Expect(Page.Locator($"#{S}Nested_Delivery_Instructions")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Page.FillAsync($"#{S}Nested_Delivery_Instructions", "Leave at door");
        await Page.FillAsync($"#{S}Nested_Delivery_ContactPhone", "123-456-7890");

        await Page.Locator("#save-delivery-btn").ClickAsync();

        var result = Page.Locator("#delivery-result");
        await Expect(result).ToContainTextAsync("saved", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Section 11: Standalone Contact Form Partial (Own TModel) ────

    private const string C = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ContactFormModel__";

    [Test]
    public async Task Contact_partial_loads_form()
    {
        await NavigateTo(Path);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Contact Form" }).ClickAsync();

        var status = Page.Locator("#contact-status");
        await Expect(status).ToContainTextAsync("loaded", new() { Timeout = 5000 });

        // Contact fields should exist in DOM (own TModel prefix)
        await Expect(Page.Locator($"#{C}Name")).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(Page.Locator($"#{C}Email")).ToBeVisibleAsync();
        await Expect(Page.Locator($"#{C}Message")).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Contact_partial_client_validation_blocks_submit()
    {
        await NavigateTo(Path);

        // Load the standalone partial
        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Contact Form" }).ClickAsync();
        await Expect(Page.Locator($"#{C}Name")).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Click "Send Message" with empty fields — client validation blocks POST
        await Page.GetByRole(AriaRole.Button, new() { Name = "Send Message" }).ClickAsync();

        var nameError = Page.Locator("#contact-form [data-valmsg-for='Name']");
        await Expect(nameError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(nameError).ToContainTextAsync("required");

        // POST never fired — result stays at default
        var result = Page.Locator("#contact-result");
        await Expect(result).ToContainTextAsync("Not submitted yet");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Contact_partial_valid_data_saves()
    {
        await NavigateTo(Path);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Contact Form" }).ClickAsync();
        await Expect(Page.Locator($"#{C}Name")).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Fill valid data
        await Page.FillAsync($"#{C}Name", "John Doe");
        await Page.FillAsync($"#{C}Email", "john@example.com");
        await Page.FillAsync($"#{C}Message", "Hello, this is a test message!");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Send Message" }).ClickAsync();

        var result = Page.Locator("#contact-result");
        await Expect(result).ToContainTextAsync("Message sent", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Contact_partial_does_not_impact_parent_validation()
    {
        await NavigateTo(Path);

        // Load the standalone partial first
        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Contact Form" }).ClickAsync();
        await Expect(Page.Locator($"#{C}Name")).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Now test parent plan's Section 1 validation still works
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate All" }).ClickAsync();

        var nameError = Page.Locator("#all-rules-form [data-valmsg-for='AllRules.Name']");
        await Expect(nameError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(nameError).ToContainTextAsync("required");

        // Parent result stays at default (validation blocked POST)
        var result = Page.Locator("#all-rules-result");
        await Expect(result).ToContainTextAsync("Click to validate");

        AssertNoConsoleErrors();
    }
}
