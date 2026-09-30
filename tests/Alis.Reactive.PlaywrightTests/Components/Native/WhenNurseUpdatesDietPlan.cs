namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// As a nurse, I want a resident's diet plan to open with the dietary restrictions on file checked,
/// so that the kitchen keeps following them after I save, and only the changes I make are applied.
/// </summary>
[TestFixture]
public class WhenNurseUpdatesDietPlan : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/NativeCheckList/DietPlan";

    private ILocator Restriction(string name) => Page.GetByRole(AriaRole.Checkbox, new() { Name = name, Exact = true });
    private ILocator SaveDietPlan => Page.Locator("#save-diet-plan-btn");
    private ILocator DietPlanStatus => Page.Locator("#diet-plan-status");

    private async Task OpenHaroldJenningsDietPlan()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    [Test]
    public async Task the_restrictions_on_file_are_checked_when_the_diet_plan_opens()
    {
        await OpenHaroldJenningsDietPlan();

        await Expect(Restriction("Gluten-free")).ToBeCheckedAsync();
        await Expect(Restriction("Low sodium")).ToBeCheckedAsync();
        await Expect(Restriction("Pureed texture")).Not.ToBeCheckedAsync();
        await Expect(Restriction("Diabetic")).Not.ToBeCheckedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_without_changes_keeps_the_restrictions_on_file()
    {
        await OpenHaroldJenningsDietPlan();

        await ClickWhenStable(SaveDietPlan);

        await Expect(DietPlanStatus).ToHaveTextAsync("Diet plan saved for Harold Jennings: Gluten-free, Low sodium.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task removing_a_restriction_on_file_saves_the_rest()
    {
        await OpenHaroldJenningsDietPlan();

        await Restriction("Gluten-free").UncheckAsync();
        await ClickWhenStable(SaveDietPlan);

        await Expect(DietPlanStatus).ToHaveTextAsync("Diet plan saved for Harold Jennings: Low sodium.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_every_restriction_saves_none()
    {
        await OpenHaroldJenningsDietPlan();

        await Restriction("Gluten-free").UncheckAsync();
        await Restriction("Low sodium").UncheckAsync();
        await ClickWhenStable(SaveDietPlan);

        await Expect(DietPlanStatus).ToHaveTextAsync("Diet plan saved for Harold Jennings: no dietary restrictions.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task adding_a_restriction_saves_it_with_the_ones_on_file()
    {
        await OpenHaroldJenningsDietPlan();

        await Restriction("Pureed texture").CheckAsync();
        await ClickWhenStable(SaveDietPlan);

        await Expect(DietPlanStatus).ToHaveTextAsync(
            "Diet plan saved for Harold Jennings: Gluten-free, Low sodium, Pureed texture.");
        AssertNoConsoleErrors();
    }
}
