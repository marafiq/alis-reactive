using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// As the move-in coordinator, I want to record Dorothy Price's care level, room type and meal plan before
/// her move-in day, be warned at once about a choice the community cannot honour, and have the office
/// record what I chose, so that the care team, housekeeping and the kitchen are ready when she arrives.
/// </summary>
/// <remarks>
/// The memory care floor has no suites: the page warns as soon as the two are chosen together, and only
/// the office refuses them when the preferences are saved. A radio group raises change when the
/// coordinator picks an option (HTML, the radio input's input and change events, which the group raises
/// again on its own element). Not exercised: writing the group's value. The old page never wrote it, and a
/// write lands on the group's hidden input without checking a radio (the group's script follows only the
/// radios' own change events), so the radios would go on showing the old choice while the group reads and
/// sends the written one.
/// </remarks>
[TestFixture]
public class WhenRadioOptionSelected : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/NativeRadioGroup";
    private const string Refused = "the server responded with a status of 400";

    private PagePlan<NativeRadioGroupModel> _plan = null!;

    private NativeRadioGroupLocator CareLevel => _plan.NativeRadioGroup(m => m.CareLevel);
    private NativeRadioGroupLocator RoomType => _plan.NativeRadioGroup(m => m.RoomType);
    private NativeRadioGroupLocator MealPlan => _plan.NativeRadioGroup(m => m.MealPlan);
    private ILocator CareLevelError => _plan.ErrorFor(m => m.CareLevel);
    private ILocator RoomTypeError => _plan.ErrorFor(m => m.RoomType);
    private ILocator MealPlanError => _plan.ErrorFor(m => m.MealPlan);
    private ILocator AssessmentNotice => Page.Locator("#assessment-notice");
    private ILocator SuiteWarning => Page.Locator("#suite-warning");
    private ILocator SummaryMeals => Page.Locator("#summary-meals");
    private ILocator SavePreferences => Page.Locator("#save-preferences-btn");
    private ILocator Status => Page.Locator("#preferences-status");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<NativeRadioGroupModel>.FromPage(Page);
    }

    [Test]
    public async Task choosing_a_meal_plan_puts_it_on_the_move_in_summary()
    {
        await NavigateAndBoot();
        await Expect(SummaryMeals).ToHaveTextAsync("Not chosen");

        await MealPlan.Choose("Vegetarian");

        await Expect(SummaryMeals).ToHaveTextAsync("Vegetarian");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task memory_care_books_an_assessment_before_move_in_day()
    {
        await NavigateAndBoot();

        await CareLevel.Choose("Memory Care");

        await Expect(AssessmentNotice).ToHaveTextAsync("A memory care assessment is booked before move-in day.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task other_care_levels_need_no_assessment()
    {
        await NavigateAndBoot();
        await CareLevel.Choose("Memory Care");
        await Expect(AssessmentNotice).ToHaveTextAsync("A memory care assessment is booked before move-in day.");

        await CareLevel.Choose("Assisted Living");

        await Expect(AssessmentNotice).ToHaveTextAsync("No assessment is needed before move-in.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_suite_chosen_for_memory_care_is_warned_against_at_once()
    {
        await NavigateAndBoot();
        await CareLevel.Choose("Memory Care");

        await RoomType.Choose("Private suite");

        await Expect(SuiteWarning).ToHaveTextAsync("The memory care floor has no suites.");
        await Expect(SuiteWarning).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task memory_care_chosen_after_a_suite_is_warned_against_at_once()
    {
        await NavigateAndBoot();
        await RoomType.Choose("Private suite");

        await CareLevel.Choose("Memory Care");

        await Expect(SuiteWarning).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_suite_for_assisted_living_is_not_warned_against()
    {
        await NavigateAndBoot();
        await CareLevel.Choose("Assisted Living");

        await RoomType.Choose("Private suite");

        await Expect(SuiteWarning).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task memory_care_in_a_private_room_is_not_warned_against()
    {
        await NavigateAndBoot();
        await RoomType.Choose("Private room");

        await CareLevel.Choose("Memory Care");

        await Expect(AssessmentNotice).ToHaveTextAsync("A memory care assessment is booked before move-in day.");
        await Expect(SuiteWarning).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_the_preferences_tells_the_coordinator_what_the_office_recorded()
    {
        await NavigateAndBoot();
        await CareLevel.Choose("Assisted Living");
        await RoomType.Choose("Private room");
        await MealPlan.Choose("Vegetarian");

        await ClickWhenStable(SavePreferences);

        await Expect(Status).ToHaveTextAsync("Move-in preferences saved for Dorothy Price: Assisted Living, Private room, Vegetarian meals.");
        AssertNoConsoleErrors();
    }

    // Refused in the browser: preferences sent to the server would answer 400, a console error.
    [Test]
    public async Task saving_with_nothing_chosen_says_what_is_missing()
    {
        await NavigateAndBoot();

        await ClickWhenStable(SavePreferences);

        await Expect(CareLevelError).ToHaveTextAsync("Choose the care level.");
        await Expect(RoomTypeError).ToHaveTextAsync("Choose the room type.");
        await Expect(MealPlanError).ToHaveTextAsync("Choose the meal plan.");
        await Expect(Status).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    // A radio never reaches the group's own element with input or blur; only the group's change rechecks it.
    [Test]
    public async Task choosing_a_room_type_clears_its_refusal_without_saving_again()
    {
        await NavigateAndBoot();
        await ClickWhenStable(SavePreferences);
        await Expect(RoomTypeError).ToHaveTextAsync("Choose the room type.");

        await RoomType.Choose("Private room");

        await Expect(RoomTypeError).ToBeHiddenAsync();
        await Expect(MealPlanError).ToHaveTextAsync("Choose the meal plan.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_office_refuses_a_suite_on_the_memory_care_floor()
    {
        await NavigateAndBoot();
        await CareLevel.Choose("Memory Care");
        await RoomType.Choose("Private suite");
        await MealPlan.Choose("Diabetic");

        await ClickWhenStable(SavePreferences);

        await Expect(RoomTypeError).ToHaveTextAsync("The memory care floor has no suites. Choose a private or shared room.");
        await Expect(Status).ToHaveTextAsync("");
        AssertNoConsoleErrorsExcept(Refused);
    }
}
