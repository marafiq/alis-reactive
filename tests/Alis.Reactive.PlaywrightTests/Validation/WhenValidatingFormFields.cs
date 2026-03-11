namespace Alis.Reactive.PlaywrightTests.Validation;

[TestFixture]
public class WhenValidatingFormFields : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation";
    private const string S = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ValidationShowcaseModel__";

    [Test]
    public async Task PageLoadsWithNoErrors()
    {
        await NavigateTo(Path);
        await Expect(Page).ToHaveTitleAsync("Validation — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    // ── Section 1: Client-Side Rule Types ────────────────────

    [Test]
    public async Task RequiredFieldShowsError()
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
    public async Task EmailFieldShowsError()
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
    public async Task RegexFieldShowsError()
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
    public async Task RangeFieldShowsError()
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
    public async Task ServerErrorsDisplayAtFields()
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
    public async Task NestedFieldErrorsDisplay()
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
    public async Task ConditionalRuleAppliesWhenMet()
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
    public async Task ConditionalRuleSkipsWhenNotMet()
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
    public async Task TypingClearsFieldError()
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
    public async Task ClientValidationBlocksSubmit()
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
    public async Task CombinedFlowPassesClientThenHitsServer()
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
    public async Task HiddenFieldsSkippedDuringValidation()
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
    public async Task RevealedFieldsValidateAfterToggle()
    {
        await NavigateTo(Path);

        // Fill Name, check toggle to reveal extra fields
        await Page.FillAsync($"#{S}Hidden_Name", "Visible User");
        await Page.CheckAsync("#hf_toggle");

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
    public async Task HiddenFieldsSubmitToServerSuccessfully()
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
    public async Task HiddenFieldsClientBlocksEmptyName()
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
    public async Task DbValidationRejectsDuplicateEmail()
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
    public async Task DbValidationRejectsReservedUsername()
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
    public async Task DbValidationPassesWithCleanData()
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
    public async Task DbValidationClientBlocksEmptyFields()
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

    // ── Section 9: Partial View + Server Validation ───────────

    [Test]
    public async Task PartialViewLoadsAddressForm()
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
    public async Task PartialServerValidationShowsErrors()
    {
        await NavigateTo(Path);

        // Load the partial first
        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Address Form" }).ClickAsync();
        await Expect(Page.Locator($"#{S}Nested_Address_Street")).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Click "Save Address" with empty fields → 400
        await Page.Locator("#save-partial-btn").ClickAsync();

        // Result should indicate server errors
        var result = Page.Locator("#partial-save-result");
        await Expect(result).ToContainTextAsync("validation errors", new() { Timeout = 5000 });

        // Error spans in the partial should be visible (dotted names → matched by data-valmsg-for)
        var streetError = Page.Locator("#partial-form [data-valmsg-for='Nested.Address.Street']");
        await Expect(streetError).ToBeVisibleAsync(new() { Timeout = 3000 });
        await Expect(streetError).ToContainTextAsync("required");

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task PartialValidDataSavesSuccessfully()
    {
        await NavigateTo(Path);

        // Load the partial
        await Page.GetByRole(AriaRole.Button, new() { Name = "Load Address Form" }).ClickAsync();
        await Expect(Page.Locator($"#{S}Nested_Address_Street")).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Fill valid address data
        await Page.FillAsync($"#{S}Nested_Address_Street", "123 Main St");
        await Page.FillAsync($"#{S}Nested_Address_City", "Springfield");
        await Page.FillAsync($"#{S}Nested_Address_ZipCode", "62701");

        // Click "Save Address" → 200
        await Page.Locator("#save-partial-btn").ClickAsync();

        var result = Page.Locator("#partial-save-result");
        await Expect(result).ToContainTextAsync("saved successfully", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
