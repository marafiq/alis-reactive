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
}
