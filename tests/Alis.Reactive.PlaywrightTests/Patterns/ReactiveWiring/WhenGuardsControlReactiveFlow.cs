namespace Alis.Reactive.PlaywrightTests.Patterns.ReactiveWiring;

/// <summary>
/// Verifies conditions inside .Reactive() with cross-vendor mutations, ElseIf chains,
/// and auto-fill cascades. This is the heart of the framework: a user interacts with
/// one component, conditions evaluate, and dependent components/elements update.
///
/// Page under test: /Sandbox/Patterns/PlaygroundSyntax/ReactiveConditions
///
/// Form layout:
///   Status  (native dropdown, reactive)  -- ElseIf drives Amount, City, address visibility, status text
///   Amount  (fusion numeric, reactive)   -- ElseIf tier ladder drives amount-tier text + color
///   City    (nested native dropdown, reactive) -- ElseIf auto-fills State + PostalCode
///   State   (nested native dropdown, target only)
///   PostalCode (nested fusion numeric, target only)
/// </summary>
[TestFixture]
public class WhenGuardsControlReactiveFlow : PlaywrightTestBase
{
    /// <summary>IdGenerator type scope for PlaygroundSyntaxModel.</summary>
    private const string S = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_PlaygroundSyntaxModel";

    private async Task NavigateAndBoot()
    {
        await NavigateTo("/Sandbox/Patterns/PlaygroundSyntax/ReactiveConditions");
        await WaitForTraceMessage("booted", 5000);
    }

    [Test]
    public async Task selecting_active_status_sets_amount_and_shows_address_section()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "active" });

        var result = Page.Locator("#status-result");
        await Expect(result).ToContainTextAsync("Active", new() { Timeout = 3000 });
        await Expect(result).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-emerald-700"));

        // SF FusionNumericTextBox renders TWO inputs with the same ID — use .First for the visible one
        // SF formats numeric display (e.g. "100.00"), so use regex to match the numeric value
        var amountInput = Page.Locator($"#{S}__Amount").First;
        await Expect(amountInput).ToHaveValueAsync(new System.Text.RegularExpressions.Regex(@"^100(\.00)?$"), new() { Timeout = 3000 });

        var citySelect = Page.Locator($"#{S}__Address_City");
        await Expect(citySelect).ToHaveValueAsync("seattle", new() { Timeout = 3000 });

        await Expect(Page.Locator("#address-section")).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selecting_inactive_hides_address_and_zeros_amount()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "inactive" });

        var result = Page.Locator("#status-result");
        await Expect(result).ToContainTextAsync("Inactive", new() { Timeout = 3000 });
        await Expect(result).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-amber-600"));

        // SF may format as "0" or "0.00" — regex handles both
        var amountInput = Page.Locator($"#{S}__Amount").First;
        await Expect(amountInput).ToHaveValueAsync(new System.Text.RegularExpressions.Regex(@"^0(\.00)?$"), new() { Timeout = 3000 });

        await Expect(Page.Locator("#address-section")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_amount_updates_tier_classification()
    {
        await NavigateAndBoot();

        // SF FusionNumericTextBox renders TWO inputs with the same ID — use .First for the visible one
        var input = Page.Locator($"#{S}__Amount").First;
        var tier = Page.Locator("#amount-tier");

        await input.ClickAsync();
        await input.FillAsync("5500");
        await input.PressAsync("Tab");

        await Expect(tier).ToHaveTextAsync("High value order", new() { Timeout = 3000 });
        await Expect(tier).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-rose-600"));

        await input.ClickAsync();
        await input.FillAsync("2500");
        await input.PressAsync("Tab");

        await Expect(tier).ToHaveTextAsync("Standard order", new() { Timeout = 3000 });
        await Expect(tier).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-sky-600"));

        await input.ClickAsync();
        await input.FillAsync("500");
        await input.PressAsync("Tab");

        await Expect(tier).ToHaveTextAsync("Small order", new() { Timeout = 3000 });
        await Expect(tier).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-slate-500"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selecting_city_autofills_state_and_postal_code()
    {
        await NavigateAndBoot();

        var citySelect = Page.Locator($"#{S}__Address_City");
        var stateSelect = Page.Locator($"#{S}__Address_State");
        // SF FusionNumericTextBox renders TWO inputs — .First targets the visible one
        var postalInput = Page.Locator($"#{S}__Address_PostalCode").First;
        var autoText = Page.Locator("#city-auto");

        await citySelect.SelectOptionAsync(new SelectOptionValue { Value = "seattle" });

        await Expect(stateSelect).ToHaveValueAsync("WA", new() { Timeout = 3000 });
        // SF formats numeric display (e.g. "98,101.00"), so use regex to match the core digits
        await Expect(postalInput).ToHaveValueAsync(new System.Text.RegularExpressions.Regex("98.?101"), new() { Timeout = 3000 });
        await Expect(autoText).ToContainTextAsync("WA, 98101");

        await citySelect.SelectOptionAsync(new SelectOptionValue { Value = "portland" });

        await Expect(stateSelect).ToHaveValueAsync("OR", new() { Timeout = 3000 });
        await Expect(postalInput).ToHaveValueAsync(new System.Text.RegularExpressions.Regex("97.?201"), new() { Timeout = 3000 });
        await Expect(autoText).ToContainTextAsync("OR, 97201");

        AssertNoConsoleErrors();
    }

    /// <summary>
    /// Exercises the Status ElseIf chain in sequence to catch branch state leakage
    /// across class and visibility mutations.
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
        await statusSelect.SelectOptionAsync(new SelectOptionValue { Value = "active" });

        await Expect(result).ToContainTextAsync("Active", new() { Timeout = 3000 });
        await Expect(result).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-emerald-700"));
        await Expect(amountInput).ToHaveValueAsync(
            new System.Text.RegularExpressions.Regex(@"^100(\.00)?$"), new() { Timeout = 3000 });
        await Expect(citySelect).ToHaveValueAsync("seattle", new() { Timeout = 3000 });
        await Expect(addressSection).ToBeVisibleAsync();
        await statusSelect.SelectOptionAsync(new SelectOptionValue { Value = "inactive" });

        await Expect(result).ToContainTextAsync("Inactive", new() { Timeout = 3000 });
        await Expect(result).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-amber-600"));
        // Must NOT still have the green class from active phase (state leak check)
        await Expect(result).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-emerald-700"));
        await Expect(amountInput).ToHaveValueAsync(
            new System.Text.RegularExpressions.Regex(@"^0(\.00)?$"), new() { Timeout = 3000 });
        await Expect(addressSection).ToBeHiddenAsync();
        await statusSelect.SelectOptionAsync(new SelectOptionValue { Value = "pending" });

        await Expect(result).ToContainTextAsync("Pending or empty", new() { Timeout = 3000 });
        await Expect(result).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-slate-500"));
        // Must NOT still have amber from inactive phase (state leak check)
        await Expect(result).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-amber-600"));
        await Expect(result).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-emerald-700"));
        await Expect(addressSection).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    /// <summary>
    /// Verifies hidden address fields keep explicitly selected values and Status
    /// SetValue on City does not cascade through City's reactive pipeline.
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

        await statusSelect.SelectOptionAsync(new SelectOptionValue { Value = "active" });
        await Expect(citySelect).ToHaveValueAsync("seattle", new() { Timeout = 3000 });
        await Expect(addressSection).ToBeVisibleAsync();

        await citySelect.SelectOptionAsync(new SelectOptionValue { Value = "portland" });
        await Expect(stateSelect).ToHaveValueAsync("OR", new() { Timeout = 3000 });
        await Expect(postalInput).ToHaveValueAsync(
            new System.Text.RegularExpressions.Regex("97.?201"), new() { Timeout = 3000 });

        await statusSelect.SelectOptionAsync(new SelectOptionValue { Value = "inactive" });
        await Expect(addressSection).ToBeHiddenAsync();

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

    /// <summary>
    /// Verifies the City Else branch clears the State dropdown and resets the
    /// auto-fill text after a prior city selection.
    /// </summary>
    [Test]
    public async Task selecting_empty_city_clears_state_and_resets_auto_text()
    {
        await NavigateAndBoot();

        var citySelect = Page.Locator($"#{S}__Address_City");
        var stateSelect = Page.Locator($"#{S}__Address_State");
        var autoText = Page.Locator("#city-auto");

        await citySelect.SelectOptionAsync(new SelectOptionValue { Value = "seattle" });
        await Expect(stateSelect).ToHaveValueAsync("WA", new() { Timeout = 3000 });
        await Expect(autoText).ToContainTextAsync("WA, 98101");

        await citySelect.SelectOptionAsync(new SelectOptionValue { Value = "" });

        await Expect(stateSelect).ToHaveValueAsync("", new() { Timeout = 3000 });
        await Expect(autoText).ToHaveTextAsync("Select a city", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task page_loads_with_all_echoes_at_default_values()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("#status-result")).ToHaveTextAsync("\u2014");
        await Expect(Page.Locator("#amount-tier")).ToHaveTextAsync("\u2014");
        await Expect(Page.Locator("#city-auto")).ToHaveTextAsync("Select a city");

        await Expect(Page.Locator("#address-section")).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task amount_tier_boundary_values_evaluate_correctly()
    {
        await NavigateAndBoot();

        var input = Page.Locator($"#{S}__Amount").First;
        var tier = Page.Locator("#amount-tier");

        await input.ClickAsync();
        await input.FillAsync("5000");
        await input.PressAsync("Tab");
        await Expect(tier).ToHaveTextAsync("High value order", new() { Timeout = 3000 });
        await Expect(tier).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-rose-600"));

        await input.ClickAsync();
        await input.FillAsync("1000");
        await input.PressAsync("Tab");
        await Expect(tier).ToHaveTextAsync("Standard order", new() { Timeout = 3000 });
        await Expect(tier).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-sky-600"));

        await input.ClickAsync();
        await input.FillAsync("999");
        await input.PressAsync("Tab");
        await Expect(tier).ToHaveTextAsync("Small order", new() { Timeout = 3000 });
        await Expect(tier).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-slate-500"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selecting_pending_directly_fires_else_branch()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "pending" });

        var result = Page.Locator("#status-result");
        await Expect(result).ToContainTextAsync("Pending or empty", new() { Timeout = 3000 });
        await Expect(result).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-slate-500"));

        await Expect(Page.Locator("#address-section")).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    /// <summary>
    /// Exercises every City ElseIf branch in sequence to catch stale sibling field values.
    /// </summary>
    [Test]
    public async Task all_three_cities_autofill_correctly_in_sequence()
    {
        await NavigateAndBoot();

        var citySelect = Page.Locator($"#{S}__Address_City");
        var stateSelect = Page.Locator($"#{S}__Address_State");
        var postalInput = Page.Locator($"#{S}__Address_PostalCode").First;
        var autoText = Page.Locator("#city-auto");

        await citySelect.SelectOptionAsync(new SelectOptionValue { Value = "seattle" });
        await Expect(stateSelect).ToHaveValueAsync("WA", new() { Timeout = 3000 });
        await Expect(postalInput).ToHaveValueAsync(new System.Text.RegularExpressions.Regex("98.?101"), new() { Timeout = 3000 });
        await Expect(autoText).ToContainTextAsync("WA, 98101");

        await citySelect.SelectOptionAsync(new SelectOptionValue { Value = "portland" });
        await Expect(stateSelect).ToHaveValueAsync("OR", new() { Timeout = 3000 });
        await Expect(postalInput).ToHaveValueAsync(new System.Text.RegularExpressions.Regex("97.?201"), new() { Timeout = 3000 });
        await Expect(autoText).ToContainTextAsync("OR, 97201");

        await citySelect.SelectOptionAsync(new SelectOptionValue { Value = "denver" });
        await Expect(stateSelect).ToHaveValueAsync("CO", new() { Timeout = 3000 });
        await Expect(postalInput).ToHaveValueAsync(new System.Text.RegularExpressions.Regex("80.?201"), new() { Timeout = 3000 });
        await Expect(autoText).ToContainTextAsync("CO, 80201");

        AssertNoConsoleErrors();
    }

    /// <summary>
    /// Captures Fusion behavior where programmatic SetValue fires change events and
    /// cascades into another reactive pipeline.
    /// </summary>
    [Test]
    public async Task programmatic_amount_set_cascades_into_tier_pipeline()
    {
        await NavigateAndBoot();

        var amountTier = Page.Locator("#amount-tier");

        await Expect(amountTier).ToHaveTextAsync("\u2014");

        await Page.Locator($"#{S}__Status").SelectOptionAsync(new SelectOptionValue { Value = "active" });

        await Expect(Page.Locator("#status-result")).ToContainTextAsync("Active", new() { Timeout = 3000 });

        await Expect(amountTier).ToHaveTextAsync("Small order", new() { Timeout = 3000 });
        await Expect(amountTier).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-slate-500"));

        AssertNoConsoleErrors();
    }
}
