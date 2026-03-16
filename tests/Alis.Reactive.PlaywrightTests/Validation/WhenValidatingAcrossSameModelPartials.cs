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

    // ── Scenario: Filling all fields first time and submitting succeeds ──

    [Test]
    public async Task submitting_with_all_partials_filled_correctly_succeeds()
    {
        // WHY: proves the complete happy path — load both partials, fill every
        // field correctly on the first attempt, submit, and get success. This
        // exercises the merged validation surface without ever triggering an
        // error state, confirming that a clean-sheet submission through the
        // merged plan works end-to-end.

        await LoadBothPartials();

        // Fill root fields
        await Page.FillAsync($"#{S}Root_Name", "Happy Path User");
        await Page.FillAsync($"#{S}Root_Email", "happy@example.com");
        await Page.Locator($"#{S}Root_Amount").ClickAsync();
        await Page.Locator($"#{S}Root_Amount").FillAsync("999");
        await Page.Locator($"#{S}Root_Amount").PressAsync("Tab");

        // Fill address partial fields
        await Page.FillAsync($"#{S}Nested_Address_Street", "1 Infinite Loop");
        await Page.FillAsync($"#{S}Nested_Address_City", "Cupertino");
        await Page.FillAsync($"#{S}Nested_Address_ZipCode", "95014");

        // Fill delivery partial fields
        await Page.FillAsync($"#{S}Nested_Delivery_Instructions", "Leave with security");
        await Page.FillAsync($"#{S}Nested_Delivery_ContactPhone", "408-996-1010");

        // Submit — should succeed on first attempt, no validation errors
        await Page.Locator("#same-merge-save-btn").ClickAsync();

        await Expect(Page.Locator("#same-merge-result"))
            .ToContainTextAsync("Merged root saved", new() { Timeout = 5000 });

        // No validation error messages should be visible anywhere
        await Expect(Page.Locator("#same-merge-form [data-valmsg-for='Root.Name']"))
            .ToBeEmptyAsync();
        await Expect(Page.Locator("#same-merge-form [data-valmsg-for='Nested.Address.Street']"))
            .ToBeEmptyAsync();
        await Expect(Page.Locator("#same-merge-form [data-valmsg-for='Nested.Delivery.Instructions']"))
            .ToBeEmptyAsync();

        AssertNoConsoleErrors();
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

    // ── Scenario 3: One partial filled, other empty — errors only on empty partial ──

    [Test]
    public async Task filling_one_partial_and_leaving_other_empty_shows_errors_only_on_empty()
    {
        // WHY: proves merged validation evaluates each partial's fields independently —
        // filling all address fields completely, leaving all delivery fields empty,
        // and submitting shows errors ONLY on delivery fields. Address fields pass
        // validation cleanly within the same merged surface.

        await LoadBothPartials();

        // Step 1: Fill ALL root fields and ALL address fields — leave delivery empty
        await Page.FillAsync($"#{S}Root_Name", "Complete User");
        await Page.FillAsync($"#{S}Root_Email", "complete@example.com");
        await Page.Locator($"#{S}Root_Amount").ClickAsync();
        await Page.Locator($"#{S}Root_Amount").FillAsync("200");
        await Page.Locator($"#{S}Root_Amount").PressAsync("Tab");
        await Page.FillAsync($"#{S}Nested_Address_Street", "456 Oak Avenue");
        await Page.FillAsync($"#{S}Nested_Address_City", "Portland");
        await Page.FillAsync($"#{S}Nested_Address_ZipCode", "97201");

        // Delivery fields intentionally left empty

        // Step 2: Submit — errors only on delivery, not on address or root
        await Page.Locator("#same-merge-save-btn").ClickAsync();

        // Delivery error must appear (it's required and empty)
        await Expect(Page.Locator("#same-merge-form [data-valmsg-for='Nested.Delivery.Instructions']"))
            .ToContainTextAsync("Delivery instructions are required.", new() { Timeout = 3000 });

        // Root fields must NOT have errors — they were filled correctly
        await Expect(Page.Locator("#same-merge-form [data-valmsg-for='Root.Name']"))
            .ToBeEmptyAsync();
        await Expect(Page.Locator("#same-merge-form [data-valmsg-for='Root.Email']"))
            .ToBeEmptyAsync();

        // Address fields must NOT have errors — they were filled correctly
        await Expect(Page.Locator("#same-merge-form [data-valmsg-for='Nested.Address.Street']"))
            .ToBeEmptyAsync();
        await Expect(Page.Locator("#same-merge-form [data-valmsg-for='Nested.Address.City']"))
            .ToBeEmptyAsync();

        // Result should show validation errors (server-side rejected due to delivery)
        await Expect(Page.Locator("#same-merge-result"))
            .Not.ToContainTextAsync("Merged root saved");

        // Step 3: Fill delivery and resubmit — now everything passes
        await Page.FillAsync($"#{S}Nested_Delivery_Instructions", "Leave at reception");
        await Page.FillAsync($"#{S}Nested_Delivery_ContactPhone", "555-123-4567");

        await Page.Locator("#same-merge-save-btn").ClickAsync();

        await Expect(Page.Locator("#same-merge-result"))
            .ToContainTextAsync("Merged root saved", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
