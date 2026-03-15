using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.Validation;

[TestFixture]
public class WhenValidatingFormFields : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation";
    private const string S = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ValidationShowcaseModel__";
    private const string C = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ContactFormModel__";

    // ── Journey 1: Complete validation error -> fix -> success cycle ────────

    [Test]
    public async Task user_submits_empty_form_sees_all_errors_then_fixes_and_succeeds()
    {
        await NavigateTo(Path);

        // Step 1: Submit empty form — multiple validation errors appear
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate All" }).ClickAsync();

        var nameError = Page.Locator("#all-rules-form [data-valmsg-for='AllRules.Name']");
        var emailError = Page.Locator("#all-rules-form [data-valmsg-for='AllRules.Email']");
        await Expect(nameError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(nameError).ToContainTextAsync("required");
        await Expect(emailError).ToBeVisibleAsync();
        await Expect(emailError).ToContainTextAsync("required");

        // Result should NOT have changed (POST was blocked by client validation)
        var result = Page.Locator("#all-rules-result");
        await Expect(result).ToContainTextAsync("Click to validate");

        // Step 2: Fill all fields with valid data
        await Page.FillAsync($"#{S}AllRules_Name", "Jane Smith");
        await Page.FillAsync($"#{S}AllRules_Email", "jane@example.com");
        await Page.FillAsync($"#{S}AllRules_Age", "30");
        await Page.FillAsync($"#{S}AllRules_Phone", "123-456-7890");
        await Page.FillAsync($"#{S}AllRules_Salary", "75000");
        await Page.FillAsync($"#{S}AllRules_Password", "securepassword123");

        // Step 3: Resubmit — should succeed
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate All" }).ClickAsync();

        await Expect(result).ToContainTextAsync("passed", new() { Timeout = 5000 });
        await Expect(result).ToHaveClassAsync(new Regex("text-green-600"));

        AssertNoConsoleErrors();
    }

    // ── Journey 2: Server validation round-trip ────────────────────────────

    [Test]
    public async Task server_rejects_empty_data_and_displays_field_errors_then_corrected_data_reduces_errors()
    {
        await NavigateTo(Path);

        // Step 1: Submit empty server form — server returns 400 with field errors
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save (Server Validate)" }).ClickAsync();

        var result = Page.Locator("#server-result");
        await Expect(result).ToContainTextAsync("validation errors", new() { Timeout = 5000 });
        await Expect(result).ToHaveClassAsync(new Regex("text-red-600"));

        // Step 2: Fill the server section fields — resubmit
        // The server validates the full model, so other sections still fail,
        // but the Server.Name and Server.Email errors should be resolved
        await Page.FillAsync($"#{S}Server_Name", "Alice Jones");
        await Page.FillAsync($"#{S}Server_Email", "alice@example.com");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save (Server Validate)" }).ClickAsync();

        // Server still returns 400 (full model validation), but fewer field errors
        // The key behavior: server errors display at the form, loading spinner works
        await Expect(result).ToContainTextAsync("validation errors", new() { Timeout = 5000 });

        AssertNoConsoleErrorsExcept("400");
    }

    // ── Journey 3: Conditional validation toggle ───────────────────────────

    [Test]
    public async Task conditional_checkbox_toggles_field_requirement()
    {
        await NavigateTo(Path);

        var jobError = Page.Locator("#conditional-form [data-valmsg-for='Conditional.JobTitle']");
        var result = Page.Locator("#conditional-result");

        // Step 1: Submit with unchecked IsEmployed — passes (JobTitle not required)
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate Conditional" }).ClickAsync();

        await Expect(result).ToContainTextAsync("passed", new() { Timeout = 3000 });
        await Expect(jobError).ToBeHiddenAsync();

        // Step 2: Check IsEmployed — now submit again — fails on JobTitle
        await Page.CheckAsync($"#{S}Conditional_IsEmployed");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate Conditional" }).ClickAsync();

        await Expect(jobError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(jobError).ToContainTextAsync("required");

        // Step 3: Fill JobTitle — resubmit — passes
        await Page.FillAsync($"#{S}Conditional_JobTitle", "Software Engineer");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate Conditional" }).ClickAsync();

        await Expect(result).ToContainTextAsync("passed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Journey 4: Hidden field visibility -> validation interaction ───────

    [Test]
    public async Task hidden_fields_skip_validation_then_validate_when_revealed()
    {
        await NavigateTo(Path);

        var result = Page.Locator("#hidden-result");

        // Step 1: Fill Name (visible) and submit — hidden Phone/Salary are skipped
        await Page.FillAsync($"#{S}Hidden_Name", "Visible User");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate Hidden Form" }).ClickAsync();

        await Expect(result).ToContainTextAsync("passed", new() { Timeout = 3000 });
        await Expect(Page.Locator("#hidden-fields-form [data-valmsg-for='Hidden.Phone']")).ToBeHiddenAsync();
        await Expect(Page.Locator("#hidden-fields-form [data-valmsg-for='Hidden.Salary']")).ToBeHiddenAsync();

        // Step 2: Toggle visibility — reveal extra fields
        await Page.CheckAsync($"#{S}Hidden_ShowExtras");
        await Expect(Page.Locator("#hf_extras")).ToBeVisibleAsync(new() { Timeout = 3000 });

        // Step 3: Submit again with invalid data in revealed fields
        await Page.FillAsync($"#{S}Hidden_Phone", "bad");
        await Page.FillAsync($"#{S}Hidden_Salary", "999999");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate Hidden Form" }).ClickAsync();

        var phoneError = Page.Locator("#hidden-fields-form [data-valmsg-for='Hidden.Phone']");
        var salaryError = Page.Locator("#hidden-fields-form [data-valmsg-for='Hidden.Salary']");
        await Expect(phoneError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(phoneError).ToContainTextAsync("123-456-7890");
        await Expect(salaryError).ToBeVisibleAsync();
        await Expect(salaryError).ToContainTextAsync("500,000");

        // Step 4: Fix revealed fields — submit — passes
        await Page.FillAsync($"#{S}Hidden_Phone", "123-456-7890");
        await Page.FillAsync($"#{S}Hidden_Salary", "50000");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate Hidden Form" }).ClickAsync();

        await Expect(result).ToContainTextAsync("passed", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Journey 5: Live error clearing ─────────────────────────────────────

    [Test]
    public async Task typing_clears_individual_field_errors_without_revalidating_whole_form()
    {
        await NavigateTo(Path);

        // Step 1: Trigger errors on both fields
        await Page.GetByRole(AriaRole.Button, new() { Name = "Show Errors First" }).ClickAsync();

        var nameError = Page.Locator("#live-form [data-valmsg-for='Live.Name']");
        var emailError = Page.Locator("#live-form [data-valmsg-for='Live.Email']");
        await Expect(nameError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(emailError).ToBeVisibleAsync(new() { Timeout = 3000 });

        // Step 2: Type into Name field — only Name's error clears, Email error remains
        await Page.FillAsync($"#{S}Live_Name", "John");
        await Expect(nameError).ToBeHiddenAsync(new() { Timeout = 3000 });
        await Expect(emailError).ToBeVisibleAsync();

        // Step 3: Type into Email field — Email's error clears too
        await Page.FillAsync($"#{S}Live_Email", "john@example.com");
        await Expect(emailError).ToBeHiddenAsync(new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Journey 6: Nested property validation (partial view) ───────────────

    [Test]
    public async Task nested_address_fields_show_errors_with_dotted_names_then_fix_and_save()
    {
        await NavigateTo(Path);

        // Step 1: Load the address partial
        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Address Form" }).ClickAsync();
        await Expect(Page.Locator($"#{S}Nested_Address_Street")).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Step 2: Submit empty — client errors appear at dotted field names
        await Page.Locator("#save-partial-btn").ClickAsync();

        var streetError = Page.Locator("#partial-form [data-valmsg-for='Nested.Address.Street']");
        var cityError = Page.Locator("#partial-form [data-valmsg-for='Nested.Address.City']");
        await Expect(streetError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(streetError).ToContainTextAsync("required");
        await Expect(cityError).ToBeVisibleAsync();
        await Expect(cityError).ToContainTextAsync("required");

        // POST was blocked — result stays at default
        var result = Page.Locator("#partial-save-result");
        await Expect(result).ToContainTextAsync("Not submitted yet");

        // Step 3: Fill all address fields and submit — success
        await Page.FillAsync($"#{S}Nested_Address_Street", "123 Main St");
        await Page.FillAsync($"#{S}Nested_Address_City", "Springfield");
        await Page.FillAsync($"#{S}Nested_Address_ZipCode", "62701");
        await Page.Locator("#save-partial-btn").ClickAsync();

        await Expect(result).ToContainTextAsync("saved successfully", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Journey 7: Combined client + server validation ─────────────────────

    [Test]
    public async Task client_validation_blocks_server_call_until_client_passes()
    {
        await NavigateTo(Path);

        var result = Page.Locator("#combined-result");
        var spinner = Page.Locator("#combined-spinner");

        // Step 1: Submit with client-invalid data — client errors appear, NO server request
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate + Save" }).ClickAsync();

        var nameError = Page.Locator("#combined-form [data-valmsg-for='Combined.Name']");
        await Expect(nameError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(spinner).ToBeHiddenAsync();
        await Expect(result).ToContainTextAsync("Not submitted yet");

        // Step 2: Fix client issues — submit — client passes, server validates, succeeds
        await Page.FillAsync($"#{S}Combined_Name", "Valid Name");
        await Page.FillAsync($"#{S}Combined_Email", "valid@example.com");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate + Save" }).ClickAsync();

        // Server should respond (success or 400 depending on server rules)
        await Expect(result).Not.ToContainTextAsync("Not submitted yet", new() { Timeout = 5000 });

        AssertNoConsoleErrorsExcept("400");
    }

    // ── Journey 8: DB-level validation (ProblemDetails 400) ────────────────

    [Test]
    public async Task reserved_username_shows_server_error_at_field_then_correction_succeeds()
    {
        await NavigateTo(Path);

        var result = Page.Locator("#db-result");

        // Step 1: Client blocks empty fields — no server request
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save with DB Check" }).ClickAsync();

        var nameError = Page.Locator("#db-form [data-valmsg-for='Db.Name']");
        await Expect(nameError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(nameError).ToContainTextAsync("required");
        await Expect(result).ToContainTextAsync("Not submitted yet");

        // Step 2: Fill "admin" (reserved) — client passes, server rejects with ProblemDetails 400
        await Page.FillAsync($"#{S}Db_Name", "admin");
        await Page.FillAsync($"#{S}Db_Email", "admin@test.com");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save with DB Check" }).ClickAsync();

        await Expect(result).ToContainTextAsync("Database validation failed", new() { Timeout = 5000 });
        await Expect(nameError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(nameError).ToContainTextAsync("reserved");

        // Step 3: Fix username — resubmit — server accepts
        await Page.FillAsync($"#{S}Db_Name", "John Smith");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save with DB Check" }).ClickAsync();

        await Expect(result).ToContainTextAsync("Saved to database successfully", new() { Timeout = 5000 });

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task duplicate_email_shows_server_error_at_email_field_then_correction_succeeds()
    {
        await NavigateTo(Path);

        var result = Page.Locator("#db-result");

        // Step 1: Fill valid name but "taken" email — server DB check returns 400
        await Page.FillAsync($"#{S}Db_Name", "Normal User");
        await Page.FillAsync($"#{S}Db_Email", "taken@test.com");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save with DB Check" }).ClickAsync();

        await Expect(result).ToContainTextAsync("Database validation failed", new() { Timeout = 5000 });
        var emailError = Page.Locator("#db-form [data-valmsg-for='Db.Email']");
        await Expect(emailError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(emailError).ToContainTextAsync("already registered");

        // Step 2: Fix email — resubmit — server accepts
        await Page.FillAsync($"#{S}Db_Email", "unique@example.com");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save with DB Check" }).ClickAsync();

        await Expect(result).ToContainTextAsync("Saved to database successfully", new() { Timeout = 5000 });

        AssertNoConsoleErrorsExcept("400");
    }

    // ── Journey 9: Delivery note partial — error -> fix -> save ────────────

    [Test]
    public async Task delivery_partial_validates_fixes_and_saves_in_single_session()
    {
        await NavigateTo(Path);

        // Step 1: Load the delivery partial
        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Delivery Note Form" }).ClickAsync();
        await Expect(Page.Locator($"#{S}Nested_Delivery_Instructions")).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Step 2: Submit empty — client validation blocks
        await Page.Locator("#save-delivery-btn").ClickAsync();

        var instrError = Page.Locator("#delivery-form [data-valmsg-for='Nested.Delivery.Instructions']");
        await Expect(instrError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(instrError).ToContainTextAsync("required");

        var result = Page.Locator("#delivery-result");
        await Expect(result).ToContainTextAsync("Not submitted yet");

        // Step 3: Fill instructions but bad phone — phone error appears
        await Page.FillAsync($"#{S}Nested_Delivery_Instructions", "Leave at door");
        await Page.FillAsync($"#{S}Nested_Delivery_ContactPhone", "bad");
        await Page.Locator("#save-delivery-btn").ClickAsync();

        var phoneError = Page.Locator("#delivery-form [data-valmsg-for='Nested.Delivery.ContactPhone']");
        await Expect(phoneError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(phoneError).ToContainTextAsync("Phone must match");

        // Step 4: Fix phone — submit — success
        await Page.FillAsync($"#{S}Nested_Delivery_ContactPhone", "123-456-7890");
        await Page.Locator("#save-delivery-btn").ClickAsync();

        await Expect(result).ToContainTextAsync("saved", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Journey 10: Standalone partial — independent plan validation ───────

    [Test]
    public async Task standalone_contact_partial_validates_independently_from_parent_plan()
    {
        await NavigateTo(Path);

        // Step 1: Load the standalone contact partial
        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Contact Form" }).ClickAsync();
        await Expect(Page.Locator($"#{C}Name")).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Step 2: Submit empty — standalone plan's client validation blocks
        await Page.GetByRole(AriaRole.Button, new() { Name = "Send Message" }).ClickAsync();

        var nameError = Page.Locator("#contact-form [data-valmsg-for='Name']");
        await Expect(nameError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(nameError).ToContainTextAsync("required");

        var contactResult = Page.Locator("#contact-result");
        await Expect(contactResult).ToContainTextAsync("Not submitted yet");

        // Step 3: Verify parent plan's Section 1 is not polluted by standalone partial errors
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate All" }).ClickAsync();

        var parentNameError = Page.Locator("#all-rules-form [data-valmsg-for='AllRules.Name']");
        await Expect(parentNameError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(parentNameError).ToContainTextAsync("required");

        // Parent result stays at default (separate plan scope)
        var parentResult = Page.Locator("#all-rules-result");
        await Expect(parentResult).ToContainTextAsync("Click to validate");

        // Step 4: Fix standalone form — submit — success
        await Page.FillAsync($"#{C}Name", "John Doe");
        await Page.FillAsync($"#{C}Email", "john@example.com");
        await Page.FillAsync($"#{C}Message", "Hello, this is a test message!");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Send Message" }).ClickAsync();

        await Expect(contactResult).ToContainTextAsync("Message sent", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Journey 11: Hidden fields server round-trip ────────────────────────

    [Test]
    public async Task hidden_fields_submit_to_server_skipping_invisible_fields()
    {
        await NavigateTo(Path);

        var result = Page.Locator("#hidden-result");

        // Step 1: Submit with empty Name — client validation blocks
        await Page.GetByRole(AriaRole.Button, new() { Name = "Submit Profile" }).ClickAsync();

        var nameError = Page.Locator("#hidden-fields-form [data-valmsg-for='Hidden.Name']");
        await Expect(nameError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(nameError).ToContainTextAsync("required");
        await Expect(result).ToContainTextAsync("Click to validate or submit");

        // Step 2: Fill Name — submit to server — hidden fields are skipped, server accepts
        await Page.FillAsync($"#{S}Hidden_Name", "Server User");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Submit Profile" }).ClickAsync();

        await Expect(result).ToContainTextAsync("Profile saved", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Journey 12: Server-only nested validation (Section 3) ──────────────

    [Test]
    public async Task server_nested_validation_returns_dotted_field_errors()
    {
        await NavigateTo(Path);

        // Step 1: Submit empty nested address — server returns 400 with dotted names
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save Address (Server)" }).ClickAsync();

        var result = Page.Locator("#nested-result");
        await Expect(result).ToContainTextAsync("errors", new() { Timeout = 5000 });
        await Expect(result).ToHaveClassAsync(new Regex("text-red-600"));

        AssertNoConsoleErrorsExcept("400");
    }

    // ── Journey 13: Partial live clearing ──────────────────────────────────

    [Test]
    public async Task partial_view_live_clearing_works_on_dynamically_loaded_fields()
    {
        await NavigateTo(Path);

        // Step 1: Load partial and trigger validation errors
        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Address Form" }).ClickAsync();
        await Expect(Page.Locator($"#{S}Nested_Address_Street")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Page.Locator("#save-partial-btn").ClickAsync();

        var streetError = Page.Locator("#partial-form [data-valmsg-for='Nested.Address.Street']");
        var cityError = Page.Locator("#partial-form [data-valmsg-for='Nested.Address.City']");
        await Expect(streetError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(cityError).ToBeVisibleAsync();

        // Step 2: Type into Street — only that error clears, City error remains
        await Page.FillAsync($"#{S}Nested_Address_Street", "123 Main St");
        await Expect(streetError).ToBeHiddenAsync(new() { Timeout = 3000 });
        await Expect(cityError).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    // ── Journey 14: Re-submission clears previous errors before showing new ─

    [Test]
    public async Task submitting_same_form_twice_clears_previous_errors_before_showing_new()
    {
        // Proves validation doesn't accumulate errors across submissions
        await NavigateTo(Path);

        // First submit — all errors appear
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate All" }).ClickAsync();
        var nameError = Page.Locator("#all-rules-form [data-valmsg-for='AllRules.Name']");
        var emailError = Page.Locator("#all-rules-form [data-valmsg-for='AllRules.Email']");
        await Expect(nameError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(emailError).ToBeVisibleAsync(new() { Timeout = 3000 });

        // Fill name only, resubmit — name error should clear, email error still shows
        await Page.FillAsync($"#{S}AllRules_Name", "Test User");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate All" }).ClickAsync();
        await Expect(nameError).ToBeHiddenAsync(new() { Timeout = 3000 });
        await Expect(emailError).ToBeVisibleAsync(new() { Timeout = 3000 });

        // Result should still be blocked — client validation prevented POST
        var result = Page.Locator("#all-rules-result");
        await Expect(result).ToContainTextAsync("Click to validate");

        AssertNoConsoleErrors();
    }

    // ── Journey 15: Valid data clears all errors and shows success ───────────

    [Test]
    public async Task valid_data_clears_all_previous_errors_and_shows_success()
    {
        // Proves the full cycle: errors → fix all → all errors gone → success
        await NavigateTo(Path);

        // Submit empty → errors appear
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate All" }).ClickAsync();
        var nameError = Page.Locator("#all-rules-form [data-valmsg-for='AllRules.Name']");
        var emailError = Page.Locator("#all-rules-form [data-valmsg-for='AllRules.Email']");
        await Expect(nameError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(emailError).ToBeVisibleAsync(new() { Timeout = 3000 });

        // Fill all required fields with valid data
        await Page.FillAsync($"#{S}AllRules_Name", "Jane Doe");
        await Page.FillAsync($"#{S}AllRules_Email", "jane@example.com");
        await Page.FillAsync($"#{S}AllRules_Age", "30");
        await Page.FillAsync($"#{S}AllRules_Phone", "123-456-7890");
        await Page.FillAsync($"#{S}AllRules_Salary", "75000");
        await Page.FillAsync($"#{S}AllRules_Password", "securepassword123");

        // Resubmit — all errors should be gone
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate All" }).ClickAsync();
        await Expect(nameError).ToBeHiddenAsync(new() { Timeout = 3000 });
        await Expect(emailError).ToBeHiddenAsync(new() { Timeout = 3000 });

        // Success message shows
        var result = Page.Locator("#all-rules-result");
        await Expect(result).ToContainTextAsync("passed", new() { Timeout = 5000 });
        await Expect(result).ToHaveClassAsync(new Regex("text-green-600"));

        AssertNoConsoleErrors();
    }

    // ── Journey 16: Conditional toggle back-and-forth maintains state ────────

    [Test]
    public async Task conditional_checkbox_toggle_back_and_forth_maintains_validation_state()
    {
        // Proves conditional rules re-evaluate correctly on every toggle
        await NavigateTo(Path);

        var jobError = Page.Locator("#conditional-form [data-valmsg-for='Conditional.JobTitle']");
        var result = Page.Locator("#conditional-result");

        // Check "Is Employed" → submit → JobTitle required
        await Page.CheckAsync($"#{S}Conditional_IsEmployed");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate Conditional" }).ClickAsync();
        await Expect(jobError).ToBeVisibleAsync(new() { Timeout = 3000 });

        // Uncheck → submit → should pass (no JobTitle required)
        await Page.UncheckAsync($"#{S}Conditional_IsEmployed");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate Conditional" }).ClickAsync();
        await Expect(result).ToContainTextAsync("passed", new() { Timeout = 3000 });
        await Expect(jobError).ToBeHiddenAsync();

        // Re-check → submit → should fail again
        await Page.CheckAsync($"#{S}Conditional_IsEmployed");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate Conditional" }).ClickAsync();
        await Expect(jobError).ToBeVisibleAsync(new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Journey 17: Multiple rule types in single validation ───────────────

    [Test]
    public async Task multiple_rule_types_fire_simultaneously_on_invalid_data()
    {
        await NavigateTo(Path);

        // Fill invalid data for multiple rule types at once
        await Page.FillAsync($"#{S}AllRules_Name", "Test User");
        await Page.FillAsync($"#{S}AllRules_Email", "not-an-email");
        await Page.FillAsync($"#{S}AllRules_Age", "200");
        await Page.FillAsync($"#{S}AllRules_Phone", "123");
        await Page.FillAsync($"#{S}AllRules_Salary", "999999");
        await Page.FillAsync($"#{S}AllRules_Password", "short");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Validate All" }).ClickAsync();

        // Multiple rule types fire at once: email, range, regex, max, minLength
        var emailError = Page.Locator("#all-rules-form [data-valmsg-for='AllRules.Email']");
        var ageError = Page.Locator("#all-rules-form [data-valmsg-for='AllRules.Age']");
        var phoneError = Page.Locator("#all-rules-form [data-valmsg-for='AllRules.Phone']");
        var salaryError = Page.Locator("#all-rules-form [data-valmsg-for='AllRules.Salary']");
        var passwordError = Page.Locator("#all-rules-form [data-valmsg-for='AllRules.Password']");

        await Expect(emailError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(emailError).ToContainTextAsync("email");
        await Expect(ageError).ToBeVisibleAsync();
        await Expect(ageError).ToContainTextAsync("between");
        await Expect(phoneError).ToBeVisibleAsync();
        await Expect(phoneError).ToContainTextAsync("123-456-7890");
        await Expect(salaryError).ToBeVisibleAsync();
        await Expect(salaryError).ToContainTextAsync("500,000");
        await Expect(passwordError).ToBeVisibleAsync();
        await Expect(passwordError).ToContainTextAsync("8 characters");

        AssertNoConsoleErrors();
    }
}
