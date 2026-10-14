using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.AutoComplete;

namespace Alis.Reactive.PlaywrightTests.Patterns.FormPatterns;

/// <summary>
/// As a test author, I want a page's inputs found from the model members they are bound to, so that a test
/// names the physician field as <c>m =&gt; m.Physician</c> and a renamed member breaks the test at compile time.
/// </summary>
/// <remarks>
/// Uses the Notify Physician page: a Syncfusion AutoComplete bound to <c>Physician</c> and a native text
/// box bound to <c>Concern</c>.
/// </remarks>
[TestFixture]
public class WhenPlanDrivesComponentDiscovery : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/AutoComplete";

    private PagePlan<FusionAutoCompleteModel> _plan = null!;

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<FusionAutoCompleteModel>.FromPage(Page);
    }

    [Test]
    public async Task the_plan_finds_each_input_by_the_model_member_it_is_bound_to()
    {
        await NavigateAndBoot();

        var physician = _plan.FindComponent(m => m.Physician);
        var concern = _plan.FindComponent(m => m.Concern);

        Assert.That(physician, Is.Not.Null);
        Assert.That(physician!.Vendor, Is.EqualTo("fusion"));
        Assert.That(concern, Is.Not.Null);
        Assert.That(concern!.Vendor, Is.EqualTo("native"));
        await Expect(Page.Locator($"#{physician.ElementId}")).ToBeEditableAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_field_found_through_the_plan_takes_the_nurses_typing()
    {
        await NavigateAndBoot();
        var physician = _plan.AutoComplete(m => m.Physician);

        await physician.Type("oka");

        await Expect(physician.PopupItem("Dr. James Okafor")).ToBeVisibleAsync(new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }
}
