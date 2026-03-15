namespace Alis.Reactive.PlaywrightTests.Validation;

[TestFixture]
public class WhenIsolatingStandaloneValidationPlans : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/StandaloneIsolation";
    private const string S = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ValidationMergeHarnessModel__";
    private const string C = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ContactFormModel__";

    [Test]
    public async Task ParentSubmitTouchesOnlyTheParentLogicalPlan()
    {
        await NavigateTo(Path);

        await Page.Locator("#isolation-load-address-btn").ClickAsync();
        await Page.Locator("#isolation-load-contact-btn").ClickAsync();

        await Expect(Page.Locator($"#{S}Nested_Address_Street")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator($"#{C}Name")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Page.Locator("#isolation-save-parent-btn").ClickAsync();

        await Expect(Page.Locator("#isolation-parent-form [data-valmsg-for='Root.Name']"))
            .ToContainTextAsync("Name is required.", new() { Timeout = 3000 });
        await Expect(Page.Locator("#isolation-parent-form [data-valmsg-for='Nested.Address.Street']"))
            .ToContainTextAsync("Street is required.", new() { Timeout = 3000 });
        await Expect(Page.Locator("#isolation-contact-form [data-valmsg-for='Name']"))
            .ToBeEmptyAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task StandaloneContactSubmitDoesNotPolluteTheParentValidationSurface()
    {
        await NavigateTo(Path);

        await Page.Locator("#isolation-load-address-btn").ClickAsync();
        await Page.Locator("#isolation-load-contact-btn").ClickAsync();

        await Expect(Page.Locator($"#{C}Name")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Page.Locator("#isolation-send-contact-btn").ClickAsync();

        await Expect(Page.Locator("#isolation-contact-form [data-valmsg-for='Name']"))
            .ToContainTextAsync("Name is required.", new() { Timeout = 3000 });
        await Expect(Page.Locator("#isolation-contact-form [data-valmsg-for='Message']"))
            .ToContainTextAsync("Message is required.", new() { Timeout = 3000 });
        await Expect(Page.Locator("#isolation-parent-form [data-valmsg-for='Root.Name']"))
            .ToBeEmptyAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task BothPlansCanSucceedIndependentlyOnTheSamePage()
    {
        await NavigateTo(Path);

        await Page.Locator("#isolation-load-address-btn").ClickAsync();
        await Page.Locator("#isolation-load-contact-btn").ClickAsync();

        await Expect(Page.Locator($"#{S}Nested_Address_Street")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator($"#{C}Name")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Page.FillAsync($"#{S}Root_Name", "Parent User");
        await Page.FillAsync($"#{S}Root_Email", "parent@example.com");
        await Page.FillAsync($"#{S}Nested_Address_Street", "500 Market Street");
        await Page.FillAsync($"#{S}Nested_Address_City", "Philadelphia");
        await Page.FillAsync($"#{S}Nested_Address_ZipCode", "19106");

        await Page.Locator("#isolation-save-parent-btn").ClickAsync();

        await Expect(Page.Locator("#isolation-parent-result"))
            .ToContainTextAsync("Parent root saved", new() { Timeout = 5000 });

        await Page.FillAsync($"#{C}Name", "Contact User");
        await Page.FillAsync($"#{C}Email", "contact@example.com");
        await Page.FillAsync($"#{C}Message", "This message is long enough.");

        await Page.Locator("#isolation-send-contact-btn").ClickAsync();

        await Expect(Page.Locator("#isolation-contact-result"))
            .ToContainTextAsync("Standalone contact sent", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
