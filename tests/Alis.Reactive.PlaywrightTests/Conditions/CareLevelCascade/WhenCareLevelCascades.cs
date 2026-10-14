using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.CareLevelCascade;

namespace Alis.Reactive.PlaywrightTests.Conditions.CareLevelCascade;

/// <summary>
/// As a care coordinator updating a resident's care after a reassessment, I want the care protocol to
/// follow the care level I choose and the community's escort policy applied when I ask for it, so that a
/// resident moving into memory care or skilled nursing gets the monitoring and the escort the policy
/// requires, and one moving out of them keeps neither.
/// </summary>
/// <remarks>
/// Walter Cho starts in assisted living on the basic support protocol, going out without an escort. Memory
/// care puts him on enhanced monitoring, skilled nursing on the full clinical protocol, any other level
/// back on basic support. The escort policy reads the care level as it stands when it is applied: memory
/// care and skilled nursing go out with an escort, every other level without, whatever was switched by
/// hand. Everything the page renders is exercised.
/// </remarks>
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

    // He goes out without an escort until the policy is applied: the switch moves only on the button.
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
