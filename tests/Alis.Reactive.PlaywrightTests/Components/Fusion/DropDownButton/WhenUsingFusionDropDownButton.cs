using System.Text.RegularExpressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.DropDownButton;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.DropDownButton;

/// <summary>
/// As a charge nurse at the end of the evening shift, I want to print my wing's reports from one menu, so that
/// the night shift starts with the handoff on paper and no report is printed twice.
/// </summary>
/// <remarks>
/// The menu offers Shift handoff, Medication pass, Incident summary and Midnight census, which waits for the
/// midnight head count. It stays locked until a wing is chosen; choosing one unlocks it and puts the keyboard
/// on it. A disabled button takes no focus, so the unlock is applied with DataBind before FocusIn; without
/// DataBind the button is still disabled when FocusIn runs and never gets the keyboard. No incident was logged
/// this shift, so the incident summary has nothing to print.
/// Not exercised: reading Content, Disabled and CssClass back (no step here needs the menu's own label, state
/// or classes; the writes below prove the same JavaScript members); Toggle closing an open menu (every gesture
/// that reaches the page while the menu is open closes it on its own, a click outside or on an item);
/// Select's Item.Disabled (a disabled item cannot be clicked or reached by the arrow keys, so every Select the
/// nurse can raise carries false and nothing on the page could differ).
/// </remarks>
[TestFixture]
public class WhenUsingFusionDropDownButton : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionDropDownButton";

    private PagePlan<FusionDropDownButtonModel> _plan = null!;

    private FusionDropDownButtonLocator Reports => new(Page, "shift-reports-menu");
    private ILocator Status => Page.Locator("#print-status");
    private ILocator LastPrinted => Page.Locator("#last-printed");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<FusionDropDownButtonModel>.FromPage(Page);
        await Expect(Reports.Button).ToHaveTextAsync("Print report", new() { Timeout = 5000 });
    }

    private async Task ChooseWing(string wing)
    {
        await _plan.NativeDropDown(m => m.Wing).Select(wing);
        await Expect(Reports.Button).ToBeEnabledAsync(new() { Timeout = 5000 });
    }

    private async Task PrintTheShiftHandoff()
    {
        await ChooseWing("Maple");
        await Reports.Choose("Shift handoff");
        await Expect(Status).ToHaveTextAsync("Sent to the Maple nurses' station printer.", new() { Timeout = 5000 });
    }

    [Test]
    public async Task the_print_menu_is_locked_until_a_wing_is_chosen()
    {
        await NavigateAndBoot();

        await Expect(Reports.Button).ToBeDisabledAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_a_wing_unlocks_the_print_menu_with_the_keyboard_on_it()
    {
        await NavigateAndBoot();

        await _plan.NativeDropDown(m => m.Wing).Select("Maple");

        await Expect(Reports.Button).ToBeEnabledAsync(new() { Timeout = 5000 });
        await Expect(Reports.Button).ToBeFocusedAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_the_wing_locks_the_print_menu_again()
    {
        await NavigateAndBoot();
        await ChooseWing("Maple");

        await _plan.NativeDropDown(m => m.Wing).Select("-- Choose a wing --");

        await Expect(Reports.Button).ToBeDisabledAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task printing_a_report_says_which_printer_it_went_to_and_names_it_as_last_printed()
    {
        await NavigateAndBoot();
        await ChooseWing("Maple");

        await Reports.Choose("Shift handoff");

        await Expect(Status).ToHaveTextAsync("Sent to the Maple nurses' station printer.", new() { Timeout = 5000 });
        await Expect(LastPrinted).ToHaveTextAsync("Last printed: Shift handoff");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_printed_report_leaves_the_menu()
    {
        await NavigateAndBoot();
        await PrintTheShiftHandoff();

        await ClickWhenStable(Reports.Button);

        await Expect(Reports.Item("Medication pass")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Reports.Item("Shift handoff")).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task after_a_print_the_menu_offers_to_print_another_report()
    {
        await NavigateAndBoot();

        await PrintTheShiftHandoff();

        await Expect(Reports.Button).ToHaveTextAsync("Print another report", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task after_a_print_the_menu_turns_green()
    {
        await NavigateAndBoot();

        await PrintTheShiftHandoff();

        await Expect(Reports.Button).ToHaveClassAsync(new Regex(@"\be-success\b"), new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_incident_summary_with_nothing_in_it_leaves_the_menu()
    {
        await NavigateAndBoot();
        await ChooseWing("Maple");

        await Reports.Choose("Incident summary");

        await Expect(Status).ToHaveTextAsync("No incidents were logged on Maple wing this shift.", new() { Timeout = 5000 });
        await Expect(Reports.Item("Medication pass")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Reports.Item("Incident summary")).ToHaveCountAsync(0);
        AssertNoConsoleErrors();
    }

    // The menu closes as the item is chosen; only the page opens it again.
    [Test]
    public async Task after_a_report_with_nothing_in_it_the_menu_opens_again_for_another_choice()
    {
        await NavigateAndBoot();
        await ChooseWing("Maple");

        await Reports.Choose("Incident summary");

        await Expect(Status).ToHaveTextAsync("No incidents were logged on Maple wing this shift.", new() { Timeout = 5000 });
        await Expect(Reports.Button).ToHaveAttributeAsync("aria-expanded", "true", new() { Timeout = 5000 });
        await Expect(Reports.Item("Shift handoff")).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }
}
