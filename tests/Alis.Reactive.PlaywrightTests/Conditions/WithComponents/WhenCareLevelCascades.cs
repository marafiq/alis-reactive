using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Conditions.WithComponents;

/// <summary>
/// Exercises condition → component mutation patterns end-to-end:
///   Section 1: DropDown condition → SetValue on another DropDown (cascade)
///   Section 2: DropDown condition → SetChecked on Switch (cross-component type)
///
/// Page under test: /Sandbox/Conditions/CareLevelCascade
///
/// This is the first test where condition branches MUTATE other components.
/// All prior condition tests only flip element text/visibility. This slice proves
/// Then/Else branches can SetValue on dropdowns and SetChecked on switches.
///
/// Senior living domain: care level drives protocol assignment and escort requirements.
/// </summary>
[TestFixture]
public class WhenCareLevelCascades : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/CareLevelCascade";

    private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_CareLevelModel";
    private const string CareLevelId = Scope + "__CareLevel";
    private const string ProtocolId = Scope + "__Protocol";
    private const string RequiresEscortId = Scope + "__RequiresEscort";

    private DropDownListLocator CareLevel => new(Page, CareLevelId);
    private DropDownListLocator Protocol => new(Page, ProtocolId);
    private SwitchLocator RequiresEscort => new(Page, RequiresEscortId);

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    /// <summary>
    /// Select a care level via SF DDL's native API, then wait for the cascade.
    /// Uses the ej2 instance value property which triggers the SF change event
    /// the same way a user selection does. More robust than keyboard ArrowDown
    /// which doesn't wrap for re-selections in SF DDL.
    /// </summary>
    private async Task SelectCareLevelAndWait(string text)
    {
        await Page.EvaluateAsync($@"(() => {{
            const el = document.getElementById('{CareLevelId}');
            const ej2 = el.ej2_instances[0];
            ej2.value = '{text}';
            ej2.dataBind();
        }})()");

        // Wait for the cascade to confirm the change event fired
        await Expect(Page.Locator("#s1-current-level"))
            .ToHaveTextAsync(text, new() { Timeout = 5000 });
    }

    // ── Section 1: Condition → SetValue on another dropdown ──

    [Test]
    public async Task memory_care_sets_protocol_to_enhanced_monitoring()
    {
        await NavigateAndBoot();

        await SelectCareLevelAndWait("Memory Care");

        // Before: current level updated
        await Expect(Page.Locator("#s1-current-level"))
            .ToHaveTextAsync("Memory Care", new() { Timeout = 5000 });

        // Condition output: protocol dropdown got SetValue("Enhanced Monitoring")
        await Expect(Protocol.Input).ToHaveValueAsync("Enhanced Monitoring", new() { Timeout = 5000 });

        // After: action status confirms cascade ran
        await Expect(Page.Locator("#s1-action-status"))
            .ToHaveTextAsync("cascade-complete", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task skilled_nursing_sets_protocol_to_full_clinical()
    {
        await NavigateAndBoot();

        await SelectCareLevelAndWait("Skilled Nursing");

        // Before: current level updated
        await Expect(Page.Locator("#s1-current-level"))
            .ToHaveTextAsync("Skilled Nursing", new() { Timeout = 5000 });

        // Condition output: protocol dropdown got SetValue("Full Clinical")
        await Expect(Protocol.Input).ToHaveValueAsync("Full Clinical", new() { Timeout = 5000 });

        // After: cascade complete
        await Expect(Page.Locator("#s1-action-status"))
            .ToHaveTextAsync("cascade-complete", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task independent_clears_protocol_dropdown()
    {
        await NavigateAndBoot();

        // First set to Memory Care to populate protocol
        await SelectCareLevelAndWait("Memory Care");
        await Expect(Protocol.Input).ToHaveValueAsync("Enhanced Monitoring", new() { Timeout = 5000 });

        // Now switch to Independent → protocol should clear
        await SelectCareLevelAndWait("Independent");

        await Expect(Page.Locator("#s1-current-level"))
            .ToHaveTextAsync("Independent", new() { Timeout = 5000 });

        // Protocol cleared (empty value — SF shows placeholder)
        await Expect(Protocol.Input).ToHaveValueAsync("", new() { Timeout = 5000 });

        await Expect(Page.Locator("#s1-action-status"))
            .ToHaveTextAsync("cascade-complete", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task assisted_living_clears_protocol_dropdown()
    {
        await NavigateAndBoot();

        // Set to Skilled Nursing first
        await SelectCareLevelAndWait("Skilled Nursing");
        await Expect(Protocol.Input).ToHaveValueAsync("Full Clinical", new() { Timeout = 5000 });

        // Assisted Living is Else branch → protocol clears
        await SelectCareLevelAndWait("Assisted Living");

        await Expect(Protocol.Input).ToHaveValueAsync("", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task cascade_transitions_across_all_care_levels()
    {
        await NavigateAndBoot();

        // Memory Care → Enhanced Monitoring
        await SelectCareLevelAndWait("Memory Care");
        await Expect(Protocol.Input).ToHaveValueAsync("Enhanced Monitoring", new() { Timeout = 5000 });

        // Skilled Nursing → Full Clinical
        await SelectCareLevelAndWait("Skilled Nursing");
        await Expect(Protocol.Input).ToHaveValueAsync("Full Clinical", new() { Timeout = 5000 });

        // Independent → cleared
        await SelectCareLevelAndWait("Independent");
        await Expect(Protocol.Input).ToHaveValueAsync("", new() { Timeout = 5000 });

        // Back to Memory Care → Enhanced Monitoring (no sticky state)
        await SelectCareLevelAndWait("Memory Care");
        await Expect(Protocol.Input).ToHaveValueAsync("Enhanced Monitoring", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // ── Section 2: Condition → SetChecked on Switch ──

    [Test]
    public async Task memory_care_enables_escort_requirement()
    {
        await NavigateAndBoot();

        await SelectCareLevelAndWait("Memory Care");
        await Page.Locator("#s2-apply-btn").ClickAsync();

        // Before: checking shows care level
        await Expect(Page.Locator("#s2-checking"))
            .ToHaveTextAsync("Memory Care", new() { Timeout = 5000 });

        // Condition output: switch checked + text
        await Expect(RequiresEscort.Input).ToBeCheckedAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#s2-result"))
            .ToHaveTextAsync("escort required", new() { Timeout = 3000 });

        // After: policy applied
        await Expect(Page.Locator("#s2-policy-status"))
            .ToHaveTextAsync("policy-applied", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task skilled_nursing_enables_escort_requirement()
    {
        await NavigateAndBoot();

        await SelectCareLevelAndWait("Skilled Nursing");
        await Page.Locator("#s2-apply-btn").ClickAsync();

        await Expect(RequiresEscort.Input).ToBeCheckedAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#s2-result"))
            .ToHaveTextAsync("escort required", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task independent_disables_escort_requirement()
    {
        await NavigateAndBoot();

        // First enable escort via Memory Care
        await SelectCareLevelAndWait("Memory Care");
        await Page.Locator("#s2-apply-btn").ClickAsync();
        await Expect(RequiresEscort.Input).ToBeCheckedAsync(new() { Timeout = 5000 });

        // Now switch to Independent → escort unchecked
        await SelectCareLevelAndWait("Independent");
        await Page.Locator("#s2-apply-btn").ClickAsync();

        await Expect(RequiresEscort.Input).Not.ToBeCheckedAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#s2-result"))
            .ToHaveTextAsync("no escort needed", new() { Timeout = 3000 });

        await Expect(Page.Locator("#s2-policy-status"))
            .ToHaveTextAsync("policy-applied", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task escort_policy_transitions_with_care_level_changes()
    {
        await NavigateAndBoot();

        // Memory Care → escort on
        await SelectCareLevelAndWait("Memory Care");
        await Page.Locator("#s2-apply-btn").ClickAsync();
        await Expect(RequiresEscort.Input).ToBeCheckedAsync(new() { Timeout = 5000 });

        // Independent → escort off
        await SelectCareLevelAndWait("Independent");
        await Page.Locator("#s2-apply-btn").ClickAsync();
        await Expect(RequiresEscort.Input).Not.ToBeCheckedAsync(new() { Timeout = 5000 });

        // Skilled Nursing → escort on again
        await SelectCareLevelAndWait("Skilled Nursing");
        await Page.Locator("#s2-apply-btn").ClickAsync();
        await Expect(RequiresEscort.Input).ToBeCheckedAsync(new() { Timeout = 5000 });

        // Assisted Living → escort off
        await SelectCareLevelAndWait("Assisted Living");
        await Page.Locator("#s2-apply-btn").ClickAsync();
        await Expect(RequiresEscort.Input).Not.ToBeCheckedAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
