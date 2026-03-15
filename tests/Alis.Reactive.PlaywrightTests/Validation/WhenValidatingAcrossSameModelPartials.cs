namespace Alis.Reactive.PlaywrightTests.Validation;

[TestFixture]
public class WhenValidatingAcrossSameModelPartials : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/SameModelMerge";
    private const string S = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ValidationMergeHarnessModel__";

    [Test]
    public async Task PageLoadsWithNoErrors()
    {
        await NavigateTo(Path);
        await Expect(Page).ToHaveTitleAsync("Validation Same Model Merge — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task LoadingAddressAndDeliveryPartialsActivatesOneMergedValidationSurface()
    {
        await NavigateTo(Path);

        await Page.Locator("#same-merge-load-address-btn").ClickAsync();
        await Page.Locator("#same-merge-load-delivery-btn").ClickAsync();

        await Expect(Page.Locator("#same-merge-address-status"))
            .ToContainTextAsync("Address partial loaded", new() { Timeout = 5000 });
        await Expect(Page.Locator("#same-merge-delivery-status"))
            .ToContainTextAsync("Delivery partial loaded", new() { Timeout = 5000 });

        await Expect(Page.Locator($"#{S}Root_Name")).ToBeVisibleAsync();
        await Expect(Page.Locator($"#{S}Nested_Address_Street")).ToBeVisibleAsync();
        await Expect(Page.Locator($"#{S}Nested_Delivery_Instructions")).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task OneSubmitValidatesParentAndBothSameModelPartialsTogether()
    {
        await NavigateTo(Path);

        await Page.Locator("#same-merge-load-address-btn").ClickAsync();
        await Page.Locator("#same-merge-load-delivery-btn").ClickAsync();

        await Expect(Page.Locator($"#{S}Nested_Address_Street")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator($"#{S}Nested_Delivery_Instructions")).ToBeVisibleAsync(new() { Timeout = 5000 });

        await Page.Locator("#same-merge-save-btn").ClickAsync();

        await Expect(Page.Locator("#same-merge-form [data-valmsg-for='Root.Name']"))
            .ToContainTextAsync("Name is required.", new() { Timeout = 3000 });
        await Expect(Page.Locator("#same-merge-form [data-valmsg-for='Nested.Address.Street']"))
            .ToContainTextAsync("Street is required.", new() { Timeout = 3000 });
        await Expect(Page.Locator("#same-merge-form [data-valmsg-for='Nested.Delivery.Instructions']"))
            .ToContainTextAsync("Delivery instructions are required.", new() { Timeout = 3000 });

        await Expect(Page.Locator("#same-merge-result")).ToContainTextAsync("Not submitted yet");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task ReloadingTheSameAddressSlotReplacesThatContributionAndPreservesDelivery()
    {
        await NavigateTo(Path);

        await Page.Locator("#same-merge-load-address-btn").ClickAsync();
        await Page.Locator("#same-merge-load-delivery-btn").ClickAsync();

        await Expect(Page.Locator("#same-merge-address-version"))
            .ToContainTextAsync("Address partial v1", new() { Timeout = 5000 });

        await Page.Locator("#same-merge-reload-address-btn").ClickAsync();

        await Expect(Page.Locator("#same-merge-address-version"))
            .ToContainTextAsync("Address partial v2", new() { Timeout = 5000 });
        await Expect(Page.Locator("#same-merge-delivery-status"))
            .ToContainTextAsync("Delivery partial loaded", new() { Timeout = 5000 });

        await Page.FillAsync($"#{S}Root_Name", "Merged User");
        await Page.FillAsync($"#{S}Root_Email", "merged@example.com");
        await Page.FillAsync($"#{S}Nested_Address_Street", "123 Main St");
        await Page.FillAsync($"#{S}Nested_Address_City", "Springfield");
        await Page.FillAsync($"#{S}Nested_Address_ZipCode", "62701");
        await Page.FillAsync($"#{S}Nested_Delivery_Instructions", "Leave at front desk");
        await Page.FillAsync($"#{S}Nested_Delivery_ContactPhone", "123-456-7890");
        await Page.Locator($"#{S}Root_Amount").ClickAsync();
        await Page.Locator($"#{S}Root_Amount").FillAsync("150");
        await Page.Locator($"#{S}Root_Amount").PressAsync("Tab");

        await Page.Locator("#same-merge-save-btn").ClickAsync();

        await Expect(Page.Locator("#same-merge-result"))
            .ToContainTextAsync("Merged root saved", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
