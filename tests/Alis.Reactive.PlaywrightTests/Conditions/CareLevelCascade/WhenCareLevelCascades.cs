using Alis.Reactive.PlaywrightTests.Support.Controls;

namespace Alis.Reactive.PlaywrightTests.Conditions.CareLevelCascade;

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
        await WaitForPageReady(10000);
    }

    private async Task SelectCareLevelAndWait(string text)
    {
        await CareLevel.Select(text);

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
