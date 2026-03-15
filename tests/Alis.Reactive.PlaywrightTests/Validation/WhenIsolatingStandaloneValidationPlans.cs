namespace Alis.Reactive.PlaywrightTests.Validation;

[TestFixture]
public class WhenIsolatingStandaloneValidationPlans : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/StandaloneIsolation";
    private const string S = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ValidationMergeHarnessModel__";
    private const string C = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ContactFormModel__";

    private async Task LoadBothPartials()
    {
        await NavigateTo(Path);

        await Page.Locator("#isolation-load-address-btn").ClickAsync();
        await Page.Locator("#isolation-load-contact-btn").ClickAsync();

        await Expect(Page.Locator($"#{S}Nested_Address_Street")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator($"#{C}Name")).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    // ── Scenario 1: Standalone partial validation does not pollute parent form ─

    [Test]
    public async Task standalone_partial_validation_does_not_pollute_parent_form()
    {
        // WHY: proves ResolvePlan() creates isolated validation surface —
        // submitting the standalone contact form empty triggers errors ONLY in
        // the contact form, not the parent form. Then submitting the parent
        // form validates independently with its own rules.

        await LoadBothPartials();

        // Step 1: Submit the standalone contact form empty — errors appear ONLY in contact form
        await Page.Locator("#isolation-send-contact-btn").ClickAsync();

        await Expect(Page.Locator("#isolation-contact-form [data-valmsg-for='Name']"))
            .ToContainTextAsync("Name is required.", new() { Timeout = 3000 });
        await Expect(Page.Locator("#isolation-contact-form [data-valmsg-for='Message']"))
            .ToContainTextAsync("Message is required.", new() { Timeout = 3000 });

        // Parent form validation messages must be empty — no crosstalk
        await Expect(Page.Locator("#isolation-parent-form [data-valmsg-for='Root.Name']"))
            .ToBeEmptyAsync();
        await Expect(Page.Locator("#isolation-parent-form [data-valmsg-for='Nested.Address.Street']"))
            .ToBeEmptyAsync();

        // Step 2: Submit the parent form empty — parent validates independently
        await Page.Locator("#isolation-save-parent-btn").ClickAsync();

        await Expect(Page.Locator("#isolation-parent-form [data-valmsg-for='Root.Name']"))
            .ToContainTextAsync("Name is required.", new() { Timeout = 3000 });
        await Expect(Page.Locator("#isolation-parent-form [data-valmsg-for='Nested.Address.Street']"))
            .ToContainTextAsync("Street is required.", new() { Timeout = 3000 });

        // Contact form errors remain scoped to contact — still showing from earlier
        await Expect(Page.Locator("#isolation-contact-form [data-valmsg-for='Name']"))
            .ToContainTextAsync("Name is required.");

        AssertNoConsoleErrors();
    }

    // ── Scenario 2: Parent and standalone both validate independently on same page ─

    [Test]
    public async Task parent_and_standalone_both_validate_independently_on_same_page()
    {
        // WHY: proves two plans (parent ReactivePlan + standalone ReactivePlan)
        // coexist without crosstalk — filling parent with valid data and
        // submitting succeeds, then filling standalone with invalid data
        // and submitting only shows standalone errors.

        await LoadBothPartials();

        // Step 1: Fill parent form with valid data and submit — success
        await Page.FillAsync($"#{S}Root_Name", "Parent User");
        await Page.FillAsync($"#{S}Root_Email", "parent@example.com");
        await Page.FillAsync($"#{S}Nested_Address_Street", "500 Market Street");
        await Page.FillAsync($"#{S}Nested_Address_City", "Philadelphia");
        await Page.FillAsync($"#{S}Nested_Address_ZipCode", "19106");

        await Page.Locator("#isolation-save-parent-btn").ClickAsync();

        await Expect(Page.Locator("#isolation-parent-result"))
            .ToContainTextAsync("Parent root saved", new() { Timeout = 5000 });

        // Step 2: Submit standalone contact form empty — only standalone errors
        await Page.Locator("#isolation-send-contact-btn").ClickAsync();

        await Expect(Page.Locator("#isolation-contact-form [data-valmsg-for='Name']"))
            .ToContainTextAsync("Name is required.", new() { Timeout = 3000 });
        await Expect(Page.Locator("#isolation-contact-form [data-valmsg-for='Message']"))
            .ToContainTextAsync("Message is required.", new() { Timeout = 3000 });

        // Parent result stays at success — was NOT invalidated by standalone submit
        await Expect(Page.Locator("#isolation-parent-result"))
            .ToContainTextAsync("Parent root saved");

        // Step 3: Fix standalone and submit — standalone succeeds too
        await Page.FillAsync($"#{C}Name", "Contact User");
        await Page.FillAsync($"#{C}Email", "contact@example.com");
        await Page.FillAsync($"#{C}Message", "This message is long enough.");

        await Page.Locator("#isolation-send-contact-btn").ClickAsync();

        await Expect(Page.Locator("#isolation-contact-result"))
            .ToContainTextAsync("Standalone contact sent", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario 3: Bidirectional isolation — errors never cross plan boundaries ──

    [Test]
    public async Task standalone_errors_do_not_appear_in_parent_form_and_vice_versa()
    {
        // WHY: proves complete bidirectional isolation — submitting the parent
        // form empty shows errors ONLY inside the parent form area, never in
        // the standalone contact area. Then submitting the standalone contact
        // form empty shows errors ONLY inside the contact area, never in the
        // parent area. Neither plan's error state leaks into the other.

        await LoadBothPartials();

        // Step 1: Submit parent form empty — parent errors appear
        await Page.Locator("#isolation-save-parent-btn").ClickAsync();

        await Expect(Page.Locator("#isolation-parent-form [data-valmsg-for='Root.Name']"))
            .ToContainTextAsync("Name is required.", new() { Timeout = 3000 });
        await Expect(Page.Locator("#isolation-parent-form [data-valmsg-for='Nested.Address.Street']"))
            .ToContainTextAsync("Street is required.", new() { Timeout = 3000 });

        // Standalone contact form must be completely clean — no parent errors leaked
        await Expect(Page.Locator("#isolation-contact-form [data-valmsg-for='Name']"))
            .ToBeEmptyAsync();
        await Expect(Page.Locator("#isolation-contact-form [data-valmsg-for='Message']"))
            .ToBeEmptyAsync();
        await Expect(Page.Locator("#isolation-contact-result"))
            .ToContainTextAsync("Not submitted yet");

        // Step 2: Submit standalone contact form empty — contact errors appear
        await Page.Locator("#isolation-send-contact-btn").ClickAsync();

        await Expect(Page.Locator("#isolation-contact-form [data-valmsg-for='Name']"))
            .ToContainTextAsync("Name is required.", new() { Timeout = 3000 });
        await Expect(Page.Locator("#isolation-contact-form [data-valmsg-for='Message']"))
            .ToContainTextAsync("Message is required.", new() { Timeout = 3000 });

        // Parent form errors must still be scoped to parent — not cleared or changed
        await Expect(Page.Locator("#isolation-parent-form [data-valmsg-for='Root.Name']"))
            .ToContainTextAsync("Name is required.");
        await Expect(Page.Locator("#isolation-parent-form [data-valmsg-for='Nested.Address.Street']"))
            .ToContainTextAsync("Street is required.");

        // Parent result was NOT affected by the standalone contact submission
        await Expect(Page.Locator("#isolation-parent-result"))
            .Not.ToContainTextAsync("Standalone contact");

        AssertNoConsoleErrors();
    }
}
