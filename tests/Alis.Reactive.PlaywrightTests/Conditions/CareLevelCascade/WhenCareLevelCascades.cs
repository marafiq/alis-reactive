using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.CareLevelCascade;

namespace Alis.Reactive.PlaywrightTests.Conditions.CareLevelCascade;

[TestFixture]
public class WhenCareLevelCascades : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/CareLevelCascade";

    private PagePlan<CareLevelModel> _plan = null!;

    private DropDownListLocator CareLevel => _plan.DropDownList(m => m.CareLevel);
    private DropDownListLocator Protocol => _plan.DropDownList(m => m.Protocol);
    private SwitchLocator RequiresEscort => _plan.Switch(m => m.RequiresEscort);
    private ILocator ApplyTheEscortPolicy => Page.Locator("#apply-escort-policy-btn");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<CareLevelModel>.FromPage(Page);
        await Expect(Protocol.Input).ToHaveValueAsync("Basic Support", new() { Timeout = 5000 });
    }

    [Test]
    public async Task moving_to_memory_care_puts_the_resident_on_enhanced_monitoring()
    {
        await NavigateAndBoot();

        await CareLevel.Select("Memory Care");

        await Expect(Protocol.Input).ToHaveValueAsync("Enhanced Monitoring", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task moving_to_skilled_nursing_puts_the_resident_on_the_full_clinical_protocol()
    {
        await NavigateAndBoot();

        await CareLevel.Select("Skilled Nursing");

        await Expect(Protocol.Input).ToHaveValueAsync("Full Clinical", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task moving_from_memory_care_to_independent_living_returns_the_resident_to_basic_support()
    {
        await NavigateAndBoot();
        await CareLevel.Select("Memory Care");
        await Expect(Protocol.Input).ToHaveValueAsync("Enhanced Monitoring", new() { Timeout = 5000 });

        await CareLevel.Select("Independent");

        await Expect(Protocol.Input).ToHaveValueAsync("Basic Support", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task applying_the_escort_policy_to_a_memory_care_resident_requires_an_escort()
    {
        await NavigateAndBoot();
        await CareLevel.Select("Memory Care");
        await Expect(Protocol.Input).ToHaveValueAsync("Enhanced Monitoring", new() { Timeout = 5000 });
        await Expect(RequiresEscort.Input).Not.ToBeCheckedAsync();

        await ClickWhenStable(ApplyTheEscortPolicy);

        await Expect(RequiresEscort.Input).ToBeCheckedAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task applying_the_escort_policy_to_a_skilled_nursing_resident_requires_an_escort()
    {
        await NavigateAndBoot();
        await CareLevel.Select("Skilled Nursing");
        await Expect(Protocol.Input).ToHaveValueAsync("Full Clinical", new() { Timeout = 5000 });

        await ClickWhenStable(ApplyTheEscortPolicy);

        await Expect(RequiresEscort.Input).ToBeCheckedAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task applying_the_escort_policy_to_an_assisted_living_resident_turns_off_an_escort_switched_on_by_hand()
    {
        await NavigateAndBoot();
        await RequiresEscort.Toggle();
        await Expect(RequiresEscort.Input).ToBeCheckedAsync(new() { Timeout = 5000 });

        await ClickWhenStable(ApplyTheEscortPolicy);

        await Expect(RequiresEscort.Input).Not.ToBeCheckedAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
