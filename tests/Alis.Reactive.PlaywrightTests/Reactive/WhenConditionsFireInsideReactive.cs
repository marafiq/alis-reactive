namespace Alis.Reactive.PlaywrightTests.Reactive;

/// <summary>
/// Verifies conditions inside .Reactive() with cross-vendor mutations, ElseIf chains,
/// and auto-fill cascades. This is the heart of the framework: a user interacts with
/// one component, conditions evaluate, and dependent components/elements update.
///
/// Page under test: /Sandbox/PlaygroundSyntax/ReactiveConditions
///
/// Form layout:
///   Status  (native dropdown, reactive)  -- ElseIf drives Amount, City, address visibility, status text
///   Amount  (fusion numeric, reactive)   -- ElseIf tier ladder drives amount-tier text + color
///   City    (nested native dropdown, reactive) -- ElseIf auto-fills State + PostalCode
///   State   (nested native dropdown, target only)
///   PostalCode (nested fusion numeric, target only)
/// </summary>
[TestFixture]
public class WhenConditionsFireInsideReactive : PlaywrightTestBase
{
    /// <summary>IdGenerator type scope for PlaygroundSyntaxModel.</summary>
    private const string S = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_PlaygroundSyntaxModel";

    private async Task NavigateAndBoot()
    {
        await NavigateTo("/Sandbox/PlaygroundSyntax/ReactiveConditions");
        await WaitForTraceMessage("booted", 5000);
    }

    // ── Scenario: Status dropdown drives entire form state ──

    /// <summary>
    /// Selecting "active" triggers the first ElseIf branch which:
    /// - Sets Fusion NumericTextBox Amount to 100 (cross-vendor: native event -> fusion mutation)
    /// - Sets Native DropDown City to "seattle" (native -> native mutation)
    /// - Shows the address section
    /// - Sets status-result text green with "Active" message
    ///
    /// WHY: proves ElseIf with cross-vendor mutations (Native dropdown -> Fusion numeric + Native dropdown)
    /// </summary>
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
        // SF NumericTextBox renders TWO inputs with the same ID — use .First for the visible one
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

    /// <summary>
    /// Selecting "inactive" triggers the second ElseIf branch which:
    /// - Sets Fusion NumericTextBox Amount to 0
    /// - Hides the address section entirely
    /// - Sets status-result text amber with "Inactive" message
    ///
    /// WHY: proves ElseIf branch correctly hides/shows sections and zeros component values
    /// </summary>
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

    /// <summary>
    /// Entering different amounts evaluates the ElseIf tier ladder:
    /// - >= 5000: "High value order" (rose)
    /// - >= 1000: "Standard order" (sky)
    /// - else:    "Small order" (slate)
    ///
    /// WHY: proves numeric ElseIf ladder with Gte() comparisons and class swapping
    /// </summary>
    [Test]
    public async Task changing_amount_updates_tier_classification()
    {
        await NavigateAndBoot();

        // SF NumericTextBox renders TWO inputs with the same ID — use .First for the visible one
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

    /// <summary>
    /// Selecting a city auto-fills the State dropdown and PostalCode numeric:
    /// - seattle  -> State=WA, PostalCode=98101
    /// - portland -> State=OR, PostalCode=97201
    /// Both are cross-component mutations (native dropdown -> native dropdown + fusion numeric).
    ///
    /// WHY: proves cross-component reactive wiring with nested property IDs (Address_City -> Address_State + Address_PostalCode)
    /// </summary>
    [Test]
    public async Task selecting_city_autofills_state_and_postal_code()
    {
        await NavigateAndBoot();

        var citySelect = Page.Locator($"#{S}__Address_City");
        var stateSelect = Page.Locator($"#{S}__Address_State");
        // SF NumericTextBox renders TWO inputs — .First targets the visible one
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

    /// <summary>
    /// Exercises every ElseIf branch of the Status dropdown in sequence:
    ///   1. "active"   → Amount=100, City=seattle, address visible, green text
    ///   2. "inactive" → Amount=0, address hidden, amber text
    ///   3. "pending"  → Else branch fires: "Pending or empty", address shows again, slate text
    ///
    /// This catches state leaks: if a previous branch's AddClass or Show/Hide
    /// lingers when a later branch fires, the assertions will fail. Each branch
    /// must correctly add its own color class AND remove the other two.
    ///
    /// WHY: proves the complete ElseIf chain works for ALL branches in chronological
    /// sequence, not just each branch in isolation (which the single-branch tests cover)
    /// </summary>
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

    /// <summary>
    /// Exercises a cross-vendor workflow spanning two reactive pipelines:
    ///   1. Status → "active": sets City=seattle, shows address, Amount=100
    ///   2. User manually selects City → "portland": auto-fills State=OR, PostalCode=97201
    ///   3. Status → "inactive": hides address section, sets Amount=0
    ///   4. Status → "active" again: shows address, sets City=seattle (overwritten),
    ///      but State and PostalCode retain their portland values (OR, 97201)
    ///      because programmatic SetValue on City does NOT fire City's change event
    ///
    /// WHY: proves that (a) hiding an element preserves its descendant component values,
    /// (b) the active branch's SetValue("seattle") on City is a value-only write that
    /// does not cascade through City's reactive pipeline, and (c) sibling fields (State,
    /// PostalCode) are only updated by explicit user interaction, not by side effects
    /// </summary>
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
}
