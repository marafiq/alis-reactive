using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Conditions.CareLevelCascade;

/// <summary>
/// Exercises condition branches that mutate other components through SetValue and SetChecked.
/// </summary>
[TestFixture]
public class WhenCareLevelCascades : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/CareLevelCascade";

    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_CareLevelModel";
    private const string CareLevelId = GeneratedTypeScope + "__CareLevel";
    private const string ProtocolId = GeneratedTypeScope + "__Protocol";
    private const string RequiresEscortId = GeneratedTypeScope + "__RequiresEscort";

    private DropDownListLocator CareLevel => new(Page, CareLevelId);
    private DropDownListLocator Protocol => new(Page, ProtocolId);
    private SwitchLocator RequiresEscort => new(Page, RequiresEscortId);

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    private async Task SelectCareLevelAndWait(string text)
    {
        await CareLevel.Select(text);

        // Wait for the cascade to confirm the change event fired.
        await Expect(Page.Locator("#s1-current-level"))
            .ToHaveTextAsync(text, new() { Timeout = 5000 });
    }

    [Test]
    public async Task memory_care_sets_protocol_to_enhanced_monitoring()
    {
        await NavigateAndBoot();

        await SelectCareLevelAndWait("Memory Care");

        await Expect(Page.Locator("#s1-current-level"))
            .ToHaveTextAsync("Memory Care", new() { Timeout = 5000 });

        await Expect(Protocol.Input).ToHaveValueAsync("Enhanced Monitoring", new() { Timeout = 5000 });

        await Expect(Page.Locator("#s1-action-status"))
            .ToHaveTextAsync("cascade-complete", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task skilled_nursing_sets_protocol_to_full_clinical()
    {
        await NavigateAndBoot();

        await SelectCareLevelAndWait("Skilled Nursing");

        await Expect(Page.Locator("#s1-current-level"))
            .ToHaveTextAsync("Skilled Nursing", new() { Timeout = 5000 });

        await Expect(Protocol.Input).ToHaveValueAsync("Full Clinical", new() { Timeout = 5000 });

        await Expect(Page.Locator("#s1-action-status"))
            .ToHaveTextAsync("cascade-complete", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task independent_clears_protocol_dropdown()
    {
        await NavigateAndBoot();

        await SelectCareLevelAndWait("Memory Care");
        await Expect(Protocol.Input).ToHaveValueAsync("Enhanced Monitoring", new() { Timeout = 5000 });

        await SelectCareLevelAndWait("Independent");

        await Expect(Page.Locator("#s1-current-level"))
            .ToHaveTextAsync("Independent", new() { Timeout = 5000 });

        // Empty value makes Syncfusion show the placeholder.
        await Expect(Protocol.Input).ToHaveValueAsync("", new() { Timeout = 5000 });

        await Expect(Page.Locator("#s1-action-status"))
            .ToHaveTextAsync("cascade-complete", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task assisted_living_clears_protocol_dropdown()
    {
        await NavigateAndBoot();

        await SelectCareLevelAndWait("Skilled Nursing");
        await Expect(Protocol.Input).ToHaveValueAsync("Full Clinical", new() { Timeout = 5000 });

        await SelectCareLevelAndWait("Assisted Living");

        await Expect(Protocol.Input).ToHaveValueAsync("", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task cascade_transitions_across_all_care_levels()
    {
        await NavigateAndBoot();

        await SelectCareLevelAndWait("Memory Care");
        await Expect(Protocol.Input).ToHaveValueAsync("Enhanced Monitoring", new() { Timeout = 5000 });

        await SelectCareLevelAndWait("Skilled Nursing");
        await Expect(Protocol.Input).ToHaveValueAsync("Full Clinical", new() { Timeout = 5000 });

        await SelectCareLevelAndWait("Independent");
        await Expect(Protocol.Input).ToHaveValueAsync("", new() { Timeout = 5000 });

        await SelectCareLevelAndWait("Memory Care");
        await Expect(Protocol.Input).ToHaveValueAsync("Enhanced Monitoring", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task memory_care_enables_escort_requirement()
    {
        await NavigateAndBoot();

        await SelectCareLevelAndWait("Memory Care");
        await Page.Locator("#s2-apply-btn").ClickAsync();

        await Expect(Page.Locator("#s2-checking"))
            .ToHaveTextAsync("Memory Care", new() { Timeout = 5000 });

        await Expect(RequiresEscort.Input).ToBeCheckedAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#s2-result"))
            .ToHaveTextAsync("escort required", new() { Timeout = 3000 });

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

        await SelectCareLevelAndWait("Memory Care");
        await Page.Locator("#s2-apply-btn").ClickAsync();
        await Expect(RequiresEscort.Input).ToBeCheckedAsync(new() { Timeout = 5000 });

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

        await SelectCareLevelAndWait("Memory Care");
        await Page.Locator("#s2-apply-btn").ClickAsync();
        await Expect(RequiresEscort.Input).ToBeCheckedAsync(new() { Timeout = 5000 });

        await SelectCareLevelAndWait("Independent");
        await Page.Locator("#s2-apply-btn").ClickAsync();
        await Expect(RequiresEscort.Input).Not.ToBeCheckedAsync(new() { Timeout = 5000 });

        await SelectCareLevelAndWait("Skilled Nursing");
        await Page.Locator("#s2-apply-btn").ClickAsync();
        await Expect(RequiresEscort.Input).ToBeCheckedAsync(new() { Timeout = 5000 });

        await SelectCareLevelAndWait("Assisted Living");
        await Page.Locator("#s2-apply-btn").ClickAsync();
        await Expect(RequiresEscort.Input).Not.ToBeCheckedAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
