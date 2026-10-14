using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.DropDownList;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.DropDownList;

/// <summary>
/// As an admissions coordinator, I want to choose the care level a new resident needs and then a wing that
/// offers it, with the wing list following the care level, so that no resident is placed in a wing that
/// cannot give her the care she needs.
/// </summary>
/// <remarks>
/// Eleanor Whitfield moves in on Monday. Independent Living is offered in Birch (room open) and Aspen;
/// Assisted Living in Maple and Cedar (Main building, Cedar has the open room) and Laurel (Garden house);
/// Memory Care in Willow and Juniper (Garden house), neither with a room open. The St. Mary's referral
/// recommends Memory Care. A care level with an open room fills in that wing; one without moves the cursor
/// to the wing list and opens it. Entering the wing list opens it once a care level has filled it.
/// Not exercised: the render helper without a build step (a list rendered with nothing to choose is no
/// placement). The Focus and Blur payloads carry nothing to read. Change's IsInteracted cannot be told apart
/// from the payload's <c>e</c> and <c>event</c> fields, which EJ2 fills from the same browser event
/// (drop-down-list.js:1859-1869).
/// </remarks>
[TestFixture]
public class WhenUsingFusionDropDownList : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/DropDownList";

    private PagePlan<FusionDropDownListModel> _plan = null!;

    private FusionDropDownListLocator CareLevel => new(Page, _plan.FindComponent(m => m.CareLevel)!.ElementId);
    private FusionDropDownListLocator Wing => new(Page, _plan.FindComponent(m => m.Wing)!.ElementId);
    private ILocator UseReferral => Page.Locator("#use-referral-btn");
    private ILocator CareLevelHint => Page.Locator("#care-level-hint");
    private ILocator ReferralNote => Page.Locator("#referral-note");
    private ILocator OpenRoom => Page.Locator("#open-room");
    private ILocator NoOpenRoom => Page.Locator("#no-open-room");
    private ILocator Placement => Page.Locator("#placement");
    private ILocator Admit => Page.Locator("#admit-btn");
    private ILocator AdmissionStatus => Page.Locator("#admission-status");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<FusionDropDownListModel>.FromPage(Page);
        await Expect(CareLevel.DropDownButton).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    // Assisted Living has a room open in Cedar, which the page fills in.
    private async Task ChooseAssistedLiving()
    {
        await CareLevel.Choose("Assisted Living");
        await Expect(OpenRoom).ToHaveTextAsync("A room is open this week in Cedar.", new() { Timeout = 5000 });
    }

    [Test]
    public async Task choosing_a_care_level_fills_in_the_wing_with_a_room_open_this_week()
    {
        await NavigateAndBoot();

        await CareLevel.Choose("Assisted Living");

        await Expect(OpenRoom).ToHaveTextAsync("A room is open this week in Cedar.", new() { Timeout = 5000 });
        await Expect(Wing.Input).ToHaveValueAsync("Cedar");
        await Expect(Placement).ToHaveTextAsync("Eleanor Whitfield: Assisted Living, Cedar wing.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_wing_list_offers_only_the_wings_of_the_care_level_by_building()
    {
        await NavigateAndBoot();
        await ChooseAssistedLiving();

        await Wing.Open();

        await Expect(Wing.Items).ToHaveTextAsync(new[] { "Maple", "Cedar", "Laurel" });
        await Expect(Wing.GroupHeaders).ToHaveTextAsync(new[] { "Main building", "Garden house" });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_care_level_with_no_open_room_opens_the_wing_list_to_choose_the_wing_to_wait_for()
    {
        await NavigateAndBoot();

        await CareLevel.Choose("Memory Care");

        await Expect(NoOpenRoom).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Wing.Wrapper).ToBeFocusedAsync(new() { Timeout = 5000 });
        await Expect(Wing.Popup).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Wing.Items).ToHaveTextAsync(new[] { "Willow", "Juniper" });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_another_care_level_replaces_the_wings_and_clears_the_wing_chosen_before()
    {
        await NavigateAndBoot();
        await ChooseAssistedLiving();

        await CareLevel.Choose("Memory Care");

        await Expect(Wing.Popup).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Wing.Items).ToHaveTextAsync(new[] { "Willow", "Juniper" });
        await Expect(Wing.Input).ToHaveValueAsync("");
        await Expect(Placement).ToBeHiddenAsync();
        await Expect(OpenRoom).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task tabbing_into_the_wing_opens_its_list()
    {
        await NavigateAndBoot();
        await ChooseAssistedLiving();
        await Expect(Wing.Popup).ToBeHiddenAsync();

        await CareLevel.Wrapper.PressAsync("Tab");

        await Expect(Wing.Wrapper).ToBeFocusedAsync(new() { Timeout = 5000 });
        await Expect(Wing.Popup).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Wing.Items).ToHaveTextAsync(new[] { "Maple", "Cedar", "Laurel" });
        AssertNoConsoleErrors();
    }

    // With no care level the wing list has nothing in it, so entering it opens nothing.
    [Test]
    public async Task leaving_the_care_level_without_choosing_one_says_to_choose_it_first()
    {
        await NavigateAndBoot();

        await CareLevel.Wrapper.PressAsync("Tab");

        await Expect(CareLevelHint).ToHaveTextAsync("Choose a care level to see the wings that offer it.", new() { Timeout = 5000 });
        await Expect(Wing.Wrapper).ToBeFocusedAsync();
        await Expect(Wing.Popup).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // The clear button empties the care level and leaves its list open; only the page closes it.
    [Test]
    public async Task clearing_the_care_level_closes_its_list_and_takes_the_wing_with_it()
    {
        await NavigateAndBoot();
        await ChooseAssistedLiving();
        await CareLevel.Open();

        await ClickWhenStable(CareLevel.ClearButton);

        await Expect(CareLevel.Input).ToHaveValueAsync("", new() { Timeout = 5000 });
        await Expect(CareLevel.Popup).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(Wing.Input).ToHaveValueAsync("");
        await Expect(Placement).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // The clear button empties the wing and leaves its list open and focused; only the page closes it.
    [Test]
    public async Task clearing_the_wing_closes_its_list_and_leaves_the_field()
    {
        await NavigateAndBoot();
        await ChooseAssistedLiving();
        await Wing.Open();

        await ClickWhenStable(Wing.ClearButton);

        await Expect(Wing.Input).ToHaveValueAsync("", new() { Timeout = 5000 });
        await Expect(Wing.Popup).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(Wing.Wrapper).Not.ToBeFocusedAsync(new() { Timeout = 5000 });
        await Expect(Placement).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task using_the_referral_chooses_memory_care_and_says_where_it_came_from()
    {
        await NavigateAndBoot();

        await ClickWhenStable(UseReferral);

        await Expect(CareLevel.Input).ToHaveValueAsync("Memory Care", new() { Timeout = 5000 });
        await Expect(ReferralNote).ToHaveTextAsync("Care level taken from the St. Mary's referral.");
        await Expect(NoOpenRoom).ToBeVisibleAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_the_care_level_by_hand_takes_down_the_referral_note()
    {
        await NavigateAndBoot();
        await ClickWhenStable(UseReferral);
        await Expect(ReferralNote).ToBeVisibleAsync(new() { Timeout = 5000 });

        await ChooseAssistedLiving();

        await Expect(ReferralNote).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(CareLevel.Input).ToHaveValueAsync("Assisted Living");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task admitting_moves_the_resident_into_the_wing_with_the_open_room()
    {
        await NavigateAndBoot();
        await ChooseAssistedLiving();

        await ClickWhenStable(Admit);

        await Expect(AdmissionStatus).ToHaveTextAsync(
            "Eleanor Whitfield moves into Cedar (Assisted Living) on Monday, October 20.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task admitting_to_a_wing_with_no_open_room_puts_her_on_its_waitlist()
    {
        await NavigateAndBoot();
        await CareLevel.Choose("Memory Care");
        await Expect(Wing.Popup).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Wing.Pick("Willow");
        await Expect(Placement).ToHaveTextAsync("Eleanor Whitfield: Memory Care, Willow wing.", new() { Timeout = 5000 });

        await ClickWhenStable(Admit);

        await Expect(AdmissionStatus).ToHaveTextAsync(
            "Eleanor Whitfield is on the waitlist for Willow (Memory Care).", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task admitting_without_a_care_level_or_wing_says_what_is_missing()
    {
        await NavigateAndBoot();

        await ClickWhenStable(Admit);

        await Expect(_plan.ErrorFor(m => m.CareLevel)).ToHaveTextAsync("Choose the care level the resident needs.", new() { Timeout = 5000 });
        await Expect(_plan.ErrorFor(m => m.Wing)).ToHaveTextAsync("Choose the wing the resident moves into.");
        await Expect(AdmissionStatus).ToHaveTextAsync("Eleanor Whitfield is not placed yet.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_the_missing_care_level_clears_its_message_without_admitting_again()
    {
        await NavigateAndBoot();
        await ClickWhenStable(Admit);
        await Expect(_plan.ErrorFor(m => m.CareLevel)).ToBeVisibleAsync(new() { Timeout = 5000 });

        await ChooseAssistedLiving();

        await Expect(_plan.ErrorFor(m => m.CareLevel)).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(_plan.ErrorFor(m => m.Wing)).ToBeHiddenAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
