namespace Alis.Reactive.PlaywrightTests.Validation;

[TestFixture]
public class WhenMultipleValidationPartialsInteract : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/MultiPartialWorkflow";
    private const string S = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ValidationMergeHarnessModel__";
    private const string C = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ContactFormModel__";

    private async Task LoadAllThreePartials()
    {
        await NavigateTo(Path);

        await Page.Locator("#workflow-load-address-btn").ClickAsync();
        await Page.Locator("#workflow-load-delivery-btn").ClickAsync();
        await Page.Locator("#workflow-load-contact-btn").ClickAsync();

        await Expect(Page.Locator($"#{S}Nested_Address_Street")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator($"#{S}Nested_Delivery_Instructions")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator($"#{C}Name")).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    // ── Scenario: Loading all partials produces zero console errors ──

    [Test]
    public async Task loading_all_partials_does_not_produce_console_errors()
    {
        // WHY: proves that merging three partial plans (two same-model + one
        // standalone) into the page at runtime does not cause any JS errors.
        // Plan merge, component registration, and validation wiring must all
        // complete cleanly — a single console error here means the merge
        // pipeline has a structural defect.

        await LoadAllThreePartials();

        // All three status labels must reflect successful load
        await Expect(Page.Locator("#workflow-address-status"))
            .ToContainTextAsync("Address partial loaded", new() { Timeout = 3000 });
        await Expect(Page.Locator("#workflow-delivery-status"))
            .ToContainTextAsync("Delivery partial loaded", new() { Timeout = 3000 });
        await Expect(Page.Locator("#workflow-contact-status"))
            .ToContainTextAsync("Standalone contact loaded", new() { Timeout = 3000 });

        // Neither form should show any result text yet — no submissions happened
        await Expect(Page.Locator("#workflow-root-result"))
            .ToContainTextAsync("Not submitted yet");
        await Expect(Page.Locator("#workflow-contact-result"))
            .ToContainTextAsync("Not submitted yet");

        // The critical assertion: zero console errors after all three partial loads
        AssertNoConsoleErrors();
    }

    // ── Scenario: Root + same-model + standalone all coexist without interference ─

    [Test]
    public async Task root_plus_same_model_plus_standalone_all_coexist()
    {
        // WHY: proves three validation surfaces (root with merged same-model
        // partials, and standalone contact) don't interfere — submitting the root
        // form only validates root + merged partials, submitting the contact form
        // only validates contact. Then both can succeed independently.

        await LoadAllThreePartials();

        // Step 1: Submit root form empty — root + merged partials validate,
        // standalone contact stays clean
        await Page.Locator("#workflow-save-root-btn").ClickAsync();

        await Expect(Page.Locator("#workflow-root-form [data-valmsg-for='Root.Name']"))
            .ToContainTextAsync("Name is required.", new() { Timeout = 3000 });
        await Expect(Page.Locator("#workflow-root-form [data-valmsg-for='Nested.Address.Street']"))
            .ToContainTextAsync("Street is required.", new() { Timeout = 3000 });
        await Expect(Page.Locator("#workflow-root-form [data-valmsg-for='Nested.Delivery.Instructions']"))
            .ToContainTextAsync("Delivery instructions are required.", new() { Timeout = 3000 });

        // Standalone contact form must be empty — no crosstalk from root submit
        await Expect(Page.Locator("#workflow-contact-form [data-valmsg-for='Name']"))
            .ToBeEmptyAsync();

        // Step 2: Submit standalone contact form empty — only contact validates,
        // root errors remain scoped to root
        await Page.Locator("#workflow-send-contact-btn").ClickAsync();

        await Expect(Page.Locator("#workflow-contact-form [data-valmsg-for='Name']"))
            .ToContainTextAsync("Name is required.", new() { Timeout = 3000 });

        // Root errors remain — they don't get cleared by contact submission
        await Expect(Page.Locator("#workflow-root-form [data-valmsg-for='Root.Name']"))
            .ToContainTextAsync("Name is required.");

        // Step 3: Fix root form and submit — root succeeds
        await Page.FillAsync($"#{S}Root_Name", "Workflow User");
        await Page.FillAsync($"#{S}Root_Email", "workflow@example.com");
        await Page.Locator($"#{S}Root_Amount").ClickAsync();
        await Page.Locator($"#{S}Root_Amount").FillAsync("300");
        await Page.Locator($"#{S}Root_Amount").PressAsync("Tab");
        await Page.FillAsync($"#{S}Nested_Address_Street", "42 Wallaby Way");
        await Page.FillAsync($"#{S}Nested_Address_City", "Sydney");
        await Page.FillAsync($"#{S}Nested_Address_ZipCode", "20000");
        await Page.FillAsync($"#{S}Nested_Delivery_Instructions", "Ring the bell twice");
        await Page.FillAsync($"#{S}Nested_Delivery_ContactPhone", "123-456-7890");

        await Page.Locator("#workflow-save-root-btn").ClickAsync();

        await Expect(Page.Locator("#workflow-root-result"))
            .ToContainTextAsync("Workflow root saved", new() { Timeout = 5000 });

        // Step 4: Fix standalone contact and submit — contact succeeds independently
        await Page.FillAsync($"#{C}Name", "Workflow Contact");
        await Page.FillAsync($"#{C}Email", "workflow.contact@example.com");
        await Page.FillAsync($"#{C}Message", "Need delivery confirmation tomorrow.");

        await Page.Locator("#workflow-send-contact-btn").ClickAsync();

        await Expect(Page.Locator("#workflow-contact-result"))
            .ToContainTextAsync("Workflow contact sent", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario 2: Fixing root errors does not affect standalone error state ──

    [Test]
    public async Task fixing_root_errors_while_standalone_has_errors_does_not_clear_standalone()
    {
        // WHY: proves fixing one plan's errors and resubmitting does NOT affect
        // another plan's error state — root and standalone are completely independent
        // validation surfaces. Fixing root leaves standalone errors intact.

        await LoadAllThreePartials();

        // Step 1: Submit root form empty — root errors appear
        await Page.Locator("#workflow-save-root-btn").ClickAsync();

        await Expect(Page.Locator("#workflow-root-form [data-valmsg-for='Root.Name']"))
            .ToContainTextAsync("Name is required.", new() { Timeout = 3000 });
        await Expect(Page.Locator("#workflow-root-form [data-valmsg-for='Nested.Address.Street']"))
            .ToContainTextAsync("Street is required.", new() { Timeout = 3000 });
        await Expect(Page.Locator("#workflow-root-form [data-valmsg-for='Nested.Delivery.Instructions']"))
            .ToContainTextAsync("Delivery instructions are required.", new() { Timeout = 3000 });

        // Step 2: Submit standalone contact form empty — contact errors appear
        await Page.Locator("#workflow-send-contact-btn").ClickAsync();

        await Expect(Page.Locator("#workflow-contact-form [data-valmsg-for='Name']"))
            .ToContainTextAsync("Name is required.", new() { Timeout = 3000 });
        await Expect(Page.Locator("#workflow-contact-form [data-valmsg-for='Message']"))
            .ToContainTextAsync("Message is required.", new() { Timeout = 3000 });

        // Step 3: Fix ALL root fields and resubmit root — root succeeds
        await Page.FillAsync($"#{S}Root_Name", "Fixed User");
        await Page.FillAsync($"#{S}Root_Email", "fixed@example.com");
        await Page.Locator($"#{S}Root_Amount").ClickAsync();
        await Page.Locator($"#{S}Root_Amount").FillAsync("500");
        await Page.Locator($"#{S}Root_Amount").PressAsync("Tab");
        await Page.FillAsync($"#{S}Nested_Address_Street", "789 Elm Boulevard");
        await Page.FillAsync($"#{S}Nested_Address_City", "Boston");
        await Page.FillAsync($"#{S}Nested_Address_ZipCode", "02101");
        await Page.FillAsync($"#{S}Nested_Delivery_Instructions", "Use the side entrance");
        await Page.FillAsync($"#{S}Nested_Delivery_ContactPhone", "617-555-0199");

        await Page.Locator("#workflow-save-root-btn").ClickAsync();

        await Expect(Page.Locator("#workflow-root-result"))
            .ToContainTextAsync("Workflow root saved", new() { Timeout = 5000 });

        // Step 4: Standalone contact errors must STILL be visible — root success
        // did NOT clear standalone's error state
        await Expect(Page.Locator("#workflow-contact-form [data-valmsg-for='Name']"))
            .ToContainTextAsync("Name is required.");
        await Expect(Page.Locator("#workflow-contact-form [data-valmsg-for='Message']"))
            .ToContainTextAsync("Message is required.");

        // Contact result was NOT affected by root's successful submission
        await Expect(Page.Locator("#workflow-contact-result"))
            .Not.ToContainTextAsync("Workflow root saved");

        AssertNoConsoleErrors();
    }
}
