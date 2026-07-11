using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.NumericTextBox;

// Journey: a care coordinator sets a resident's Monthly Service Plan — how many catered
// meals and wellness check-ins the resident receives each week. The plan carries over from
// last month; the coordinator adjusts meals with steppers and plan templates, gets guidance
// on wellness check-ins, then saves the plan.
[TestFixture]
public class WhenNumericValueEntered : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/NumericTextBox";
    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_NumericTextBoxModel";
    private const string MealsId = GeneratedTypeScope + "__MealsPerWeek";
    private const string WellnessId = GeneratedTypeScope + "__WellnessChecksPerWeek";

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

    // RENDERS — the FusionNumericTextBox builder renders both fields bound to the plan carried
    // over from last month: 7 meals and 2 wellness check-ins.
    [Test]
    public async Task plan_opens_showing_the_meals_carried_over_from_last_month()
    {
        await OpenPlan();

        await Expect(Meals.Input).ToHaveValueAsync("7.00");
        await Expect(Wellness.Input).ToHaveValueAsync("2.00");
        await Expect(MealsSummary).ToHaveTextAsync("7");

        AssertNoConsoleErrors();
    }

    // INTERACTS — typing a new meal count fires Changed through the .Reactive wiring; the
    // FusionNumericTextBoxChangeArgs.Value carries the new number into the visible summary.
    [Test]
    public async Task entering_a_new_meal_count_updates_what_the_resident_receives()
    {
        await OpenPlan();

        await Meals.FillAndBlur("10");

        await Expect(Meals.Input).ToHaveValueAsync("10.00");
        await Expect(MealsSummary).ToHaveTextAsync("10");

        AssertNoConsoleErrors();
    }

    // FusionNumericTextBoxChangeArgs.PreviousValue carries the value before the change: lowering
    // 7 to 5 records the prior 7.
    [Test]
    public async Task lowering_the_meal_count_records_what_it_changed_from()
    {
        await OpenPlan();

        await Meals.FillAndBlur("5");

        await Expect(MealsSummary).ToHaveTextAsync("5");
        await Expect(MealsPrevious).ToHaveTextAsync("7");

        AssertNoConsoleErrors();
    }

    // FusionNumericTextBoxChangeArgs.IsInteracted is true when the coordinator types the value
    // themselves; the source line reads as an entered number.
    [Test]
    public async Task a_meal_count_the_coordinator_typed_is_marked_as_entered()
    {
        await OpenPlan();

        await Meals.FillAndBlur("9");

        await Expect(MealsSource).ToHaveTextAsync("You entered this number of meals.");

        AssertNoConsoleErrors();
    }

    // IsInteracted is false when a plan template writes the value through SetValue; the same source
    // line reads as applied-from-template, distinguishing it from a typed entry.
    [Test]
    public async Task a_meal_count_applied_from_a_template_is_marked_as_applied()
    {
        await OpenPlan();

        await ApplyStandardPlan.ClickAsync();

        await Expect(MealsSource).ToHaveTextAsync("This number of meals was applied from a plan template.");

        AssertNoConsoleErrors();
    }

    // Increment raises the meal count by one step; the summary follows.
    [Test]
    public async Task adding_a_meal_raises_the_weekly_meal_count_by_one()
    {
        await OpenPlan();

        await AddMeal.ClickAsync();

        await Expect(Meals.Input).ToHaveValueAsync("8.00");
        await Expect(MealsSummary).ToHaveTextAsync("8");

        AssertNoConsoleErrors();
    }

    // Decrement lowers the meal count by one step; the summary follows.
    [Test]
    public async Task removing_a_meal_lowers_the_weekly_meal_count_by_one()
    {
        await OpenPlan();

        await RemoveMeal.ClickAsync();

        await Expect(Meals.Input).ToHaveValueAsync("6.00");
        await Expect(MealsSummary).ToHaveTextAsync("6");

        AssertNoConsoleErrors();
    }

    // SetValue writes the standard plan's meal count (14) onto the field.
    [Test]
    public async Task applying_the_standard_plan_sets_meals_to_fourteen()
    {
        await OpenPlan();

        await ApplyStandardPlan.ClickAsync();

        await Expect(Meals.Input).ToHaveValueAsync("14.00");
        await Expect(MealsSummary).ToHaveTextAsync("14");

        AssertNoConsoleErrors();
    }

    // SetMin lowers the field's minimum so a reduced-diet plan can go below the standard floor of 4.
    // Before: entering 3 clamps up to the minimum of 4. After SetMin(2): entering 3 sticks at 3.
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

    // Focus fires when the wellness field gains focus, showing nurse-visit guidance.
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

    // Blur fires when the wellness field loses focus, tidying the guidance to a saved-state note.
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

    // FocusIn moves focus into the wellness field, which fires Focus and shows the guidance —
    // proving FocusIn placed the cursor there without the coordinator clicking the field.
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

    // FocusOut removes focus from the wellness field, which fires Blur and tidies the guidance —
    // proving FocusOut took the cursor out without the coordinator tabbing away.
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

    // SUBMITS — the Value() source feeds the gather body; the server confirmation the coordinator
    // sees reflects the saved meal count.
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

    // GATHERS — the framework gather pipeline carries the meals Value() source into the POST body
    // under the declared key. (Framework gather test: asserts request.PostData.)
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

        Assert.That(body, Does.Contain("\"mealsPerWeek\":12"),
            "the gather pipeline must carry the meals Value() source under its declared key");

        AssertNoConsoleErrors();
    }
}
