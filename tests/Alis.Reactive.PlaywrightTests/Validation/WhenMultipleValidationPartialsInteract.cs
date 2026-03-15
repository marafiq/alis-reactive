namespace Alis.Reactive.PlaywrightTests.Validation;

[TestFixture]
public class WhenMultipleValidationPartialsInteract : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/MultiPartialWorkflow";
    private const string S = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ValidationMergeHarnessModel__";
    private const string C = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ContactFormModel__";

    [Test]
    public async Task RootWorkflowValidationStaysScopedToRootAndSameModelPartials()
    {
        await NavigateTo(Path);

        await Page.Locator("#workflow-load-address-btn").ClickAsync();
        await Page.Locator("#workflow-load-delivery-btn").ClickAsync();
        await Page.Locator("#workflow-load-contact-btn").ClickAsync();

        await Expect(Page.Locator($"#{S}Nested_Address_Street")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator($"#{S}Nested_Delivery_Instructions")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator($"#{C}Name")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Page.Locator("#workflow-save-root-btn").ClickAsync();

        await Expect(Page.Locator("#workflow-root-form [data-valmsg-for='Root.Name']"))
            .ToContainTextAsync("Name is required.", new() { Timeout = 3000 });
        await Expect(Page.Locator("#workflow-root-form [data-valmsg-for='Nested.Address.Street']"))
            .ToContainTextAsync("Street is required.", new() { Timeout = 3000 });
        await Expect(Page.Locator("#workflow-root-form [data-valmsg-for='Nested.Delivery.Instructions']"))
            .ToContainTextAsync("Delivery instructions are required.", new() { Timeout = 3000 });
        await Expect(Page.Locator("#workflow-contact-form [data-valmsg-for='Name']"))
            .ToBeEmptyAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task RootAndStandalonePlansCanBeCompletedInSequence()
    {
        await NavigateTo(Path);

        await Page.Locator("#workflow-load-address-btn").ClickAsync();
        await Page.Locator("#workflow-load-delivery-btn").ClickAsync();
        await Page.Locator("#workflow-load-contact-btn").ClickAsync();

        await Expect(Page.Locator($"#{S}Nested_Address_Street")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator($"#{S}Nested_Delivery_Instructions")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator($"#{C}Name")).ToBeVisibleAsync(new() { Timeout = 5000 });

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

        await Page.Locator("#workflow-send-contact-btn").ClickAsync();
        await Expect(Page.Locator("#workflow-contact-form [data-valmsg-for='Name']"))
            .ToContainTextAsync("Name is required.", new() { Timeout = 3000 });

        await Page.FillAsync($"#{C}Name", "Workflow Contact");
        await Page.FillAsync($"#{C}Email", "workflow.contact@example.com");
        await Page.FillAsync($"#{C}Message", "Need delivery confirmation tomorrow.");

        await Page.Locator("#workflow-send-contact-btn").ClickAsync();

        await Expect(Page.Locator("#workflow-contact-result"))
            .ToContainTextAsync("Workflow contact sent", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task ReloadingOneSameModelPartialKeepsTheOtherPartialAndStandalonePlanUsable()
    {
        await NavigateTo(Path);

        await Page.Locator("#workflow-load-address-btn").ClickAsync();
        await Page.Locator("#workflow-load-delivery-btn").ClickAsync();
        await Page.Locator("#workflow-load-contact-btn").ClickAsync();

        await Expect(Page.Locator("#workflow-address-version"))
            .ToContainTextAsync("Address partial v1", new() { Timeout = 5000 });

        await Page.Locator("#workflow-reload-address-btn").ClickAsync();

        await Expect(Page.Locator("#workflow-address-version"))
            .ToContainTextAsync("Address partial v2", new() { Timeout = 5000 });
        await Expect(Page.Locator("#workflow-delivery-status"))
            .ToContainTextAsync("Delivery partial loaded", new() { Timeout = 5000 });
        await Expect(Page.Locator("#workflow-contact-status"))
            .ToContainTextAsync("Standalone contact loaded", new() { Timeout = 5000 });

        await Page.FillAsync($"#{S}Root_Name", "Workflow Reload");
        await Page.FillAsync($"#{S}Root_Email", "reload@example.com");
        await Page.Locator($"#{S}Root_Amount").ClickAsync();
        await Page.Locator($"#{S}Root_Amount").FillAsync("250");
        await Page.Locator($"#{S}Root_Amount").PressAsync("Tab");
        await Page.FillAsync($"#{S}Nested_Address_Street", "77 King Street");
        await Page.FillAsync($"#{S}Nested_Address_City", "Toronto");
        await Page.FillAsync($"#{S}Nested_Address_ZipCode", "M5H2N2");
        await Page.FillAsync($"#{S}Nested_Delivery_Instructions", "Call on arrival");
        await Page.FillAsync($"#{S}Nested_Delivery_ContactPhone", "123-456-7890");

        await Page.Locator("#workflow-save-root-btn").ClickAsync();

        await Expect(Page.Locator("#workflow-root-result"))
            .ToContainTextAsync("Workflow root saved", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
