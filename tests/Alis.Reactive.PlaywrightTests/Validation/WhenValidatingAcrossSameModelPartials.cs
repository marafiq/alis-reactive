namespace Alis.Reactive.PlaywrightTests.Validation;

[TestFixture]
public class WhenValidatingAcrossSameModelPartials : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/SameModelMerge";
    private const string S = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ValidationMergeHarnessModel__";

    private async Task LoadBothPartials()
    {
        await NavigateTo(Path);

        await Page.Locator("#same-merge-load-address-btn").ClickAsync();
        await Page.Locator("#same-merge-load-delivery-btn").ClickAsync();

        await Expect(Page.Locator($"#{S}Nested_Address_Street")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator($"#{S}Nested_Delivery_Instructions")).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    // ── Scenario 1: Same-model partials merge validation into single surface ──

    [Test]
    public async Task same_model_partials_merge_validation_into_single_surface()
    {
        // WHY: proves AddToComponentsMap merges across partials — when two
        // partial views share the same TModel (ValidationMergeHarnessModel),
        // submitting the parent form validates fields from BOTH partials in
        // a single validation surface. Fixing all fields and resubmitting
        // results in success.

        await LoadBothPartials();

        // Step 1: Submit empty — errors from BOTH partials appear
        await Page.Locator("#same-merge-save-btn").ClickAsync();

        // Root form field errors
        await Expect(Page.Locator("#same-merge-form [data-valmsg-for='Root.Name']"))
            .ToContainTextAsync("Name is required.", new() { Timeout = 3000 });

        // Address partial field errors (merged into same validation surface)
        await Expect(Page.Locator("#same-merge-form [data-valmsg-for='Nested.Address.Street']"))
            .ToContainTextAsync("Street is required.", new() { Timeout = 3000 });

        // Delivery partial field errors (also merged into same validation surface)
        await Expect(Page.Locator("#same-merge-form [data-valmsg-for='Nested.Delivery.Instructions']"))
            .ToContainTextAsync("Delivery instructions are required.", new() { Timeout = 3000 });

        // Result should NOT have changed (POST was blocked by client validation)
        await Expect(Page.Locator("#same-merge-result")).ToContainTextAsync("Not submitted yet");

        // Step 2: Fill ALL fields from root + both partials and submit — success
        await Page.FillAsync($"#{S}Root_Name", "Merged User");
        await Page.FillAsync($"#{S}Root_Email", "merged@example.com");
        await Page.Locator($"#{S}Root_Amount").ClickAsync();
        await Page.Locator($"#{S}Root_Amount").FillAsync("150");
        await Page.Locator($"#{S}Root_Amount").PressAsync("Tab");
        await Page.FillAsync($"#{S}Nested_Address_Street", "123 Main St");
        await Page.FillAsync($"#{S}Nested_Address_City", "Springfield");
        await Page.FillAsync($"#{S}Nested_Address_ZipCode", "62701");
        await Page.FillAsync($"#{S}Nested_Delivery_Instructions", "Leave at front desk");
        await Page.FillAsync($"#{S}Nested_Delivery_ContactPhone", "123-456-7890");

        await Page.Locator("#same-merge-save-btn").ClickAsync();

        await Expect(Page.Locator("#same-merge-result"))
            .ToContainTextAsync("Merged root saved", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario 2: Reloading one partial preserves the other partial's validation ─

    [Test]
    public async Task reloading_one_partial_preserves_the_other_partial_validation()
    {
        // WHY: proves plan merge survives partial reload — loading both partials,
        // then reloading the address partial (v1 -> v2) replaces that contribution
        // while preserving the delivery partial. The merged validation surface
        // still works end-to-end.

        await LoadBothPartials();

        // Confirm address partial v1 is loaded
        await Expect(Page.Locator("#same-merge-address-version"))
            .ToContainTextAsync("Address partial v1", new() { Timeout = 5000 });

        // Step 1: Reload address partial — v2 replaces v1
        await Page.Locator("#same-merge-reload-address-btn").ClickAsync();

        await Expect(Page.Locator("#same-merge-address-version"))
            .ToContainTextAsync("Address partial v2", new() { Timeout = 5000 });

        // Delivery partial survives the address reload
        await Expect(Page.Locator("#same-merge-delivery-status"))
            .ToContainTextAsync("Delivery partial loaded", new() { Timeout = 5000 });

        // Step 2: Fill ALL fields (root + reloaded address v2 + delivery) and submit
        await Page.FillAsync($"#{S}Root_Name", "Merged User");
        await Page.FillAsync($"#{S}Root_Email", "merged@example.com");
        await Page.Locator($"#{S}Root_Amount").ClickAsync();
        await Page.Locator($"#{S}Root_Amount").FillAsync("150");
        await Page.Locator($"#{S}Root_Amount").PressAsync("Tab");
        await Page.FillAsync($"#{S}Nested_Address_Street", "123 Main St");
        await Page.FillAsync($"#{S}Nested_Address_City", "Springfield");
        await Page.FillAsync($"#{S}Nested_Address_ZipCode", "62701");
        await Page.FillAsync($"#{S}Nested_Delivery_Instructions", "Leave at front desk");
        await Page.FillAsync($"#{S}Nested_Delivery_ContactPhone", "123-456-7890");

        await Page.Locator("#same-merge-save-btn").ClickAsync();

        await Expect(Page.Locator("#same-merge-result"))
            .ToContainTextAsync("Merged root saved", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
