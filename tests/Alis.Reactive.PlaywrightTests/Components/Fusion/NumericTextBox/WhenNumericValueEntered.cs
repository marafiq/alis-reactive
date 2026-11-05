using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.NumericTextBox;

[TestFixture]
public class WhenNumericValueEntered : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/NumericTextBox";
    private static readonly string MealsId = IdGenerator.For<NumericTextBoxModel>(m => m.MealsPerWeek);
    private static readonly string WellnessId = IdGenerator.For<NumericTextBoxModel>(m => m.WellnessChecksPerWeek);

    private NumericTextBoxLocator Meals => new(Page, MealsId);
    private NumericTextBoxLocator Wellness => new(Page, WellnessId);

    private ILocator MealsSummary => Page.Locator("#meals-summary");
    private ILocator MealsPrevious => Page.Locator("#meals-previous");
    private ILocator MealsSource => Page.Locator("#meals-source");
    private ILocator MealsFloorNote => Page.Locator("#meals-floor-note");
    private ILocator WellnessHint => Page.Locator("#wellness-hint");
    private ILocator PlanConfirmation => Page.Locator("#plan-confirmation");

    private ILocator AddMeal => Page.Locator("#meals-add");
    private ILocator RemoveMeal => Page.Locator("#meals-remove");
    private ILocator ApplyStandardPlan => Page.Locator("#meals-standard-plan");
    private ILocator AllowReducedDiet => Page.Locator("#meals-reduced-diet");
    private ILocator StartWellness => Page.Locator("#wellness-start");
    private ILocator DoneWellness => Page.Locator("#wellness-done");
    private ILocator SavePlan => Page.Locator("#plan-save");

    private async Task OpenPlan()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Meals.Input).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task plan_opens_showing_the_meals_carried_over_from_last_month()
    {
        await OpenPlan();

        await Expect(Meals.Input).ToHaveValueAsync("7.00");
        await Expect(Wellness.Input).ToHaveValueAsync("2.00");
        await Expect(MealsSummary).ToHaveTextAsync("7");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task entering_a_new_meal_count_updates_what_the_resident_receives()
    {
        await OpenPlan();

        await Meals.FillAndBlur("10");

        await Expect(Meals.Input).ToHaveValueAsync("10.00");
        await Expect(MealsSummary).ToHaveTextAsync("10");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task lowering_the_meal_count_records_what_it_changed_from()
    {
        await OpenPlan();

        await Meals.FillAndBlur("5");

        await Expect(MealsSummary).ToHaveTextAsync("5");
        await Expect(MealsPrevious).ToHaveTextAsync("7");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_meal_count_the_coordinator_typed_is_marked_as_entered()
    {
        await OpenPlan();

        await Meals.FillAndBlur("9");

        await Expect(MealsSource).ToHaveTextAsync("You entered this number of meals.");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_meal_count_applied_from_a_template_is_marked_as_applied()
    {
        await OpenPlan();

        await ApplyStandardPlan.ClickAsync();

        await Expect(MealsSource).ToHaveTextAsync("This number of meals was applied from a plan template.");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task adding_a_meal_raises_the_weekly_meal_count_by_one()
    {
        await OpenPlan();

        await AddMeal.ClickAsync();

        await Expect(Meals.Input).ToHaveValueAsync("8.00");
        await Expect(MealsSummary).ToHaveTextAsync("8");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task removing_a_meal_lowers_the_weekly_meal_count_by_one()
    {
        await OpenPlan();

        await RemoveMeal.ClickAsync();

        await Expect(Meals.Input).ToHaveValueAsync("6.00");
        await Expect(MealsSummary).ToHaveTextAsync("6");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task applying_the_standard_plan_sets_meals_to_fourteen()
    {
        await OpenPlan();

        await ApplyStandardPlan.ClickAsync();

        await Expect(Meals.Input).ToHaveValueAsync("14.00");
        await Expect(MealsSummary).ToHaveTextAsync("14");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task allowing_a_reduced_diet_lets_the_meal_count_drop_below_the_standard_floor()
    {
        await OpenPlan();

        await Meals.FillAndBlur("3");
        await Expect(Meals.Input).ToHaveValueAsync("4.00");

        await AllowReducedDiet.ClickAsync();
        await Expect(MealsFloorNote)
            .ToHaveTextAsync("A reduced-diet plan is now allowed — you can enter as few as 2 meals.");

        await Meals.FillAndBlur("3");

        await Expect(Meals.Input).ToHaveValueAsync("3.00");
        await Expect(MealsSummary).ToHaveTextAsync("3");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selecting_the_wellness_field_shows_guidance_on_check_ins()
    {
        await OpenPlan();

        await Expect(WellnessHint).ToHaveTextAsync("Select the field to see guidance on wellness check-ins.");

        await Wellness.Focus();

        await Expect(WellnessHint)
            .ToHaveTextAsync("A nurse visits for each wellness check-in. Most residents have 2 to 5 per week.");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task leaving_the_wellness_field_tidies_the_guidance()
    {
        await OpenPlan();

        await Wellness.Focus();
        await Expect(WellnessHint)
            .ToHaveTextAsync("A nurse visits for each wellness check-in. Most residents have 2 to 5 per week.");

        await Wellness.Blur();

        await Expect(WellnessHint).ToHaveTextAsync("Wellness check-ins saved with the plan below.");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task start_entering_wellness_moves_the_cursor_into_the_field()
    {
        await OpenPlan();

        await Expect(WellnessHint).ToHaveTextAsync("Select the field to see guidance on wellness check-ins.");

        await StartWellness.ClickAsync();

        await Expect(Wellness.Input).ToBeFocusedAsync();
        await Expect(WellnessHint)
            .ToHaveTextAsync("A nurse visits for each wellness check-in. Most residents have 2 to 5 per week.");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task done_with_wellness_moves_the_cursor_out_of_the_field()
    {
        await OpenPlan();

        await StartWellness.ClickAsync();
        await Expect(Wellness.Input).ToBeFocusedAsync();

        await DoneWellness.ClickAsync();

        await Expect(Wellness.Input).Not.ToBeFocusedAsync();
        await Expect(WellnessHint).ToHaveTextAsync("Wellness check-ins saved with the plan below.");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_the_plan_confirms_the_meal_count_to_the_coordinator()
    {
        await OpenPlan();

        await Meals.FillAndBlur("12");
        await SavePlan.ClickAsync();

        await Expect(PlanConfirmation)
            .ToHaveTextAsync("Saved. This resident will receive 12 catered meals each week.",
                new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_the_plan_posts_the_meal_count_to_the_server()
    {
        await OpenPlan();

        await Meals.FillAndBlur("12");

        var requestTask = Page.WaitForRequestAsync(request =>
            request.Url.Contains("/Sandbox/Components/NumericTextBox/Save") && request.Method == "POST",
            new() { Timeout = 10000 });

        await SavePlan.ClickAsync();

        var request = await requestTask;
        var body = request.PostData ?? "";

        Assert.That(body, Does.Contain("\"MealsPerWeek\":12"),
            "the gather pipeline must carry the meals Value() source under its declared key");

        AssertNoConsoleErrors();
    }
}
