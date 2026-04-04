namespace Alis.Reactive.PlaywrightTests.AllModulesTogether.ReactiveWiring;

[TestFixture]
public class WhenGuardsControlReactiveFlow : PlaywrightTestBase
{
    private const string S = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_PlaygroundSyntaxModel";

    private async Task NavigateAndBoot()
    {
        await NavigateTo("/Sandbox/AllModulesTogether/PlaygroundSyntax/ReactiveConditions");
        await WaitForPageReady(5000);
    }

    // ── Scenario: Status dropdown drives entire form state ──

    [Test]
    public async Task selecting_active_status_sets_amount_and_shows_address_section()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "active" });

        // Status result text shows Active branch fired
        var result = Page.Locator("#status-result");
        await Expect(result).ToContainTextAsync("Active", new() { Timeout = 3000 });
        await Expect(result).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-emerald-700"));

        // Cross-vendor: native dropdown event set Fusion numeric to 100
        // SF FusionNumericTextBox renders TWO inputs with the same ID — use .First for the visible one
        // SF formats numeric display (e.g. "100.00"), so use regex to match the numeric value
        var amountInput = Page.Locator($"#{S}__Amount").First;
        await Expect(amountInput).ToHaveValueAsync(new System.Text.RegularExpressions.Regex(@"^100(\.00)?$"), new() { Timeout = 3000 });

        // Cross-vendor: native dropdown event set another native dropdown (City) to "seattle"
        var citySelect = Page.Locator($"#{S}__Address_City");
        await Expect(citySelect).ToHaveValueAsync("seattle", new() { Timeout = 3000 });

        // Address section remains visible
        await Expect(Page.Locator("#address-section")).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selecting_inactive_hides_address_and_zeros_amount()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "inactive" });

        // Status result text shows Inactive branch fired
        var result = Page.Locator("#status-result");
        await Expect(result).ToContainTextAsync("Inactive", new() { Timeout = 3000 });
        await Expect(result).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-amber-600"));

        // Amount zeroed by cross-vendor mutation
        // SF may format as "0" or "0.00" — regex handles both
        var amountInput = Page.Locator($"#{S}__Amount").First;
        await Expect(amountInput).ToHaveValueAsync(new System.Text.RegularExpressions.Regex(@"^0(\.00)?$"), new() { Timeout = 3000 });

        // Address section hidden
        await Expect(Page.Locator("#address-section")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    // ── Scenario: Amount numeric drives tier classification ──

    [Test]
    public async Task changing_amount_updates_tier_classification()
    {
        await NavigateAndBoot();

        // SF FusionNumericTextBox renders TWO inputs with the same ID — use .First for the visible one
        var input = Page.Locator($"#{S}__Amount").First;
        var tier = Page.Locator("#amount-tier");

        // High value: >= 5000
        await input.ClickAsync();
        await input.FillAsync("5500");
        await input.PressAsync("Tab");

        await Expect(tier).ToHaveTextAsync("High value order", new() { Timeout = 3000 });
        await Expect(tier).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-rose-600"));

        // Standard: >= 1000 but < 5000
        await input.ClickAsync();
        await input.FillAsync("2500");
        await input.PressAsync("Tab");

        await Expect(tier).ToHaveTextAsync("Standard order", new() { Timeout = 3000 });
        await Expect(tier).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-sky-600"));

        // Small: < 1000
        await input.ClickAsync();
        await input.FillAsync("500");
        await input.PressAsync("Tab");

        await Expect(tier).ToHaveTextAsync("Small order", new() { Timeout = 3000 });
        await Expect(tier).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-slate-500"));

        AssertNoConsoleErrors();
    }

    // ── Scenario: City dropdown auto-fills sibling fields ──

    [Test]
    public async Task selecting_city_autofills_state_and_postal_code()
    {
        await NavigateAndBoot();

        var citySelect = Page.Locator($"#{S}__Address_City");
        var stateSelect = Page.Locator($"#{S}__Address_State");
        // SF FusionNumericTextBox renders TWO inputs — .First targets the visible one
        var postalInput = Page.Locator($"#{S}__Address_PostalCode").First;
        var autoText = Page.Locator("#city-auto");

        // Seattle -> WA, 98101
        await citySelect.SelectOptionAsync(new SelectOptionValue { Value = "seattle" });

        await Expect(stateSelect).ToHaveValueAsync("WA", new() { Timeout = 3000 });
        // SF formats numeric display (e.g. "98,101.00"), so use regex to match the core digits
        await Expect(postalInput).ToHaveValueAsync(new System.Text.RegularExpressions.Regex("98.?101"), new() { Timeout = 3000 });
        await Expect(autoText).ToContainTextAsync("WA, 98101");

        // Portland -> OR, 97201
        await citySelect.SelectOptionAsync(new SelectOptionValue { Value = "portland" });

        await Expect(stateSelect).ToHaveValueAsync("OR", new() { Timeout = 3000 });
        await Expect(postalInput).ToHaveValueAsync(new System.Text.RegularExpressions.Regex("97.?201"), new() { Timeout = 3000 });
        await Expect(autoText).ToContainTextAsync("OR, 97201");

        AssertNoConsoleErrors();
    }

    // ── Scenario: Full ElseIf lifecycle — all branches fire in sequence ──

    [Test]
    public async Task full_status_lifecycle_active_then_inactive_then_pending()
    {
        await NavigateAndBoot();

        var statusSelect = Page.Locator($"#{S}__Status");
        var result = Page.Locator("#status-result");
        var amountInput = Page.Locator($"#{S}__Amount").First;
        var citySelect = Page.Locator($"#{S}__Address_City");
        var addressSection = Page.Locator("#address-section");

        // ── Phase 1: Active ──
        await statusSelect.SelectOptionAsync(new SelectOptionValue { Value = "active" });

        await Expect(result).ToContainTextAsync("Active", new() { Timeout = 3000 });
        await Expect(result).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-emerald-700"));
        await Expect(amountInput).ToHaveValueAsync(
            new System.Text.RegularExpressions.Regex(@"^100(\.00)?$"), new() { Timeout = 3000 });
        await Expect(citySelect).ToHaveValueAsync("seattle", new() { Timeout = 3000 });
        await Expect(addressSection).ToBeVisibleAsync();

        // ── Phase 2: Inactive ──
        await statusSelect.SelectOptionAsync(new SelectOptionValue { Value = "inactive" });

        await Expect(result).ToContainTextAsync("Inactive", new() { Timeout = 3000 });
        await Expect(result).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-amber-600"));
        // Must NOT still have the green class from active phase (state leak check)
        await Expect(result).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-emerald-700"));
        await Expect(amountInput).ToHaveValueAsync(
            new System.Text.RegularExpressions.Regex(@"^0(\.00)?$"), new() { Timeout = 3000 });
        await Expect(addressSection).ToBeHiddenAsync();

        // ── Phase 3: Pending (Else branch) ──
        await statusSelect.SelectOptionAsync(new SelectOptionValue { Value = "pending" });

        await Expect(result).ToContainTextAsync("Pending or empty", new() { Timeout = 3000 });
        await Expect(result).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-slate-500"));
        // Must NOT still have amber from inactive phase (state leak check)
        await Expect(result).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-amber-600"));
        await Expect(result).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-emerald-700"));
        // Else branch shows address section (was hidden by inactive)
        await Expect(addressSection).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    // ── Scenario: City autofill state survives address hide/show cycle ──

    [Test]
    public async Task city_autofill_then_status_inactive_hides_address_preserving_filled_values()
    {
        await NavigateAndBoot();

        var statusSelect = Page.Locator($"#{S}__Status");
        var citySelect = Page.Locator($"#{S}__Address_City");
        var stateSelect = Page.Locator($"#{S}__Address_State");
        var postalInput = Page.Locator($"#{S}__Address_PostalCode").First;
        var addressSection = Page.Locator("#address-section");

        // Step 1: Select active — City set to "seattle" by the active branch, address visible
        await statusSelect.SelectOptionAsync(new SelectOptionValue { Value = "active" });
        await Expect(citySelect).ToHaveValueAsync("seattle", new() { Timeout = 3000 });
        await Expect(addressSection).ToBeVisibleAsync();

        // Step 2: User manually selects portland — triggers City's reactive pipeline
        // Auto-fills: State=OR, PostalCode=97201
        await citySelect.SelectOptionAsync(new SelectOptionValue { Value = "portland" });
        await Expect(stateSelect).ToHaveValueAsync("OR", new() { Timeout = 3000 });
        await Expect(postalInput).ToHaveValueAsync(
            new System.Text.RegularExpressions.Regex("97.?201"), new() { Timeout = 3000 });

        // Step 3: Select inactive — address section hides, Amount zeroed
        await statusSelect.SelectOptionAsync(new SelectOptionValue { Value = "inactive" });
        await Expect(addressSection).ToBeHiddenAsync();

        // Step 4: Select active again — address shows, City overwritten to "seattle"
        await statusSelect.SelectOptionAsync(new SelectOptionValue { Value = "active" });
        await Expect(addressSection).ToBeVisibleAsync();
        await Expect(citySelect).ToHaveValueAsync("seattle", new() { Timeout = 3000 });

        // State and PostalCode retain their portland-autofilled values because:
        // - The active branch only sets City (not State/PostalCode)
        // - Programmatic SetValue("seattle") does NOT fire City's change event,
        //   so City's reactive pipeline does NOT re-autofill State and PostalCode
        await Expect(stateSelect).ToHaveValueAsync("OR", new() { Timeout = 3000 });
        await Expect(postalInput).ToHaveValueAsync(
            new System.Text.RegularExpressions.Regex("97.?201"), new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario: Denver city autofills CO + 80201 ──

    [Test]
    public async Task selecting_denver_autofills_state_co_and_postal_80201()
    {
        await NavigateAndBoot();

        var citySelect = Page.Locator($"#{S}__Address_City");
        var stateSelect = Page.Locator($"#{S}__Address_State");
        var postalInput = Page.Locator($"#{S}__Address_PostalCode").First;
        var autoText = Page.Locator("#city-auto");

        await citySelect.SelectOptionAsync(new SelectOptionValue { Value = "denver" });

        await Expect(stateSelect).ToHaveValueAsync("CO", new() { Timeout = 3000 });
        await Expect(postalInput).ToHaveValueAsync(new System.Text.RegularExpressions.Regex("80.?201"), new() { Timeout = 3000 });
        await Expect(autoText).ToContainTextAsync("CO, 80201");

        AssertNoConsoleErrors();
    }

    // ── Scenario: City else branch clears state and resets postal code ──

    [Test]
    public async Task selecting_empty_city_clears_state_and_resets_auto_text()
    {
        await NavigateAndBoot();

        var citySelect = Page.Locator($"#{S}__Address_City");
        var stateSelect = Page.Locator($"#{S}__Address_State");
        var autoText = Page.Locator("#city-auto");

        // First, select a city to fill sibling fields
        await citySelect.SelectOptionAsync(new SelectOptionValue { Value = "seattle" });
        await Expect(stateSelect).ToHaveValueAsync("WA", new() { Timeout = 3000 });
        await Expect(autoText).ToContainTextAsync("WA, 98101");

        // Now select the empty option — Else branch should clear State and reset auto text
        await citySelect.SelectOptionAsync(new SelectOptionValue { Value = "" });

        await Expect(stateSelect).ToHaveValueAsync("", new() { Timeout = 3000 });
        await Expect(autoText).ToHaveTextAsync("Select a city", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Scenario: Initial page state before any interaction ──

    [Test]
    public async Task page_loads_with_all_echoes_at_default_values()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#status-result")).ToHaveTextAsync("\u2014");
        await Expect(Page.Locator("#amount-tier")).ToHaveTextAsync("\u2014");
        await Expect(Page.Locator("#city-auto")).ToHaveTextAsync("Select a city");

        // Address section is visible by default (not hidden until "inactive" branch)
        await Expect(Page.Locator("#address-section")).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    // ── Scenario: Amount tier boundary values ──

    [Test]
    public async Task amount_tier_boundary_values_evaluate_correctly()
    {
        await NavigateAndBoot();

        var input = Page.Locator($"#{S}__Amount").First;
        var tier = Page.Locator("#amount-tier");

        // Exactly 5000 → High value (>= 5000)
        await input.ClickAsync();
        await input.FillAsync("5000");
        await input.PressAsync("Tab");
        await Expect(tier).ToHaveTextAsync("High value order", new() { Timeout = 3000 });
        await Expect(tier).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-rose-600"));

        // Exactly 1000 → Standard (>= 1000 but < 5000)
        await input.ClickAsync();
        await input.FillAsync("1000");
        await input.PressAsync("Tab");
        await Expect(tier).ToHaveTextAsync("Standard order", new() { Timeout = 3000 });
        await Expect(tier).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-sky-600"));

        // 999 → Small (< 1000)
        await input.ClickAsync();
        await input.FillAsync("999");
        await input.PressAsync("Tab");
        await Expect(tier).ToHaveTextAsync("Small order", new() { Timeout = 3000 });
        await Expect(tier).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-slate-500"));

        AssertNoConsoleErrors();
    }

    // ── Scenario: Pending status (Else branch) fires directly ──

    [Test]
    public async Task selecting_pending_directly_fires_else_branch()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "pending" });

        var result = Page.Locator("#status-result");
        await Expect(result).ToContainTextAsync("Pending or empty", new() { Timeout = 3000 });
        await Expect(result).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-slate-500"));

        // Address section stays visible (Else branch calls Show())
        await Expect(Page.Locator("#address-section")).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    // ── Scenario: All three cities in sequence ──

    [Test]
    public async Task all_three_cities_autofill_correctly_in_sequence()
    {
        await NavigateAndBoot();

        var citySelect = Page.Locator($"#{S}__Address_City");
        var stateSelect = Page.Locator($"#{S}__Address_State");
        var postalInput = Page.Locator($"#{S}__Address_PostalCode").First;
        var autoText = Page.Locator("#city-auto");

        // Seattle → WA, 98101
        await citySelect.SelectOptionAsync(new SelectOptionValue { Value = "seattle" });
        await Expect(stateSelect).ToHaveValueAsync("WA", new() { Timeout = 3000 });
        await Expect(postalInput).ToHaveValueAsync(new System.Text.RegularExpressions.Regex("98.?101"), new() { Timeout = 3000 });
        await Expect(autoText).ToContainTextAsync("WA, 98101");

        // Portland → OR, 97201
        await citySelect.SelectOptionAsync(new SelectOptionValue { Value = "portland" });
        await Expect(stateSelect).ToHaveValueAsync("OR", new() { Timeout = 3000 });
        await Expect(postalInput).ToHaveValueAsync(new System.Text.RegularExpressions.Regex("97.?201"), new() { Timeout = 3000 });
        await Expect(autoText).ToContainTextAsync("OR, 97201");

        // Denver → CO, 80201
        await citySelect.SelectOptionAsync(new SelectOptionValue { Value = "denver" });
        await Expect(stateSelect).ToHaveValueAsync("CO", new() { Timeout = 3000 });
        await Expect(postalInput).ToHaveValueAsync(new System.Text.RegularExpressions.Regex("80.?201"), new() { Timeout = 3000 });
        await Expect(autoText).ToContainTextAsync("CO, 80201");

        AssertNoConsoleErrors();
    }

    // ── Scenario: Programmatic Amount set cascades into tier pipeline ──

    [Test]
    public async Task programmatic_amount_set_cascades_into_tier_pipeline()
    {
        await NavigateAndBoot();

        var amountTier = Page.Locator("#amount-tier");

        // Confirm initial state
        await Expect(amountTier).ToHaveTextAsync("\u2014");

        // Select active → programmatically sets Amount=100 → triggers tier pipeline
        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "active" });

        // Wait for status branch to complete
        await Expect(Page.Locator("#status-result")).ToContainTextAsync("Active", new() { Timeout = 3000 });

        // Amount was set to 100 and Fusion fires the change event, which triggers
        // the tier pipeline. 100 < 1000 = "Small order"
        await Expect(amountTier).ToHaveTextAsync("Small order", new() { Timeout = 3000 });
        await Expect(amountTier).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-slate-500"));

        AssertNoConsoleErrors();
    }
}
